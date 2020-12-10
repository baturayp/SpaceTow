using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerTransporter : MonoBehaviour
{

    Vector3 myPos;
    public GameObject camEra;

    // Start is called before the first frame update
    void Start()
    {
        myPos = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (camEra.transform.position.z > myPos.z +10)
        {
            myPos.z += 70; 
            gameObject.transform.position = myPos;
        }
    }
}
