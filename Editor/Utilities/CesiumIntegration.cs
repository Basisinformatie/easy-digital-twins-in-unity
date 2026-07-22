using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Templates.AR;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
#endif

namespace UnityEngine.XR.Templates.AR
{
    /// <summary>
    /// Provides tools to integrate Cesium into the Mobile AR Template. 
    /// This script automates the setup of the CesiumGeoreference prefab, 
    /// configures the AR scene UI with specialized zoom buttons, 
    /// and ensures optimal camera and spawning settings for large-scale geospatial data.
    /// </summary>
    public class CesiumUniquenessEnforcer : MonoBehaviour
    {
        void OnEnable()
        {
            var enforcers = Object.FindObjectsByType<CesiumUniquenessEnforcer>(FindObjectsSortMode.None);
            foreach (var enforcer in enforcers)
            {
                if (enforcer != this)
                {
                    if (enforcer != null && enforcer.gameObject != null)
                    {
                        Destroy(enforcer.gameObject, 0.1f);
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    public static class CesiumIntegrationEditor
    {
        public static void Integrate()
        {
            var spawner = Object.FindAnyObjectByType<ObjectSpawner>();
            if (spawner == null)
            {
                Debug.LogError("ObjectSpawner not found in the scene. Ensure the SampleScene is open.");
                return;
            }

            string prefabPath = "Assets/MobileARTemplateAssets/Prefabs/CesiumGeoreference.prefab";
            GameObject cesiumPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (cesiumPrefab == null)
            {
                Debug.LogError($"Prefab not found at path: {prefabPath}. Check if the prefab actually exists there.");
                return;
            }

            Debug.Log($"CesiumGeoreference prefab found with scale: {cesiumPrefab.transform.localScale}");

            ConfigurePrefabAsInteractable(prefabPath);

            Undo.RecordObject(spawner, "Configure ObjectSpawner");
            spawner.singleObjectMode = true;

            if (!spawner.objectPrefabs.Contains(cesiumPrefab))
            {
                var list = spawner.objectPrefabs.ToList();
                list.Add(cesiumPrefab);
                spawner.objectPrefabs = list;
                Debug.Log("CesiumGeoreference added to ObjectSpawner.");
            }
            else
            {
                Debug.Log("CesiumGeoreference was already in the ObjectSpawner.");
            }
            EditorUtility.SetDirty(spawner);

            int index = spawner.objectPrefabs.IndexOf(cesiumPrefab);

            var menuManager = Object.FindAnyObjectByType<ARTemplateMenuManager>();
            if (menuManager == null)
            {
                Debug.LogError("ARTemplateMenuManager not found in the scene.");
                return;
            }

            if (menuManager.objectMenu == null)
            {
                Debug.LogError("ARTemplateMenuManager has no reference to objectMenu.");
                return;
            }

            Button existingButton = menuManager.objectMenu.GetComponentsInChildren<Button>(true)
                .FirstOrDefault(b => b.name.StartsWith("Button (") && !b.name.Contains("Cesium"));
            
            if (existingButton == null)
            {
                Debug.LogError("No existing button found to duplicate in the Object Menu.");
                return;
            }

            string buttonName = "Button (Cesium)";
            Transform parent = existingButton.transform.parent;
            GameObject existingButtonGO = parent.Find(buttonName)?.gameObject;

            if (existingButtonGO != null)
            {
                Debug.Log("Cesium button already exists. Updating action only.");
            }
            else
            {
                existingButtonGO = Object.Instantiate(existingButton.gameObject, parent);
                existingButtonGO.name = buttonName;
                Undo.RegisterCreatedObjectUndo(existingButtonGO, "Create Cesium Button");
                Debug.Log("New UI button created for Cesium.");
            }

            existingButtonGO.transform.SetAsFirstSibling();

            Button newButton = existingButtonGO.GetComponent<Button>();
            
            int eventCount = newButton.onClick.GetPersistentEventCount();
            for (int i = eventCount - 1; i >= 0; i--)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(newButton.onClick, i);
            }
            
            UnityEditor.Events.UnityEventTools.AddIntPersistentListener(newButton.onClick, menuManager.SetObjectToSpawn, index);

            var text = existingButtonGO.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = "Cesium";
                EditorUtility.SetDirty(text);
            }
            else
            {
                var tmpText = existingButtonGO.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = "Cesium";
                    EditorUtility.SetDirty(tmpText);
                }
            }

            string texturePath = "Assets/3drotterdam.png";
            Texture iconTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
            if (iconTexture != null)
            {
                Transform iconTransform = existingButtonGO.transform.Find("Icon");
                RawImage rawImg = null;
                if (iconTransform != null)
                {
                    rawImg = iconTransform.GetComponent<RawImage>();
                    if (rawImg == null)
                    {
                        Image oldImg = iconTransform.GetComponent<Image>();
                        if (oldImg != null) Object.DestroyImmediate(oldImg);
                        rawImg = iconTransform.gameObject.AddComponent<RawImage>();
                    }
                }
                else
                {
                    rawImg = existingButtonGO.GetComponentInChildren<RawImage>();
                    if (rawImg == null)
                    {
                        Image existingImg = existingButtonGO.GetComponentsInChildren<Image>().FirstOrDefault(img => img.gameObject != existingButtonGO);
                        if (existingImg != null)
                        {
                            GameObject targetGO = existingImg.gameObject;
                            Object.DestroyImmediate(existingImg);
                            rawImg = targetGO.AddComponent<RawImage>();
                        }
                    }
                }

                if (rawImg != null)
                {
                    rawImg.texture = iconTexture;
                    EditorUtility.SetDirty(rawImg);
                }
            }
            
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Undo.RecordObject(mainCam, "Adjust Camera Far Clip Plane");
                mainCam.farClipPlane = 100f;
                EditorUtility.SetDirty(mainCam);
                Debug.Log($"Main Camera farClipPlane set to {mainCam.farClipPlane} for Cesium.");
            }

            SetupScaling(menuManager, spawner);

            bool menuManagerChanged = false;
            if (menuManager.cancelButton == null && menuManager.objectMenu != null)
            {
                var cancelBtn = menuManager.objectMenu.GetComponentsInChildren<Button>(true)
                    .FirstOrDefault(b => b.name.ToLower().Contains("cancel"));
                if (cancelBtn != null)
                {
                    menuManager.cancelButton = cancelBtn;
                    menuManagerChanged = true;
                    Debug.Log("Automatically assigned missing Cancel Button reference to ARTemplateMenuManager.");
                }
            }

            if (menuManagerChanged) EditorUtility.SetDirty(menuManager);

            EditorUtility.SetDirty(menuManager);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            
            EditorUtility.SetDirty(newButton);
            Debug.Log($"Cesium successfully integrated at index {index}!");
            
            Selection.activeGameObject = existingButtonGO;
            
            EditorUtility.DisplayDialog("Cesium Integration", 
                $"Cesium has been successfully integrated!\n\n" +
                $"- Prefab added to ObjectSpawner at index {index}\n" +
                $"- UI button created\n" +
                $"- Dragging and rotation enabled via ARTransformer\n" +
                $"- Camera Far Clip Plane set to {mainCam?.farClipPlane ?? 100f}\n\n" +
                $"Make sure to save the scene (Ctrl+S).", "OK");
        }
        private static void SetupScaling(ARTemplateMenuManager menuManager, ObjectSpawner spawner)
        {
            var scaleController = menuManager.GetComponent<ObjectScaleController>();
            if (scaleController == null)
            {
                scaleController = Undo.AddComponent<ObjectScaleController>(menuManager.gameObject);
            }

            SerializedObject soScale = new SerializedObject(scaleController);
            soScale.FindProperty("m_ObjectSpawner").objectReferenceValue = spawner;
            soScale.ApplyModifiedProperties();

            if (menuManager.deleteButton == null) return;

            GameObject deleteButtonGO = menuManager.deleteButton.gameObject;
            Transform parent = deleteButtonGO.transform.parent;

            CreateScaleButton(deleteButtonGO, parent, "Button (Zoom In)", "+", new Vector2(100, -250), scaleController.ScaleUp);
            CreateScaleButton(deleteButtonGO, parent, "Button (Zoom Out)", "-", new Vector2(100, -350), scaleController.ScaleDown);
        }

        private static void CreateScaleButton(GameObject template, Transform parent, string name, string label, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonGO = parent.Find(name)?.gameObject;
            if (buttonGO == null)
            {
                buttonGO = Object.Instantiate(template, parent);
                buttonGO.name = name;
                buttonGO.SetActive(true);
                Undo.RegisterCreatedObjectUndo(buttonGO, "Create " + name);
            }

            RectTransform rt = buttonGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = position;

            Button btn = buttonGO.GetComponent<Button>();
            int eventCount = btn.onClick.GetPersistentEventCount();
            for (int i = eventCount - 1; i >= 0; i--)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, i);
            }
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);

