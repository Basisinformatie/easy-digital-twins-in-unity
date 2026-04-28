using UnityEngine;

public class CustomHelicopterCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 0.125f;
    public float rotationSmoothSpeed = 0.1f;

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.TransformPoint(offset);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position, target.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed);
    }
}
