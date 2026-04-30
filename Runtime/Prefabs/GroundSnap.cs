using UnityEngine;

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
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            // Ignore controllers
            if (hit.transform.GetComponentInParent<FirstPersonController>() != null ||
                hit.transform.GetComponentInParent<ThirdPersonController>() != null ||
                hit.transform.GetComponentInParent<CustomCarController>() != null ||
                hit.transform.GetComponentInParent<CustomHelicopterController>() != null)
            {
                continue;
            }

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

    private void OnValidate()
    {
        if (autoSnap) Snap();
    }
}
