using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Rotterdam.DigitalTwins.Runtime;

namespace Rotterdam.DigitalTwins.Editor
{
    public class OUPCatalogService : ICatalogService
    {
        private const string BaseUrl = "https://hub.clearly.app/api";

        public void FetchDatasets(Action<List<OUPDataset>> onSuccess, Action<string> onError, string searchTerm = "", string hubId = "", List<string> tags = null, List<string> formats = null)
        {
            string url = $"{BaseUrl}/datasets?findability=PUBLIC"; 

            if (!string.IsNullOrEmpty(hubId))
                url += $"&hubId={hubId}";
            
            
            UnityWebRequest request = UnityWebRequest.Get(url);
            var operation = request.SendWebRequest();

            operation.completed += _ =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                }
                else
                {
                    try
                    {
                        string json = request.downloadHandler.text;
                        if (json.StartsWith("["))
                        {
                            json = "{\"items\":" + json + "}";
                        }
                        OUPDatasetResponse response = JsonUtility.FromJson<OUPDatasetResponse>(json);
                        
                        var results = response.items;
                        
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            results = results.Where(d => 
                                d.title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                                (d.description != null && d.description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            ).ToList();
                        }

                        if (formats != null && formats.Count > 0)
                        {
                            results = results.Where(d => 
                                d.formats != null && d.formats.Any(f => formats.Any(fmt => string.Equals(fmt, f.format, StringComparison.OrdinalIgnoreCase)))
                            ).ToList();
                        }

                        onSuccess?.Invoke(results);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke($"JSON Parsing error: {ex.Message}");
                    }
                }
                request.Dispose();
            };
        }

        public void FetchHubs(Action<List<OUPHub>> onSuccess, Action<string> onError)
        {
            string url = $"{BaseUrl}/hubs";
            UnityWebRequest request = UnityWebRequest.Get(url);
            var operation = request.SendWebRequest();

            operation.completed += _ =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(request.error);
                }
                else
                {
                    try
                    {
                        string json = request.downloadHandler.text;
                        if (json.StartsWith("["))
                        {
                            json = "{\"items\":" + json + "}";
                        }
                        
                        HubResponse response = JsonUtility.FromJson<HubResponse>(json);
                        onSuccess?.Invoke(response.items);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke($"JSON Parsing error: {ex.Message}");
                    }
                }
                request.Dispose();
            };
        }

        [Serializable]
        private class HubResponse
        {
            public List<OUPHub> items;
        }
    }
}