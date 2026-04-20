using CesiumForUnity;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

        public static void CreateMultiple3DTilesets(List<(string name, string url)> datasets)
        {
            if (datasets == null || datasets.Count == 0) return;

            CesiumGeoreference georeference = Object.FindAnyObjectByType<CesiumGeoreference>();
            if (georeference == null)
            {
                GameObject georefGo = new GameObject("CesiumGeoreference");
                georeference = georefGo.AddComponent<CesiumGeoreference>();
                Undo.RegisterCreatedObjectUndo(georefGo, "Create CesiumGeoreference");
            }

            GameObject lastCreated = null;
            foreach (var (name, url) in datasets)
            {
                GameObject tilesetGo = new GameObject(name);
                tilesetGo.transform.SetParent(georeference.transform);
                Cesium3DTileset tileset = tilesetGo.AddComponent<Cesium3DTileset>();

                if (!string.IsNullOrEmpty(url))
                {
                    tileset.tilesetSource = CesiumDataSource.FromUrl;
                    tileset.url = url;
                }
                
                Undo.RegisterCreatedObjectUndo(tilesetGo, $"Create {name}");
                Debug.Log($"Created {name} under CesiumGeoreference.");
                lastCreated = tilesetGo;
            }
            
            if (lastCreated != null)
            {
                Selection.activeGameObject = lastCreated;
            }
        }

        public static void SetGeoreferenceToRotterdam()
        {
            CesiumGeoreference georeference = Object.FindAnyObjectByType<CesiumGeoreference>();
            if (georeference == null)
            {
                GameObject georefGo = new GameObject("CesiumGeoreference");
                georeference = georefGo.AddComponent<CesiumGeoreference>();
                Undo.RegisterCreatedObjectUndo(georefGo, "Create CesiumGeoreference");
            }

            Undo.RecordObject(georeference, "Set Georeference to Rotterdam");
            georeference.latitude = 51.90759;
            georeference.longitude = 4.490608;
            georeference.height = 6.1;

            Debug.Log("CesiumGeoreference set to Rotterdam (51.90759, 4.490608, 6.1).");
        }
    }
}
