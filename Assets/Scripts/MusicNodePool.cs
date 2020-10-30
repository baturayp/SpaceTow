using System.Collections.Generic;
using UnityEngine;

//An object pool of music nodes
public class MusicNodePool : MonoBehaviour
{
	public static MusicNodePool instance;
	public GameObject nodePrefab;
	public GameObject meteorPrefab;
	public int initialAmount;
	private List<MusicNode> nodeList;
	private List<MeteorNode> meteorList;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		//create initial nodes
		nodeList = new List<MusicNode>();
		for (int i = 0; i < initialAmount; i++)
		{
			GameObject nodes = Instantiate(nodePrefab);
			nodes.SetActive(false);
			nodeList.Add(nodes.GetComponent<MusicNode>());
		}

		//create initial meteors
		meteorList = new List<MeteorNode>();
		for (int i = 0; i < initialAmount; i++)
		{
			GameObject meteors = Instantiate(meteorPrefab);
			meteors.SetActive(false);
			meteorList.Add(meteors.GetComponent<MeteorNode>());
		}
	}

	public MusicNode GetNode(float posX, float startY, float endY, float removeLineY, float startLineZ, float finishLineZ, float posZ, float beat, int times, float duration)
	{
		//get an empty meteor or init a new one
		MeteorNode meteor = GetMeteor();
		//check if there is an inactive instance
		foreach (MusicNode node in nodeList)
		{
			if (!node.gameObject.activeInHierarchy)
			{
				node.Initialize(posX, startY, endY, removeLineY, startLineZ, finishLineZ, posZ, beat, times, duration, meteor);
				node.gameObject.SetActive(true);
				return node;
			}
		}
		//no inactive instances, instantiate a new GetComponent
		MusicNode musicNode = Instantiate(nodePrefab).GetComponent<MusicNode>();
		musicNode.Initialize(posX, startY, endY, removeLineY, startLineZ, finishLineZ, posZ, beat, times, duration, meteor);
		nodeList.Add(musicNode);
		return musicNode;
	}

	MeteorNode GetMeteor()
	{
		//check if there is an inactive instance
		foreach (MeteorNode meteor in meteorList)
		{
			if (!meteor.gameObject.activeInHierarchy)
			{
				meteor.Initialize();
				meteor.gameObject.SetActive(true);
				return meteor;
			}
		}
		MeteorNode meteorNode = Instantiate(meteorPrefab).GetComponent<MeteorNode>();
		meteorNode.Initialize();
		meteorList.Add(meteorNode);
		return meteorNode;
	}
}
