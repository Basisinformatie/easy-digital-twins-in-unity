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
            iOS = 1 << 7,
            Universal3D = 1 << 8,
            AndroidXR = 1 << 9,
            MetaQuest = 1 << 10
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
                                            id.Contains("com.unity.xr.compositionlayers") ||
                                            id.Contains("com.unity.xr.androidxr-openxr"));

            bool isAR = packageIds.Any(id => id.Contains("com.unity.xr.arfoundation") || 
                                            id.Contains("com.unity.xr.arkit") || 
                                            id.Contains("com.unity.xr.arcore"));

            if (isVR) detectedTypes |= ProjectType.VR;
            if (isAR) detectedTypes |= ProjectType.AR;

            if (!isVR && !isAR)
                detectedTypes |= ProjectType.Universal3D;

            if (IsSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                detectedTypes |= ProjectType.Android;
                if (packageIds.Any(id => id.Contains("com.unity.xr.androidxr-openxr")))
                    detectedTypes |= ProjectType.AndroidXR;
                if (packageIds.Any(id => id.Contains("com.unity.xr.meta-openxr")))
                    detectedTypes |= ProjectType.MetaQuest;
            }

            if (IsSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                detectedTypes |= ProjectType.WebApp;

            if (IsSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows) || 
                IsSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                detectedTypes |= ProjectType.Windows;

            if (IsSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX))
                detectedTypes |= ProjectType.Mac;

            if (IsSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64))
                detectedTypes |= ProjectType.Linux;
            
            if (IsSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
                detectedTypes |= ProjectType.iOS;

            return detectedTypes;
        }

        private static bool IsSupported(BuildTargetGroup group, BuildTarget target)
        {
            return BuildPipeline.IsBuildTargetSupported(group, target);
        }

        public static string GetProjectTypes(ProjectType type)
        {
            List<string> results = new List<string>();
            if (type.HasFlag(ProjectType.Universal3D)) results.Add("Universal 3D");
            if (type.HasFlag(ProjectType.VR)) results.Add("VR");
            if (type.HasFlag(ProjectType.AR)) results.Add("AR");

            return results.Count > 0 ? string.Join(", ", results) : "Unknown";
        }

        public static string GetCompatibility(ProjectType type)
        {
            List<string> results = new List<string>();
            if (type.HasFlag(ProjectType.Android)) results.Add("Android");
            if (type.HasFlag(ProjectType.AndroidXR)) results.Add("Android XR");
            if (type.HasFlag(ProjectType.MetaQuest)) results.Add("Meta Quest");
            if (type.HasFlag(ProjectType.Windows)) results.Add("Windows");
            if (type.HasFlag(ProjectType.Mac)) results.Add("MAC");
            if (type.HasFlag(ProjectType.Linux)) results.Add("Linux");
            if (type.HasFlag(ProjectType.WebApp)) results.Add("Web app");
            if (type.HasFlag(ProjectType.iOS)) results.Add("iOS");

            return results.Count > 0 ? string.Join(", ", results) : "None";
        }
    }
}