            var txt = buttonGO.GetComponentInChildren<Text>(true);
            if (txt == null)
            {
                var tmpTxt = buttonGO.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmpTxt == null)
                {
                    GameObject textGO = new GameObject("Text");
                    textGO.transform.SetParent(buttonGO.transform, false);
                    tmpTxt = textGO.AddComponent<TextMeshProUGUI>();
                    Undo.RegisterCreatedObjectUndo(textGO, "Add button text");
                }
                
                tmpTxt.text = label;
                tmpTxt.fontSize = 40;
                tmpTxt.color = Color.white;
                tmpTxt.alignment = TextAlignmentOptions.Center;
                tmpTxt.gameObject.SetActive(true);

                RectTransform textRt = tmpTxt.GetComponent<RectTransform>();
                if (textRt != null)
                {
                    textRt.sizeDelta = new Vector2(35, 35);
                }
                
                EditorUtility.SetDirty(tmpTxt);
            }
            else
            {
                txt.text = label;
                txt.fontSize = 40;
                txt.color = Color.white;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.gameObject.SetActive(true);

                RectTransform textRt = txt.GetComponent<RectTransform>();
                if (textRt != null)
                {
                    textRt.sizeDelta = new Vector2(35, 35);
                }

                EditorUtility.SetDirty(txt);
            }

            Transform icon = buttonGO.transform.Find("Icon");
            if (icon != null)
            {
                icon.gameObject.SetActive(false);
                EditorUtility.SetDirty(icon.gameObject);
            }

            EditorUtility.SetDirty(buttonGO);
        }

        private static void ConfigurePrefabAsInteractable(string prefabPath)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            bool changed = false;

            var rb = prefabRoot.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = prefabRoot.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
                changed = true;
                Debug.Log("Rigidbody added to CesiumGeoreference prefab.");
            }

            if (prefabRoot.transform.localScale != new Vector3(0.01f, 0.01f, 0.01f))
            {
                prefabRoot.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                changed = true;
                Debug.Log("CesiumGeoreference prefab scale set to 0.01, 0.01, 0.01.");
            }

            var simple = prefabRoot.GetComponent<XRSimpleInteractable>();
            if (simple != null && simple.GetType() == typeof(XRSimpleInteractable))
            {
                Object.DestroyImmediate(simple);
                changed = true;
            }

            var grab = prefabRoot.GetComponent<XRGrabInteractable>();
            if (grab == null)
            {
                grab = prefabRoot.AddComponent<XRGrabInteractable>();
            }

            grab.selectMode = InteractableSelectMode.Multiple;
            
            grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
            grab.throwOnDetach = false;
            grab.trackPosition = true;
            grab.trackRotation = true;
            grab.useDynamicAttach = true;
            grab.matchAttachPosition = true;
            
            var uniqueness = prefabRoot.GetComponent<CesiumUniquenessEnforcer>();
            if (uniqueness == null)
            {
                uniqueness = prefabRoot.AddComponent<CesiumUniquenessEnforcer>();
            }
            
            changed = true;
            Debug.Log("XRGrabInteractable and CesiumUniquenessEnforcer configured on CesiumGeoreference prefab.");

            System.Type transformerType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.Transformers.ARTransformer, Unity.XR.Interaction.Toolkit");
            if (transformerType != null)
            {
                var transformer = prefabRoot.GetComponent(transformerType);
                if (transformer == null)
                {
                    transformer = prefabRoot.AddComponent(transformerType);
                    changed = true;
                }
                
                var minScaleProp = transformerType.GetField("m_MinScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (minScaleProp != null) minScaleProp.SetValue(transformer, 0.00001f);
                
                var maxScaleProp = transformerType.GetField("m_MaxScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (maxScaleProp != null) maxScaleProp.SetValue(transformer, 100000f);

                var translationModeProp = transformerType.GetField("m_ObjectPlaneTranslationMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (translationModeProp != null) translationModeProp.SetValue(transformer, 2);
                
                Debug.Log("ARTransformer configured on CesiumGeoreference prefab.");
            }

            var existingCollider = prefabRoot.GetComponent<Collider>();
            if (existingCollider == null)
            {
                var box = prefabRoot.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, 5f, 0f);
                box.size = new Vector3(100f, 10f, 100f);
                changed = true;
                Debug.Log("BoxCollider added to CesiumGeoreference prefab for initial selection.");
            }
            else if (existingCollider is BoxCollider box)
            {
                if (box.size.x < 100f || box.size.y < 10f)
                {
                    box.center = new Vector3(0f, 5f, 0f);
                    box.size = new Vector3(100f, 10f, 100f);
                    changed = true;
                    Debug.Log("BoxCollider size and center updated on CesiumGeoreference prefab.");
                }
            }

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
#endif
}
