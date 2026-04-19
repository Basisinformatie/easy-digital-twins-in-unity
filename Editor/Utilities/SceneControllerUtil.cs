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

            string fullPath = $"{PackagePath}{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (prefab == null)
            {
                fullPath = $"{LocalPath}{prefabName}.prefab";
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            }

            if (prefab == null)
            {
                Debug.LogError($"Controller prefab {prefabName} not found at paths: {PackagePath} or {LocalPath}");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Controller");
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
        }

        public static string GetCurrentControllerType()
        {
            if (Object.FindAnyObjectByType<FirstPersonController>(FindObjectsInactive.Include) != null)
                return "First Person";
            if (Object.FindAnyObjectByType<ThirdPersonController>(FindObjectsInactive.Include) != null)
                return "Third Person";
            return "None";
        }
    }
}