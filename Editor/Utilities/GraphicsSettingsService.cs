using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
#if USING_CESIUM
using CesiumForUnity;
#endif

namespace Rotterdam.DigitalTwins.Editor.Utilities
{
    [InitializeOnLoad]
    public static class GraphicsSettingsService
    {
        public enum GraphicsPreset
        {
            None,
            Low,
            Medium,
            High
        }

        private const string PresetPrefsKey = "Rotterdam.DigitalTwins.GraphicsPreset";

        static GraphicsSettingsService()
        {
            ObjectFactory.componentWasAdded += OnComponentWasAdded;
        }

        public static GraphicsPreset CurrentPreset
        {
            get => (GraphicsPreset)EditorPrefs.GetInt(PresetPrefsKey, (int)GraphicsPreset.None);
            set => EditorPrefs.SetInt(PresetPrefsKey, (int)value);
        }

        private static void OnComponentWasAdded(Component component)
        {
            if (CurrentPreset == GraphicsPreset.None) return;

#if USING_CESIUM
            if (component is Cesium3DTileset tileset)
            {
                ApplyToTileset(tileset, CurrentPreset);
            }
            if (component is CesiumWebMapServiceRasterOverlay wms)
            {
                ApplyToWms(wms, CurrentPreset);
            }
#endif
        }

        public static void ApplyPreset(GraphicsPreset preset)
        {
            CurrentPreset = preset;

#if USING_CESIUM
            var tilesets = UnityEngine.Object.FindObjectsByType<Cesium3DTileset>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var tileset in tilesets)
            {
                ApplyToTileset(tileset, preset);
            }

            var wmsOverlays = UnityEngine.Object.FindObjectsByType<CesiumWebMapServiceRasterOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var wms in wmsOverlays)
            {
                ApplyToWms(wms, preset);
            }
#endif

            ApplyToCamera(preset);
        }

#if USING_CESIUM
        private static void ApplyToTileset(Cesium3DTileset tileset, GraphicsPreset preset)
        {
            Undo.RecordObject(tileset, "Apply Graphics Preset");
            switch (preset)
            {
                case GraphicsPreset.None:
                    tileset.maximumScreenSpaceError = 16;
                    tileset.maximumSimultaneousTileLoads = 20;
                    tileset.maximumCachedBytes = 536870912;
                    break;
                case GraphicsPreset.Low:
                    tileset.maximumScreenSpaceError = 32;
                    tileset.maximumSimultaneousTileLoads = 3;
                    tileset.maximumCachedBytes = 67108864;
                    break;
                case GraphicsPreset.Medium:
                    tileset.maximumScreenSpaceError = 16;
                    tileset.maximumSimultaneousTileLoads = 6;
                    tileset.maximumCachedBytes = 268435456;
                    break;
                case GraphicsPreset.High:
                    tileset.maximumScreenSpaceError = 6;
                    tileset.maximumSimultaneousTileLoads = 20;
                    tileset.maximumCachedBytes = 536870912;
                    break;
            }
            EditorUtility.SetDirty(tileset);
        }

        private static void ApplyToWms(CesiumWebMapServiceRasterOverlay wms, GraphicsPreset preset)
        {
            Undo.RecordObject(wms, "Apply Graphics Preset");
            switch (preset)
            {
                case GraphicsPreset.None:
                    wms.maximumLevel = 14;
                    break;
                case GraphicsPreset.Low:
                    wms.maximumLevel = 10;
                    break;
                case GraphicsPreset.Medium:
                    wms.maximumLevel = 14;
                    break;
                case GraphicsPreset.High:
                    wms.maximumLevel = 22;
                    break;
            }
            EditorUtility.SetDirty(wms);
        }
#endif

        private static void ApplyToCamera(GraphicsPreset preset)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            }

            if (mainCamera != null)
            {
                Undo.RecordObject(mainCamera, "Apply Graphics Preset");
                
                // Far clip plane
                switch (preset)
                {
                    case GraphicsPreset.None:
                        mainCamera.farClipPlane = 1000;
                        break;
                    case GraphicsPreset.Low:
                        mainCamera.farClipPlane = 1000;
                        break;
                    case GraphicsPreset.Medium:
                        mainCamera.farClipPlane = 2000;
                        break;
                    case GraphicsPreset.High:
                        mainCamera.farClipPlane = 3500;
                        break;
                }

                ApplyURPSettings(mainCamera, preset);
                
                EditorUtility.SetDirty(mainCamera);
            }
        }

        private static void ApplyURPSettings(Camera camera, GraphicsPreset preset)
        {
            // Try to find UniversalAdditionalCameraData via reflection to avoid hard dependency
            Type cameraDataType = Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (cameraDataType == null) return;

            Component cameraData = camera.GetComponent(cameraDataType);
            if (cameraData == null) return;

            Undo.RecordObject(cameraData, "Apply Graphics Preset");

            // Antialiasing
            PropertyInfo antialiasingProp = cameraDataType.GetProperty("antialiasing");
            Type antialiasingModeType = Type.GetType("UnityEngine.Rendering.Universal.AntialiasingMode, Unity.RenderPipelines.Universal.Runtime");
            
            if (antialiasingProp != null && antialiasingModeType != null)
            {
                string targetName = "None";
                switch (preset)
                {
                    case GraphicsPreset.Medium:
                        targetName = "FastApproximateAntialiasing";
                        break;
                    case GraphicsPreset.High:
                        targetName = "SubpixelMorphologicalAntialiasing";
                        break;
                    default:
                        targetName = "None";
                        break;
                }

                try
                {
                    string[] availableNames = Enum.GetNames(antialiasingModeType);
                    string bestMatch = availableNames.FirstOrDefault(n => n.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                    
                    if (bestMatch == null)
                    {
                        if (targetName == "FastApproximateAntialiasing")
                            bestMatch = availableNames.FirstOrDefault(n => n.Contains("Fast") || n.Contains("FXAA"));
                        else if (targetName == "SubpixelMorphologicalAntialiasing")
                            bestMatch = availableNames.FirstOrDefault(n => n.Contains("Subpixel") || n.Contains("SMAA"));
                    }

                    if (bestMatch != null)
                    {
                        object aaValue = Enum.Parse(antialiasingModeType, bestMatch);
                        antialiasingProp.SetValue(cameraData, aaValue);
                    }
                    else
                    {
                        // Fallback to None if still not found
                        antialiasingProp.SetValue(cameraData, Enum.Parse(antialiasingModeType, "None"));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[GraphicsSettingsService] Failed to set Antialiasing to {targetName}: {e.Message}");
                }
            }

            // Enable post processing for presets
            if (preset != GraphicsPreset.None)
            {
                PropertyInfo postProcessProp = cameraDataType.GetProperty("renderPostProcessing");
                if (postProcessProp != null)
                {
                    postProcessProp.SetValue(cameraData, true);
                }
            }

            EditorUtility.SetDirty(cameraData);
        }
    }
}
