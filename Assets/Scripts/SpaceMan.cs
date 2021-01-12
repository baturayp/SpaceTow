using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
	public Animator spaceManAnim;
	private Coroutine attackRoutine;
	private int avoidPos;

	//animation frame values
	private readonly float[] attackStart = { 0.16f, 0.2f };
	private readonly float[] attackSuccess = { 0.4f, 0.6f };
	private readonly float[] attackFailed = { 0.8f, 1f };

	public void Punch(int animNumber, int trackNumber, float targetBeat, bool success)
	{
		//meteor punch anim
		if (trackNumber < 2)
		{
			if (attackRoutine != null) StopCoroutine(attackRoutine);
			attackRoutine = StartCoroutine(AttackAnim(targetBeat, Conductor.songposition, 0.25f, animNumber, trackNumber, success));
		}
	}

	public void Nudge()
	{
		if (attackRoutine != null) return;
		attackRoutine = StartCoroutine(NudgeAnim());
	}

	public void GotHit(int trackNumber)
	{
		if (attackRoutine != null) StopCoroutine(attackRoutine);
		if (avoidPos != trackNumber && avoidPos != 0) attackRoutine = StartCoroutine(GotHitSideAnim(trackNumber == 2 ? 3 : 2));
		else attackRoutine = StartCoroutine(GotHitCenterAnim(trackNumber));
	}

	IEnumerator AttackAnim(float targetBeat, float punchStarted, float backDuration, int animNum, int trackNumber, bool success)
	{
		spaceManAnim.speed = 0f;
		string animToPlay = animNum.ToString() + trackNumber.ToString();
		while (Conductor.songposition < targetBeat)
		{
			var animVal = Mathf.Lerp(attackStart[0], attackStart[1], (Conductor.songposition - punchStarted) / (targetBeat - punchStarted));
			spaceManAnim.Play(animToPlay, 0, animVal);
			spaceManAnim.Update(0f);
			yield return null;
		}

		spaceManAnim.Play(animToPlay, 0, attackStart[1]);

		float elapsedTime = 0.0f;
		while (elapsedTime < backDuration)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.Lerp(success ? attackSuccess[0] : attackFailed[0], success ? attackSuccess[1] : attackFailed[1], elapsedTime / backDuration);
			spaceManAnim.Play(animToPlay, 0, animVal);
			spaceManAnim.Update(0f);
			yield return null;
		}
		attackRoutine = null;
		avoidPos = 0;
		spaceManAnim.speed = 1f;
		spaceManAnim.Play("idle");
	}

	public void AvoidMove(int trackNumber, float songposition)
	{
		if (attackRoutine != null) StopCoroutine(attackRoutine);
		attackRoutine = StartCoroutine(AvoidAnim(trackNumber, songposition));
	}

	IEnumerator NudgeAnim()
	{
		spaceManAnim.speed = 0f;
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.05f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.Lerp(0.6f, 0.55f, elapsedTime / 0.05f);
			spaceManAnim.Play("00", 0, animVal);
			spaceManAnim.Update(0f);
			yield return null;
		}
		elapsedTime = 0.0f;
		while (elapsedTime < 0.05f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.Lerp(0.55f, 0.6f, elapsedTime / 0.05f);
			spaceManAnim.Play("00", 0, animVal);
			spaceManAnim.Update(0f);
			yield return null;
		}
		attackRoutine = null;
	}

	IEnumerator AvoidAnim(int trackNumber, float songpos)
	{
		spaceManAnim.speed = 0f;
		string animToPlay = "2" + trackNumber.ToString();
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.1f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.Lerp(0f, 0.6f, elapsedTime / 0.1f);
			spaceManAnim.Play(animToPlay, 0, animVal);
			spaceManAnim.Update(0f);
			yield return null;
		}
		avoidPos = trackNumber;
		yield return new WaitUntil(() => Conductor.songposition > songpos + 0.5f);
		elapsedTime = 0.0f;
		while (elapsedTime < 0.1f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.Lerp(0.6f, 1.0f, elapsedTime / 0.1f);
			spaceManAnim.Play(animToPlay, 0, animVal);
			spaceManAnim.Update(0f);
			yield return null;
		}
		attackRoutine = null;
		avoidPos = 0;
		spaceManAnim.speed = 1f;
		spaceManAnim.Play("idle");
	}

	IEnumerator GotHitCenterAnim(int trackNumber)
	{
		spaceManAnim.speed = 0f;
		int aNum = 0;
		string animToPlay = aNum.ToString() + trackNumber.ToString();
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.1f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.Lerp(0f, 1f, elapsedTime / 0.1f);
			spaceManAnim.Play(animToPlay, 0, animVal);
			spaceManAnim.Update(0f);
			yield return null;
		}
		attackRoutine = null;
		avoidPos = 0;
		spaceManAnim.speed = 1f;
		spaceManAnim.Play("idle");
	}

	IEnumerator GotHitSideAnim(int trackNumber)
	{
		spaceManAnim.speed = 0f;
		int aNum = 1;
		string animToPlay = aNum.ToString() + trackNumber.ToString();
		float elapsedTime = 0.0f;
		while (elapsedTime < 0.05f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.Lerp(0.5f, 0.7f, elapsedTime / 0.05f);
			spaceManAnim.Play(animToPlay, 0, animVal);
			spaceManAnim.Update(0f);
			yield return null;
		}
		elapsedTime = 0.0f;
		aNum = 2;
		animToPlay = aNum.ToString() + trackNumber.ToString();
		while (elapsedTime < 0.05f)
		{
			elapsedTime += Time.deltaTime;
			var animVal = Mathf.Lerp(0.4f, 0.6f, elapsedTime / 0.05f);
			spaceManAnim.Play(animToPlay, 0, animVal);
			spaceManAnim.Update(0f);
			yield return null;
		}
		avoidPos = trackNumber;
		attackRoutine = null;
	}
}
