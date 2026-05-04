using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Rotterdam.DigitalTwins.Editor.Setup
{
    public static class CesiumPatchService
    {
        private const string ForkUrl = "https://github.com/360Fabriek/cesium-unity.git";
        private const string Branch = "main";
        private static readonly string LocalPackagePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "LocalPackages", "com.cesium.unity"));

        public static bool IsBuilding { get; private set; }
        public static string LastError { get; private set; }
        public static float Progress { get; private set; }

        public static async void BuildAndApplyPatch()
        {
            if (IsBuilding) return;
            IsBuilding = true;
            LastError = null;
            Progress = 0;

            try
            {
                await Task.Run(() => PerformBuild());
                Debug.Log("[CesiumPatchService] Build completed successfully.");
                
                // Refresh manifest to point to local package
                UpdateManifestToLocalPackage();
                
                EditorUtility.DisplayDialog("Cesium Patch", "Cesium has been successfully patched and built. Unity will now re-import the package.", "OK");
            }
            catch (Exception e)
            {
                LastError = e.Message;
                Debug.LogError($"[CesiumPatchService] Build failed: {e}");
                EditorUtility.DisplayDialog("Cesium Patch Failed", $"Build failed: {e.Message}", "OK");
            }
            finally
            {
                IsBuilding = false;
                Progress = 0;
            }
        }

        private static void PerformBuild()
        {
            // 1. Clone
            Progress = 0.1f;
            CloneRepository();

            // 2. Patch CMake for macOS
            Progress = 0.3f;
            PatchForMacOS();

            // 3. Build Reinterop
            Progress = 0.5f;
            BuildReinterop();

            // 4. Build Native
            Progress = 0.7f;
            BuildNative();
            
            Progress = 1.0f;
        }

        private static void CloneRepository()
        {
            if (Directory.Exists(LocalPackagePath))
            {
                Debug.Log("[CesiumPatchService] Local package already exists. Pulling latest...");
                RunCommand("git", "pull", LocalPackagePath);
            }
            else
            {
                string parentDir = Path.GetDirectoryName(LocalPackagePath);
                if (!Directory.Exists(parentDir)) Directory.CreateDirectory(parentDir);
                
                Debug.Log("[CesiumPatchService] Cloning repository...");
                RunCommand("git", $"clone --branch {Branch} --recurse-submodules {ForkUrl} {LocalPackagePath}", parentDir);
            }
        }

        private static void PatchForMacOS()
        {
#if UNITY_EDITOR_OSX
            string filePath = Path.Combine(LocalPackagePath, "Source", "Editor", "CompileCesiumForUnityNative.cs");
            if (File.Exists(filePath))
            {
                Debug.Log("[CesiumPatchService] Patching CompileCesiumForUnityNative.cs for macOS compatibility...");
                string content = File.ReadAllText(filePath);
                if (content.Contains("-G \"Visual Studio 17 2022\""))
                {
                    content = content.Replace("library.ExtraConfigureArgs.Add(\"-G \\\"Visual Studio 17 2022\\\"\");", "// library.ExtraConfigureArgs.Add(\"-G \\\"Visual Studio 17 2022\\\"\"); // Patched for macOS");
                    File.WriteAllText(filePath, content);
                }
            }
#endif
        }

        private static void BuildReinterop()
        {
            Debug.Log("[CesiumPatchService] Building Reinterop...");
            string reinteropDir = Path.Combine(LocalPackagePath, "Reinterop~");
            if (!Directory.Exists(reinteropDir))
            {
                throw new DirectoryNotFoundException($"Reinterop~ directory not found at {reinteropDir}");
            }
            
            RunCommand("dotnet", $"publish Reinterop~ -o .", LocalPackagePath);
        }

        private static void BuildNative()
        {
            Debug.Log("[CesiumPatchService] Building native libraries (this may take 10-20 minutes)...");
            string nativeDir = Path.Combine(LocalPackagePath, "native~");
            if (!Directory.Exists(nativeDir))
            {
                throw new DirectoryNotFoundException($"native~ directory not found at {nativeDir}");
            }

            string cmakePath = "cmake"; // Assuming it's in PATH as verified
            
            // Configure
            RunCommand(cmakePath, "-B build -S . -DCMAKE_BUILD_TYPE=Release", nativeDir);
            
            // Build
            int cpuCount = Environment.ProcessorCount;
            RunCommand(cmakePath, $"--build build -j {cpuCount} --target install --config Release", nativeDir);
        }

        private static void UpdateManifestToLocalPackage()
        {
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            string manifestText = File.ReadAllText(manifestPath);
            
            string relativePath = "file:../LocalPackages/com.cesium.unity";
            
            // Find existing entry and replace it
            string pattern = "\"com.cesium.unity\"\\s*:\\s*\"[^\"]*\"";
            string replacement = $"\"com.cesium.unity\": \"{relativePath}\"";
            
            if (System.Text.RegularExpressions.Regex.IsMatch(manifestText, pattern))
            {
                manifestText = System.Text.RegularExpressions.Regex.Replace(manifestText, pattern, replacement);
            }
            else
            {
                // Add to dependencies
                int index = manifestText.IndexOf("\"dependencies\"") + 14;
                index = manifestText.IndexOf("{", index) + 1;
                manifestText = manifestText.Insert(index, $"\n    {replacement},");
            }
            
            File.WriteAllText(manifestPath, manifestText);
            AssetDatabase.Refresh();
        }

        private static void RunCommand(string command, string args, string workingDir)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    throw new Exception($"Command '{command} {args}' failed with exit code {process.ExitCode}: {error}");
                }
            }
        }

        public static bool IsLocalPackageInstalled()
        {
            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (!File.Exists(manifestPath)) return false;
            string manifestText = File.ReadAllText(manifestPath);
            return manifestText.Contains("file:../LocalPackages/com.cesium.unity");
        }
    }
}
