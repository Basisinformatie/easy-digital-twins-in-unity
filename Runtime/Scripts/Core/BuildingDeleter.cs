using UnityEngine;
using System.Collections.Generic;

#if USING_CESIUM
using CesiumForUnity;
#endif

namespace Rotterdam.DigitalTwins.Runtime
{
    /// <summary>
    /// Component to select and delete buildings in a 3D tileset.
    /// Includes a crosshair and a visual selection stick.
    /// </summary>
    public class BuildingDeleter : MonoBehaviour
    {
        [Header("Raycast Settings")]
        public float range = 1000f;
        public LayerMask layerMask = ~0;

        [Header("Crosshair Settings")]
        public Color crosshairColor = Color.green;
        public float crosshairSize = 20f;
        public bool showCrosshair = true;

        [Header("Stick Settings")]
        public Vector3 stickPosition = new Vector3(0.5f, -0.4f, 0.8f);
        public Vector3 stickRotation = new Vector3(20, -15, 0);
        public Vector3 stickScale = new Vector3(0.04f, 0.04f, 1.0f);
        public Color stickColor = new Color(0.5f, 0.3f, 0.1f);

        [Header("Animation Settings")]
        public float animationDuration = 0.1f;
        public float animationDistance = 0.2f;

        private Camera _playerCamera;
        private GameObject _stick;
        private bool _isAnimating = false;

        void Start()
        {
            InitializeCamera();
            Debug.Log("[BuildingDeleter] Started on " + gameObject.name);
        }

        private void InitializeCamera()
        {
            if (_playerCamera != null) return;

            _playerCamera = GetComponentInChildren<Camera>();
            if (_playerCamera == null)
            {
                _playerCamera = Camera.main;
            }

            if (_playerCamera == null)
            {
                Debug.LogError("[BuildingDeleter] No camera found!");
            }
        }

        void OnEnable()
        {
            InitializeCamera();
            if (_playerCamera != null)
            {
                if (_stick == null)
                {
                    CreateStick();
                }
                else
                {
                    _stick.SetActive(true);
                }
            }
        }

        void OnDisable()
        {
            if (_stick != null)
            {
                _stick.SetActive(false);
            }
        }

        void OnDestroy()
        {
            if (_stick != null)
            {
                Destroy(_stick);
            }
        }

        private void CreateStick()
        {
            _stick = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _stick.name = "SelectionStick";
            
            Collider col = _stick.GetComponent<Collider>();
            if (col != null) Destroy(col);

            _stick.transform.SetParent(_playerCamera.transform);
            _stick.transform.localPosition = stickPosition;
            _stick.transform.localRotation = Quaternion.Euler(stickRotation);
            _stick.transform.localScale = stickScale;

            Renderer rend = _stick.GetComponent<Renderer>();
            if (rend != null)
            {
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpShader != null) rend.material = new Material(urpShader);
                rend.material.color = stickColor;
            }
        }

        void Update()
        {
            bool isClicked = false;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                isClicked = true;
            }
#else
            if (Input.GetMouseButtonDown(0))
            {
                isClicked = true;
            }
#endif

            if (isClicked)
            {
                if (!_isAnimating) StartCoroutine(AnimateStick());
                TryDeleteBuilding();
            }
        }

        private System.Collections.IEnumerator AnimateStick()
        {
            _isAnimating = true;
            Vector3 originalPos = stickPosition;
            Vector3 targetPos = stickPosition + Vector3.forward * animationDistance;

            float elapsed = 0;
            while (elapsed < animationDuration)
            {
                if (_stick != null)
                    _stick.transform.localPosition = Vector3.Lerp(originalPos, targetPos, elapsed / animationDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0;
            while (elapsed < animationDuration)
            {
                if (_stick != null)
                    _stick.transform.localPosition = Vector3.Lerp(targetPos, originalPos, elapsed / animationDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_stick != null)
                _stick.transform.localPosition = originalPos;
            
            _isAnimating = false;
        }

        private void TryDeleteBuilding()
        {
            if (_playerCamera == null) return;

            Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, range, layerMask, QueryTriggerInteraction.Ignore))
            {
                GameObject hitObject = hit.transform.gameObject;
                string buildingInfo = GetBuildingMetadata(hitObject, hit);
                
                Debug.Log($"[BuildingDeleter] Building hit: {buildingInfo}. Deleting now.");
                
                hitObject.SetActive(false);
            }
        }

        private string GetBuildingMetadata(GameObject hitObject, RaycastHit hit)
        {
            string info = hitObject.name;

#if USING_CESIUM
            CesiumMetadata metadata = hitObject.GetComponentInParent<CesiumMetadata>();
            CesiumModelMetadata modelMetadata = hitObject.GetComponentInParent<CesiumModelMetadata>();

            if (metadata != null)
            {
                CesiumFeature[] features = metadata.GetFeatures(hit.transform, hit.triangleIndex);
                if (features != null && features.Length > 0)
                {
                    info = ExtractInfoFromFeature(features[0]);
                }
            }
            else if (modelMetadata != null)
            {
                 CesiumPrimitiveFeatures primitiveFeatures = hitObject.GetComponent<CesiumPrimitiveFeatures>();
                if (primitiveFeatures != null)
                {
                    long featureId = primitiveFeatures.GetFeatureIdFromRaycastHit(hit, 0);
                    if (featureId != -1 && modelMetadata.propertyTables != null)
                    {
                        foreach (var table in modelMetadata.propertyTables)
                        {
                            Dictionary<string, CesiumMetadataValue> values = table.GetMetadataValuesForFeature(featureId);
                            info = ExtractInfoFromTable(values);
                            break;
                        }
                    }
                }
            }
#endif
            return info;
        }

#if USING_CESIUM
        private string ExtractInfoFromFeature(CesiumFeature feature)
        {
            string name = feature.GetString("gml:name", "");
            if (string.IsNullOrEmpty(name)) name = feature.GetString("name", "");
            
            string attributesData = feature.GetString("attributes", "");
            if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(attributesData))
            {
                name = GetNestedValue(attributesData, "Street") + " " + GetNestedValue(attributesData, "HouseNumber");
            }

            if (string.IsNullOrEmpty(name)) name = "ID: " + feature.GetString("id", "Unknown");
            return name;
        }

        private string ExtractInfoFromTable(Dictionary<string, CesiumMetadataValue> values)
        {
            if (values.ContainsKey("name")) return values["name"].GetString();
            if (values.ContainsKey("gml:name")) return values["gml:name"].GetString();
            
            if (values.ContainsKey("attributes"))
            {
                string attr = values["attributes"].GetString();
                string street = GetNestedValue(attr, "Street");
                if (!string.IsNullOrEmpty(street)) return street;
            }

            return "Unknown building";
        }

        private string GetNestedValue(string json, string key)
        {
            if (string.IsNullOrEmpty(json)) return null;
            string normalized = json.Replace("\\\"", "\"");
            string search = $"\"{key}\":\"";
            int start = normalized.IndexOf(search);
            if (start != -1)
            {
                start += search.Length;
                int end = normalized.IndexOf("\"", start);
                if (end != -1) return normalized.Substring(start, end - start);
            }
            return null;
        }
#endif

        void OnGUI()
        {
            if (!showCrosshair) return;

            Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
            GUI.color = crosshairColor;
            
            GUI.DrawTexture(new Rect(center.x - crosshairSize / 2, center.y - 1, crosshairSize, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x - 1, center.y - crosshairSize / 2, 2, crosshairSize), Texture2D.whiteTexture);
        }
    }
}
