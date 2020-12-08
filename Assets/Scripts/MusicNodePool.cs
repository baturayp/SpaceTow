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

	public MusicNode GetNode(float posX, float startY, float endY, float removeLineY, float startLineZ, float finishLineZ, float posZ, float beat)
	{
		//get an empty meteor or init a new one
		MeteorNode meteor = GetMeteor();
		//check if there is an inactive instance
		foreach (MusicNode node in nodeList)
		{
			if (!node.gameObject.activeInHierarchy)
			{
				node.Initialize(posX, startY, endY, removeLineY, startLineZ, finishLineZ, posZ, beat, meteor);
				node.gameObject.SetActive(true);
				return node;
			}
		}
		//no inactive instances, instantiate a new GetComponent
		MusicNode musicNode = Instantiate(nodePrefab).GetComponent<MusicNode>();
		musicNode.Initialize(posX, startY, endY, removeLineY, startLineZ, finishLineZ, posZ, beat, meteor);
		nodeList.Add(musicNode);
		return musicNode;
	}

	MeteorNode GetMeteor()
	{
		int randomMeteor = Random.Range(0,5);
		MeteorNode meteorNode = Instantiate(meteorPrefab[randomMeteor]).GetComponent<MeteorNode>();
		meteorNode.Initialize();
		return meteorNode;
	}
}
