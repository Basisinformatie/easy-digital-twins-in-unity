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
        public OUPHub ownerHub;
        public List<OUPResource> resources;
        public OUPGeoExtent geoExtent;
    }

    [Serializable]
    public class OUPGeoExtent
    {
        public List<double> bbox;
    }

    [Serializable]
    public class OUPResource
    {
        public string name;
        public string format; 
        public string url;
    }

    [Serializable]
    public class OUPDigitalTwin
    {
        public string _id;
        public string title;
        public string description;
        public string previewImage;
        public OUPHub ownerHub;
        public List<string> tags;
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

    [Serializable]
    public class OUPDigitalTwinResponse
    {
        public List<OUPDigitalTwin> results;
    }
}
