namespace Rotterdam.DigitalTwins.Runtime
{
#if USING_CESIUM
    using CesiumForUnity;
#endif
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;

    /// <summary>
    /// Component to select buildings in a Cesium tileset and display metadata.
    /// </summary>
    public class BuildingSelector : MonoBehaviour
    {
        private Camera _mainCamera;

        [Header("UI Elements")]
        public GameObject infoPanel;
        public Text infoText;

        [Header("Raycast Settings")]
        public LayerMask layerMask = ~0;

        void Start()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                _mainCamera = FindObjectOfType<Camera>();
                Debug.Log("[DEBUG_LOG] Camera.main was null, searching for another camera: " + (_mainCamera != null ? _mainCamera.name : "No camera found!"));
            }

            if (infoText == null || infoPanel == null)
            {
                CreateDefaultUI();
            }

            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }

            Debug.Log("[DEBUG_LOG] BuildingSelector started on GameObject: " + gameObject.name);
        }

        private void CreateDefaultUI()
        {
            if (infoPanel == null)
            {
                GameObject existingPanel = GameObject.Find("BuildingInfoPanel");
                if (existingPanel != null) infoPanel = existingPanel;
            }

            if (infoText == null)
            {
                GameObject existingText = GameObject.Find("BuildingInfoText");
                if (existingText != null) infoText = existingText.GetComponent<Text>();
            }

            if (infoPanel == null)
            {
                GameObject canvasGo = new GameObject("CesiumInfoCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Canvas canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                infoPanel = new GameObject("BuildingInfoPanel", typeof(Image));
                infoPanel.transform.SetParent(canvasGo.transform, false);
                RectTransform panelRect = infoPanel.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0);
                panelRect.anchorMax = new Vector2(0.5f, 0);
                panelRect.pivot = new Vector2(0.5f, 0);
                panelRect.anchoredPosition = new Vector2(0, 50);
                panelRect.sizeDelta = new Vector2(600, 150);
                infoPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);

                GameObject textGo = new GameObject("BuildingInfoText", typeof(Text));
                textGo.transform.SetParent(infoPanel.transform, false);
                infoText = textGo.GetComponent<Text>();
                infoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                infoText.fontSize = 28;
                infoText.color = Color.white;
                infoText.alignment = TextAnchor.MiddleCenter;
                infoText.horizontalOverflow = HorizontalWrapMode.Wrap;
                infoText.verticalOverflow = VerticalWrapMode.Overflow;
                RectTransform textRect = textGo.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.offsetMin = new Vector2(10, 10);
                textRect.offsetMax = new Vector2(-10, -10);
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
            #endif

            #if !ROTTERDAM_ENABLE_INPUT_SYSTEM
            if (!isClicked && Input.GetMouseButtonDown(0))
            {
                isClicked = true;
            }
            #endif

            if (isClicked)
            {
                SelectBuilding();
            }
        }

        private void SelectBuilding()
        {
            Vector3 mousePos = Vector3.zero;
            bool posFound = false;

            #if ROTTERDAM_ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                posFound = true;
            }
            #endif

            #if !ROTTERDAM_ENABLE_INPUT_SYSTEM
            if (!posFound)
            {
                mousePos = Input.mousePosition;
            }
            #endif

            Ray ray = _mainCamera.ScreenPointToRay(mousePos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
            {
                GameObject hitObject = hit.transform.gameObject;
                Debug.Log("[DEBUG_LOG] Hit: " + hitObject.name);

#if USING_CESIUM
                CesiumMetadata metadata = hitObject.GetComponentInParent<CesiumMetadata>();
                CesiumModelMetadata modelMetadata = hitObject.GetComponentInParent<CesiumModelMetadata>();

                bool foundData = false;
                string displayInfo = "No metadata found.";

                if (metadata != null)
                {
                    CesiumFeature[] features = metadata.GetFeatures(hit.transform, hit.triangleIndex);
                    if (features != null && features.Length > 0)
                    {
                        displayInfo = ExtractInfoFromFeature(features[0]);
                        foundData = true;
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
                                
                                string debugAttr = "{";
                                foreach(var kvp in values) debugAttr += $"\"{kvp.Key}\":\"{kvp.Value.GetString()}\",";
                                debugAttr = debugAttr.TrimEnd(',') + "}";
                                Debug.Log("[DEBUG_LOG] attributes: " + debugAttr);

                                displayInfo = ExtractInfoFromTable(values, debugAttr);
                                foundData = true;
                                break;
                            }
                        }
                    }
                }
