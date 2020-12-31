using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MeteorNode : MonoBehaviour
{
    public GameObject meteorWhole;
    public Rigidbody[] meteorPieces;
    public Image flare;
    public GameObject canvas;
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
        StartCoroutine(Flared());
    }

    IEnumerator Flared()
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < 0.5f)
        {
            elapsedTime += Time.deltaTime;
            var fadeOut = Mathf.Lerp(1,0, elapsedTime/0.5f);
            Color imgCol = new Color(1,1,1, fadeOut);
            flare.color = imgCol;
            yield return null;
        }
        canvas.SetActive(false);
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

    public void Explode(Vector3 expVec)
    {
        meteorWhole.SetActive(false);
        SetState(true);
        Vector3 explosionPosition = meteorWhole.transform.position + expVec;
        foreach (Rigidbody piece in meteorPieces)
        {
            piece.AddExplosionForce(2.0f, explosionPosition, 5.0f, 0f, ForceMode.Impulse);
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
