using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls the rotation of the sun (directional light) based on time of day or continuous rotation.
/// </summary>
public class SunRotation : MonoBehaviour
{
    public enum RotationMode { Continuous, SpecificTime }
    [Tooltip("Pick between continuous or  a specific time.")]
    public RotationMode mode;

    [Tooltip("Continuous mode: degrees per frame. Y and Z determine orientation and orbit.")]
    public Vector3 rotationSet;
    
    [Range(0, 24)]
    [Tooltip("Time of day (0-24hours). Only used in 'SpecificTime' mode.")]
    public float timeOfDay = 19.3f;
    
    private void Update()
    {
        if (mode == RotationMode.Continuous)
        {
            transform.Rotate(rotationSet, Space.World);
        }
        else
        {
            UpdateSunPosition();
        }
    }

    private void OnValidate()
    {
        if (mode == RotationMode.SpecificTime)
        {
            UpdateSunPosition();
        }
    }

    public void UpdateSunPosition()
    {
        float angle = ((timeOfDay-1.5f) / 24f) * 360f - 90f;
        transform.localEulerAngles = new Vector3(angle, rotationSet.y, rotationSet.z);
    }
}