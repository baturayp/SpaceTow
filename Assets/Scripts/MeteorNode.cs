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
    private MeshRenderer wholeMesh;
    private MeshRenderer[] piecesMesh;
    private Material wholeMat;
    private float val;
    private int len;
    private bool paused;
    private int meteorPos;
    private float beat;

    //different meteor target points
    private readonly float[] meteorFinalX = {0, 1f, 1f, 1.1f, 1f, 0.57f, 0.57f, 0.7f, 0.75f, 0.83f, 0.81f, 1.07f};
	private readonly float[] meteorFinalY = {0, 0.42f, 0.42f, 0.42f, 0.97f, 0.5f, 0.5f, 0.5f, 0.3f, 0.47f, 0.67f, 1.05f};
	private readonly float[] explosionXOffset = {0, 0.1f, 0.1f, 0.2f, 0.2f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.2f};
	private readonly float[] explosionYOffset = {0, -0.1f, 0, 0, 0f, 0, 0.1f, 0, 0, 0.1f, -0.1f, 0.2f};
    private Vector3 explotionVector;
    private float aCos;
	private float metStartX, metStartY, metStartZ, metEndZ;
	private float expX, expY;
	private readonly float initYMultiplier = 4f;
    private GameObject towTruck;
    private bool towTruckShaking;
    private Vector3 towTruckInitial = new Vector3(0,0,-0.5f);

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
		float initPos = metStartX * initYMultiplier;
		transform.position = new Vector3(initPos , metStartY, metStartZ);
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

    public void Explode(bool success)
    {
        StartCoroutine(ExplosionRoutine(success));
    }

    void Update()
    {
        if (Conductor.pauseTimeStamp > 0f) return;

        if (towTruckShaking)
		{
			towTruck.transform.position = towTruckInitial + UnityEngine.Random.insideUnitSphere * 0.01f;
		}

        if (paused) return;

        transform.position = new Vector3(Mathf.LerpUnclamped(metStartX, metStartX * initYMultiplier, (beat - Conductor.songposition) / (Conductor.appearTime)), 
														metStartY, 
														Mathf.LerpUnclamped(metEndZ, metStartZ, (beat - Conductor.songposition) / (Conductor.appearTime)));

		//meteor rotate itself around
		transform.Rotate(aCos,aCos,aCos, Space.Self);

        //scale up meteors as they get closer
		if (Conductor.songposition > beat - 1f && Conductor.songposition < beat - 0.5f)
		{
			float ls = 0.1f / (beat - Conductor.songposition);
			transform.localScale = new Vector3(ls, ls, ls);
		}

		//make meteors glow
		if (Conductor.songposition > beat - Conductor.HitOffset)
		{
			SetMaterial(1f * (1f - ((beat) - Conductor.songposition) / Conductor.HitOffset));
		}

        if (Conductor.songposition > beat + 0.15f)
		{
			Explode(false);
		}
    }
    
    IEnumerator ExplosionRoutine(bool success)
	{
		yield return new WaitUntil(() => Conductor.songposition >= beat);
		paused = true;

		Vector3 explosionPosition = transform.position + explotionVector;
		
        if (success)
        {
            meteorWhole.SetActive(false);
            SetState(true);
            foreach (Rigidbody piece in meteorPieces)
            {
                piece.AddExplosionForce(2.0f, explosionPosition, 5.0f, 0f, ForceMode.Impulse);
            }
        }

		if (!success)
        {
            Handheld.Vibrate();
            towTruckShaking = true;
            wholeRigid.AddExplosionForce(10f, explosionPosition, 5.0f, 2f, ForceMode.Impulse);
        }
        
        //make meteors even smaller as they exploding
		float elapsedTime = 0.0f;
		
        while (elapsedTime < 0.4f)
		{
			elapsedTime += Time.deltaTime;
			float ls = Mathf.Lerp(0.2f, 0.1f, elapsedTime / 0.4f);
			Vector3 vs = new Vector3(ls, ls, ls);
			transform.localScale = success ? vs : new Vector3(0.2f,0.2f,0.2f);
			yield return null;
		}
        
        towTruckShaking = false;
		towTruck.transform.position = towTruckInitial;
        Destroy(gameObject);
	}
}
