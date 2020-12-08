using UnityEngine;

public class ParticleSystemManager : MonoBehaviour
{
	public ParticleSet[] particleSet;

	void Start()
	{
		Conductor.KeyDownEvent += BeatOnHit;
	}

	void OnDestroy()
	{
		Conductor.KeyDownEvent -= BeatOnHit;
	}

	//will be informed by the Conductor after a beat is hit
	void BeatOnHit(int track, Conductor.Rank rank)
	{
		if (rank == Conductor.Rank.PERFECT)
		{
			particleSet[track].perfect.Play();
		}
		if (rank == Conductor.Rank.GOOD)
		{
			particleSet[track].good.Play();
		}
		if (rank == Conductor.Rank.BAD)
		{
			particleSet[track].bad.Play();
		}
	}

	[System.Serializable]
	public class ParticleSet
	{
		public ParticleSystem perfect;
		public ParticleSystem good;
		public ParticleSystem bad;
	}
}