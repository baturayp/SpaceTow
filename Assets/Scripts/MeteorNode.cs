﻿using System.Collections;
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
    private bool paused;
    private int meteorPos;
    private float beat;

    //different meteor target points
    private readonly float[] meteorFinalX = {0, 0.7f, 0.7f, 0.8f, 0.9f, 0.35f, 0.45f, 0.5f, 0.5f, 0.6f, 0.5f, 0.75f};
	private readonly float[] meteorFinalY = {0, 0.3f, 0.25f, 0.3f, 0.9f, 0.45f, 0.35f, 0.4f, 0.25f, 0.45f, 0.70f, 0.85f};
	private readonly float[] explosionXOffset = {0, 0.1f, 0.1f, 0.2f, 0.2f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.2f};
	private readonly float[] explosionYOffset = {0, -0.1f, 0, 0, 0.2f, 0, 0.1f, 0, 0, 0.1f, -0.1f, 0};
    private Vector3 explotionVector;
    private float aCos;
	private float metStartX, metStartY, metStartZ, metEndZ;
	private float expX, expY;
	private readonly float initYMultiplier = 4f;
    private GameObject towTruck;
    private bool towTruckShaking;
    private Vector3 towTruckInitial = new Vector3(0,0,-0.85f);

    public void Initialize(float startLineZ, float finishLineZ, float targetBeat, int meteorPos, int trackNumber)
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

		towTruck = GameObject.FindGameObjectWithTag("towtruck");

        aCos = Mathf.Cos(targetBeat);
		paused = false;

        beat = targetBeat;
		
		//make meteor appear at a predefined random point
		this.meteorPos = meteorPos;
		metStartZ = startLineZ;
		metEndZ = finishLineZ;
		metStartX = trackNumber > 0 ? 0 + meteorFinalX[meteorPos] : 0 - meteorFinalX[meteorPos];
		metStartY = meteorFinalY[meteorPos];

		//calculate explotion coordinates
		expX = trackNumber > 0 ? 0 - explosionXOffset[meteorPos] : explosionXOffset[meteorPos];
		expY = 0 - explosionYOffset[meteorPos];
		explotionVector = new Vector3(expX, expY, 0);

		transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		float initPos = metStartX * (1 + ((beat - Conductor.songposition) * initYMultiplier));
		transform.localPosition = new Vector3(initPos , metStartY, metStartZ);

		//reset rotation
		transform.Rotate(0, 0, 0);

        //flare
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

    void SetMaterial(float value)
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

    public void Explode(bool success)
    {
        Vector3 explosionPosition = meteorWhole.transform.position + explotionVector;
        if (success)
        {
            meteorWhole.SetActive(false);
            SetState(true);
            foreach (Rigidbody piece in meteorPieces)
            {
                piece.AddExplosionForce(2f, explosionPosition, 5.0f, 2f, ForceMode.Impulse);
            }
        }
        else
        {
            wholeRigid.AddExplosionForce(10f, explosionPosition, 5.0f, 2f, ForceMode.Impulse);
        }
    }

    void Update()
    {
        if (paused) return;

        transform.localPosition = new Vector3(Mathf.Lerp(metStartX, metStartX * initYMultiplier, (beat - Conductor.songposition) / (Conductor.appearTime)), 
														metStartY, 
														Mathf.LerpUnclamped(metEndZ, metStartZ, (beat - Conductor.songposition) / (Conductor.appearTime)));
		
        if (towTruckShaking)
		{
			towTruck.transform.position = towTruckInitial + UnityEngine.Random.insideUnitSphere * 0.01f;
		}

		//meteor rotate itself around
		transform.Rotate(aCos,aCos,aCos, Space.Self);

        //scale up meteors as they get closer
		if (Conductor.songposition > beat - 1f && Conductor.songposition < beat - 0.5f)
		{
			float ls = 0.1f / (beat - Conductor.songposition);
			transform.localScale = new Vector3(ls, ls, ls);
		}

		//make meteors glow
		if (Conductor.songposition > beat - Conductor.hitOffset)
		{
			SetMaterial(1f * (1f - ((beat) - Conductor.songposition) / Conductor.hitOffset));
		}

    }
    IEnumerator ExplosionRoutine(bool successHit)
	{
		yield return new WaitUntil(() => Conductor.songposition >= beat);
		paused = true;
		Explode(successHit);
		if (!successHit) towTruckShaking = true;
		if (!successHit) Handheld.Vibrate();
		//make meteors even smaller as they exploding
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.4f)
		{
			elapsedTime += Time.deltaTime;
			float ls = Mathf.Lerp(0.2f, 0.1f, elapsedTime / 0.4f);
			Vector3 vs = new Vector3(ls, ls, ls);
			transform.localScale = successHit ? vs : new Vector3(0.2f,0.2f,0.2f);
			yield return null;
		}
        towTruckShaking = false;
		towTruck.transform.position = towTruckInitial;
        Destroy();
	}

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
