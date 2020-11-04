using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUpAndDown : MonoBehaviour
{
    public GameObject DebrisLayer;
    public float movementOffSet;
    public float posY0;

    // Start is called before the first frame update
    void Start()
    {
        movementOffSet = Random.Range(0.0f,1.0f);
        posY0 = DebrisLayer.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        DebrisLayer.transform.position = new Vector3(DebrisLayer.transform.position.x, posY0 + (Mathf.SmoothStep(0, 0.2f, (Mathf.PingPong(Time.time*0.4f + movementOffSet, 1)))), DebrisLayer.transform.position.z);
    }
}
