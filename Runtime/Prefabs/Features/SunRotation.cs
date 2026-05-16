using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SunRotation : MonoBehaviour
{
    public enum RotationMode { Continuous, SpecificTime }
    [Tooltip("Pick between continuous or  a specific time.")]
    public RotationMode mode;

    [Tooltip("Continuous mode: degrees per frame. X is speed. Y and Z determine axis orientation.")]
    public Vector3 rotationSet = new Vector3(-0.05f, 0, 0);
    
    [Range(-90, 90)]
    [Tooltip("Latitude affects the sun's arc. 0 is equator, 52 is Netherlands.")]
    public float latitude = -15f;

    [Range(0, 24)]
    [Tooltip("Time of day (0-24hours). Only used in 'SpecificTime' mode.")]
    public float timeOfDay = 12.0f;
    
    private void Update()
    {
        if (mode == RotationMode.Continuous)
        {
            float speed = rotationSet.x;
            RotateSun(speed);
        }
        else
        {
            UpdateSunPosition();
        }
    }

    private void OnValidate()
    {
        UpdateSunPosition();
    }

    private void UpdateSunPosition()
    {

        float angle = (timeOfDay - 6f) / 24f * 360f;
        ApplyRotation(angle);
    }

    private void RotateSun(float degrees)
    {
        Vector3 axis = GetRotationAxis();
        transform.Rotate(axis, degrees, Space.World);
    }

    private void ApplyRotation(float angle)
    {
        Quaternion latitudeRotation = Quaternion.Euler(latitude - 90, rotationSet.y, rotationSet.z);
        Vector3 sunDirection = latitudeRotation * Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
        
        transform.forward = -sunDirection; // Light points opposite to sun position
    }

    private Vector3 GetRotationAxis()
    {
        Quaternion latitudeRotation = Quaternion.Euler(latitude - 90, rotationSet.y, rotationSet.z);
        return latitudeRotation * Vector3.up;
    }
}