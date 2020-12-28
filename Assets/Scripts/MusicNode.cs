using System.Collections;
using System;
using UnityEngine;

public class MusicNode : MonoBehaviour
{
	[NonSerialized] public float beat;
	[NonSerialized] public int meteorPos;
	[NonSerialized] public bool paused;

	//adjust them accordingly to animations
	private float[] meteorFinalX = {0, 7, 7, 7, 6, 4, 4, 5, 6, 5, 6, 6};
	private float[] meteorFinalY = {0, -5, -5, -4.5f, 0, -3.5f, -3.5f, -4.5f, -4.5f, -3.5f, -3.5f, -0.5f};
	private float[] explosionXOffset = {0, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 2};
	private float[] explosionYOffset = {0, -1, 0, 0, 2, 0, 1, 0, 0, 1, -1, 0};
	private MeteorNode meteorNode;
	private Vector3 explotionVector;
	private float aCos;
	private float metStartX, metStartY, metStartZ, metEndZ;
	private float expX, expY;
	private int trackNumber;


	public void Initialize(float meteorStartLineZ, float meteorFinishLineZ, float targetBeat, MeteorNode meteor, int trackNumber)
	{
		this.beat = targetBeat;
		this.meteorNode = meteor;
		this.trackNumber = trackNumber;
		aCos = Mathf.Cos(targetBeat);
		paused = false;

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

		meteorNode.transform.localPosition = new Vector3(metStartX, metStartY, metStartZ);

		//reset rotation
		transform.Rotate(0, 0, 0);
	}

	void Update()
	{
		if (Conductor.pauseTimeStamp > 0f) return; //resume not managed

		//remove itself when out of the screen (remove line)
		if (Conductor.songposition > beat + 1.0f)
		{
			meteorNode.Destroy();
			gameObject.SetActive(false);
		}
		
		//avoid transforming when paused
		if (paused) return;

		//meteor position
		meteorNode.transform.localPosition = new Vector3(metStartX, metStartY, metStartZ + (metEndZ - metStartZ) * (1f - ((beat) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)));

		//meteor rotation
		meteorNode.transform.Rotate(aCos,aCos,aCos, Space.Self);
	}

	IEnumerator FadeOut()
	{
		yield return new WaitUntil(() => Conductor.songposition > beat);
		paused = true;
		meteorNode.Explode(explotionVector);
		yield return new WaitForSeconds(0.5f);
		meteorNode.Destroy();
		gameObject.SetActive(false);
	}

	public void PerfectHit()
	{
		StartCoroutine(FadeOut());
	}

	public void GoodHit()
	{
		StartCoroutine(FadeOut());
	}

	public void BadHit()
	{
		StartCoroutine(FadeOut());
	}
}
