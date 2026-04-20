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
        private List<OUPHub> _hubs = new();

        public DataComponent(ICatalogService catalogService)
        {
            _catalogService = catalogService;
            style.flexGrow = 1;

            Label label = new Label("Data Shopping");
            label.style.fontSize = 16;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 10;
            Add(label);

            VisualElement topBar = new VisualElement();
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.marginBottom = 10;

            List<string> types = new List<string> { "Datasets", "Digital Twins" };
            _typeDropdown = new DropdownField("Type", types, 0);
            _typeDropdown.style.flexGrow = 1;
            _typeDropdown.RegisterValueChangedCallback(_ => RefreshData());
            topBar.Add(_typeDropdown);

            Add(topBar);

            VisualElement filterBar = new VisualElement();
            filterBar.style.flexDirection = FlexDirection.Row;
            filterBar.style.marginBottom = 10;

            _searchField = new TextField("Search");
            _searchField.tooltip = "Search by title, description, location or tags";
            _searchField.style.flexGrow = 1;
            _searchField.RegisterValueChangedCallback(_ => RefreshData());
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

            _scrollView = new ScrollView();
            _scrollView.style.flexGrow = 1;
            _scrollView.contentContainer.style.flexDirection = FlexDirection.Row;
            _scrollView.contentContainer.style.flexWrap = Wrap.Wrap;
            Add(_scrollView);

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
            string selectedHubId = "";
            if (_hubDropdown.index > 0 && _hubs.Count >= _hubDropdown.index)
            {
                selectedHubId = _hubs[_hubDropdown.index - 1]._id;
            }

            _scrollView.Clear();

            if (_typeDropdown.index == 0) // Datasets
            {
                _catalogService.FetchDatasets(datasets =>
                {
                    foreach (var dataset in datasets)
                    {
                        _scrollView.Add(CreateDatasetCard(dataset));
                    }
                }, error => Debug.LogError($"Failed to load datasets: {error}"), _searchField.value, selectedHubId, null, new List<string> { "3dtileset", "3dtile", "3dtiles", "3dterrain" });
            }
            else // Digital Twins
            {
                _catalogService.FetchDigitalTwins(twins =>
                {
                    foreach (var twin in twins)
                    {
                        _scrollView.Add(CreateDigitalTwinCard(twin));
                    }
                }, error => Debug.LogError($"Failed to load digital twins: {error}"), _searchField.value, selectedHubId);
            }
        }

        private VisualElement CreateDatasetCard(OUPDataset dataset)
        {
            VisualElement card = CreateBaseCard(dataset.title, dataset.thumbnailUrl, dataset.tags);
            AddResourceButtons(card, dataset.title, dataset.resources);
            return card;
        }

        private VisualElement CreateDigitalTwinCard(OUPDigitalTwin twin)
        {
            VisualElement card = CreateBaseCard(twin.title, twin.previewImage, twin.tags);
            
            if (twin.ownerHub != null)
            {
                Label hubLabel = new Label(twin.ownerHub.name);
                hubLabel.style.fontSize = 9;
                hubLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                card.Add(hubLabel);
            }

            AddResourceButtons(card, twin.title, twin.resources, twin.groundPosition);

            return card;
        }

        private void AddResourceButtons(VisualElement card, string title, List<OUPResource> resources, OUPGroundPosition groundPosition = null)
        {
            if (resources == null || resources.Count == 0) return;

            var matchingResources = resources
                .Where(r => new[] { "3dtileset", "3dtile", "3dtiles", "3dterrain" }.Any(fmt => string.Equals(fmt, r.format, System.StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (matchingResources.Count == 0) return;

            var matchingFormatsStrings = matchingResources
                .Select(f => f.format.ToUpper())
                .Distinct();

            Label formatsLabel = new Label(string.Join(", ", matchingFormatsStrings));
            formatsLabel.style.fontSize = 9;
            formatsLabel.style.color = new Color(0.3f, 0.7f, 1f);
            formatsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            card.Add(formatsLabel);

            if (matchingResources.Count > 1)
            {
                Button addAllButton = new Button(() => 
                {
                    CesiumSceneHelper.SetGeoreference(groundPosition);
                    CesiumSceneHelper.CreateMultiple3DTilesets(title, matchingResources);
                });
                addAllButton.text = "Add All 3D Tilesets";
                addAllButton.style.marginTop = 5;
                addAllButton.style.height = 24;
                addAllButton.style.backgroundColor = new Color(0.1f, 0.3f, 0.6f);
                addAllButton.style.color = Color.white;
                addAllButton.style.unityFontStyleAndWeight = FontStyle.Bold;
                addAllButton.style.borderBottomLeftRadius = 3;
                addAllButton.style.borderBottomRightRadius = 3;
                addAllButton.style.borderTopLeftRadius = 3;
                addAllButton.style.borderTopRightRadius = 3;
                card.Add(addAllButton);
            }

            foreach (var res in matchingResources)
            {
                string buttonText;
                string tilesetName;
                bool isTerrain = string.Equals(res.format, "3dterrain", System.StringComparison.OrdinalIgnoreCase);

                if (matchingResources.Count == 1)
                {
                    buttonText = isTerrain ? "Add 3D Terrain" : "Add 3D Tileset";
                    tilesetName = title;
                }
                else
                {
                    string displayName = string.IsNullOrEmpty(res.name) ? res.format.ToUpper() : res.name;
                    buttonText = $"Add {displayName}";
                    tilesetName = $"{title} ({displayName})";
                }

                Button addButton = new Button(() => 
                {
                    CesiumSceneHelper.SetGeoreference(groundPosition);
                    CesiumSceneHelper.Create3DTilesetFromUrl(tilesetName, res.url);
                });
                addButton.text = buttonText;
                addButton.tooltip = res.url;
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

        private VisualElement CreateBaseCard(string titleText, string thumbUrl, List<string> tagsList)
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
            
            if (!string.IsNullOrEmpty(thumbUrl))
            {
                LoadThumbnail(thumbUrl, preview);
            }
            else
            {
                Label placeholder = new Label("No Preview");
                placeholder.style.alignSelf = Align.Center;
                placeholder.style.marginTop = 40;
                preview.Add(placeholder);
            }
            card.Add(preview);

            Label title = new Label(titleText);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.whiteSpace = WhiteSpace.Normal;
            title.style.fontSize = 12;
            card.Add(title);

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