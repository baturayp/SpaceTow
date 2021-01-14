using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularMovement : MonoBehaviour
{
    float timeCounter = 0;
    float rotVal;
    Vector3 pos0;
    void Start()
    {
        pos0 = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Conductor.paused)
            return;

        timeCounter += Time.deltaTime*0.5f;
        rotVal = (Time.deltaTime);

        float x = Mathf.Cos(timeCounter)*100;
        float y = Mathf.Sin(timeCounter)*100;
        //float z = 0;

        transform.position = new Vector3(pos0.x + x, pos0.y + y, pos0.z);
        transform.Rotate(new Vector3(0, 0, -rotVal*50));

    }
}
