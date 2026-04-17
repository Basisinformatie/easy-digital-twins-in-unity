using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rotterdam.DigitalTwins.Editor
{
    public class ShoppingWindow : EditorWindow
    {
        private VisualElement _contentContainer;

        private ICatalogService _catalogService;

        public static void ShowWindow()
        {
            ShoppingWindow wnd = GetWindow<ShoppingWindow>();
            wnd.titleContent = new GUIContent("Main Menu");
            wnd.minSize = new Vector2(350, 450);
            wnd._catalogService = new OUPCatalogService();
        }

        public void CreateGUI()
        {
            if (_catalogService == null)
                _catalogService = new OUPCatalogService();
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;

            Label label = new Label("Easy Digital Twins");
            label.style.fontSize = 20;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 20;
            label.style.whiteSpace = WhiteSpace.Normal;
            root.Add(label);

            _contentContainer = new VisualElement();
            _contentContainer.style.flexGrow = 1;
            root.Add(_contentContainer);

            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            root.Add(spacer);

            VisualElement footer = new VisualElement();
            footer.style.flexDirection = FlexDirection.Column;
            footer.style.alignItems = Align.Center;
            root.Add(footer);

            Image logo = new Image();
            string logoPath = "Packages/com.rotterdam.digital-twins/Editor/ToolkitWindow/gemeente-rotterdam-logo.png";
            Texture2D logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(logoPath);
            if (logoTexture == null)
            {
                logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/ToolkitWindow/gemeente-rotterdam-logo.png");
            }
            
            logo.image = logoTexture;
            logo.style.width = 300;
            logo.style.height = 75;
            logo.style.marginTop = 20;
            footer.Add(logo);
            
            Label poweredByLabel = new Label("Powered by Cesium");
            poweredByLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            poweredByLabel.style.fontSize = 10;
            poweredByLabel.style.alignSelf = Align.FlexEnd;
            poweredByLabel.style.marginTop = 5;
            poweredByLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
            footer.Add(poweredByLabel);

            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            _contentContainer.Clear();

            VisualElement buttonContainer = new VisualElement();
            buttonContainer.style.marginLeft = 20;
            buttonContainer.style.marginRight = 20;

            Button settingsButton = new Button { text = "Settings" };
            Button readmeButton = new Button { text = "ReadMe" };
            Button samplesButton = new Button { text = "Samples" };
            Button startButton = new Button { text = "START" };

            settingsButton.clicked += ShowSettings;
            readmeButton.clicked += ShowReadMe;
            startButton.clicked += ShowShoppingWizard;

            settingsButton.style.marginTop = 5;
            settingsButton.style.marginBottom = 5;
            settingsButton.style.paddingTop = 10;
            settingsButton.style.paddingBottom = 10;
            readmeButton.style.marginTop = 5;
            readmeButton.style.marginBottom = 5;
            readmeButton.style.paddingTop = 10;
            readmeButton.style.paddingBottom = 10;
            samplesButton.style.marginTop = 5;
            samplesButton.style.marginBottom = 5;
            samplesButton.style.paddingTop = 10;
            samplesButton.style.paddingBottom = 10;

            startButton.style.height = 60;
            startButton.style.marginTop = 20;
            startButton.style.paddingTop = 10;
            startButton.style.paddingBottom = 10;
            startButton.style.backgroundColor = new Color(0.1f, 0.5f, 0.1f);
            startButton.style.color = Color.white;
            startButton.style.fontSize = 16;
            startButton.style.unityFontStyleAndWeight = FontStyle.Bold;

            buttonContainer.Add(settingsButton);
            buttonContainer.Add(readmeButton);
            buttonContainer.Add(samplesButton);
            buttonContainer.Add(startButton);

            _contentContainer.Add(buttonContainer);
        }

        private void ShowSettings()
        {
            _contentContainer.Clear();
            _contentContainer.Add(new SettingsComponent(ShowMainMenu));
        }

        private void ShowReadMe()
        {
            _contentContainer.Clear();
            _contentContainer.Add(new ReadMeComponent(ShowMainMenu));
        }

        private void ShowShoppingWizard()
        {
            _contentContainer.Clear();

            VisualElement toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.marginBottom = 10;
            toolbar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            toolbar.style.paddingTop = 5;
            toolbar.style.paddingBottom = 5;

            Button dataTab = new Button(() => SwitchTab(new DataComponent(_catalogService))) { text = "Data" };
            Button controllerTab = new Button(() => SwitchTab(new ControllerComponent())) { text = "Controller" };
            Button locationTab = new Button(() => SwitchTab(new LocationComponent())) { text = "Location" };

            dataTab.style.flexGrow = 1;
            controllerTab.style.flexGrow = 1;
            locationTab.style.flexGrow = 1;

            toolbar.Add(dataTab);
            toolbar.Add(controllerTab);
            toolbar.Add(locationTab);

            _contentContainer.Add(toolbar);

            VisualElement tabContent = new VisualElement();
            tabContent.style.flexGrow = 1;
            tabContent.name = "TabContent";
            _contentContainer.Add(tabContent);

            Button backButton = new Button(ShowMainMenu) { text = "Terug naar Menu" };
            backButton.style.marginTop = 10;
            backButton.style.paddingTop = 8;
            backButton.style.paddingBottom = 8;
            _contentContainer.Add(backButton);

            SwitchTab(new DataComponent(_catalogService));
        }

        private void SwitchTab(VisualElement content)
        {
            VisualElement tabContent = _contentContainer.Q<VisualElement>("TabContent");
            if (tabContent != null)
            {
                tabContent.Clear();
                tabContent.Add(content);
            }
        }
    }
}