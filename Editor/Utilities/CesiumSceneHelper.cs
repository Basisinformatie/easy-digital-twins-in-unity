#if USING_CESIUM
using CesiumForUnity;
#endif
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Rotterdam.DigitalTwins.Editor
{
    public static class CesiumSceneHelper
    {
        private struct WmsRequest
        {
            public string Name;
            public string Url;
            public string Layers;
            public int MaximumLevel;
        }

        private static Queue<WmsRequest> _wmsQueue = new Queue<WmsRequest>();

        public static void CreateBlank3DTileset()
        {
#if USING_CESIUM
            Create3DTilesetFromUrl("Cesium3DTileset", "");
#else
            Debug.LogWarning("Cesium is not installed. Cannot create 3D Tileset.");
#endif
        }

        public static void RemoveGeoreference()
        {
#if USING_CESIUM
            CesiumGeoreference georeference = Object.FindAnyObjectByType<CesiumGeoreference>();
            if (georeference != null)
            {
                Undo.DestroyObjectImmediate(georeference.gameObject);
                Debug.Log("Removed CesiumGeoreference from the scene.");
            }
            else
            {
                Debug.LogWarning("No CesiumGeoreference found in the scene.");
            }
#else
            Debug.LogWarning("Cesium is not installed. Cannot remove Georeference.");
#endif
        }

        public static GameObject Create3DTilesetFromUrl(string name, string url, bool isPointCloud = false, bool isTerrain = false)
        {
#if USING_CESIUM
            CesiumGeoreference georeference = Object.FindAnyObjectByType<CesiumGeoreference>();
            if (georeference == null)
            {
                GameObject georefGo = new GameObject("CesiumGeoreference");
                georeference = georefGo.AddComponent<CesiumGeoreference>();
                Undo.RegisterCreatedObjectUndo(georefGo, "Create CesiumGeoreference");
            }

            GameObject tilesetGo = new GameObject(name);
            tilesetGo.transform.SetParent(georeference.transform);
            Cesium3DTileset tileset = tilesetGo.AddComponent<Cesium3DTileset>();

            if (!string.IsNullOrEmpty(url))
            {
                tileset.tilesetSource = CesiumDataSource.FromUrl;
                tileset.url = url;
            }

            if (isPointCloud)
            {
                tileset.pointCloudShading.attenuation = true;
            }

            if (isTerrain)
            {
                if (!name.StartsWith("Terrain", System.StringComparison.OrdinalIgnoreCase))
                {
                    name = "Terrain - " + name;
                }
                tilesetGo.name = name;
                tilesetGo.AddComponent<Rotterdam.DigitalTwins.Runtime.CesiumTerrainTag>();
            }
            
            Undo.RegisterCreatedObjectUndo(tilesetGo, $"Create {name}");
            Selection.activeGameObject = tilesetGo;
            
            Debug.Log($"Created {name} under CesiumGeoreference.");
            return tilesetGo;
#else
            Debug.LogWarning("Cesium is not installed. Cannot create 3D Tileset.");
            return null;
#endif
        }

        public static void SetGeoreferenceToRotterdam()
        {
            SetGeoreference(51.90759, 4.490608, 6.1);
        }

        public static void SetGeoreference(double lat, double lon, double height)
        {
#if USING_CESIUM
            CesiumGeoreference georeference = Object.FindAnyObjectByType<CesiumGeoreference>();
            if (georeference == null)
            {
                GameObject georefGo = new GameObject("CesiumGeoreference");
                georeference = georefGo.AddComponent<CesiumGeoreference>();
                Undo.RegisterCreatedObjectUndo(georefGo, "Create CesiumGeoreference");
            }

            Undo.RecordObject(georeference, "Set Georeference");
            georeference.latitude = lat;
            georeference.longitude = lon;
            georeference.height = height;

            Debug.Log($"CesiumGeoreference set to ({lat}, {lon}, {height}).");
#else
            Debug.LogWarning("Cesium is not installed. Cannot set Georeference.");
#endif
        }

        public static (double lat, double lon, double height) GetGeoreference()
        {
#if USING_CESIUM
            CesiumGeoreference georeference = Object.FindAnyObjectByType<CesiumGeoreference>();
            if (georeference != null)
            {
                return (georeference.latitude, georeference.longitude, georeference.height);
            }
#endif
            return (0, 0, 0);
        }

        public static void CreateMultiple3DTilesets(string baseName, List<Rotterdam.DigitalTwins.Runtime.OUPResource> resources)
        {
            if (resources == null || resources.Count == 0) return;

            var allowedFormats = new[] { "3dtileset", "3dtile", "3dtiles", "3dterrain", "3d tiles", "3d-tiles", "3dpointclouds", "WMS", "wms" };
            var matchingResources = resources
                .Where(r => allowedFormats.Any(fmt => string.Equals(fmt, r.format, System.StringComparison.OrdinalIgnoreCase)))
                .ToList();

            List<GameObject> createdTerrains = new();

            foreach (var res in matchingResources.Where(r => !string.Equals(r.format, "WMS", System.StringComparison.OrdinalIgnoreCase)))
            {
                string displayName = string.IsNullOrEmpty(res.name) ? res.format.ToUpper() : res.name;
                string tilesetName = $"{baseName} ({displayName})";
                bool isPointCloud = string.Equals(res.format, "3dpointclouds", System.StringComparison.OrdinalIgnoreCase);
                bool isTerrain = string.Equals(res.format, "3dterrain", System.StringComparison.OrdinalIgnoreCase);
                
                GameObject go = Create3DTilesetFromUrl(tilesetName, res.url, isPointCloud, isTerrain);
                if (isTerrain && go != null)
                {
                    createdTerrains.Add(go);
                }
            }

            var wmsResources = matchingResources.Where(r => string.Equals(r.format, "WMS", System.StringComparison.OrdinalIgnoreCase)).ToList();
            if (wmsResources.Count > 0)
            {
                foreach (var res in wmsResources)
                {
                    string displayName = string.IsNullOrEmpty(res.name) ? res.format.ToUpper() : res.name;
                    string tilesetName = $"{baseName} ({displayName})";
                    AddWmsOverlay(tilesetName, res.url);
                }
            }
        }

        public static void AddWmsOverlay(string name, string url, string layers = "0", int maximumLevel = 22)
        {
            _wmsQueue.Enqueue(new WmsRequest { Name = name, Url = url, Layers = layers, MaximumLevel = maximumLevel });

            if (_wmsQueue.Count == 1)
            {
                ProcessWmsQueue();
            }
        }

        private static void ProcessWmsQueue()
        {
            if (_wmsQueue.Count == 0) return;

#if USING_CESIUM
            var request = _wmsQueue.Peek();
            var tilesets = Object.FindObjectsByType<Cesium3DTileset>(FindObjectsSortMode.None);
            
            if (tilesets.Length == 0)
            {
                EditorUtility.DisplayDialog("No 3D Tileset Found", "Could not find a 3D Tileset in the scene. Please add a 3D tileset first.", "OK");
                _wmsQueue.Clear();
                return;
            }

            GenericMenu menu = new GenericMenu();
            string queueInfo = _wmsQueue.Count > 1 ? $" ({_wmsQueue.Count} pending)" : "";
            menu.AddDisabledItem(new GUIContent($"Select 3D Tileset for WMS{queueInfo}: {request.Name}"));
            menu.AddSeparator("");
            foreach (var tileset in tilesets.OrderBy(t => t.gameObject.name))
            {
                GameObject target = tileset.gameObject;
                menu.AddItem(new GUIContent(target.name), false, () => 
                {
                    AddWmsToGameObject(target, request.Url, request.Layers, request.MaximumLevel);
                    _wmsQueue.Dequeue();
                    ProcessWmsQueue();
                });
            }
            
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Skip this WMS"), false, () => 
            {
                _wmsQueue.Dequeue();
                ProcessWmsQueue();
            });
            menu.AddItem(new GUIContent("Cancel all remaining WMS"), false, () => 
            {
                _wmsQueue.Clear();
            });
            
            menu.ShowAsContext();
#else
            Debug.LogWarning("Cesium is not installed. Cannot add WMS overlay.");
            _wmsQueue.Clear();
#endif
        }

#if USING_CESIUM
        private static void AddWmsToGameObject(GameObject target, string url, string layers = "0", int maximumLevel = 22)
        {
            string baseUrl = url;
            string parsedLayers = "";

            try
            {
                if (url.Contains("?"))
                {
                    if (System.Uri.TryCreate(url, System.UriKind.Absolute, out var uri))
                    {
                        baseUrl = uri.GetLeftPart(System.UriPartial.Path);
                        var query = uri.Query.TrimStart('?');
                        var queryParams = query.Split('&');
                        foreach (var param in queryParams)
                        {
                            if (param.StartsWith("layers=", System.StringComparison.OrdinalIgnoreCase))
                            {
                                parsedLayers = System.Uri.UnescapeDataString(param.Substring(7));
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to parse WMS URL: {url}. Error: {e.Message}");
            }

            string finalLayers = (layers != "0") ? layers : (string.IsNullOrEmpty(parsedLayers) ? "0" : parsedLayers);

            CesiumWebMapServiceRasterOverlay wms = target.AddComponent<CesiumWebMapServiceRasterOverlay>();
            wms.baseUrl = baseUrl;
            wms.layers = finalLayers;
            wms.maximumLevel = maximumLevel;

            Undo.RegisterCreatedObjectUndo(wms, "Add WMS Overlay");
            Selection.activeGameObject = target;
            Debug.Log($"Added WMS Overlay (layers: {finalLayers}, maxLevel: {maximumLevel}) to {target.name}");
        }
#endif
    }
}