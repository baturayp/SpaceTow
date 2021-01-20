using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUpAndDown : MonoBehaviour
{
    public float startZ;
    public float endZ;
    public float fullScalePoint;
    private Vector3 initialPosition;
    
    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (Conductor.pauseTimeStamp > 0f) return;
        //DebrisLayer.transform.position = new Vector3(DebrisLayer.transform.position.x, posY0 + (Mathf.SmoothStep(0, upDownAmount, (Mathf.PingPong(Time.time*0.4f + movementOffSet, 1)))), DebrisLayer.transform.position.z);
        //transform.localPosition -= new Vector3(0, Mathf.SmoothStep(-0.02f, 0.02f, (Mathf.PingPong(Time.time*0.4f + movementOffSet, 1))),0.1f);
        if (transform.position.z <= endZ)
        {
            transform.position = new Vector3(initialPosition.x, initialPosition.y, startZ);
        }

        // if (transform.position.z > fullScalePoint)
        // {
        //     transform.localScale = new Vector3(1/(transform.position.z - fullScalePoint), 1/(transform.position.z - fullScalePoint), 1/(transform.position.z - fullScalePoint));
        // }
        // else
        // {
        //     transform.localScale = new Vector3(1,1,1);
        // }

        transform.position -= new Vector3(0, Mathf.SmoothStep(-0.02f, 0.02f, (Mathf.PingPong(Time.time, 1))), 0.3f);
    }
}
