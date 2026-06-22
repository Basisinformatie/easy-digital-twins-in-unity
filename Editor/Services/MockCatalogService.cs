using System;
using System.Collections.Generic;
using Rotterdam.DigitalTwins.Runtime;
using UnityEngine;

namespace Rotterdam.DigitalTwins.Editor
{
    /// <summary>
    /// Mock implementation of the catalog service for demonstration purposes.
    /// </summary>
    public class MockCatalogService : ICatalogService
    {
        public string Name => "Mock Catalogus (Demo)";

        public void FetchDatasets(Action<List<OUPDataset>> onSuccess, Action<string> onError, string searchTerm = "", string hubId = "", List<string> tags = null, List<string> formats = null)
        {
            Debug.Log("[Mock] Fetching datasets...");
            var mockList = new List<OUPDataset>
            {
                new OUPDataset { _id = "mock1", title = "Mock Dataset 1", description = "Demo data" },
                new OUPDataset { _id = "mock2", title = "Mock Dataset 2", description = "Meer demo data" }
            };
            onSuccess?.Invoke(mockList);
        }

        public void FetchDigitalTwins(Action<List<OUPDigitalTwin>> onSuccess, Action<string> onError, string searchTerm = "", string hubId = "", List<string> tags = null)
        {
            Debug.Log("[Mock] Fetching digital twins...");
            onSuccess?.Invoke(new List<OUPDigitalTwin>());
        }

        public void FetchHubs(Action<List<OUPHub>> onSuccess, Action<string> onError)
        {
            Debug.Log("[Mock] Fetching hubs...");
            var mockHubs = new List<OUPHub>
            {
                new OUPHub { _id = "hub1", name = "Mock Hub Alpha" },
                new OUPHub { _id = "hub2", name = "Mock Hub Beta" }
            };
            onSuccess?.Invoke(mockHubs);
        }

        public void CheckStatus(Action<string, bool> onResult)
        {
            onResult?.Invoke("200 OK (Mock)", true);
        }
    }
}