#else
                bool foundData = false;
                string displayInfo = null;
#endif

                if (foundData)
                {
                    ShowUI(displayInfo);
                }
                else
                {
                    if (infoPanel != null) infoPanel.SetActive(false);
                }
            }
            else
            {
                if (infoPanel != null) infoPanel.SetActive(false);
            }
        }

#if USING_CESIUM
        private string ExtractInfoFromFeature(CesiumFeature feature)
        {
            string street = null, houseNumber = null, postalCode = null, year = null, name = null;

            string attributesData = feature.GetString("attributes", "");
            if (!string.IsNullOrEmpty(attributesData))
            {
                street = GetNestedValue(attributesData, "Street");
                if (string.IsNullOrEmpty(street)) street = GetNestedValue(attributesData, "openbareruimte");
                if (string.IsNullOrEmpty(street)) street = GetNestedValue(attributesData, "straat");

                houseNumber = GetNestedValue(attributesData, "HouseNumber");
                if (string.IsNullOrEmpty(houseNumber)) houseNumber = GetNestedValue(attributesData, "huisnummer");

                postalCode = GetNestedValue(attributesData, "PostalCode");
                if (string.IsNullOrEmpty(postalCode)) postalCode = GetNestedValue(attributesData, "postcode");

                year = GetNestedValue(attributesData, "yearOfConstruction");
                if (string.IsNullOrEmpty(year)) year = GetNestedValue(attributesData, "oorspronkelijkbouwjaar");
                if (string.IsNullOrEmpty(year)) year = GetNestedValue(attributesData, "bouwjaar");

                name = GetNestedValue(attributesData, "gml:name");
                if (string.IsNullOrEmpty(name)) name = GetNestedValue(attributesData, "name");
            }

            if (string.IsNullOrEmpty(street)) street = feature.GetString("openbareruimte", "");
            if (string.IsNullOrEmpty(street)) street = feature.GetString("straat", "");
            if (string.IsNullOrEmpty(street)) street = feature.GetString("Street", "");

            if (string.IsNullOrEmpty(houseNumber)) houseNumber = feature.GetString("huisnummer", "");
            if (string.IsNullOrEmpty(houseNumber)) houseNumber = feature.GetString("HouseNumber", "");

            if (string.IsNullOrEmpty(postalCode)) postalCode = feature.GetString("postcode", "");
            if (string.IsNullOrEmpty(postalCode)) postalCode = feature.GetString("PostalCode", "");

            if (string.IsNullOrEmpty(year)) year = feature.GetString("yearOfConstruction", "");
            if (string.IsNullOrEmpty(year)) year = feature.GetString("oorspronkelijkbouwjaar", "");
            if (string.IsNullOrEmpty(year)) year = feature.GetString("bouwjaar", "");

            if (string.IsNullOrEmpty(name)) name = feature.GetString("gml:name", "");
            if (string.IsNullOrEmpty(name)) name = feature.GetString("name", "");

            string addressData = feature.GetString("Address", "");
            if (!string.IsNullOrEmpty(addressData))
            {
                if (string.IsNullOrEmpty(street)) street = GetNestedValue(addressData, "Street");
                if (string.IsNullOrEmpty(houseNumber)) houseNumber = GetNestedValue(addressData, "HouseNumber");
                if (string.IsNullOrEmpty(postalCode)) postalCode = GetNestedValue(addressData, "PostalCode");
            }

            if (!string.IsNullOrEmpty(street) || !string.IsNullOrEmpty(year))
            {
                if (string.IsNullOrEmpty(street)) street = "Unknown";
                if (string.IsNullOrEmpty(year)) year = "Unknown";
                return $"Address: {street} {houseNumber} {postalCode}\nYear of construction: {year}";
            }

            if (!string.IsNullOrEmpty(name)) return $"Name: {name}";
            if (!string.IsNullOrEmpty(attributesData)) return "Data: " + attributesData;
            
            return "Object ID: " + feature.GetString("id", "Unknown");
        }
