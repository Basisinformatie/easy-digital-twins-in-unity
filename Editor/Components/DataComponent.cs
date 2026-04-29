using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Rotterdam.DigitalTwins.Runtime;
using System.Linq;

namespace Rotterdam.DigitalTwins.Editor
{
    public class DataComponent : VisualElement
    {
        private readonly ICatalogService _catalogService;
        private ScrollView _scrollView;
        private TextField _searchField;
        private DropdownField _hubDropdown;
        private DropdownField _typeDropdown;
        private Toggle _resourceFilterToggle;
        private List<OUPHub> _hubs = new();

        private VisualElement _loadingIndicator;
        private int _currentRequestId = 0;
        private IVisualElementScheduledItem _searchScheduledItem;

        private static readonly string[] AllowedFormats = { "3dtileset", "3dtile", "3dtiles", "3dterrain", "3d tiles", "3d-tiles", "3dpointclouds" };

        public DataComponent(ICatalogService catalogService)
        {
            _catalogService = catalogService;
            style.flexGrow = 1;

            Label label = new Label("Browse Data Catalogue");
            label.style.fontSize = 16;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 10;
            Add(label);
            
            Label info = new Label("Making a selection adds live city data to your experience. Using the Open Urban Platform (OUP) as a catalogue we can browse and add data, the added data sources are processed and live streamed with Cesium.");
            info.style.whiteSpace = WhiteSpace.Normal;
            info.style.marginTop = 10;
            info.style.marginBottom = 10;
            Add(info);

            VisualElement topBar = new VisualElement();
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.marginBottom = 10;

            List<string> types = new List<string> { "Datasets", "Digital Twins" };
            _typeDropdown = new DropdownField("Type", types, 0);
            _typeDropdown.style.flexGrow = 1;
            _typeDropdown.RegisterValueChangedCallback(_ => {
                _resourceFilterToggle.style.display = _typeDropdown.index == 1 ? DisplayStyle.Flex : DisplayStyle.None;
                RefreshData();
            });
            topBar.Add(_typeDropdown);

            _resourceFilterToggle = new Toggle("Only 3D Resources");
            _resourceFilterToggle.tooltip = "Hide digital twins that do not have any supported 3D resources (3D Tiles, Terrain, etc.)";
            _resourceFilterToggle.style.display = DisplayStyle.None;
            _resourceFilterToggle.style.marginLeft = 10;
            _resourceFilterToggle.RegisterValueChangedCallback(_ => RefreshData());
            topBar.Add(_resourceFilterToggle);

            Add(topBar);

            VisualElement filterBar = new VisualElement();
            filterBar.style.flexDirection = FlexDirection.Row;
            filterBar.style.marginBottom = 10;

            _searchField = new TextField("Search");
            _searchField.tooltip = "Search by title, description, location or tags";
            _searchField.style.flexGrow = 1;
            _searchField.RegisterValueChangedCallback(_ => {
                _searchScheduledItem?.Pause();
                _searchScheduledItem = _searchField.schedule.Execute(() => RefreshData()).StartingIn(300);
            });
            filterBar.Add(_searchField);

            _hubDropdown = new DropdownField();
            _hubDropdown.style.width = 120;
            _hubDropdown.style.marginLeft = 5;
            _hubDropdown.RegisterValueChangedCallback(_ => RefreshData());
            filterBar.Add(_hubDropdown);

            Add(filterBar);

            Button blankTilesetButton = new Button(() => CesiumSceneHelper.CreateBlank3DTileset());
            blankTilesetButton.text = "Blank 3D Tiles Tileset";
            blankTilesetButton.tooltip = "Creates a CesiumGeoreference and a Blank 3D Tiles Tileset in the scene.";
            blankTilesetButton.style.height = 30;
            blankTilesetButton.style.marginBottom = 10;
            blankTilesetButton.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            blankTilesetButton.style.borderBottomLeftRadius = 5;
            blankTilesetButton.style.borderBottomRightRadius = 5;
            blankTilesetButton.style.borderTopLeftRadius = 5;
            blankTilesetButton.style.borderTopRightRadius = 5;
            blankTilesetButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(blankTilesetButton);

            VisualElement resultsContainer = new VisualElement();
            resultsContainer.style.flexGrow = 1;
            Add(resultsContainer);

            _scrollView = new ScrollView();
            _scrollView.style.flexGrow = 1;
            _scrollView.contentContainer.style.flexDirection = FlexDirection.Row;
            _scrollView.contentContainer.style.flexWrap = Wrap.Wrap;
            resultsContainer.Add(_scrollView);

            _loadingIndicator = new VisualElement();
            _loadingIndicator.style.alignItems = Align.Center;
            _loadingIndicator.style.justifyContent = Justify.Center;
            _loadingIndicator.style.position = Position.Absolute;
            _loadingIndicator.style.width = Length.Percent(100);
            _loadingIndicator.style.height = Length.Percent(100);
            _loadingIndicator.style.backgroundColor = new Color(0, 0, 0, 0.1f);
            _loadingIndicator.style.display = DisplayStyle.None;
            _loadingIndicator.pickingMode = PickingMode.Ignore;

            Label spinner = new Label("↻");
            spinner.style.fontSize = 40;
            spinner.style.color = new Color(0.1f, 0.5f, 0.1f);
            _loadingIndicator.Add(spinner);
            
            _loadingIndicator.schedule.Execute(() => {
                float currentRotate = spinner.transform.rotation.eulerAngles.z;
                spinner.transform.rotation = Quaternion.Euler(0, 0, currentRotate + 20);
            }).Every(50);
            
            resultsContainer.Add(_loadingIndicator);

            LoadHubs();
            RefreshData();
        }

