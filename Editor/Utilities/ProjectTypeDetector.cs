using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rotterdam.DigitalTwins.Editor.Utilities
{
    public static class ProjectTypeDetector
    {
        [Flags]
        public enum ProjectType
        {
            None = 0,
            VR = 1 << 0,
            AR = 1 << 1,
            Android = 1 << 2,
            WebApp = 1 << 3,
            Windows = 1 << 4,
            Mac = 1 << 5,
            Linux = 1 << 6,
            Universal3D = 1 << 7
        }

        public static void GetProjectType(Action<ProjectType> callback)
        {
            ListRequest request = Client.List(true);
            EditorApplication.update += Progress;

            void Progress()
            {
                if (request.IsCompleted)
                {
                    EditorApplication.update -= Progress;
                    if (request.Status == StatusCode.Success)
                    {
                        callback(Evaluate(request.Result));
                    }
                    else
                    {
                        callback(ProjectType.None);
                    }
                }
            }
        }

        private static ProjectType Evaluate(PackageCollection packages)
        {
            ProjectType detectedTypes = ProjectType.None;
            var packageIds = packages.Select(p => p.name).ToList();

            bool isVR = packageIds.Any(id => id.Contains("com.unity.xr.openxr") || 
                                            id.Contains("com.unity.xr.meta-openxr") || 
                                            id.Contains("com.unity.xr.hands") ||
                                            id.Contains("com.unity.xr.interaction.toolkit") ||
                                            id.Contains("com.unity.xr.compositionlayers") ||
                                            id.Contains("com.unity.xr.androidxr-openxr"));

            bool isAR = packageIds.Any(id => id.Contains("com.unity.xr.arfoundation") || 
                                            id.Contains("com.unity.xr.arkit") || 
                                            id.Contains("com.unity.xr.arcore"));

            if (isVR) detectedTypes |= ProjectType.VR;
            if (isAR) detectedTypes |= ProjectType.AR;

            BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;

            if (activeTarget == BuildTarget.Android || packageIds.Contains("com.unity.modules.androidjni"))
                detectedTypes |= ProjectType.Android;

            if (activeTarget == BuildTarget.WebGL)
                detectedTypes |= ProjectType.WebApp;

            if (activeTarget == BuildTarget.StandaloneWindows || 
                activeTarget == BuildTarget.StandaloneWindows64)
                detectedTypes |= ProjectType.Windows;

            if (activeTarget == BuildTarget.StandaloneOSX)
                detectedTypes |= ProjectType.Mac;

            if (activeTarget == BuildTarget.StandaloneLinux64)
                detectedTypes |= ProjectType.Linux;

            if (!isVR && !isAR)
                detectedTypes |= ProjectType.Universal3D;

            return detectedTypes;
        }

        public static string FormatProjectType(ProjectType type)
        {
            if (type == ProjectType.None) return "Unknown";
            
            List<string> results = new List<string>();
            if (type.HasFlag(ProjectType.Universal3D)) results.Add("Universal 3D");
            if (type.HasFlag(ProjectType.VR)) results.Add("VR");
            if (type.HasFlag(ProjectType.AR)) results.Add("AR");
            if (type.HasFlag(ProjectType.Android)) results.Add("Android");
            if (type.HasFlag(ProjectType.WebApp)) results.Add("Web app");
            if (type.HasFlag(ProjectType.Windows)) results.Add("Windows");
            if (type.HasFlag(ProjectType.Mac)) results.Add("Mac");
            if (type.HasFlag(ProjectType.Linux)) results.Add("Linux");

            return results.Count > 0 ? string.Join(", ", results) : "Unknown";
        }
    }
}
