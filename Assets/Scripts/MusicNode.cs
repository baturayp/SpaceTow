using System.Collections;
using System;
using UnityEngine;

public class MusicNode : MonoBehaviour
{
	public GameObject[] meteorPrefab;
	public GameObject[] obstaclePrefab;
	[NonSerialized] public float beat;
	[NonSerialized] public int objPos;
	[NonSerialized] public bool paused;
	[NonSerialized] public int trackNumber;
	private GameObject towTruck;
	private bool towTruckShaking;
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

		towTruckInitialPos = new Vector3(0,0,-0.85f);
		towTruck = GameObject.FindGameObjectWithTag("towtruck");

		meteorNode = GetMeteor();
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
		obstacleNode = Instantiate(obstaclePrefab[0]).GetComponent<ObstacleNode>();
		//obstacleNode.Initialize();
		return obstacleNode;
	}

	void Update()
	{
		if (Conductor.pauseTimeStamp > 0f) return; //resume not managed

		if (towTruckShaking)
		{
			towTruck.transform.position = towTruckInitialPos + UnityEngine.Random.insideUnitSphere * 0.01f;
		}

		//avoid transforming when paused
		if (paused) return;
		
		//missed the punch
		if (Conductor.songposition > beat + Conductor.hitOffset)
		{
			meteorNode.Explode(false);
		}
	}

	public void Explode(bool successHit)
	{
		gameObject.SetActive(false);
	}
}
