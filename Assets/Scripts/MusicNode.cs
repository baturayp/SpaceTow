using System.Collections;
using System;
using UnityEngine;

public class MusicNode : MonoBehaviour
{
	[NonSerialized] public float startY;
	[NonSerialized] public float endY;
	[NonSerialized] public float removeLineY;
	[NonSerialized] public float beat;
	[NonSerialized] public int times;
	[NonSerialized] public float duration;
	[NonSerialized] public bool paused;
	[NonSerialized] public bool restartedLong;
	[NonSerialized] public bool pressed;
	private MeteorNode meteorNode;
	private float aCos;
	private float startZ, endZ;


	public void Initialize(float posX, float startY, float endY, float removeLineY, float startLineZ, float finishLineZ, float posZ, float targetBeat, int times, float duration, MeteorNode meteor)
	{
		this.startY = startY;
		this.endY = endY;
		this.beat = targetBeat;
		this.times = times;
		this.duration = duration;
		this.removeLineY = removeLineY;
		this.startZ = startLineZ;
		this.endZ = finishLineZ;
		this.meteorNode = meteor;
		aCos = Mathf.Cos(targetBeat);

		paused = false;
		restartedLong = false;
		pressed = false;

		//set position
		transform.position = new Vector3(posX, startY, posZ);

		meteorNode.transform.position = new Vector3(posX, endY, startLineZ);

		//set scale
		transform.localScale = new Vector3(1, 1, 1);

		//reset rotation
		transform.Rotate(0, 0, 0);
	}

	void Update()
	{
		if (Conductor.pauseTimeStamp > 0f) return; //resume not managed

		if (restartedLong) //restarted long notes
        {
			transform.position = new Vector3(transform.position.x, startY + (endY - startY) * (1f - ((beat + duration) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)), transform.position.z);
		}

		//remove itself when out of the screen (remove line)
		if (transform.position.y < removeLineY)
		{
			meteorNode.Destroy();
			gameObject.SetActive(false);
		}
		
		//avoid transforming when paused
		if (paused) return;

		//meteor position
		meteorNode.transform.position = new Vector3(transform.position.x, endY, startZ + (endZ - startZ) * (1f - ((beat) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)));

		//meteor rotation
		meteorNode.transform.Rotate(aCos,aCos,aCos, Space.Self);

		//node position
		transform.position = new Vector3(transform.position.x, startY + (endY - startY) * (1f - ((beat) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)), transform.position.z);
	}

	public void FadeOutRedirector()
	{
		StartCoroutine(FadeOut());
	}

	IEnumerator FadeOut()
	{
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.5f)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		meteorNode.Destroy();
		gameObject.SetActive(false);
	}

	//remove (multi-times note failed), might apply some animations later
	public void MultiTimesFailed()
	{
		gameObject.SetActive(false);
	}

	//if the node is removed, return true
	public bool MultiTimesHit()
	{
		//update text
		times--;
		if (times == 0)
		{
			StartCoroutine(FadeOut());
			return true;
		}
		return false;
	}

	public void PerfectHit()
	{
		paused = true;
		meteorNode.Explode();
		StartCoroutine(FadeOut());
	}

	public void GoodHit()
	{
		paused = true;
		meteorNode.Explode();
		StartCoroutine(FadeOut());
	}

	public void BadHit()
	{
		paused = true;
		meteorNode.Explode();
		StartCoroutine(FadeOut());
	}
}
