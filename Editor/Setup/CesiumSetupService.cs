using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using System.IO;

namespace Rotterdam.DigitalTwins.Editor.Setup
{
    public class CesiumSetupService
    {
        public const string PackageName = "com.cesium.unity";
        private const string RegistryName = "Cesium";
        private const string RegistryUrl = "https://unity.pkg.cesium.com";
        private const string RegistryScope = "com.cesium.unity";

        public static void EnsureCesiumIsInstalled()
        {
            if (IsPackageInstalled(PackageName))
            {
                Debug.Log($"[CesiumSetupService] {PackageName} is already installed.");
                return;
            }

            Debug.Log($"[CesiumSetupService] Waiting for {PackageName} installation...");
            AddScopedRegistry();
            InstallPackage(PackageName);
        }

        public static bool IsPackageInstalled(string packageName)
        {
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (!File.Exists(manifestPath)) return false;

            string manifestText = File.ReadAllText(manifestPath);
            return System.Text.RegularExpressions.Regex.IsMatch(manifestText, $"\"{packageName}\"\\s*:");
        }

        public static bool IsPackageFolderPresent()
        {
            return AssetDatabase.IsValidFolder($"Packages/{PackageName}");
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