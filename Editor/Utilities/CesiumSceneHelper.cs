using CesiumForUnity;
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
            Create3DTilesetFromUrl("Cesium3DTileset", "");
        }

        public static void Create3DTilesetFromUrl(string name, string url)
        {
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
            
            Undo.RegisterCreatedObjectUndo(tilesetGo, $"Create {name}");
            Selection.activeGameObject = tilesetGo;
            
            Debug.Log($"Created {name} under CesiumGeoreference.");
        }

        public static void SetGeoreferenceToRotterdam()
        {
            SetGeoreference(51.90759, 4.490608, 6.1);
        }

        public static void SetGeoreference(double lat, double lon, double height)
        {
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
        }

        public static void CreateMultiple3DTilesets(string baseName, List<Rotterdam.DigitalTwins.Runtime.OUPResource> resources)
        {
            if (resources == null || resources.Count == 0) return;

            // We use the same format check as in DataComponent
            var allowedFormats = new[] { "3dtileset", "3dtile", "3dtiles", "3dterrain", "3d tiles", "3d-tiles" };
            var matchingResources = resources
                .Where(r => allowedFormats.Any(fmt => string.Equals(fmt, r.format, System.StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var res in matchingResources)
            {
                string displayName = string.IsNullOrEmpty(res.name) ? res.format.ToUpper() : res.name;
                string tilesetName = $"{baseName} ({displayName})";
                Create3DTilesetFromUrl(tilesetName, res.url);
            }
        }
    }
}
