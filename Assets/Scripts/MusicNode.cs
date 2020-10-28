using System.Collections;
using System;
using UnityEngine;

public class MusicNode : MonoBehaviour
{
	public TextMesh timesText;
	public SpriteRenderer ringSprite;
	public Color color;
	[NonSerialized] public float startY;
	[NonSerialized] public float endY;
	[NonSerialized] public float removeLineY;
	[NonSerialized] public float beat;
	[NonSerialized] public int times;
	[NonSerialized] public float duration;
	[NonSerialized] public bool paused;
	[NonSerialized] public bool restartedLong;
	[NonSerialized] public bool pressed;


	public void Initialize(float posX, float startY, float endY, float removeLineY, float posZ, float targetBeat, int times, float duration, Color color)
	{
		this.startY = startY;
		this.endY = endY;
		this.beat = targetBeat;
		this.times = times;
		this.duration = duration;
		this.removeLineY = removeLineY;
		this.color = color;

		paused = false;
		restartedLong = false;
		pressed = false;

		//set position
		transform.position = new Vector3(posX, startY, posZ);

		//set color
		ringSprite.color = color;

		//set scale
		transform.localScale = new Vector3(1, 1, 1);

		//reset rotation
		transform.Rotate(0, 0, 0);

		//set times
		if (times > 0) timesText.text = times.ToString();
		else timesText.text = "";
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
			gameObject.SetActive(false);
		}
		
		//avoid transforming when paused
		if (paused) return;

		transform.position = new Vector3(transform.position.x, startY + (endY - startY) * (1f - ((beat) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)), transform.position.z);
	}

	public void FadeOutRedirector()
	{
		StartCoroutine(FadeOut());
	}

	IEnumerator FadeOut()
	{
		float elapsedTime = 0.0f;
		Color c = ringSprite.color;
		while (elapsedTime < 0.2f)
		{
			elapsedTime += Time.deltaTime;
			c.a = 1.0f - Mathf.Clamp01(elapsedTime / 0.2f);
			ringSprite.color = c;
			transform.localScale = new Vector3(1 + elapsedTime, 1 + elapsedTime, 1);
			yield return null;
		}
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
		ringSprite.color = Color.green;
		times--;
		if (times == 0)
		{
			StartCoroutine(FadeOut());
			return true;
		}

		timesText.text = times.ToString();

		return false;
	}

	public void PerfectHit()
	{
		paused = true;
		if (duration == 0) ringSprite.color = Color.green;
		StartCoroutine(FadeOut());
	}

	public void GoodHit()
	{
		paused = true;
		if (duration == 0) ringSprite.color = Color.yellow;
		StartCoroutine(FadeOut());
	}

	public void BadHit()
	{
		paused = true;
		if (duration == 0) ringSprite.color = Color.red;
		StartCoroutine(FadeOut());
	}
}
