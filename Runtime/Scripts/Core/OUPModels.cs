using System;
using System.Collections.Generic;

namespace Rotterdam.DigitalTwins.Runtime
{
    [Serializable]
    public class OUPDataset
    {
        public string id;
        public string title;
        public string description;
        public string thumbnailUrl;
        public List<string> tags;
        public string ownerHubId;
        public List<OUPFormat> formats;
    }

    [Serializable]
    public class OUPFormat
    {
        public string format; 
        public string url;
    }

    [Serializable]
    public class OUPHub
    {
        public string id;
        public string title;
        public string description;
    }

    [Serializable]
    public class OUPDatasetResponse
    {
        public List<OUPDataset> items;
    }
}
