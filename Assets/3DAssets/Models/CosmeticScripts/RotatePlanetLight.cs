using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlanetLight : MonoBehaviour
{
    public RectTransform myRectTransform;
    public LensFlare flare;
    private float rotVal;
    private Quaternion rot;

    void Start()
    {
        InvokeRepeating("LightLoop", 0, 0.5f);
    }

    void LightLoop()
    {
        flare.enabled = flare.enabled == false ? true : false;
    }
    void Update()
    {
        rotVal = (Conductor.songposition*10);
        rot = Quaternion.Euler(0,0,rotVal);
        myRectTransform.transform.rotation = rot;
    }
}
