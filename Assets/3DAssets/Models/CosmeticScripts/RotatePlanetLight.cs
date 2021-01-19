using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlanetLight : MonoBehaviour
{
    public RectTransform myRectTransform;
    private float rotVal;
    private Quaternion rot;

    void Update()
    {
        rotVal = (Conductor.songposition*10);
        rot = Quaternion.Euler(0,0,rotVal);
        myRectTransform.transform.rotation = rot;
    }
}