        private void LoadHubs()
        {
            _catalogService.FetchHubs(hubs =>
            {
                _hubs = hubs;
                var choices = new List<string> { "All Hubs" };
                choices.AddRange(hubs.Select(h => h.name));
                _hubDropdown.choices = choices;
                _hubDropdown.index = 0;
            }, error => Debug.LogError($"Failed to load hubs: {error}"));
        }

        private void RefreshData()
        {
            int requestId = ++_currentRequestId;

            string selectedHubId = "";
            if (_hubDropdown.index > 0 && _hubs.Count >= _hubDropdown.index)
            {
                selectedHubId = _hubs[_hubDropdown.index - 1]._id;
            }

            _scrollView.Clear();
            _loadingIndicator.style.display = DisplayStyle.Flex;

            if (_typeDropdown.index == 0) // Datasets
            {
                _catalogService.FetchDatasets(datasets =>
                {
                    if (requestId != _currentRequestId) return;
                    _loadingIndicator.style.display = DisplayStyle.None;

                    foreach (var dataset in datasets)
                    {
                        _scrollView.Add(CreateDatasetCard(dataset));
                    }
                }, error => {
                    if (requestId != _currentRequestId) return;
                    _loadingIndicator.style.display = DisplayStyle.None;
                    Debug.LogError($"Failed to load datasets: {error}");
                }, _searchField.value, selectedHubId, null, AllowedFormats.ToList());
            }
            else // Digital Twins
            {
                _catalogService.FetchDigitalTwins(twins =>
                {
                    if (requestId != _currentRequestId) return;
                    _loadingIndicator.style.display = DisplayValueToNone();

                    foreach (var twin in twins)
                    {
                        if (_resourceFilterToggle.value)
                        {
                            bool hasResources = twin.configuration?.Any(c => 
                                c.resources?.Any(r => AllowedFormats.Any(fmt => string.Equals(fmt, r.format, System.StringComparison.OrdinalIgnoreCase))) == true) == true;
                            
                            if (!hasResources) continue;
                        }
                        _scrollView.Add(CreateDigitalTwinCard(twin));
                    }
                }, error => {
                    if (requestId != _currentRequestId) return;
                    _loadingIndicator.style.display = DisplayValueToNone();
                    Debug.LogError($"Failed to load digital twins: {error}");
                }, _searchField.value, selectedHubId);
            }

            DisplayStyle DisplayValueToNone() => DisplayStyle.None;
        }

