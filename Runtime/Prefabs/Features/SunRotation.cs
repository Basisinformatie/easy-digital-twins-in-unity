using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SunRotation : MonoBehaviour
{
    public Vector3 rotationSet;
    
    private void Update()
    {
        transform.Rotate(rotationSet, Space.World);
    }
    
}