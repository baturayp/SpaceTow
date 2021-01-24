using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlanetLight : MonoBehaviour
{
    public RectTransform myRectTransform;
    private float rotVal;
    private Vector3 rot0;

    public void Start()
    {
        rot0 = myRectTransform.transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (Conductor.paused)
            return;

        rotVal += (Time.deltaTime)*50;
        
        myRectTransform.transform.rotation = Quaternion.Euler(rot0.x, rot0.y, rotVal);
    }
}