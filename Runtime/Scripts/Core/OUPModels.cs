using System;
using System.Collections.Generic;

namespace Rotterdam.DigitalTwins.Runtime
{
    /// <summary>
    /// Data models for the OUP API responses.
    /// </summary>
    [Serializable]
    public class OUPDataset
    {
        public string _id = default!;
        public string title = default!;
        public string description = default!;
        public string thumbnailUrl = default!;
        public List<string> tags = default!;
        public OUPHub ownerHub = default!;
        public List<OUPResource> resources = default!;
        public OUPGeoExtent geoExtent = default!;
    }

    [Serializable]
    public class OUPGeoExtent
    {
        public List<double> bbox = default!;
    }

    [Serializable]
    public class OUPResource
    {
        public string name = default!;
        public string format = default!; 
        public string url = default!;
    }

    [Serializable]
    public class OUPDigitalTwin
    {
        public string _id = default!;
        public string title = default!;
        public string description = default!;
        public string previewImage = default!;
        public OUPHub ownerHub = default!;
        public List<string> tags = default!;
        public List<OUPConfiguration> configuration = default!;
        public OUPViewpoint viewpoint = default!;
    }

    [Serializable]
    public class OUPConfiguration
    {
        public string datasetId = default!;
        public string datasetTitle = default!;
        public List<OUPResource> resources = default!;
    }

    [Serializable]
    public class OUPViewpoint
    {
        public List<double> groundPosition = default!;
    }

    [Serializable]
    public class OUPHub
    {
        public string _id = default!;
        public string name = default!;
        public string description = default!;
    }

    [Serializable]
    public class OUPDatasetResponse
    {
        public List<OUPDataset> results = default!;
    }

    [Serializable]
    public class OUPDigitalTwinResponse
    {
        public List<OUPDigitalTwin> results = default!;
    }

    [Serializable]
    public class OUPHubResponse
    {
        public List<OUPHub> results = default!;
    }
}
