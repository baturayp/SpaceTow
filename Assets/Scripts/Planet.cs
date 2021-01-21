using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public GameObject planet;
    public GameObject clouds;
    private float scaleFactor;
    private Quaternion rot;

    private void Update()
    {
        scaleFactor = Conductor.songposition < 60f ? 4f : (Conductor.songposition / 60f) * 4f;
        planet.transform.localScale = new Vector3(scaleFactor,scaleFactor,scaleFactor);

		if (clouds == null) return;
        //clouds rotation
        rot = Quaternion.Euler(0,Conductor.songposition*10,0);
        clouds.transform.rotation = rot;
    }
}