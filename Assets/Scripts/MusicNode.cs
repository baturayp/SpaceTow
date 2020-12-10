using System.Collections;
using System;
using UnityEngine;

public class MusicNode : MonoBehaviour
{
	[NonSerialized] public float startY;
	[NonSerialized] public float endY;
	[NonSerialized] public float removeLineY;
	[NonSerialized] public float beat;
	[NonSerialized] public int meteorPos;
	[NonSerialized] public bool paused;

	//adjust them accordingly to animations
	private float[] meteorFinalX = {0, 0, 0, 0, 2};
	private float[] meteorFinalY = {0, 0, 0, 0, 4};
	private float[] explosionXOffset = {0, 1, 1, 2, 2};
	private float[] explosionYOffset = {0, 1, 0, 0, -2};
	private MeteorNode meteorNode;
	private Vector3 explotionVector;
	private float aCos;
	private float metStartX, metStartY, metStartZ, metEndZ;
	private float expX, expY;
	private int trackNumber;


	public void Initialize(float posX, float startY, float endY, float removeLineY, float meteorStartLineZ, float meteorFinishLineZ, float targetBeat, MeteorNode meteor, int trackNumber)
	{
		this.startY = startY;
		this.endY = endY;
		this.beat = targetBeat;
		this.removeLineY = removeLineY;
		this.meteorNode = meteor;
		this.trackNumber = trackNumber;
		aCos = Mathf.Cos(targetBeat);
		paused = false;

		//make meteor appear at a predefined random point
		meteorPos = UnityEngine.Random.Range(1,5);
		metStartZ = meteorStartLineZ;
		metEndZ = meteorFinishLineZ;
		metStartX = trackNumber > 0 ? posX - meteorFinalX[meteorPos] : posX + meteorFinalX[meteorPos];
		metStartY = endY + meteorFinalY[meteorPos];

		//calculate explotion coordinates
		expX = trackNumber > 0 ? 0 - explosionXOffset[meteorPos] : explosionXOffset[meteorPos];
		expY = explosionYOffset[meteorPos];
		explotionVector = new Vector3(expX, expY, 0);

		//set position
		transform.position = new Vector3(posX, startY, 0);

		meteorNode.transform.position = new Vector3(metStartX, metStartY, metStartZ);

		//reset rotation
		transform.Rotate(0, 0, 0);
	}

	void Update()
	{
		if (Conductor.pauseTimeStamp > 0f) return; //resume not managed

		//remove itself when out of the screen (remove line)
		if (transform.position.y < removeLineY)
		{
			meteorNode.Destroy();
			gameObject.SetActive(false);
		}
		
		//avoid transforming when paused
		if (paused) return;

		//meteor position
		meteorNode.transform.position = new Vector3(metStartX, metStartY, metStartZ + (metEndZ - metStartZ) * (1f - ((beat) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)));

		//meteor rotation
		meteorNode.transform.Rotate(aCos,aCos,aCos, Space.Self);

		//node position
		transform.position = new Vector3(transform.position.x, startY + (endY - startY) * (1f - ((beat) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)), transform.position.z);
	}

	IEnumerator FadeOut()
	{
		yield return new WaitForSeconds(0.5f);
		meteorNode.Destroy();
		gameObject.SetActive(false);
	}

	public void PerfectHit()
	{
		paused = true;
		meteorNode.Explode(explotionVector);
		StartCoroutine(FadeOut());
	}

	public void GoodHit()
	{
		paused = true;
		meteorNode.Explode(explotionVector);
		StartCoroutine(FadeOut());
	}

	public void BadHit()
	{
		paused = true;
		meteorNode.Explode(explotionVector);
		StartCoroutine(FadeOut());
	}
}