#endif

#if USING_CESIUM
        private string ExtractInfoFromTable(Dictionary<string, CesiumMetadataValue> values, string rawDataFallback = null)
        {
            string street = null, houseNumber = null, postalCode = null, year = null, name = null;

            string attributesData = GetValue(values, "attributes");
            if (!string.IsNullOrEmpty(attributesData))
            {
                street = GetNestedValue(attributesData, "Street");
                if (string.IsNullOrEmpty(street)) street = GetNestedValue(attributesData, "openbareruimte");
                if (string.IsNullOrEmpty(street)) street = GetNestedValue(attributesData, "straat");

                houseNumber = GetNestedValue(attributesData, "HouseNumber");
                if (string.IsNullOrEmpty(houseNumber)) houseNumber = GetNestedValue(attributesData, "huisnummer");

                postalCode = GetNestedValue(attributesData, "PostalCode");
                if (string.IsNullOrEmpty(postalCode)) postalCode = GetNestedValue(attributesData, "postcode");

                year = GetNestedValue(attributesData, "yearOfConstruction");
                if (string.IsNullOrEmpty(year)) year = GetNestedValue(attributesData, "oorspronkelijkbouwjaar");
                if (string.IsNullOrEmpty(year)) year = GetNestedValue(attributesData, "bouwjaar");

                name = GetNestedValue(attributesData, "gml:name");
                if (string.IsNullOrEmpty(name)) name = GetNestedValue(attributesData, "name");
            }

            if (string.IsNullOrEmpty(street)) street = GetValue(values, "openbareruimte", "straat", "Street");
            if (string.IsNullOrEmpty(houseNumber)) houseNumber = GetValue(values, "huisnummer", "HouseNumber");
            if (string.IsNullOrEmpty(postalCode)) postalCode = GetValue(values, "postcode", "PostalCode");
            if (string.IsNullOrEmpty(year)) year = GetValue(values, "yearOfConstruction", "oorspronkelijkbouwjaar", "bouwjaar");
            if (string.IsNullOrEmpty(name)) name = GetValue(values, "gml:name", "name");

            string addressData = GetValue(values, "Address");
            if (!string.IsNullOrEmpty(addressData))
            {
                if (string.IsNullOrEmpty(street)) street = GetNestedValue(addressData, "Street");
                if (string.IsNullOrEmpty(houseNumber)) houseNumber = GetNestedValue(addressData, "HouseNumber");
                if (string.IsNullOrEmpty(postalCode)) postalCode = GetNestedValue(addressData, "PostalCode");
            }

            if (!string.IsNullOrEmpty(street) || !string.IsNullOrEmpty(year))
            {
                if (string.IsNullOrEmpty(street)) street = "Unknown";
                if (string.IsNullOrEmpty(year)) year = "Unknown";
                return $"Address: {street} {houseNumber} {postalCode}\nYear of construction: {year}";
            }

            if (!string.IsNullOrEmpty(name)) return $"Name: {name}";
            if (!string.IsNullOrEmpty(attributesData)) return "Data: " + attributesData;
            if (!string.IsNullOrEmpty(rawDataFallback)) return "Data: " + rawDataFallback;

            return "Object ID: " + GetValue(values, "id", "ID");
        }
#endif

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
            search = $"\"{key}\":";
            start = normalized.IndexOf(search);
            if (start != -1)
            {
                start += search.Length;
                int end = normalized.IndexOfAny(new char[] { ',', '}', ']' }, start);
                if (end != -1) return normalized.Substring(start, end - start).Trim(' ', '"');
            }
            return null;
        }

#if USING_CESIUM
        private string GetValue(Dictionary<string, CesiumMetadataValue> values, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (values.ContainsKey(key))
                {
                    string val = values[key].GetString("");
                    if (!string.IsNullOrEmpty(val)) return val;
                }
            }
            return null;
        }
#endif

        private void ShowUI(string text)
        {
            if (infoText != null) infoText.text = text;
            if (infoPanel != null) infoPanel.SetActive(true);
        }
    }
}