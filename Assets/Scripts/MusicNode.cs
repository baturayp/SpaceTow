using System.Collections;
using System;
using UnityEngine;

public class MusicNode : MonoBehaviour
{
	[NonSerialized] public float beat;
	[NonSerialized] public int meteorPos;
	[NonSerialized] public bool isObstacle;
	[NonSerialized] public bool paused;
	[NonSerialized] public int trackNumber;
	private GameObject towTruck;
	private bool towTruckShaking;
	private Vector3 towTruckInitialPos;

	//adjust them accordingly to animations
	private float[] meteorFinalX = {0, 0.7f, 0.7f, 0.8f, 0.9f, 0.35f, 0.45f, 0.5f, 0.5f, 0.6f, 0.5f, 0.75f};
	private float[] meteorFinalY = {0, 0.3f, 0.25f, 0.3f, 0.9f, 0.45f, 0.35f, 0.4f, 0.25f, 0.45f, 0.70f, 0.85f};
	private float[] explosionXOffset = {0, 0.1f, 0.1f, 0.2f, 0.2f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.2f};
	private float[] explosionYOffset = {0, -0.1f, 0, 0, 0.2f, 0, 0.1f, 0, 0, 0.1f, -0.1f, 0};
	private MeteorNode meteorNode;
	private Vector3 explotionVector;
	private float aCos;
	private float metStartX, metStartY, metStartZ, metEndZ;
	private float expX, expY;
	private float initYMultiplier = 4f;


	public void Initialize(float meteorStartLineZ, float meteorFinishLineZ, float targetBeat, MeteorNode meteor, int trackNumber)
	{
		this.beat = targetBeat;
		this.meteorNode = meteor;
		this.trackNumber = trackNumber;
		aCos = Mathf.Cos(targetBeat);
		paused = false;
		
		towTruck = GameObject.FindGameObjectWithTag("towtruck");
		towTruckInitialPos = towTruck.transform.position;
		
		//make meteor appear at a predefined random point
		meteorPos = UnityEngine.Random.Range(1,12);
		metStartZ = meteorStartLineZ;
		metEndZ = meteorFinishLineZ;
		metStartX = trackNumber > 0 ? 0 + meteorFinalX[meteorPos] : 0 - meteorFinalX[meteorPos];
		metStartY = meteorFinalY[meteorPos];

		//calculate explotion coordinates
		expX = trackNumber > 0 ? 0 - explosionXOffset[meteorPos] : explosionXOffset[meteorPos];
		expY = 0 - explosionYOffset[meteorPos];
		explotionVector = new Vector3(expX, expY, 0);

		meteorNode.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		float initPos = metStartX * (1 + ((beat - Conductor.songposition) * initYMultiplier));
		meteorNode.transform.localPosition = new Vector3(initPos , metStartY, metStartZ);

		//reset rotation
		transform.Rotate(0, 0, 0);

		//overwrite variables if its an obstacle
		if (trackNumber > 1) 
		{
			isObstacle = true;
			meteorPos = UnityEngine.Random.Range(1,3);
			metStartX = trackNumber > 2 ? 0.2f : -0.2f;
			expX = trackNumber > 2 ? -0.2f : 0.2f;
			explotionVector = new Vector3(expX, 0, 0);
			float initPosNew = metStartX * (1 + ((beat - Conductor.songposition) * initYMultiplier));
			meteorNode.transform.localPosition = new Vector3(initPosNew , metStartY, metStartZ);
		}
	}

	void Update()
	{
		if (Conductor.pauseTimeStamp > 0f) return; //resume not managed

		if (towTruckShaking && !isObstacle)
		{
			towTruck.transform.position = towTruckInitialPos + UnityEngine.Random.insideUnitSphere * 0.01f;
		}

		//avoid transforming when paused
		if (paused) return;
		
		//missed the punch
		if (Conductor.songposition > beat + Conductor.hitOffset)
		{
			Explode(10f, 2f, false);
		}

		//main meteor transform
		//meteorNode.transform.localPosition = new Vector3(metStartX * (1 + ((beat - Conductor.songposition) * initYMultiplier)), metStartY, metStartZ + (metEndZ - metStartZ) * (1f - ((beat) - Conductor.songposition) / (Conductor.appearTime)));
		
		meteorNode.transform.localPosition = new Vector3(Mathf.Lerp(metStartX, metStartX * initYMultiplier, (beat - Conductor.songposition) / (Conductor.appearTime)), 
														metStartY, 
														Mathf.LerpUnclamped(metEndZ, metStartZ, (beat - Conductor.songposition) / (Conductor.appearTime)));
		
		//meteor rotate itself around
		meteorNode.transform.Rotate(aCos,aCos,aCos, Space.Self);

		//scale up meteors as they get closer
		if (Conductor.songposition > beat - 1f && Conductor.songposition < beat - 0.5f)
		{
			float ls = 0.1f / (beat - Conductor.songposition);
			meteorNode.transform.localScale = new Vector3(ls, ls, ls);
		}

		//make meteors glow
		if (Conductor.songposition > beat - Conductor.hitOffset && !isObstacle)
		{
			meteorNode.SetMaterial(1f * (1f - ((beat) - Conductor.songposition) / Conductor.hitOffset));
		}
	}

	IEnumerator ExplosionRoutine(float force, float upwardsModifier, bool successHit)
	{
		yield return new WaitUntil(() => Conductor.songposition >= beat);
		paused = true;
		meteorNode.Explode(explotionVector, force, upwardsModifier, successHit);
		if (!successHit) towTruckShaking = true;
		if (!successHit) Handheld.Vibrate();
		//make meteors even smaller as they exploding
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.4f)
		{
			elapsedTime += Time.deltaTime;
			float ls = Mathf.Lerp(0.2f, 0.1f, elapsedTime / 0.4f);
			Vector3 vs = new Vector3(ls, ls, ls);
			meteorNode.transform.localScale = successHit ? vs : new Vector3(0.2f,0.2f,0.2f);
			yield return null;
		}
		towTruckShaking = false;
		towTruck.transform.position = towTruckInitialPos;
		meteorNode.Destroy();
		gameObject.SetActive(false);
	}

	public void Explode(float force, float upwardsModifier, bool successHit)
	{
		StartCoroutine(ExplosionRoutine(force, upwardsModifier, successHit));
	}
}
