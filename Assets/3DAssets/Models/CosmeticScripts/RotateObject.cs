using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 1;
    private float rotVal;


    void Update()
    {
        rotVal = (Time.deltaTime * rotationSpeed) %360;

        transform.Rotate(new Vector3 (0, rotVal, 0));

    }
}
