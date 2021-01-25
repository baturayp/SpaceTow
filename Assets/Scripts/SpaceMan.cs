using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
	public Animator spaceMan;
	//private Coroutine routine;
	private const float CrossFade = 0.1f;
	private readonly int[] backPosition = { 0, 1, 1, 1, 2, 3, 3, 4, 4, 4, 4, 2 };
	private Coroutine punchRunning;

	private void Update()
	{
		spaceMan.enabled = !Conductor.paused;
	}

	//successful punch animation
	public void Punch(int animNumber, int trackNumber, float offset, bool success)
	{
		var animToPlay = animNumber.ToString() + trackNumber;
		spaceMan.CrossFadeInFixedTime(animToPlay, CrossFade, 0);
		if (punchRunning != null) StopCoroutine(punchRunning);
		punchRunning = StartCoroutine(PunchRunning());
	}

	//delayed punch
	public void DelayedPunch(int animNumber, int trackNumber)
	{
		var animToPlay = backPosition[animNumber].ToString() + trackNumber + "B";
		spaceMan.CrossFadeInFixedTime(animToPlay, CrossFade, 0, 0.2f);
		punchRunning = StartCoroutine(PunchRunning());
		if (punchRunning != null) StopCoroutine(punchRunning);
		punchRunning = StartCoroutine(PunchRunning());
	}

	public void Empty()
	{
		if (punchRunning != null) return;
		spaceMan.CrossFadeInFixedTime("E", CrossFade, 0);
	}

	//target is too far, don't play full punch clip
	public void IsTooFar(int trackNumber)
	{
		if (punchRunning != null) return;
		var animToPlay = trackNumber + "F";
		spaceMan.CrossFadeInFixedTime(animToPlay, CrossFade, 0);
	}

	//an obstacle got hit on spaceman, considering character's position
	public void GotHit(int trackNumber)
	{
		if (punchRunning != null) return;
		var hitSide = Conductor.avoidPos == 0 ? "H" : "S";
		var animToPlay = trackNumber + hitSide;
		spaceMan.CrossFadeInFixedTime(animToPlay, CrossFade, 0);
	}

	//take avoid position
	public void Avoid(int trackNumber)
	{
		if (punchRunning != null) return;
		spaceMan.CrossFadeInFixedTime(trackNumber.ToString(), CrossFade, 0);
	}

	public void AvoidBack(int trackNumber)
	{
		if (punchRunning != null) return;
		spaceMan.CrossFadeInFixedTime(trackNumber + "B", CrossFade, 0);
	}

	private IEnumerator PunchRunning()
	{
		yield return new WaitForSeconds(0.1f);
		punchRunning = null;
	}
}