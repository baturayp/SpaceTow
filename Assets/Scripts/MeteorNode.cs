using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MeteorNode : MonoBehaviour
{
    public GameObject meteorWhole;
    public GameObject[] meteorPieces;

    public void Initialize()
    {
        //nothing to do yet
    }

    public void BreakUpRedirector()
    {
        //redirect
    }

    IEnumerator BreakUp()
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < 0.8f)
        {
            elapsedTime += Time.deltaTime;
            //move pieces
            yield return null;
        }
    }
}
