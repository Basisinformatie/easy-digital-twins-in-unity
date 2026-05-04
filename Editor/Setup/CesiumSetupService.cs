using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using System.IO;

namespace Rotterdam.DigitalTwins.Editor.Setup
{
    public class CesiumSetupService
    {
        private const string PackageName = "com.cesium.unity";
        private const string RegistryName = "Cesium";
        private const string RegistryUrl = "https://unity.pkg.cesium.com";
        private const string RegistryScope = "com.cesium.unity";
        private const string ForkUrl = "https://github.com/360Fabriek/cesium-unity.git";

        public static void EnsureCesiumIsInstalled()
        {
            if (IsPackageInstalled(PackageName))
            {
                return;
            }

            InstallOfficialCesium();
        }

        public static void InstallOfficialCesium()
        {
            Debug.Log($"[CesiumSetupService] Installing official {PackageName}...");
            
            // Remove local package if it exists in manifest
            RemoveLocalPackage();
            
            AddScopedRegistry();
            InstallPackage(PackageName);
        }

        private static void RemoveLocalPackage()
        {
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (!File.Exists(manifestPath)) return;
            string manifestText = File.ReadAllText(manifestPath);
            
            string pattern = "\"com.cesium.unity\"\\s*:\\s*\"file:[^\"]*\"";
            if (System.Text.RegularExpressions.Regex.IsMatch(manifestText, pattern))
            {
                manifestText = System.Text.RegularExpressions.Regex.Replace(manifestText, pattern, "\"com.cesium.unity\": \"1.23.0\""); // Default back to a version
                File.WriteAllText(manifestPath, manifestText);
            }
        }

        public static bool IsForkInstalled()
        {
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (!File.Exists(manifestPath)) return false;

            string manifestText = File.ReadAllText(manifestPath);
            return manifestText.Contains(ForkUrl);
        }

        private static bool IsPackageInstalled(string packageName)
        {
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (!File.Exists(manifestPath)) return false;

            string manifestText = File.ReadAllText(manifestPath);
            return System.Text.RegularExpressions.Regex.IsMatch(manifestText, $"\"{packageName}\"\\s*:");
        }

        private static void AddScopedRegistry()
        {
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (!File.Exists(manifestPath)) return;

            string manifestText = File.ReadAllText(manifestPath);
            
            if (manifestText.Contains(RegistryUrl))
            {
                return;
            }
            
            string registryJson = $@"
    {{
      ""name"": ""{RegistryName}"",
      ""url"": ""{RegistryUrl}"",
      ""scopes"": [
        ""{RegistryScope}""
      ]
    }}";

            var scopedRegistriesMatch = System.Text.RegularExpressions.Regex.Match(manifestText, "\"scopedRegistries\"\\s*:\\s*\\[");

            if (scopedRegistriesMatch.Success)
            {
                int index = scopedRegistriesMatch.Index + scopedRegistriesMatch.Length;
                manifestText = manifestText.Insert(index, registryJson + ",");
            }
            else
            {
                int index = manifestText.IndexOf("{");
                if (index >= 0)
                {
                    manifestText = manifestText.Insert(index + 1, "\n  \"scopedRegistries\": [" + registryJson + "\n  ],");
                }
            }

            File.WriteAllText(manifestPath, manifestText);
            AssetDatabase.Refresh();
        }

        private static void InstallPackage(string packageName)
        {
            Client.Add(packageName);
            Debug.Log($"[CesiumSetupService] {PackageName} installation finishing...");

        }
    }
}