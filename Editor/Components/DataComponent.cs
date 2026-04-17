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

            List<string> sources = new List<string> { "Open Urban Platform (OUP)" };
            DropdownField sourceDropdown = new DropdownField("Datasource", sources, 0);
            sourceDropdown.style.marginBottom = 10;
            Add(sourceDropdown);

            VisualElement filterBar = new VisualElement();
            filterBar.style.flexDirection = FlexDirection.Row;
            filterBar.style.marginBottom = 10;

            _searchField = new TextField("Search");
            _searchField.style.flexGrow = 1;
            _searchField.RegisterValueChangedCallback(_ => RefreshData());
            filterBar.Add(_searchField);

            _hubDropdown = new DropdownField();
            _hubDropdown.style.width = 120;
            _hubDropdown.style.marginLeft = 5;
            _hubDropdown.RegisterValueChangedCallback(_ => RefreshData());
            filterBar.Add(_hubDropdown);

            Add(filterBar);

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

            _catalogService.FetchDatasets(datasets =>
            {
                _scrollView.Clear();
                foreach (var dataset in datasets)
                {
                    _scrollView.Add(CreateDatasetCard(dataset));
                }
            }, error => Debug.LogError($"Failed to load datasets: {error}"), _searchField.value, selectedHubId, null, new List<string> { "3dtileset", "wms", "3dtile", "3dtiles" });
        }

        private VisualElement CreateDatasetCard(OUPDataset dataset)
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
            
            if (!string.IsNullOrEmpty(dataset.thumbnailUrl))
            {
                LoadThumbnail(dataset.thumbnailUrl, preview);
            }
            else
            {
                Label placeholder = new Label("No Preview");
                placeholder.style.alignSelf = Align.Center;
                placeholder.style.marginTop = 40;
                preview.Add(placeholder);
            }
            card.Add(preview);

            Label title = new Label(dataset.title);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.whiteSpace = WhiteSpace.Normal;
            title.style.fontSize = 12;
            card.Add(title);

            if (dataset.tags != null && dataset.tags.Count > 0)
            {
                Label tags = new Label(string.Join(", ", dataset.tags.Take(2)));
                tags.style.fontSize = 10;
                tags.style.color = Color.gray;
                card.Add(tags);
            }

            if (dataset.resources != null && dataset.resources.Count > 0)
            {
                var matchingFormats = dataset.resources
                    .Where(f => new[] { "3dtileset", "wms", "3dtile", "3dtiles" }.Any(fmt => string.Equals(fmt, f.format, System.StringComparison.OrdinalIgnoreCase)))
                    .Select(f => f.format.ToUpper())
                    .Distinct();
                
                if (matchingFormats.Any())
                {
                    Label formatsLabel = new Label(string.Join(", ", matchingFormats));
                    formatsLabel.style.fontSize = 9;
                    formatsLabel.style.color = new Color(0.3f, 0.7f, 1f);
                    formatsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    card.Add(formatsLabel);
                }
            }

            /*
            Button selectBtn = new Button(() => OnDatasetSelected(dataset)) { text = "Select" };
            selectBtn.style.marginTop = 5;
            card.Add(selectBtn);
            */
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