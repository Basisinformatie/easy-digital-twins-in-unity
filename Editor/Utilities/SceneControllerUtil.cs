using UnityEditor;
using UnityEngine;
using Rotterdam.DigitalTwins.Runtime;

namespace Rotterdam.DigitalTwins.Editor
{
    public static class SceneControllerUtil
    {
        private const string PackagePath = "Packages/com.rotterdam.digital-twins/Runtime/Prefabs/Controllers/";
        private const string LocalPath = "Assets/Runtime/Prefabs/Controllers/";

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
            if (platform != null)
            {
                instance.transform.position = platform.transform.position + Vector3.up * 0.05f + Vector3.forward * 0.01f;
            }

            Selection.activeGameObject = instance;
            
            Debug.Log($"Controller replaced with {prefabName}.");
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