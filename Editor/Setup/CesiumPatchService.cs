using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Rotterdam.DigitalTwins.Editor.Setup
{
    public static class CesiumPatchService
    {
        private const string ForkUrl = "https://github.com/360Fabriek/cesium-unity.git";
        private static string BuildDir => Path.Combine(Directory.GetCurrentDirectory(), "Library", "CesiumPatchBuild");
        private static string ProjectRootDir => Path.Combine(BuildDir, "Project");
        private static string PackagesDir => Path.Combine(ProjectRootDir, "Packages");
        private static string RepoDir => Path.Combine(PackagesDir, "com.cesium.unity");

        public static async void StartPatchBuild()
        {
            if (!EditorUtility.DisplayDialog("Experimental: Build Patched Cesium",
                "This will clone, build, and install a patched version of Cesium from 360Fabriek. \n\n" +
                "Requirements: git, dotnet SDK, and cmake must be installed on your system. \n\n" +
                "WARNING: The build process involves compiling C++ code and can take 10-20 minutes. Your computer may become slow and Unity will remain interactive but should be left alone. \n\n" +
                "Proceed?", "Yes, Start Build", "Cancel"))
            {
                return;
            }

            try
            {
                await BuildAndInstallAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[CesiumPatchService] Patch failed: {e.Message}");
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Patch Failed", $"An error occurred during the build process: {e.Message} \n\nCheck the Console for details.", "OK");
            }
        }

        private static async Task BuildAndInstallAsync()
        {
            EditorUtility.DisplayProgressBar("Patching Cesium", "Initializing...", 0f);

            if (!CheckTools()) return;

            if (Directory.Exists(BuildDir))
            {
                EditorUtility.DisplayProgressBar("Patching Cesium", "Cleaning old build directory...", 0.05f);
                try { Directory.Delete(BuildDir, true); } catch { /* Ignore delete errors */ }
            }
            Directory.CreateDirectory(BuildDir);
            Directory.CreateDirectory(ProjectRootDir);
            Directory.CreateDirectory(PackagesDir);

            EditorUtility.DisplayProgressBar("Patching Cesium", "Cloning repository (this may take a while)...", 0.1f);
            if (!await RunProcessAsync("git", $"clone --recurse-submodules {ForkUrl} \"{RepoDir}\"", PackagesDir))
            {
                throw new Exception("Failed to clone repository.");
            }

#if UNITY_EDITOR_OSX
            EditorUtility.DisplayProgressBar("Patching Cesium", "Applying macOS compatibility fixes...", 0.3f);
            FixBuildScriptForMac();
#endif

            EditorUtility.DisplayProgressBar("Patching Cesium", "Publishing Reinterop...", 0.4f);
            if (!await RunProcessAsync("dotnet", "publish Reinterop~ -o .", RepoDir))
            {
                throw new Exception("Failed to publish Reinterop.");
            }

            string unityVersion = Application.unityVersion;
            string platform = GetBuildPlatform();
            EditorUtility.DisplayProgressBar("Patching Cesium", $"Running main build for {platform} (10-20 mins)...", 0.5f);
            

            if (!await RunProcessAsync("dotnet", $"run --project Build~ package --unity-version {unityVersion} --platform {platform}", RepoDir))
            {
                throw new Exception("Main build failed. Check Console for details.");
            }

            EditorUtility.DisplayProgressBar("Patching Cesium", "Installing package...", 0.9f);
            string tgzPath = FindPackageTgz();
            if (string.IsNullOrEmpty(tgzPath))
            {
                throw new Exception("Could not find generated .tgz package.");
            }

            Debug.Log($"[CesiumPatchService] Installing package from: {tgzPath}");
            Client.Add($"file:{tgzPath}");
            
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Patch Successful", "The patched Cesium version has been built and is now being installed. \n\nUnity will re-import the package, which may take several minutes.", "OK");
        }

        private static bool CheckTools()
        {
            bool gitOk = CanRun("git", "--version");
            bool dotnetOk = CanRun("dotnet", "--version");
            bool cmakeOk = CanRun("cmake", "--version");

            if (!gitOk) ShowToolError("git");
            if (!dotnetOk) ShowToolError("dotnet SDK");
            if (!cmakeOk) ShowToolError("cmake");

            return gitOk && dotnetOk && cmakeOk;
        }

        private static bool CanRun(string fileName, string args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(fileName, args)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p.WaitForExit();
                return p.ExitCode == 0;
            }
            catch { return false; }
        }

        private static void ShowToolError(string tool)
        {
            EditorUtility.DisplayDialog("Tool Missing", $"The required tool '{tool}' was not found in your PATH. Please install it to use the automated patcher.", "OK");
        }

        private static async Task<bool> RunProcessAsync(string fileName, string arguments, string workingDir)
        {
            Debug.Log($"[CesiumPatchService] Running: {fileName} {arguments} in {workingDir}");
            ProcessStartInfo psi = new ProcessStartInfo(fileName, arguments)
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };



            using var process = new Process { StartInfo = psi };
            process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Debug.Log($"[CesiumPatchBuild] {e.Data}"); };
            process.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Debug.LogError($"[CesiumPatchBuild] {e.Data}"); };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.HasExited)
                {
                    await Task.Delay(500);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CesiumPatchService] Process execution failed: {e.Message}");
                return false;
            }

            return process.ExitCode == 0;
        }

        private static void FixBuildScriptForMac()
        {
            string filePath = Path.Combine(RepoDir, "Source", "Editor", "CompileCesiumForUnityNative.cs");
            if (!File.Exists(filePath))
            {
                Debug.LogWarning("[CesiumPatchService] Could not find CompileCesiumForUnityNative.cs to apply macOS fix.");
                return;
            }

            string content = File.ReadAllText(filePath);
            string badLine = "library.ExtraConfigureArgs.Add(\"-G \\\"Visual Studio 17 2022\\\"\");";
            if (content.Contains(badLine))
            {
                content = content.Replace(badLine, "// " + badLine + " (Removed by CesiumPatchService for macOS compatibility)");
                File.WriteAllText(filePath, content);
                Debug.Log("[CesiumPatchService] Applied macOS compatibility fix to build script.");
            }
        }

        private static string GetBuildPlatform()
        {
#if UNITY_EDITOR_WIN
            return "Windows";
#elif UNITY_EDITOR_OSX
            return "macOS";
#elif UNITY_EDITOR_LINUX
            return "Linux";
#else
            return "macOS"; 
#endif
        }

        private static string FindPackageTgz()
        {
            if (Directory.Exists(ProjectRootDir))
            {
                string[] files = Directory.GetFiles(ProjectRootDir, "com.cesium.unity-*.tgz");
                if (files.Length > 0) return files[0];
            }
            
            if (Directory.Exists(RepoDir))
            {
                string[] files = Directory.GetFiles(RepoDir, "com.cesium.unity-*.tgz");
                if (files.Length > 0) return files[0];
            }

            return null;
        }
    }
}
