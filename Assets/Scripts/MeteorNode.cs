using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MeteorNode : MonoBehaviour
{
    public GameObject meteorWhole;
    public Rigidbody[] meteorPieces;

    public void Initialize()
    {
        //nothing to do yet
    }

    public void Explode(Vector3 expVec)
    {
        meteorWhole.SetActive(false);
        Vector3 explosionPosition = meteorWhole.transform.position + expVec;
        foreach (Rigidbody piece in meteorPieces)
        {
            piece.AddExplosionForce(20.0f, explosionPosition, 5.0f, 0f, ForceMode.Impulse);
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
