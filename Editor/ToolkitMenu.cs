using UnityEditor;
using Rotterdam.DigitalTwins.Editor;

namespace Rotterdam.DigitalTwins.Editor
{
    public class ToolkitMenu
    {
        [MenuItem("Rotterdam Digital Twins/Launch UI")]
        public static void OpenShoppingWindow()
        {
            CesiumSetupService.CheckAndInstallCesium(ShoppingWindow.ShowWindow);
        }
    }
}