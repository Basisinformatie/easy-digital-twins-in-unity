using UnityEditor;
using UnityEngine;
using Rotterdam.DigitalTwins.Runtime;
using System.Threading;

namespace Rotterdam.DigitalTwins.Editor
{
    public static class SceneControllerUtil
    {
        private const string PackagePath = "Packages/com.rotterdam.digital-twins/Runtime/Prefabs/Controllers/";
        private const string LocalPath = "Assets/Runtime/Prefabs/Controllers/";
        private const string FeaturesPackagePath = "Packages/com.rotterdam.digital-twins/Runtime/Prefabs/Features/";
        private const string FeaturesLocalPath = "Assets/Runtime/Prefabs/Features/";

        public static void ReplaceController(string prefabName)
        {
            RemoveExistingControllers();

            string subFolder = prefabName.Contains("FirstPerson") ? "FirstPerson/" : 
                               prefabName.Contains("ThirdPerson") ? "ThirdPerson/" : 
                               prefabName.Contains("Helicopter") ? "Helicopter/" : "Car/";
            string fullPath = $"{PackagePath}{subFolder}{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (prefab == null)
            {
                fullPath = $"{LocalPath}{subFolder}{prefabName}.prefab";
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            }

            if (prefab == null)
            {
                Debug.LogError($"Controller prefab {prefabName} not found at paths: {PackagePath}{subFolder} or {LocalPath}{subFolder}");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Controller");

            GameObject platform = GetOrCreateStartingPlatform();
            Thread.Sleep(50);
            if (platform != null)
            {
                instance.transform.position = platform.transform.position + Vector3.up * 0.05f + Vector3.forward * 0.01f;
            }

            Selection.activeGameObject = instance;
            
            Debug.Log($"Controller replaced with {prefabName}.");
        }

        public static void SetAdaptiveLighting(bool enabled)
        {
            if (enabled)
            {
                EnableAdaptiveLighting();
            }
            else
            {
                DisableAdaptiveLighting();
            }
        }

        private static void EnableAdaptiveLighting()
        {
            RemoveObjectByName("Directional Light");

            GameObject adaptiveLighting = InstantiateFeaturePrefab("Adaptive Lighting");
            if (adaptiveLighting == null) return;

            Material skybox = LoadFeatureAsset<Material>("ToolboxSky1.mat");
            if (skybox != null)
            {
                RenderSettings.skybox = skybox;
            }

            Light sunLight = adaptiveLighting.GetComponent<Light>();
            if (sunLight != null)
            {
                RenderSettings.sun = sunLight;
            }
            
            Debug.Log("Adaptive Lighting enabled.");
        }

        private static void DisableAdaptiveLighting()
        {
            RemoveObjectByName("Adaptive Lighting");

            GameObject directionalLight = InstantiateFeaturePrefab("Directional Light");
            
            if (directionalLight != null)
            {
                RenderSettings.sun = directionalLight.GetComponent<Light>();
            }
            
            Debug.Log("Adaptive Lighting disabled.");
        }

        private static void RemoveObjectByName(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                Undo.DestroyObjectImmediate(go);
            }
        }

        private static GameObject InstantiateFeaturePrefab(string prefabName)
        {
            string fileName = prefabName.EndsWith(".prefab") ? prefabName : prefabName + ".prefab";
            string fullPath = $"{FeaturesPackagePath}{fileName}";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (prefab == null)
            {
                fullPath = $"{FeaturesLocalPath}{fileName}";
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            }

            if (prefab == null)
            {
                Debug.LogError($"Prefab {fileName} not found at paths: {FeaturesPackagePath} or {FeaturesLocalPath}");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, $"Instantiate {prefabName}");
            return instance;
        }

        private static T LoadFeatureAsset<T>(string fileName) where T : Object
        {
            string fullPath = $"{FeaturesPackagePath}{fileName}";
            T asset = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (asset == null)
            {
                fullPath = $"{FeaturesLocalPath}{fileName}";
                asset = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            }
            return asset;
        }

        public static bool IsAdaptiveLightingEnabled()
        {
            return GameObject.Find("Adaptive Lighting") != null;
        }

        public static SunRotation GetSunRotation()
        {
            GameObject adaptiveLighting = GameObject.Find("Adaptive Lighting");
            return adaptiveLighting != null ? adaptiveLighting.GetComponent<SunRotation>() : null;
        }

