using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    GameObject myCamera;
    void Start()
    {
        myCamera = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        myCamera.transform.position += new Vector3(0,0,Time.deltaTime*5);
    }
}
