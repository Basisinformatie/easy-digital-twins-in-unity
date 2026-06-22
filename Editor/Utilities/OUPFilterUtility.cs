using System;
using System.Collections.Generic;
using System.Linq;
using Rotterdam.DigitalTwins.Runtime;

namespace Rotterdam.DigitalTwins.Editor.Utilities
{
    public static class OUPFilterUtility
    {
        public static List<OUPDataset> FilterDatasets(List<OUPDataset> datasets, string searchTerm, string hubId, List<string> formats)
        {
            var results = datasets;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                results = results.Where(d => 
                    (d.title != null && d.title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) || 
                    (d.description != null && d.description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (d.tags != null && d.tags.Any(t => t.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(hubId))
            {
                results = results.Where(d => d.ownerHub != null && d.ownerHub._id == hubId).ToList();
            }

            if (formats != null && formats.Count > 0)
            {
                results = results.Where(d => 
                    d.resources != null && d.resources.Any(f => formats.Any(fmt => string.Equals(fmt, f.format, StringComparison.OrdinalIgnoreCase)))
                ).ToList();
            }

            return results;
        }

        public static List<OUPDigitalTwin> FilterDigitalTwins(List<OUPDigitalTwin> digitalTwins, string searchTerm, List<string> tags)
        {
            var results = digitalTwins;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                results = results.Where(dt => 
                    (dt.title != null && dt.title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) || 
                    (dt.description != null && dt.description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (dt.tags != null && dt.tags.Any(t => t.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                ).ToList();
            }

            if (tags != null && tags.Count > 0)
            {
                 results = results.Where(dt => dt.tags != null && tags.All(t => dt.tags.Contains(t))).ToList();
            }

            return results;
        }
    }
}
