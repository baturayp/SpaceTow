using System.Collections.Generic;
using UnityEngine;

//An object pool of music nodes
public class MusicNodePool : MonoBehaviour
{
	public static MusicNodePool instance;
	public GameObject nodePrefab;
	public GameObject[] meteorPrefab;
	public int initialAmount;
	private List<MusicNode> nodeList;
	public GameObject movingParts;

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
	}

	public MusicNode GetNode(float startLineZ, float finishLineZ, float beat, int trackNumber)
	{
		//get an empty meteor or init a new one
		MeteorNode meteor = GetMeteor();
		//check if there is an inactive instance
		foreach (MusicNode node in nodeList)
		{
			if (!node.gameObject.activeInHierarchy)
			{
				node.Initialize(startLineZ, finishLineZ, beat, meteor, trackNumber);
				node.gameObject.SetActive(true);
				return node;
			}
		}
		//no inactive instances, instantiate a new GetComponent
		MusicNode musicNode = Instantiate(nodePrefab).GetComponent<MusicNode>();
		musicNode.Initialize(startLineZ, finishLineZ, beat, meteor, trackNumber);
		nodeList.Add(musicNode);
		return musicNode;
	}

	MeteorNode GetMeteor()
	{
		int randomMeteor = Random.Range(0,5);
		//instantiate as a child of movingParts
		MeteorNode meteorNode = Instantiate(meteorPrefab[randomMeteor], movingParts.transform).GetComponent<MeteorNode>();
		meteorNode.Initialize();
		return meteorNode;
	}
}
