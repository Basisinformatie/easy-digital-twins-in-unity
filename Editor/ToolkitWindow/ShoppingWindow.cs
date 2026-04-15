using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rotterdam.DigitalTwins.Editor
{
    public class ShoppingWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            ShoppingWindow wnd = GetWindow<ShoppingWindow>();
            wnd.titleContent = new GUIContent("Easy Digital Twins");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;

            Label label = new Label("Gemeente Rotterdam Easy Digital Twins");
            label.style.fontSize = 20;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 20;
            label.style.whiteSpace = WhiteSpace.Normal;
            root.Add(label);

            VisualElement buttonContainer = new VisualElement();
            buttonContainer.style.marginLeft = 20;
            buttonContainer.style.marginRight = 20;

            Button settingsButton = new Button { text = "Settings" };
            Button readmeButton = new Button { text = "ReadMe" };
            Button samplesButton = new Button { text = "Samples" };
            Button startButton = new Button { text = "START" };

            startButton.style.height = 50;
            startButton.style.marginTop = 15;
            startButton.style.backgroundColor = new Color(0.1f, 0.5f, 0.1f); // Groenachtig voor START
            startButton.style.color = Color.white;
            startButton.style.fontSize = 16;
            startButton.style.unityFontStyleAndWeight = FontStyle.Bold;

            buttonContainer.Add(settingsButton);
            buttonContainer.Add(readmeButton);
            buttonContainer.Add(samplesButton);
            buttonContainer.Add(startButton);
            root.Add(buttonContainer);

            // Spacer om het logo naar beneden te duwen
            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            root.Add(spacer);

            Image logo = new Image();
            Texture2D logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.rotterdam.digital-twins/Editor/ToolkitWindow/gemeente-rotterdam-logo.png");
            
            // Fallback voor development
            if (logoTexture == null)
            {
                logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/ToolkitWindow/gemeente-rotterdam-logo.png");
            }

            if (logoTexture != null)
            {
                logo.image = logoTexture;
                logo.style.width = 300;
                logo.style.height = 75;
                logo.style.marginTop = 20;
                logo.style.alignSelf = Align.Center;
                root.Add(logo);
            }
        }
    }
}