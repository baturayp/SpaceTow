using System.Collections;
using System;
using UnityEngine;

public class MusicNode : MonoBehaviour
{
	public GameObject[] meteorPrefab;
	public GameObject[] obstaclePrefab;
	[NonSerialized] public float beat;
	[NonSerialized] public int objPos;
	[NonSerialized] public int trackNumber;
	private GameObject towTruck;
	private bool isObstacle;
	private Vector3 towTruckInitialPos;
	private float startLineZ, finishLineZ;
	private MeteorNode meteorNode;
	private ObstacleNode obstacleNode;

	public void Initialize(float startLineZ, float finishLineZ, float targetBeat, int trackNumber)
	{
		this.beat = targetBeat;
		this.trackNumber = trackNumber;
		this.startLineZ = startLineZ;
		this.finishLineZ = finishLineZ;

		if (trackNumber > 1)
		{
			obstacleNode = GetObstacle();
			isObstacle = true;
		}
		else
		{
			meteorNode = GetMeteor();
			isObstacle = false;
		}
	}

	void Update()
	{
		if (Conductor.songposition > beat + 2f) gameObject.SetActive(false);
	}

	MeteorNode GetMeteor()
	{
		int randomPos = UnityEngine.Random.Range(1,12);
		int randomMeteor = UnityEngine.Random.Range(0,5);
		meteorNode = Instantiate(meteorPrefab[randomMeteor]).GetComponent<MeteorNode>();
		meteorNode.Initialize(startLineZ, finishLineZ, beat, randomPos, trackNumber);
		objPos = randomPos;
		return meteorNode;
	}

	ObstacleNode GetObstacle()
	{
		int randomPos = UnityEngine.Random.Range(1,3);
		int len = obstaclePrefab.Length;
		int random = UnityEngine.Random.Range(0, len);
		obstacleNode = Instantiate(obstaclePrefab[random]).GetComponent<ObstacleNode>();
		obstacleNode.Initialize(startLineZ, finishLineZ, beat, trackNumber);
		objPos = randomPos;
		return obstacleNode;
	}

	public void Score(bool successHit)
	{
		if(isObstacle) obstacleNode.Bounce(true);
		else meteorNode.Explode(true);
	}
}
