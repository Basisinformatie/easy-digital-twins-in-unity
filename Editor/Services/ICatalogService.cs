using System;
using System.Collections.Generic;
using Rotterdam.DigitalTwins.Runtime;

namespace Rotterdam.DigitalTwins.Editor
{
    public interface ICatalogService
    {
        void FetchDatasets(Action<List<OUPDataset>> onSuccess, Action<string> onError, string searchTerm = "", string hubId = "", List<string> tags = null, List<string> formats = null);
        void FetchHubs(Action<List<OUPHub>> onSuccess, Action<string> onError);
    }
}