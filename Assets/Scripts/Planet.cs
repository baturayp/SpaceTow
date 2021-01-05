using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public GameObject planet;
    private float scaleFactor;
    
    void Update()
    {
        scaleFactor = Conductor.songposition < 60f ? 4f : (Conductor.songposition / 60f) * 4f;
        planet.transform.localScale = new Vector3(scaleFactor,scaleFactor,scaleFactor);
    }
}
