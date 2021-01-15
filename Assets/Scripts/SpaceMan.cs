using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
	public Animator spaceMan;
	private Coroutine routine;
	private readonly float crossFade = 0.1f;

	//animation frame values
	private readonly float[] attackStart = { 0.16f, 0.2f };
	private readonly float[] attackSuccess = { 0.4f, 0.6f };
	private readonly float[] attackFailed = { 0.8f, 1f };
	private readonly int[] backPosition = { 0, 1, 1, 1, 2, 3, 3, 4, 4, 4, 4, 2 };

	//successful punch animation
	public void Punch(int animNumber, int trackNumber, float targetBeat, bool success)
	{
		//if (routine != null) StopCoroutine(routine);
		//routine = StartCoroutine(PunchRoutine(targetBeat, Conductor.songposition, 0.2f, animNumber, trackNumber, success));
	}

	//delayed punch
	public void DelayedPunch(int animNumber, int trackNumber)
	{
		//if (routine != null) StopCoroutine(routine);
		//routine = StartCoroutine(DelayedPunchRoutine(Conductor.songposition + 0.2f, Conductor.songposition, animNumber, trackNumber));
	}

	//when obstacles present play nudge animation
	public void Nudge()
	{
		//
	}

	public void Empty()
	{
		//
	}

	//target is too far, don't play full punch clip
	public void IsTooFar(int trackNumber)
	{
		//
	}

	//an obstacle got hit on spaceman, considering character's position
	public void GotHit(int trackNumber)
	{
		//
	}

	//take avoid position
	public void Avoid(int trackNumber)
	{
		//if (routine != null) StopCoroutine(routine);
		routine = StartCoroutine(AvoidRoutine(trackNumber));
	}

	IEnumerator PunchRoutine(float targetBeat, float punchStarted, float backDuration, int animNum, int trackNumber, bool success)
	{
		string animToPlay = animNum.ToString() + trackNumber.ToString();
		spaceMan.CrossFadeInFixedTime(animToPlay, crossFade, 0);
		while (Conductor.songposition < targetBeat)
		{
			var punchTime = Mathf.Lerp(attackStart[0], attackStart[1], (Conductor.songposition - punchStarted) / (targetBeat - punchStarted));
			spaceMan.SetFloat("punchTime", punchTime);
			yield return null;
		}
		float elapsedTime = 0.0f;
		while (elapsedTime < backDuration)
		{
			elapsedTime += Time.deltaTime;
			var punchTime = Mathf.SmoothStep(success ? attackSuccess[0] : attackFailed[0], success ? attackSuccess[1] : attackFailed[1], elapsedTime / backDuration);
			spaceMan.SetFloat("punchTime", punchTime);
			yield return null;
		}
		spaceMan.SetFloat("punchTime", 0f);
		routine = null;
		spaceMan.Play("idle", 0);
	}

	IEnumerator DelayedPunchRoutine(float targetBeat, float punchStarted, int animNum, int trackNumber)
	{
		string animToPlay = backPosition[animNum].ToString() + trackNumber.ToString() + "9";
		spaceMan.Play(animToPlay, 0);
		while (Conductor.songposition < targetBeat)
		{
			var punchTime = Mathf.Lerp(0f, 1f, (Conductor.songposition - punchStarted) / (targetBeat - punchStarted));
			spaceMan.SetFloat("punchTime", punchTime);
			yield return null;
		}
		spaceMan.SetFloat("punchTime", 0f);
		routine = null;
		spaceMan.Play("idle", 0);
	}

	IEnumerator AvoidRoutine(int trackNumber)
	{
		string animToPlay = trackNumber.ToString();
		spaceMan.CrossFadeInFixedTime(animToPlay, crossFade, 0);
		yield return new WaitUntil(() => Conductor.avoidPos != trackNumber);
		animToPlay = trackNumber.ToString() + "B";
		spaceMan.CrossFadeInFixedTime(animToPlay, crossFade, 0);
		routine = null;
	}

}
