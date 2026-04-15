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

            Image logo = new Image();
            Texture2D logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.rotterdam.digital-twins/Editor/ToolkitWindow/gemeente-rotterdam-logo.png");
            logo.image = logoTexture;
            logo.style.width = 200;
            logo.style.height = 50;
            logo.style.marginBottom = 10;
            root.Add(logo);

            Label label = new Label("Gemeente Rotterdam Easy Digital Twins");
            label.style.fontSize = 20;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 20;
            
            root.Add(label);

            Button startButton = new Button { text = "START" };
            Button settingsButton = new Button { text = "Settings" };
            Button readmeButton = new Button { text = "ReadMe" };
            Button samplesButton = new Button { text = "Samples" };

            startButton.style.height = 40;
            startButton.style.marginTop = 10;
            startButton.style.backgroundColor = new Color(0.1f, 0.5f, 0.1f); // Groenachtig voor START
            startButton.style.color = Color.white;

            root.Add(settingsButton);
            root.Add(readmeButton);
            root.Add(samplesButton);
            root.Add(startButton);
        }
    }
}