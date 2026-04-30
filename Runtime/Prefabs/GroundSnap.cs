using UnityEngine;

namespace Rotterdam.DigitalTwins.Runtime
{
    [ExecuteAlways]
    public class GroundSnap : MonoBehaviour
    {
        [Header("Settings")]
        public bool autoSnap = true;
        public float offset = 0.01f;
        public LayerMask groundLayer = -1;
        
        [Header("Editor Only")]
        public bool snapInEditor = true;

        void Update()
        {
            if (!Application.isPlaying)
            {
                if (snapInEditor && autoSnap)
                {
                    Snap();
                }
            }
        }
        
        public void Snap()
        {
            Vector3 origin = transform.position;
            origin.y += 500f; 

            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 1000f, groundLayer);
            
            float highestY = float.MinValue;
            bool found = false;

            foreach (var hit in hits)
            {
                if (IsIgnored(hit.transform))
                    continue;

                if (hit.point.y > highestY)
                {
                    highestY = hit.point.y;
                    found = true;
                }
            }

            if (found)
            {
                Vector3 newPos = transform.position;
                newPos.y = highestY + offset;
                
                if (Mathf.Abs(transform.position.y - newPos.y) > 0.0001f)
                {
                    transform.position = newPos;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UnityEditor.EditorUtility.SetDirty(gameObject);
                    }
#endif
                }
            }
        }

        private bool IsIgnored(Transform t)
        {
            if (t == transform || t.IsChildOf(transform)) return true;
            Transform current = t;
            while (current != null)
            {
                string n = current.name;
                if (n.EndsWith("Rig", System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                current = current.parent;
            }

            return false;
        }

        private void OnValidate()
        {
            if (autoSnap) Snap();
        }
    }
}
