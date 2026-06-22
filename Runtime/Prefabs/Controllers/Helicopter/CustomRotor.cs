using UnityEngine;

namespace Rotterdam.DigitalTwins.Runtime
{
    /// <summary>
    /// Component representing a rotating helicopter rotor.
    /// </summary>
    public class CustomRotor : MonoBehaviour
    {
    public enum Axis { X, Y, Z }
    public Axis rotationAxis = Axis.Y;
    public float rotationSpeed = 1000f;
    public bool invertRotation = false;

    void Update()
    {
        float angle = rotationSpeed * Time.deltaTime * (invertRotation ? -1f : 1f);
        switch (rotationAxis)
        {
            case Axis.X:
                transform.Rotate(Vector3.right, angle, Space.Self);
                break;
            case Axis.Y:
                transform.Rotate(Vector3.up, angle, Space.Self);
                break;
            case Axis.Z:
                transform.Rotate(Vector3.forward, angle, Space.Self);
                break;
        }
    }
}
}
