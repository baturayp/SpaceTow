using System.Collections;
using System;
using UnityEngine;

public class MusicNode : MonoBehaviour
{
	public TextMesh timesText;
	public GameObject timesTextBackground;
	public SpriteRenderer ringSprite;
	public LineRenderer longLineRenderer;
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
		longLineRenderer.enabled = false;
		ResetLongLinePosition();
		transform.position = new Vector3(posX, startY, posZ);

		//set color
		ringSprite.color = color;
		SetGradientColors(color, color, 1);

		//set scale
		transform.localScale = new Vector3(1, 1, 1);

		//reset rotation
		transform.Rotate(0, 0, 0);

		//set times
		if (times > 0)
		{
			timesText.text = times.ToString();
			timesTextBackground.SetActive(true);
			longLineRenderer.enabled = false;
		}
		else if (duration > 0)
		{
			timesTextBackground.SetActive(false);
			ringSprite.color = new Color(1, 1, 1, 0);
			longLineRenderer.enabled = true;
		}
		else
		{
			timesTextBackground.SetActive(false);
			longLineRenderer.enabled = false;
		}

	}

	void Update()
	{
		if (Conductor.pauseTimeStamp > 0f) return; //resume not managed

		//draw long note line
		if (duration > 0)
        {
			longLineRenderer.SetPosition(0, new Vector3(transform.position.x, startY + (endY - startY) * (1f - ((beat) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)), transform.position.z));
			longLineRenderer.SetPosition(1, new Vector3(transform.position.x, startY + (endY - startY) * (1f - ((beat + (0.2f / Conductor.tempo)) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)), transform.position.z));
			longLineRenderer.SetPosition(2, new Vector3(transform.position.x, startY + (endY - startY) * (1f - ((beat + duration - (0.2f / Conductor.tempo)) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)), transform.position.z));
			longLineRenderer.SetPosition(3, new Vector3(transform.position.x, startY + (endY - startY) * (1f - ((beat + duration) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)), transform.position.z));
		}

		if (restartedLong) //restarted long notes
        {
			transform.position = new Vector3(transform.position.x, startY + (endY - startY) * (1f - ((beat + duration) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)), transform.position.z);
		}

		//remove itself when out of the screen (remove line)
		if (transform.position.y < removeLineY)
		{
			gameObject.SetActive(false);
		}

		if (paused) return; //multi-times notes might be paused on the finish line

		transform.position = new Vector3(transform.position.x, startY + (endY - startY) * (1f - ((beat) - Conductor.songposition) / (Conductor.BeatsShownOnScreen / Conductor.tempo)), transform.position.z);
	}

	public void DeactivationRedirector(Color color)
    {
		if (duration > 0) StartCoroutine(FadeOutLong(color));
		else StartCoroutine(FadeOutSingle());
    }

	IEnumerator FadeOutSingle()
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

	IEnumerator FadeOutLong(Color color)
	{
		float elapsedTime = 0.0f;
		float alpha = 1.0f;
		while (elapsedTime < 0.2f)
		{
			elapsedTime += Time.deltaTime;
			alpha = 1.0f - Mathf.Clamp01(elapsedTime / 0.2f);
			SetGradientColors(color, color, alpha);
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
			DeactivationRedirector(Color.green);
			return true;
		}

		timesText.text = times.ToString();

		return false;
	}

	public void PerfectHit()
	{
		paused = true;
		if (duration == 0) ringSprite.color = Color.green;
		DeactivationRedirector(Color.green);
	}

	public void GoodHit()
	{
		paused = true;
		if (duration == 0) ringSprite.color = Color.yellow;
		DeactivationRedirector(Color.yellow);
	}

	public void BadHit()
	{
		paused = true;
		if (duration == 0) ringSprite.color = Color.red;
		DeactivationRedirector(Color.red);
	}

	void ResetLongLinePosition()
    {
		longLineRenderer.SetPosition(0, new Vector3(0, 0, 0));
		longLineRenderer.SetPosition(1, new Vector3(0, 0, 0));
		longLineRenderer.SetPosition(2, new Vector3(0, 0, 0));
		longLineRenderer.SetPosition(3, new Vector3(0, 0, 0));
	}

	public void SetGradientColors(Color color1, Color color2, float alpha)
    {
		Gradient gradient = new Gradient();
		GradientColorKey startcolor = new GradientColorKey(color1, 0);
		GradientColorKey endcolor = new GradientColorKey(color2, 1);
		GradientAlphaKey startalpha = new GradientAlphaKey(alpha, 0);
		GradientAlphaKey endalpha = new GradientAlphaKey(alpha, 1);
		gradient.colorKeys = new GradientColorKey[] { startcolor, endcolor };
		gradient.alphaKeys = new GradientAlphaKey[] { startalpha, endalpha };
		longLineRenderer.colorGradient = gradient;
	}
}
