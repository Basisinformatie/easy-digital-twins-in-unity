using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        // 06:00 (Oost) -> X = 0
        // 12:00 (Middag) -> X = 90
        // 18:00 (West) -> X = 180
        // 24:00/00:00 (Middernacht) -> X = 270
        float angle = (timeOfDay / 24f) * 360f - 90f;
        
        // Y = 270 zorgt ervoor dat de zon van Oost (+X) naar West (-X) draait over de X-as.
        float yRotation = rotationSet.y != 0 ? rotationSet.y : 270f;
        transform.localEulerAngles = new Vector3(angle, yRotation, rotationSet.z);
    }
}