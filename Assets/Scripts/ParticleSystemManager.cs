using UnityEngine;

public class ParticleSystemManager : MonoBehaviour
{
	public ParticleSet[] particleSet;

	public ParticleSystem perfectionEffect;
	public ParticleSystem heartScoreEffect;

	void Start()
	{
		Conductor.BeatOnHitEvent += BeatOnHit;
		Conductor.KeyUpBeatEvent += KeyUpBeat;
	}

	void OnDestroy()
	{
		Conductor.BeatOnHitEvent -= BeatOnHit;
		Conductor.KeyUpBeatEvent -= KeyUpBeat;
	}

	//will be informed by the Conductor after a beat is hit
	void BeatOnHit(int track, Conductor.Rank rank)
	{
		if (rank == Conductor.Rank.PERFECT)
		{
			particleSet[track].perfect.Play();
			perfectionEffect.Play();
			//heartScoreEffect.Play();
		}
		if (rank == Conductor.Rank.GOOD)
		{
			perfectionEffect.Play();
			particleSet[track].good.Play();
		}
		if (rank == Conductor.Rank.BAD)
		{
			particleSet[track].bad.Play();
		}
		if (rank == Conductor.Rank.CONT)
		{
			particleSet[track].cont.Play();
		}
	}

	void KeyUpBeat(int track)
	{
		particleSet[track].cont.Pause();
		particleSet[track].cont.Clear();
	}

	[System.Serializable]
	public class ParticleSet
	{
		public ParticleSystem perfect;
		public ParticleSystem good;
		public ParticleSystem bad;
		public ParticleSystem cont;
	}
}