using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
	public Animator spaceMan;
	private Coroutine routine;

	//animation frame values
	private readonly float[] attackStart = { 0.16f, 0.2f };
	private readonly float[] attackSuccess = { 0.4f, 0.6f };
	private readonly float[] attackFailed = { 0.8f, 1f };

	//successful punch animation
	public void Punch(int animNumber, int trackNumber, float targetBeat, bool success)
	{
		if (routine != null) StopCoroutine(routine);
		routine = StartCoroutine(PunchAnim(targetBeat, Conductor.songposition, 0.20f, animNumber, trackNumber, success));
	}

	//delayed punch
	public void DelayedPunch(int animNumber, int trackNumber)
	{
		if (routine != null) StopCoroutine(routine);
		routine = StartCoroutine(DelayedPunchAnim(animNumber, trackNumber));
	}

	//when obstacles present play nudge animation
	public void Nudge()
	{
		if (routine != null) return;
		routine = StartCoroutine(NudgeAnim());
	}

	public void Empty()
	{
		if (routine != null) return;
		routine = StartCoroutine(EmptyAnim());
	}

	//target is too far, don't play full punch clip
	public void IsTooFar(int trackNumber)
	{
		if (routine != null) return;
		routine = StartCoroutine(TooFarAnim(trackNumber));
	}

	//an obstacle got hit on spaceman, considering character's position
	public void GotHit(int trackNumber)
	{
		if (routine != null) StopCoroutine(routine);
		if (Conductor.avoidPos != 0) routine = StartCoroutine(GotHitSideAnim(trackNumber == 2 ? 3 : 2));
		else routine = StartCoroutine(GotHitCenterAnim(trackNumber));
	}

	//take avoid position
	public void Avoid(int trackNumber, float songposition)
	{
		if (routine != null) StopCoroutine(routine);
		routine = StartCoroutine(AvoidAnim(trackNumber, songposition));
	}

	IEnumerator PunchAnim(float targetBeat, float punchStarted, float backDuration, int animNum, int trackNumber, bool success)
	{
		spaceMan.speed = 0f;
		string animToPlay = animNum.ToString() + trackNumber.ToString();
		while (Conductor.songposition < targetBeat)
		{
			var animVal = Mathf.Lerp(attackStart[0], attackStart[1], (Conductor.songposition - punchStarted) / (targetBeat - punchStarted));
			spaceMan.Play(animToPlay, 0, animVal);
			spaceMan.Update(0f);
			yield return null;
		}

		float elapsedTime = 0.0f;
		while (elapsedTime < backDuration)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.Lerp(success ? attackSuccess[0] : attackFailed[0], success ? attackSuccess[1] : attackFailed[1], elapsedTime / backDuration);
			spaceMan.Play(animToPlay, 0, animVal);
			spaceMan.Update(0f);
			yield return null;
		}
		routine = null;
		spaceMan.speed = 1f;
		spaceMan.Play("idle");
	}

	IEnumerator DelayedPunchAnim(float animNum, int trackNumber)
	{
		spaceMan.speed = 0f;
		string animToPlay = animNum.ToString() + trackNumber.ToString();
		spaceMan.Play(animToPlay, 0, attackSuccess[0]);
		spaceMan.Update(0f);

		float elapsedTime = 0.0f;
		while (elapsedTime < 0.2f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.SmoothStep(attackSuccess[0], attackSuccess[1], elapsedTime / 0.2f);
			spaceMan.Play(animToPlay, 0, animVal);
			spaceMan.Update(0f);
			yield return null;
		}
		routine = null;
		spaceMan.speed = 1f;
		spaceMan.Play("idle");
	}

	IEnumerator NudgeAnim()
	{
		spaceMan.speed = 0f;
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.2f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.SmoothStep(0.6f, 0.55f, Mathf.PingPong(elapsedTime * 5, 1f));
			spaceMan.Play("00", 0, animVal);
			spaceMan.Update(0f);
			yield return null;
		}
		elapsedTime = 0.0f;
		routine = null;
		spaceMan.speed = 1f;
		spaceMan.Play("idle");
	}

	IEnumerator TooFarAnim(int trackNumber)
	{
		spaceMan.speed = 0f;
		string animToPlay = "10" + trackNumber.ToString();
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.4f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.SmoothStep(0.0f, 0.14f, Mathf.PingPong(elapsedTime * 5, 1f));
			spaceMan.Play(animToPlay, 0, animVal);
			spaceMan.Update(0f);
			yield return null;
		}
		routine = null;
		spaceMan.speed = 1f;
		spaceMan.Play("idle");
	}

	IEnumerator EmptyAnim()
	{
		spaceMan.speed = 1f;
		spaceMan.Play("idle");
		spaceMan.SetBool("tpose", true);
		yield return new WaitForSeconds(0.05f);
		spaceMan.SetBool("tpose", false);
		routine = null;
	}

	IEnumerator AvoidAnim(int trackNumber, float songpos)
	{
		spaceMan.speed = 0f;
		string animToPlay = "2" + trackNumber.ToString();
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.15f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.SmoothStep(0f, 0.6f, elapsedTime / 0.15f);
			spaceMan.Play(animToPlay, 0, animVal);
			spaceMan.Update(0f);
			yield return null;
		}
		yield return new WaitUntil(() => Conductor.songposition > Conductor.avoidMoveWait);
		elapsedTime = 0.0f;
		while (elapsedTime < 0.15f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.SmoothStep(0.6f, 1.0f, elapsedTime / 0.15f);
			spaceMan.Play(animToPlay, 0, animVal);
			spaceMan.Update(0f);
			yield return null;
		}
		routine = null;
		spaceMan.speed = 1f;
		spaceMan.Play("idle");
	}

	IEnumerator GotHitCenterAnim(int trackNumber)
	{
		spaceMan.speed = 0f;
		int aNum = 0;
		string animToPlay = aNum.ToString() + trackNumber.ToString();
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.1f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.SmoothStep(0f, 1f, elapsedTime / 0.1f);
			spaceMan.Play(animToPlay, 0, animVal);
			spaceMan.Update(0f);
			yield return null;
		}
		routine = null;
		spaceMan.speed = 1f;
		spaceMan.Play("idle");
	}

	IEnumerator GotHitSideAnim(int trackNumber)
	{
		spaceMan.speed = 0f;
		int aNum = 1;
		string animToPlay = aNum.ToString() + trackNumber.ToString();
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.15f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.SmoothStep(0.5f, 0.7f, elapsedTime / 0.15f);
			spaceMan.Play(animToPlay, 0, animVal);
			spaceMan.Update(0f);
			yield return null;
		}
		elapsedTime = 0.0f;
		aNum = 2;
		animToPlay = aNum.ToString() + trackNumber.ToString();
		while (elapsedTime < 0.15f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.SmoothStep(0.4f, 0.6f, elapsedTime / 0.15f);
			spaceMan.Play(animToPlay, 0, animVal);
			spaceMan.Update(0f);
			yield return null;
		}
		routine = null;
	}
}
