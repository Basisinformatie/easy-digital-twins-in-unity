using UnityEditor;
using Rotterdam.DigitalTwins.Editor;

namespace Rotterdam.DigitalTwins.Editor
{
    public class ToolkitMenu
    {
        [MenuItem("Rotterdam Digital Twins/Launch UI")]
        public static void OpenShoppingWindow()
        {
            CesiumSetupService.EnsureCesiumIsInstalled();
            
            // We gebruiken reflectie om het window te openen, omdat de main editor assembly 
            // mogelijk nog niet gecompileerd is als Cesium ontbreekt.
            var windowType = System.Type.GetType("Rotterdam.DigitalTwins.Editor.ShoppingWindow, com.rotterdam.digital-twins.Editor");
            if (windowType != null)
            {
                var method = windowType.GetMethod("ShowWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    Debug.LogError("Could not find ShowWindow method in ShoppingWindow.");
                }
            }
            else
            {
                // Als het window type niet gevonden wordt, is de main assembly waarschijnlijk niet geladen 
                // omdat Cesium nog bezig is met installeren of ontbreekt.
                if (IsPackageInstalled("com.cesium.unity"))
                {
                    Debug.LogWarning("Cesium is installed but the Toolkit UI is not yet available. Please wait for compilation to finish.");
                }
                else
                {
                    Debug.Log("Cesium for Unity is being installed. The Toolkit UI will be available after installation.");
                }
            }
        }

        private static bool IsPackageInstalled(string packageName)
        {
            string manifestPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "manifest.json");
            if (System.IO.File.Exists(manifestPath))
            {
                string manifestText = System.IO.File.ReadAllText(manifestPath);
                return manifestText.Contains(packageName);
            }
            return false;
        }
    }
}