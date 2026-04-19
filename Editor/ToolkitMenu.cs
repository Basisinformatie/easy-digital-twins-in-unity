using UnityEditor;

namespace Rotterdam.DigitalTwins.Editor
{
    public class ToolkitMenu
    {
        [MenuItem("Rotterdam Digital Twins/Launch UI")]
        public static void OpenShoppingWindow()
        {
            CesiumSetupService.EnsureCesiumIsInstalled();
            ShoppingWindow.ShowWindow();
        }
    }
}