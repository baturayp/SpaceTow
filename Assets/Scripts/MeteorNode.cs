using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MeteorNode : MonoBehaviour
{
    public GameObject meteorWhole;
    public Rigidbody[] meteorPieces;
    public Rigidbody wholeRigid;
    public LensFlare flare;
    private MeshRenderer wholeMesh;
    private MeshRenderer[] piecesMesh;
    private Material wholeMat;
    private float val;
    private int len;

    public void Initialize()
    {
        len = meteorPieces.Length;
        wholeMesh = meteorWhole.GetComponent<MeshRenderer>();
        wholeMat = wholeMesh.material;
        piecesMesh = new MeshRenderer[len];
        
        for (int i = 0; i < len; i++)
        {
            piecesMesh[i] = meteorPieces[i].transform.gameObject.GetComponent<MeshRenderer>();
        }

        val = 0;
        SetState(false);
        
        StartCoroutine(FlareUp());
    }

    void SetState(bool state)
    {
        for (int i = 0; i < len; i++)
        {
            meteorPieces[i].transform.gameObject.SetActive(state);
            piecesMesh[i].material.SetFloat("_FresnelPower", val);
        }
    }

    public void SetMaterial(float value)
    {
        wholeMat.SetFloat("_FresnelPower", value);
        val = value;
    }

    IEnumerator FlareUp()
    {
        meteorWhole.SetActive(false);
        flare.fadeSpeed = 5f;
        flare.enabled = true;
        yield return new WaitForSeconds(0.2f);
        meteorWhole.SetActive(true);
        flare.enabled = false;
    }

    public void Explode(Vector3 expVec, float force, float upwardsModifier, bool success)
    {
        Vector3 explosionPosition = meteorWhole.transform.position + expVec;

        if (success)
        {
            meteorWhole.SetActive(false);
            SetState(true);
            foreach (Rigidbody piece in meteorPieces)
            {
                piece.AddExplosionForce(force, explosionPosition, 5.0f, upwardsModifier, ForceMode.Impulse);
            }
        }
        else
        {
            wholeRigid.AddExplosionForce(force, explosionPosition, 5.0f, upwardsModifier, ForceMode.Impulse);
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
