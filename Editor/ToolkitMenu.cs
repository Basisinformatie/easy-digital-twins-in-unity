using UnityEditor;

namespace Rotterdam.DigitalTwins.Editor
{
    public class ToolkitMenu
    {
        [MenuItem("Tools/Easy Digital Twins")]
        public static void OpenShoppingWindow()
        {
            ShoppingWindow.ShowWindow();
        }
    }
}