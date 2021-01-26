using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public GameObject planet;
    public GameObject clouds;
    private float scaleFactor;
    private float verticalCor;
    private Quaternion rot;
    private float songChangeRate;

    private void Update()
    {
        if (Conductor.songposition < 60f)
        {
            scaleFactor = 3f;
            verticalCor = 0f;
        }
        else
        {
            songChangeRate = (Conductor.songposition) / 60f;
            scaleFactor = songChangeRate  * 3f;
            verticalCor = ((songChangeRate) * -10f) +10;
        }

        if (Conductor.songposition > 180f)
        {
            scaleFactor = 9f;
            verticalCor = -20;
        }

        planet.transform.localScale = new Vector3(scaleFactor,scaleFactor,scaleFactor);
        planet.transform.localPosition = new Vector3(0, verticalCor, 390);
        

		if (clouds == null) return;
        //clouds rotation
        rot = Quaternion.Euler(0,Conductor.songposition*10,0);
        clouds.transform.rotation = rot;
    }
}