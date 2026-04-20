using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Rotterdam.DigitalTwins.Editor
{
    public class CesiumSetupService
    {
        private const string PackageName = "com.cesium.unity";
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

            Debug.Log($"[CesiumSetupService] {PackageName} installing...");
            AddScopedRegistry();
            InstallPackage(PackageName);
        }

        private static bool IsPackageInstalled(string packageName)
        {
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (File.Exists(manifestPath))
            {
                string manifestText = File.ReadAllText(manifestPath);
                return manifestText.Contains(packageName);
            }
            return false;
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