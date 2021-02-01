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
	private bool isObstacle;
	private float startLineZ, finishLineZ;
	private MeteorNode meteorNode;
	private ObstacleNode obstacleNode;

	public void Initialize(float startLine, float finishLine, float targetBeat, int trackNum)
	{
		beat = targetBeat;
		trackNumber = trackNum;
		startLineZ = startLine;
		finishLineZ = finishLine;

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

	private void Update()
	{
		if (Conductor.songposition > beat + 2f) gameObject.SetActive(false);
	}

	private MeteorNode GetMeteor()
	{
		var randomPos = UnityEngine.Random.Range(1,9);
		//int randomPos = 8;
		var randomMeteor = UnityEngine.Random.Range(0,5);
		meteorNode = Instantiate(meteorPrefab[randomMeteor]).GetComponent<MeteorNode>();
		meteorNode.Initialize(startLineZ, finishLineZ, beat, randomPos, trackNumber);
		objPos = randomPos;
		return meteorNode;
	}

	private ObstacleNode GetObstacle()
	{
		var randomPos = UnityEngine.Random.Range(1,3);
		var len = obstaclePrefab.Length;
		var random = UnityEngine.Random.Range(0, len);
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