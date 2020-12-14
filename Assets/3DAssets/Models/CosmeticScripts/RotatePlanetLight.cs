using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlanetLight : MonoBehaviour
{
    public RectTransform myRectTransform;
    private float rotVal;


    void Update()
    {
        rotVal = (Time.deltaTime * 30)%360;


        myRectTransform.Rotate(new Vector3 (0,0, rotVal));
    }
}
