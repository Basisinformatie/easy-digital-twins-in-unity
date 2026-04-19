using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Rotterdam.DigitalTwins.Editor
{
    public class CesiumSetupService
    {
        private const string PackageName = "com.cesium.unity";
        private const string RegistryName = "Cesium";
        private const string RegistryUrl = "https://unity.pkg.cesium.com";
        private static readonly string[] Scopes = { "com.cesium.unity" };

        public static void CheckAndInstallCesium(Action onComplete)
        {
            ListRequest request = Client.List(true);
            
            void Progress()
            {
                if (request.IsCompleted)
                {
                    EditorApplication.update -= Progress;
                    if (request.Status == StatusCode.Success)
                    {
                        var isInstalled = request.Result.Any(p => p.name == PackageName);
                        if (!isInstalled)
                        {
                            InstallCesium(onComplete);
                        }
                        else
                        {
                            onComplete?.Invoke();
                        }
                    }
                    else
                    {
                        Debug.LogError("Check for Cesium installation failed.");
                    }
                }
            }

            EditorApplication.update += Progress;
        }

        private static void InstallCesium(Action onComplete)
        {
            Debug.Log("Cesium for Unity is not installed. Starting installation...");
            AddScopedRegistry();
            AddRequest addRequest = Client.Add(PackageName);

            void Progress()
            {
                if (addRequest.IsCompleted)
                {
                    EditorApplication.update -= Progress;
                    if (addRequest.Status == StatusCode.Success)
                    {
                        Debug.Log("Cesium for Unity successfully installed.");
                        onComplete?.Invoke();
                    }
                    else
                    {
                        Debug.LogError($"Cesium installation failed: {addRequest.Error.message}");
                    }
                }
            }
            EditorApplication.update += Progress;
        }

        private static void AddScopedRegistry()
        {
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (!File.Exists(manifestPath)) return;

            string content = File.ReadAllText(manifestPath);
            
            if (content.Contains(RegistryUrl)) return;

            if (content.Contains("\"scopedRegistries\""))
            {
                int index = content.IndexOf("\"scopedRegistries\"", StringComparison.Ordinal);
                int openBracketIndex = content.IndexOf('[', index);
                string newRegistry = $"\n    {{\n      \"name\": \"{RegistryName}\",\n      \"url\": \"{RegistryUrl}\",\n      \"scopes\": [\n        \"{Scopes[0]}\"\n      ]\n    }},";
                content = content.Insert(openBracketIndex + 1, newRegistry);
            }
            else
            {
                int lastBraceIndex = content.LastIndexOf('}');
                string newSection = $",\n  \"scopedRegistries\": [\n    {{\n      \"name\": \"{RegistryName}\",\n      \"url\": \"{RegistryUrl}\",\n      \"scopes\": [\n        \"{Scopes[0]}\"\n      ]\n    }}\n  ]";
                content = content.Insert(lastBraceIndex, newSection);
            }

            File.WriteAllText(manifestPath, content);
            AssetDatabase.Refresh();
        }
    }
}