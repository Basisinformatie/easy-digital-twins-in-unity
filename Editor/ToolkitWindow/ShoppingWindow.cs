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

            Label label = new Label("by Gemeente Rotterdam");
            Label label2 = new Label("Powered by Cesium");
            
            Button button = new Button();
            button.text = "Test";
            
            root.Add(label);
            root.Add(label2);
            root.Add(button);
        }
    }
}