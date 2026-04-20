using CesiumForUnity;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Rotterdam.DigitalTwins.Runtime;

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

        public static void CreateMultiple3DTilesets(string groupName, List<OUPResource> resources)
        {
            if (resources == null || resources.Count == 0) return;

            var allowedFormats = new[] { "3dtileset", "3dtile", "3dtiles", "3d tiles", "3d-tiles", "3dterrain" };
            foreach (var res in resources)
            {
                if (res.format != null && allowedFormats.Any(fmt => string.Equals(fmt, res.format, System.StringComparison.OrdinalIgnoreCase)))
                {
                    string displayName = string.IsNullOrEmpty(res.name) ? res.format.ToUpper() : res.name;
                    string tilesetName = $"{groupName} ({displayName})";
                    Create3DTilesetFromUrl(tilesetName, res.url);
                }
            }
        }

        public static void SetGeoreferenceToRotterdam()
        {
            SetGeoreference(new OUPGroundPosition { latitude = 51.90759, longitude = 4.490608, height = 6.1 });
        }

        public static void SetGeoreference(OUPGroundPosition groundPosition)
        {
            if (groundPosition == null) return;
            
            CesiumGeoreference georeference = Object.FindAnyObjectByType<CesiumGeoreference>();
            if (georeference == null)
            {
                GameObject georefGo = new GameObject("CesiumGeoreference");
                georeference = georefGo.AddComponent<CesiumGeoreference>();
                Undo.RegisterCreatedObjectUndo(georefGo, "Create CesiumGeoreference");
            }

            Undo.RecordObject(georeference, "Set Georeference");
            georeference.latitude = groundPosition.latitude;
            georeference.longitude = groundPosition.longitude;
            georeference.height = groundPosition.height;

            Debug.Log($"CesiumGeoreference set to ({groundPosition.latitude}, {groundPosition.longitude}, {groundPosition.height}).");
        }
    }
}
