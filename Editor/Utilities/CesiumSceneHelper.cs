using CesiumForUnity;
using UnityEditor;
using UnityEngine;

namespace Rotterdam.DigitalTwins.Editor
{
    public static class CesiumSceneHelper
    {
        public static void CreateBlank3DTileset()
        {
            CesiumGeoreference georeference = Object.FindAnyObjectByType<CesiumGeoreference>();
            if (georeference == null)
            {
                GameObject georefGo = new GameObject("CesiumGeoreference");
                georeference = georefGo.AddComponent<CesiumGeoreference>();
                Undo.RegisterCreatedObjectUndo(georefGo, "Create CesiumGeoreference");
            }

            GameObject tilesetGo = new GameObject("Cesium3DTileset");
            tilesetGo.transform.SetParent(georeference.transform);
            tilesetGo.AddComponent<Cesium3DTileset>();
            
            Undo.RegisterCreatedObjectUndo(tilesetGo, "Create Blank 3D Tileset");
            Selection.activeGameObject = tilesetGo;
            
            Debug.Log("Created Blank 3D Tiles Tileset under CesiumGeoreference.");
        }
    }
}
