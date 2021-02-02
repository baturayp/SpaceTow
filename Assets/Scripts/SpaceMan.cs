using System.Collections;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
	public Animator spaceMan;
	private readonly int[] backPosition = { 0, 1, 2, 3, 4, 3, 3, 1, 2 };
	private static readonly int FarL = Animator.StringToHash("farL");
	private static readonly int FarR = Animator.StringToHash("farR");
	private static readonly int HitL = Animator.StringToHash("hitL");
	private static readonly int HitR = Animator.StringToHash("hitR");
	private static readonly int EmptyHit = Animator.StringToHash("emptyHit");
	private static readonly int SideHit = Animator.StringToHash("sideHit");

	private void Update()
	{
		spaceMan.enabled = !Conductor.paused;
	}

	//successful punch animation
	public void Punch(int animNumber, int trackNumber, bool success)
	{
		var animToPlay = animNumber.ToString() + trackNumber + (success ? "3" : "4");
		spaceMan.CrossFadeInFixedTime(animToPlay, 0.07f, 0);
	}

	//delayed punch
	public void DelayedPunch(int animNumber, int trackNumber)
	{
		var animToPlay = backPosition[animNumber].ToString() + trackNumber + "8";
		spaceMan.CrossFadeInFixedTime(animToPlay, 0.07f, 0);
	}

	public void Empty()
	{
		spaceMan.SetTrigger(EmptyHit);
	}

	//target is too far, don't play full punch clip
	public void IsTooFar(int trackNumber)
	{
		spaceMan.SetTrigger(trackNumber > 0 ? FarR : FarL);
	}

	//an obstacle got hit on spaceman, considering character's position
	public void GotHit(int trackNumber)
	{
		spaceMan.SetTrigger(trackNumber > 2 ? HitR : HitL);
	}

	public void GotHitFromSide()
	{
		spaceMan.SetTrigger(SideHit);
	}

	//take avoid position
	public void Avoid(int trackNumber)
	{
		spaceMan.CrossFadeInFixedTime(trackNumber.ToString(), 0.1f, 0);
	}

	public void AvoidBack(int trackNumber)
	{
		spaceMan.CrossFadeInFixedTime(trackNumber + "B", 0.05f, 0);
	}
}