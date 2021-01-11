using System.Collections.Generic;
using UnityEngine;

//An object pool of music nodes
public class MusicNodePool : MonoBehaviour
{
	public static MusicNodePool instance;
	public GameObject nodePrefab;
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

	public MusicNode GetNode(float startLineZ, float finishLineZ, float beat, int trackNumber)
	{
		//check if there is an inactive instance
		foreach (MusicNode node in nodeList)
		{
			if (!node.gameObject.activeInHierarchy)
			{
				node.Initialize(startLineZ, finishLineZ, beat, trackNumber);
				node.gameObject.SetActive(true);
				return node;
			}
		}
		//no inactive instances, instantiate a new GetComponent
		MusicNode musicNode = Instantiate(nodePrefab).GetComponent<MusicNode>();
		musicNode.Initialize(startLineZ, finishLineZ, beat, trackNumber);
		nodeList.Add(musicNode);
		return musicNode;
	}
}
