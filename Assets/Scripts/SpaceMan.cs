using System.Collections;
using UnityEngine;

public class SpaceMan : MonoBehaviour
{
	public Animator spaceMan;
	private readonly int[] backPosition = { 0, 18, 18, 18, 28, 38, 38, 48, 48, 48, 48, 28 };
	private static readonly int PunchL = Animator.StringToHash("punchL");
	private static readonly int PunchR = Animator.StringToHash("punchR");
	private static readonly int AnimNumber = Animator.StringToHash("punchAnim");
	private static readonly int AvoidWait = Animator.StringToHash("avoidWait");
	private static readonly int FarL = Animator.StringToHash("farL");
	private static readonly int FarR = Animator.StringToHash("farR");
	private static readonly int HitL = Animator.StringToHash("hitL");
	private static readonly int HitR = Animator.StringToHash("hitR");
	private static readonly int EmptyHit = Animator.StringToHash("emptyHit");
	private static readonly int ShortAvoidL = Animator.StringToHash("shortAvoidL");
	private static readonly int ShortAvoidR = Animator.StringToHash("shortAvoidR");
	private static readonly int LongAvoidL = Animator.StringToHash("longAvoidL");
	private static readonly int LongAvoidR = Animator.StringToHash("longAvoidR");

	private void Update()
	{
		spaceMan.enabled = !Conductor.paused;
	}

	//successful punch animation
	public void Punch(int animNumber, int trackNumber, bool success)
	{
		if (success) spaceMan.SetInteger(AnimNumber, animNumber);
		else spaceMan.SetInteger(AnimNumber, animNumber * 10 + 4);
		spaceMan.SetTrigger(trackNumber > 0 ? PunchR : PunchL);
	}

	//delayed punch
	public void DelayedPunch(int animNumber, int trackNumber)
	{
		spaceMan.SetInteger(AnimNumber, backPosition[animNumber]);
		spaceMan.SetTrigger(trackNumber > 0 ? PunchR : PunchL);
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

	//take avoid position
	public void Avoid(int trackNumber)
	{
		spaceMan.SetBool(AvoidWait, true);
		spaceMan.SetTrigger(trackNumber > 2 ? LongAvoidR : LongAvoidL);
	}

	public void ShortAvoid(int trackNumber)
	{
		spaceMan.SetTrigger(trackNumber > 2 ? ShortAvoidR : ShortAvoidL);
	}

	public void AvoidBack(int trackNumber)
	{
		spaceMan.SetBool(AvoidWait, false);
	}
}