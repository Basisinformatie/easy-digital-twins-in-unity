using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rotterdam.DigitalTwins.Editor.Utilities
{
    /// <summary>
    /// Utility for detecting the Unity project type and target platforms based on installed packages.
    /// </summary>
    public static class ProjectTypeDetector
    {
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

            bool isVR = packageIds.Any(id => id.Contains("com.unity.xr.meta-openxr") || 
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
            return ProjectTypeStrings.GetProjectTypes(type);
        }

        public static string GetCompatibility(ProjectType type)
        {
            return ProjectTypeStrings.GetCompatibility(type);
        }
    }
}
