using UnityEngine;
using System.Collections.Generic;

namespace Rotterdam.DigitalTwins.Runtime
{
    /// <summary>
    /// Component to draw on meshes via raycast from the camera.
    /// Can be attached to the player controller.
    /// </summary>
    public class MeshPainter : MonoBehaviour
    {
        [Header("Raycast Settings")]
        public float range = 1000f;
        public LayerMask layerMask = ~0;

        [Header("Crosshair Settings")]
        public Color crosshairColor = Color.red;
        public float crosshairSize = 20f;
        public bool showCrosshair = true;

        [Header("Paint Settings")]
        public List<Color> paintColors = new List<Color> { Color.red, Color.green, Color.blue, Color.yellow, Color.white, Color.black };
        public int currentColorIndex = 0;
        public float lineWidth = 0.05f;
        public float minDistance = 0.02f;
        public float surfaceOffset = 0.01f;
        public Material paintMaterial;

        private Camera _playerCamera;
        private LineRenderer _currentLine;
        private List<Vector3> _points = new List<Vector3>();

        void Start()
        {
            InitializeCamera();
            Debug.Log("[MeshPainter] Started on " + gameObject.name);

            if (paintMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");

                if (shader != null)
                {
                    paintMaterial = new Material(shader);
                    paintMaterial.name = "PaintMaterial_Generated";
                }
                else
                {
                    Debug.LogWarning("[MeshPainter] Could not find a suitable shader for paintMaterial! Using 'Shader.Find(\"Standard\")' as a last resort.");
                    Shader standard = Shader.Find("Standard");
                    if (standard != null) paintMaterial = new Material(standard);
                }
            }
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
                Debug.LogError("[MeshPainter] No camera found!");
            }
        }

        void OnEnable()
        {
            InitializeCamera();
        }

        void Update()
        {
            if (_playerCamera == null) return;

            HandleColorSwitch();

            bool isPressing = false;
            bool startedThisFrame = false;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                isPressing = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
                startedThisFrame = UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
            }
#else
            isPressing = Input.GetMouseButton(0);
            startedThisFrame = Input.GetMouseButtonDown(0);
#endif

            if (startedThisFrame)
            {
                StartLine();
            }
            else if (isPressing && _currentLine != null)
            {
                UpdateLine();
            }
            else
            {
                _currentLine = null;
            }
        }

        private void HandleColorSwitch()
        {
            float scroll = 0;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                scroll = UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y;
            }
#else
            scroll = Input.GetAxis("Mouse ScrollWheel");
#endif

            if (scroll != 0 && paintColors.Count > 0)
            {
                if (scroll > 0) currentColorIndex++;
                else currentColorIndex--;

                if (currentColorIndex >= paintColors.Count) currentColorIndex = 0;
                if (currentColorIndex < 0) currentColorIndex = paintColors.Count - 1;

                Debug.Log($"[MeshPainter] Selected color index: {currentColorIndex}");
            }
        }

        private void StartLine()
        {
            Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, range, layerMask))
            {
                GameObject lineObj = new GameObject("PaintLine_" + System.DateTime.Now.Ticks);
                _currentLine = lineObj.AddComponent<LineRenderer>();
                
                _currentLine.startWidth = lineWidth;
                _currentLine.endWidth = lineWidth;
                _currentLine.useWorldSpace = true;
                _currentLine.material = paintMaterial;
                
                Color colorToUse = (paintColors != null && paintColors.Count > currentColorIndex && currentColorIndex >= 0) 
                    ? paintColors[currentColorIndex] 
                    : Color.red;

                _currentLine.startColor = colorToUse;
                _currentLine.endColor = colorToUse;

                if (paintMaterial != null)
                {
                    Material matInstance = new Material(paintMaterial);
                    if (matInstance.HasProperty("_Color"))
                        matInstance.color = colorToUse;
                    else if (matInstance.HasProperty("_BaseColor"))
                        matInstance.SetColor("_BaseColor", colorToUse);
                    
                    _currentLine.material = matInstance;
                }

                _currentLine.positionCount = 0;
                _currentLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                
                _points.Clear();
                AddPoint(hit.point + hit.normal * surfaceOffset);
            }
        }

        private void UpdateLine()
        {
            Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, range, layerMask))
            {
                Vector3 newPoint = hit.point + hit.normal * surfaceOffset;
                
                if (_points.Count == 0 || Vector3.Distance(_points[_points.Count - 1], newPoint) > minDistance)
                {
                    AddPoint(newPoint);
                }
            }
        }

        private void AddPoint(Vector3 point)
        {
            _points.Add(point);
            _currentLine.positionCount = _points.Count;
            _currentLine.SetPosition(_points.Count - 1, point);
        }

        void OnGUI()
        {
            if (!showCrosshair || _playerCamera == null) return;

            Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
            
            Color activeColor = (paintColors != null && paintColors.Count > currentColorIndex && currentColorIndex >= 0) 
                ? paintColors[currentColorIndex] 
                : crosshairColor;
            
            GUI.color = activeColor;
            
            GUI.DrawTexture(new Rect(center.x - crosshairSize / 2, center.y - 1, crosshairSize, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x - 1, center.y - crosshairSize / 2, 2, crosshairSize), Texture2D.whiteTexture);

            if (paintColors.Count > 0 && currentColorIndex < paintColors.Count)
            {
                GUI.Label(new Rect(center.x + crosshairSize, center.y - 10, 200, 20), $"Paint Color [{currentColorIndex}]");
            }
        }
    }
}
