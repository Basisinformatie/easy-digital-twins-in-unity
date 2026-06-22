using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Rotterdam.DigitalTwins.Runtime;
using Rotterdam.DigitalTwins.Editor.Utilities;

namespace Rotterdam.DigitalTwins.Editor
{
    public class OUPCatalogService : ICatalogService
    {
        private const string BaseUrl = "https://hub.clearly.app/api";

        public string Name => "Open Urban Platform (OUP)";

        public void FetchDatasets(Action<List<OUPDataset>> onSuccess, Action<string> onError, string searchTerm = "", string hubId = "", List<string> tags = null, List<string> formats = null)
        {
            List<string> queryParams = new List<string>();

            if (!string.IsNullOrEmpty(searchTerm))
                queryParams.Add($"search={UnityWebRequest.EscapeURL(searchTerm)}");

            if (!string.IsNullOrEmpty(hubId))
                queryParams.Add($"ownerHubId={UnityWebRequest.EscapeURL(hubId)}");

            if (tags != null && tags.Count > 0)
            {
                string tagsJson = "[\"" + string.Join("\",\"", tags) + "\"]";
                queryParams.Add($"tags={UnityWebRequest.EscapeURL(tagsJson)}");
            }

            if (formats != null && formats.Count > 0)
            {
                string formatsJson = "[\"" + string.Join("\",\"", formats) + "\"]";
                queryParams.Add($"formats={UnityWebRequest.EscapeURL(formatsJson)}");
            }

            queryParams.Add("withDefinedGeoExtent=true");
            queryParams.Add("findability=LIMITED");
            queryParams.Add("datasetHubStatus=%5B%22OWNED_BY_HUB%22%5D");

            string url = $"{BaseUrl}/datasets";
            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("accept", "application/json");
            request.timeout = 10;
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
                        if (json.Trim().StartsWith("["))
                        {
                            json = "{\"results\":" + json + "}";
                        }
                        OUPDatasetResponse response = JsonUtility.FromJson<OUPDatasetResponse>(json);
                        var results = response?.results ?? new List<OUPDataset>();
                        Debug.Log($"[OUP] Fetched {results.Count} datasets from {url}");
                        
                        results = OUPFilterUtility.FilterDatasets(results, searchTerm, hubId, formats);

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

        public void FetchDigitalTwins(Action<List<OUPDigitalTwin>> onSuccess, Action<string> onError, string searchTerm = "", string hubId = "", List<string> tags = null)
        {
            List<string> queryParams = new List<string>();

            if (!string.IsNullOrEmpty(searchTerm))
                queryParams.Add($"search={UnityWebRequest.EscapeURL(searchTerm)}");

            if (!string.IsNullOrEmpty(hubId))
                queryParams.Add($"hubId={UnityWebRequest.EscapeURL(hubId)}");

            string url = $"{BaseUrl}/digital-twins";
            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("accept", "application/json");
            request.timeout = 10;
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
                        if (json.Trim().StartsWith("["))
                        {
                            json = "{\"results\":" + json + "}";
                        }
                        OUPDigitalTwinResponse response = JsonUtility.FromJson<OUPDigitalTwinResponse>(json);
                        var results = response?.results ?? new List<OUPDigitalTwin>();
                        Debug.Log($"[OUP] Fetched {results.Count} digital twins.");

                        results = OUPFilterUtility.FilterDigitalTwins(results, searchTerm, tags);

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
            request.SetRequestHeader("accept", "application/json");
            request.timeout = 10;
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
                        if (json.Trim().StartsWith("["))
                        {
                            json = "{\"results\":" + json + "}";
                        }
                        
                        OUPHubResponse response = JsonUtility.FromJson<OUPHubResponse>(json);
                        var results = response?.results ?? new List<OUPHub>();
                        Debug.Log($"[OUP] Fetched {results.Count} hubs.");
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

        public void CheckStatus(Action<string, bool> onResult)
        {
            string url = $"{BaseUrl}/hubs";
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 5;
            var operation = request.SendWebRequest();
            operation.completed += _ =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    string status = request.responseCode > 0 ? $"{request.responseCode} Offline" : "Offline";
                    onResult?.Invoke(status, false);
                }
                else
                {
                    onResult?.Invoke($"{request.responseCode} OK", true);
                }
                request.Dispose();
            };
        }

    }
}