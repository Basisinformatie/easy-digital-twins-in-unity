using UnityEditor;
using Rotterdam.DigitalTwins.Editor.Setup;
using System;
using System.Reflection;

namespace Rotterdam.DigitalTwins.Editor.Setup
{
    public class ToolkitMenu
    {
        [MenuItem("Rotterdam Digital Twins/Launch UI")]
        public static void OpenShoppingWindow()
        {
            CesiumSetupService.EnsureCesiumIsInstalled();
            
            try
            {
                Assembly editorAssembly = Assembly.Load("com.rotterdam.digital-twins.Editor");
                Type shoppingWindowType = editorAssembly.GetType("Rotterdam.DigitalTwins.Editor.ShoppingWindow");
                if (shoppingWindowType != null)
                {
                    MethodInfo showWindowMethod = shoppingWindowType.GetMethod("ShowWindow", BindingFlags.Public | BindingFlags.Static);
                    if (showWindowMethod != null)
                    {
                        showWindowMethod.Invoke(null, null);
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("[ToolkitMenu] ShowWindow method not found in ShoppingWindow.");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[ToolkitMenu] ShoppingWindow type not found. Is the main Editor assembly compiled? (This is normal if Cesium is not yet installed)");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[ToolkitMenu] Could not open the main UI: {e.Message}. This can happen if Cesium is still being installed.");
            }
        }
    }
}