        private VisualElement CreateDatasetCard(OUPDataset dataset)
        {
            string infoUrl = $"https://hub.clearly.app/datasets/{dataset._id}/information";
            VisualElement card = CreateBaseCard(dataset.title, null, dataset.tags, dataset.ownerHub?.name, infoUrl);

            if (dataset.resources != null && dataset.resources.Count > 0)
            {
                var allMatchingResources = dataset.resources
                    .Where(r => AllowedFormats.Any(fmt => string.Equals(fmt, r.format, System.StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                
                var matchingFormatsStrings = allMatchingResources
                    .Select(f => f.format.ToUpper())
                    .Distinct();
                
                if (matchingFormatsStrings.Any())
                {
                    Label formatsLabel = new Label(string.Join(", ", matchingFormatsStrings));
                    formatsLabel.style.fontSize = 9;
                    formatsLabel.style.color = new Color(0.3f, 0.7f, 1f);
                    formatsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    card.Add(formatsLabel);
                    
                    var tilesetResources = allMatchingResources;

                    foreach (var res in tilesetResources)
                    {
                        string buttonText;
                        string tilesetName;
                        bool isTerrain = string.Equals(res.format, "3dterrain", System.StringComparison.OrdinalIgnoreCase);
                        bool isPointCloud = string.Equals(res.format, "3dpointclouds", System.StringComparison.OrdinalIgnoreCase);

                        if (tilesetResources.Count == 1)
                        {
                            buttonText = isTerrain ? "Add 3D Terrain" : "Add 3D Tileset";
                            tilesetName = dataset.title;
                        }
                        else
                        {
                            string displayName = string.IsNullOrEmpty(res.name) ? res.format.ToUpper() : res.name;
                            string displayButtonName = displayName.Length > 17 ? displayName.Substring(0, 17) + "..." : displayName;
                            buttonText = $"Add {displayButtonName}";
                            tilesetName = $"{dataset.title} ({displayName})";
                        }

                        Button addButton = new Button(() => CesiumSceneHelper.Create3DTilesetFromUrl(tilesetName, res.url, isPointCloud));
                        addButton.text = buttonText;
                        addButton.tooltip = res.name;
                        addButton.style.marginTop = 5;
                        addButton.style.height = 20;
                        addButton.style.fontSize = 10;
                        addButton.style.backgroundColor = new Color(0.2f, 0.5f, 0.2f);
                        addButton.style.color = Color.white;
                        addButton.style.borderBottomLeftRadius = 3;
                        addButton.style.borderBottomRightRadius = 3;
                        addButton.style.borderTopLeftRadius = 3;
                        addButton.style.borderTopRightRadius = 3;
                        card.Add(addButton);
                    }
                }
            }

            return card;
        }

        private VisualElement CreateDigitalTwinCard(OUPDigitalTwin twin)
        {
            string infoUrl = $"https://hub.clearly.app/digital-twins/{twin._id}/information";
            VisualElement card = CreateBaseCard(twin.title, twin.previewImage, twin.tags, null, infoUrl);
            
            if (twin.ownerHub != null)
            {
                Label hubLabel = new Label(twin.ownerHub.name);
                hubLabel.style.fontSize = 9;
                hubLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                card.Add(hubLabel);
            }
            
            var allResources = new List<OUPResource>();
            if (twin.configuration != null)
            {
                foreach (var config in twin.configuration)
                {
                    if (config.resources != null)
                    {
                        allResources.AddRange(config.resources);
                    }
                }
            }

            var matchingResources = allResources
                .Where(r => AllowedFormats.Any(fmt => string.Equals(fmt, r.format, System.StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (matchingResources.Count > 0)
            {
                Button addButton = new Button(() => {
                    if (twin.viewpoint?.groundPosition != null && twin.viewpoint.groundPosition.Count >= 2)
                    {
                        double lon = twin.viewpoint.groundPosition[0];
                        double lat = twin.viewpoint.groundPosition[1];
                        double height = twin.viewpoint.groundPosition.Count > 2 ? twin.viewpoint.groundPosition[2] : 0;
                        CesiumSceneHelper.SetGeoreference(lat, lon, height);
                    }
                    CesiumSceneHelper.CreateMultiple3DTilesets(twin.title, matchingResources);
                });
                
                addButton.text = matchingResources.Count > 1 ? "Add Digital Twin (All)" : "Add Digital Twin";
                addButton.style.marginTop = 5;
                addButton.style.height = 20;
                addButton.style.fontSize = 10;
                addButton.style.backgroundColor = new Color(0.2f, 0.5f, 0.2f);
                addButton.style.color = Color.white;
                addButton.style.borderBottomLeftRadius = 3;
                addButton.style.borderBottomRightRadius = 3;
                addButton.style.borderTopLeftRadius = 3;
                addButton.style.borderTopRightRadius = 3;
                card.Add(addButton);
            }

            return card;
        }

        private VisualElement CreateBaseCard(string titleText, string thumbUrl, List<string> tagsList, string previewText = null, string infoUrl = null)
        {
            VisualElement card = new VisualElement();
            card.style.width = 150;
            card.style.marginRight = 10;
            card.style.marginBottom = 10;
            card.style.paddingLeft = 5;
            card.style.paddingRight = 5;
            card.style.paddingTop = 5;
            card.style.paddingBottom = 5;
            card.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            card.style.borderBottomLeftRadius = 5;
            card.style.borderBottomRightRadius = 5;
            card.style.borderTopLeftRadius = 5;
            card.style.borderTopRightRadius = 5;

            VisualElement preview = new VisualElement();
            preview.style.height = 100;
            preview.style.backgroundColor = Color.black;
            preview.style.marginBottom = 5;
            preview.style.justifyContent = Justify.Center;
            preview.style.alignItems = Align.Center;
            preview.style.overflow = Overflow.Hidden;
            
            if (!string.IsNullOrEmpty(thumbUrl))
            {
                LoadThumbnail(thumbUrl, preview);
            }
            else
            {
                Label placeholder = new Label(!string.IsNullOrEmpty(previewText) ? previewText : "No Preview");
                placeholder.style.fontSize = 11;
                placeholder.style.color = new Color(0.8f, 0.8f, 0.8f);
                placeholder.style.unityFontStyleAndWeight = FontStyle.Bold;
                placeholder.style.whiteSpace = WhiteSpace.Normal;
                placeholder.style.unityTextAlign = TextAnchor.MiddleCenter;
                placeholder.style.paddingLeft = 5;
                placeholder.style.paddingRight = 5;
                preview.Add(placeholder);
            }

            if (!string.IsNullOrEmpty(infoUrl))
            {
                VisualElement triangle = new VisualElement();
                triangle.style.position = Position.Absolute;
                triangle.style.top = 0;
                triangle.style.left = 0;
                triangle.style.width = 0;
                triangle.style.height = 0;
                triangle.style.borderTopWidth = 20;
                triangle.style.borderRightWidth = 20;
                triangle.style.borderTopColor = new Color(0.12f, 0.45f, 0.85f);
                triangle.style.borderRightColor = Color.clear;

                triangle.RegisterCallback<ClickEvent>(e => {
                    Application.OpenURL(infoUrl);
                    e.StopPropagation();
                });
                triangle.tooltip = "Open Catalogue Page";

                Label infoLabel = new Label("i");
                infoLabel.style.position = Position.Absolute;
                infoLabel.style.top = -20;
                infoLabel.style.left = 1;
                infoLabel.style.color = Color.white;
                infoLabel.style.fontSize = 10;
                infoLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                infoLabel.pickingMode = PickingMode.Ignore;
                triangle.Add(infoLabel);

                preview.Add(triangle);
            }
            
            card.Add(preview);

            VisualElement titleContainer = new VisualElement();
            titleContainer.style.flexDirection = FlexDirection.Row;
            titleContainer.style.alignItems = Align.Center;

            Label title = new Label(titleText);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.whiteSpace = WhiteSpace.Normal;
            title.style.fontSize = 12;
            title.style.flexShrink = 1;
            titleContainer.Add(title);

            card.Add(titleContainer);

            if (tagsList != null && tagsList.Count > 0)
            {
                Label tags = new Label(string.Join(", ", tagsList.Take(2)));
                tags.style.fontSize = 10;
                tags.style.color = Color.gray;
                card.Add(tags);
            }

            return card;
        }

        private void LoadThumbnail(string url, VisualElement container)
        {
            var request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
            var op = request.SendWebRequest();
            op.completed += _ =>
            {
                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    var texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
                    container.style.backgroundImage = new StyleBackground(Background.FromTexture2D(texture));
                }
                request.Dispose();
            };
        }
        

    }
}