using System;
using System.Collections.Generic;

namespace Rotterdam.DigitalTwins.Editor.Utilities
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

    public static class ProjectTypeStrings
    {
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
