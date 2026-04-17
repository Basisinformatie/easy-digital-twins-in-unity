using System;
using System.Collections.Generic;

namespace Rotterdam.DigitalTwins.Runtime
{
    [Serializable]
    public class OUPDataset
    {
        public string _id;
        public string title;
        public string description;
        public string thumbnailUrl;
        public List<string> tags;
        public string ownerHubId;
        public List<OUPResource> resources;
    }

    [Serializable]
    public class OUPResource
    {
        public string format; 
        public string url;
    }

    [Serializable]
    public class OUPHub
    {
        public string _id;
        public string name;
        public string description;
    }

    [Serializable]
    public class OUPDatasetResponse
    {
        public List<OUPDataset> results;
    }
}