        public static void SetSunRotationMode(SunRotation.RotationMode mode)
        {
            SunRotation sunRotation = GetSunRotation();
            if (sunRotation != null)
            {
                Undo.RecordObject(sunRotation, "Set Sun Rotation Mode");
                sunRotation.mode = mode;
                EditorUtility.SetDirty(sunRotation);
            }
        }

        public static void SetSunRotationTime(float time)
        {
            SunRotation sunRotation = GetSunRotation();
            if (sunRotation != null)
            {
                Undo.RecordObject(sunRotation, "Set Sun Rotation Time");
                sunRotation.timeOfDay = time;
                EditorUtility.SetDirty(sunRotation);
            }
        }

        public static void SetSunRotationCycle(float seconds)
        {
            SunRotation sunRotation = GetSunRotation();
            if (sunRotation != null)
            {
                Undo.RecordObject(sunRotation, "Set Sun Rotation Cycle");
                float rotationVal = 0.5f / seconds;
                sunRotation.rotationSet = new Vector3(rotationVal, sunRotation.rotationSet.y, sunRotation.rotationSet.z);
                EditorUtility.SetDirty(sunRotation);
            }
        }

        public static void SetSunRotationLatitude(float latitude)
        {
            SunRotation sunRotation = GetSunRotation();
            if (sunRotation != null)
            {
                Undo.RecordObject(sunRotation, "Set Sun Rotation Latitude");
                sunRotation.latitude = latitude;
                EditorUtility.SetDirty(sunRotation);
            }
        }

        public static void AddMainCamera()
        {
            string prefabName = "Main Camera.prefab";
            string fullPath = $"{FeaturesPackagePath}{prefabName}";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (prefab == null)
            {
                fullPath = $"{FeaturesLocalPath}{prefabName}";
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            }

            if (prefab == null)
            {
                Debug.LogError($"Main Camera prefab not found at paths: {FeaturesPackagePath} or {FeaturesLocalPath}");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Add Main Camera");

            Selection.activeGameObject = instance;
            Debug.Log("Main Camera added.");
        }

        public static void RemoveExistingControllers()
        {
            var firstPersonControllers = Object.FindObjectsByType<FirstPersonController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var controller in firstPersonControllers)
            {
                Undo.DestroyObjectImmediate(controller.gameObject);
            }

            var thirdPersonControllers = Object.FindObjectsByType<ThirdPersonController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var controller in thirdPersonControllers)
            {
                Undo.DestroyObjectImmediate(controller.gameObject);
            }

            var carControllers = Object.FindObjectsByType<CustomCarController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var controller in carControllers)
            {
                Undo.DestroyObjectImmediate(controller.gameObject);
            }

            var helicopterControllers = Object.FindObjectsByType<CustomHelicopterController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var controller in helicopterControllers)
            {
                Undo.DestroyObjectImmediate(controller.gameObject);
            }
            
            RemoveExistingCameras();
        }

        public static void RemoveExistingCameras()
        {
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var camera in cameras)
            {
                Undo.DestroyObjectImmediate(camera.gameObject);
            }
        }

        private static GameObject GetOrCreateStartingPlatform()
        {
            GameObject platform = GameObject.Find("StartingPlatform");
            if (platform == null)
            {
                const string platformPackagePath = "Packages/com.rotterdam.digital-twins/Runtime/Prefabs/StartingPlatform.prefab";
                const string platformLocalPath = "Assets/Runtime/Prefabs/StartingPlatform.prefab";

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(platformPackagePath);
                if (prefab == null)
                {
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(platformLocalPath);
                }

                if (prefab != null)
                {
                    platform = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    platform.name = "StartingPlatform";
                    Undo.RegisterCreatedObjectUndo(platform, "Instantiate Starting Platform");

                    var groundSnap = platform.GetComponent<GroundSnap>();
                    if (groundSnap != null)
                    {
                        groundSnap.Snap();
                    }
                }
                else
                {
                    Debug.LogWarning("StartingPlatform prefab not found.");
                }
            }
            return platform;
        }
        
        public static string GetCurrentControllerType()
        {
            if (Object.FindAnyObjectByType<FirstPersonController>(FindObjectsInactive.Include) != null)
                return "First Person";
            if (Object.FindAnyObjectByType<ThirdPersonController>(FindObjectsInactive.Include) != null)
                return "Third Person";
            if (Object.FindAnyObjectByType<CustomCarController>(FindObjectsInactive.Include) != null)
                return "Car";
            if (Object.FindAnyObjectByType<CustomHelicopterController>(FindObjectsInactive.Include) != null)
                return "Helicopter";
            return "None";
        }
    }
}