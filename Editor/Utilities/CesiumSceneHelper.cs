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
        public static void CreateBlank3DTileset()
        {
#if USING_CESIUM
            Create3DTilesetFromUrl("Cesium3DTileset", "");
#else
            Debug.LogWarning("Cesium is not installed. Cannot create 3D Tileset.");
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
#if USING_CESIUM
                if (createdTerrains.Count == 1)
                {
                    // If we just created exactly one terrain for this digital twin, add all WMS to it automatically
                    foreach (var res in wmsResources)
                    {
                        AddWmsToGameObject(createdTerrains[0], res.url);
                    }
                    return;
                }
#endif
                
                // Fallback: search scene for terrain (or ask user if multiple)
                foreach (var res in wmsResources)
                {
                    string displayName = string.IsNullOrEmpty(res.name) ? res.format.ToUpper() : res.name;
                    string tilesetName = $"{baseName} ({displayName})";
                    AddWmsOverlay(tilesetName, res.url);
                }
            }
        }

        public static void AddWmsOverlay(string name, string url)
        {
#if USING_CESIUM
            var terrainTags = Object.FindObjectsByType<Rotterdam.DigitalTwins.Runtime.CesiumTerrainTag>(FindObjectsSortMode.None);
            
            if (terrainTags.Length == 0)
            {
                EditorUtility.DisplayDialog("No Terrain Found", "Could not find a 3D Terrain tileset in the scene. Please add a terrain tileset first.", "OK");
                return;
            }

            if (terrainTags.Length == 1)
            {
                AddWmsToGameObject(terrainTags[0].gameObject, url);
            }
            else
            {
                GenericMenu menu = new GenericMenu();
                menu.AddDisabledItem(new GUIContent($"Select terrain for WMS: {name}"));
                menu.AddSeparator("");
                foreach (var tag in terrainTags.OrderBy(t => t.gameObject.name))
                {
                    GameObject target = tag.gameObject;
                    menu.AddItem(new GUIContent(target.name), false, () => AddWmsToGameObject(target, url));
                }
                menu.ShowAsContext();
            }
#else
            Debug.LogWarning("Cesium is not installed. Cannot add WMS overlay.");
#endif
        }

#if USING_CESIUM
        private static void AddWmsToGameObject(GameObject target, string url)
        {
            string baseUrl = url;
            string layers = "";

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
                                layers = System.Uri.UnescapeDataString(param.Substring(7));
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to parse WMS URL: {url}. Error: {e.Message}");
            }

            CesiumWebMapServiceRasterOverlay wms = target.AddComponent<CesiumWebMapServiceRasterOverlay>();
            wms.baseUrl = baseUrl;
            wms.layers = 0;
            wms.maximumLevel = 22;

            Undo.RegisterCreatedObjectUndo(wms, "Add WMS Overlay");
            Selection.activeGameObject = target;
            Debug.Log($"Added WMS Overlay (layers: {layers}) to {target.name}");
        }
#endif
    }
}