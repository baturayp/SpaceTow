using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Conductor : MonoBehaviour
{
	public enum Rank { HIT, MISS, WASTE };

	//keypress action
	public delegate void ScoreAction( Rank rank);
	public static event ScoreAction ScoreEvent;

	//song completion
	public delegate void SongCompletedAction();
	public static event SongCompletedAction SongCompletedEvent;
	private float songLength;

	//if the whole game is paused
	public static bool paused = true;
	private bool songStarted;

	public static float pauseTimeStamp = -1f; //negative means not managed
	private float pausedTime;

	private SongInfo songInfo;

	//song to play without selecting it from main menu
	public SongInfo developmentSong;

	public static int fullNoteCounts;

	//z axis belongs to meteor object
	public float meteorStartLineZ, meteorFinishLineZ;

	public static float hitOffset = 0.15f;

	//current song position and remaining time
	public static float songposition;

	//index for each tracks
	private int[] trackNextIndices;

	//queue, saving the MusicNodes which currently on screen
	private Queue<MusicNode> beatQueue;

	//keep a reference of the sound tracks
	private SongInfo.Track[] tracks;
	public SpaceMan spaceMan;
	
	private float dsptimesong;
	private float endTime;
	private int appearTimeLength;
	public static float appearTime;

	//count down canvas
	public GameObject countDownCanvas;

	//total tracks
	private readonly int len = 4;
	private Coroutine nextPunchWait;

	//audio related stuff
	public AudioSource songLayer;
	public AudioClip punchClip;
	
	//touch controls
	private Vector2 startPos;
	private float startTime;
	private const float maxTime = 0.5f;
	private const  float minDistance = 0.10f;


	IEnumerator PunchCrack(float beatTime)
	{
		yield return new WaitUntil(() => songposition >= beatTime);
		songLayer.PlayOneShot(punchClip);
	}
	IEnumerator Wait(float until)
	{
		yield return new WaitUntil(() => songposition > until);
		nextPunchWait = null;
	}
	
	void PlayerInputted(int track)
	{
		if (nextPunchWait != null) return;
		
		if (beatQueue.Count != 0)
		{
			//peek the node in the queue
			MusicNode frontNode = beatQueue.Peek();

			float offset = Mathf.Abs(frontNode.beat - songposition);

			if (offset < hitOffset) //hitted
			{
				spaceMan.Punch(frontNode.meteorPos, frontNode.trackNumber, frontNode.beat, true);
				ScoreEvent?.Invoke(Rank.HIT);
				frontNode.Explode(2.0f, 0f, true);
				beatQueue.Dequeue();
				StartCoroutine(PunchCrack(frontNode.beat));
				nextPunchWait = StartCoroutine(Wait(frontNode.beat));
			}

			//wasted (empty) hit
			else 
			{
				ScoreEvent?.Invoke(Rank.WASTE);
				spaceMan.Punch(frontNode.meteorPos, frontNode.trackNumber, songposition + 0.2f, false);
				nextPunchWait = StartCoroutine(Wait(songposition + 0.4f));
			}
		}
		else 
		{
			ScoreEvent?.Invoke(Rank.WASTE);
			spaceMan.Punch(0, 0, songposition + 0.2f, false);
			nextPunchWait = StartCoroutine(Wait(songposition + 0.4f));
		}
	}

	void Start()
	{
		//reset static variables
		paused = true;
		pauseTimeStamp = -1f;

		//display countdown canvas
		countDownCanvas.SetActive(true);

		//get the song info from messenger
		//songInfo = SongInfoMessenger.Instance.currentSong;
		songInfo = developmentSong;

		appearTime = 2f;
		appearTimeLength = songInfo.appearTime.Length;
		
		fullNoteCounts = songInfo.TotalHitCounts();

		//listen playing ui for lost state
		PlayingUIController.LostEvent += SongCompleted;

		songLength = songInfo.song.length;

		//initialize arrays
		trackNextIndices = new int[len];
		beatQueue = new Queue<MusicNode>();
		for (int i = 0; i < len; i++)
		{
			trackNextIndices[i] = 0;
		}

		tracks = songInfo.tracks;

		endTime = songInfo.endTime;

		//initialize audioSource
		songLayer.clip = songInfo.song;

		//load Audio data
		songLayer.clip.LoadAudioData();
		songLayer.volume = 1f;

		//start countdown
		StartCoroutine(CountDown());
	}

	void StartSong()
	{
		//get dsptime
		dsptimesong = (float)AudioSettings.dspTime;

		//play song
		songLayer.Play();

		//unpause
		paused = false;
		songStarted = true;
	}

	void Update()
	{
		//for count down
		if (!songStarted) return;

		//for pausing
		if (paused)
		{
			if (pauseTimeStamp < 0f) //not managed
			{
				pauseTimeStamp = (float)AudioSettings.dspTime;
				songLayer.Pause();
			}

			return;
		}
		if (pauseTimeStamp > 0f) //resume not managed
		{
			pausedTime += (float)AudioSettings.dspTime - pauseTimeStamp;
			songLayer.Play();
			pauseTimeStamp = -1f;
		}

		//calculate songposition
		songposition = (float)(AudioSettings.dspTime - dsptimesong - pausedTime) * songLayer.pitch - (songInfo.songOffset);

		for (int i = 0; i < appearTimeLength; i++)
		{
			if(songposition >= songInfo.appearTime[i].startTime) appearTime = songInfo.appearTime[i].offsetVal;
		}

		//check if need to instantiate new nodes
		float beatToShow = songposition + appearTime;

		//loop the tracks for new MusicNodes
		for (int i = 0; i < len; i++)
		{
			int nextIndex = trackNextIndices[i];
			SongInfo.Track currTrack = tracks[i];

			if (nextIndex < currTrack.notes.Length && currTrack.notes[nextIndex].dueTo < beatToShow)
			{
				SongInfo.Note currNote = currTrack.notes[nextIndex];

				//get a new node
				MusicNode musicNode = MusicNodePool.instance.GetNode(meteorStartLineZ, meteorFinishLineZ, currNote.dueTo, i);

				//enqueue
				beatQueue.Enqueue(musicNode);

				//update the next index
				trackNextIndices[i]++;
			}
		}

		if (Input.touches.Length > 0)
		{
			Touch t = Input.GetTouch(0);
			if (t.phase == TouchPhase.Began)
			{
				startPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width);
				startTime = Time.time;
			}
			if (t.phase == TouchPhase.Ended)
			{
				Vector2 endPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width);
				Vector2 swipe = new Vector2(endPos.x - startPos.x, endPos.y - startPos.y);
				
				if (swipe.magnitude < minDistance)
				{ // Tap
					PlayerInputted(0);
				}
				else if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
				{ // Horizontal swipe
					if (swipe.x > 0) PlayerInputted(3);
					else PlayerInputted(2);
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.S)) PlayerInputted(1);
		if (Input.GetKeyDown(KeyCode.A)) PlayerInputted(0);
		if (Input.GetKeyDown(KeyCode.LeftArrow)) PlayerInputted(2);
		if (Input.GetKeyDown(KeyCode.RightArrow)) PlayerInputted(3);

		if (beatQueue.Count != 0)
		{
			MusicNode currNode = beatQueue.Peek();
			if (currNode.beat < songposition - hitOffset)
			{
				beatQueue.Dequeue();
				ScoreEvent?.Invoke(Rank.MISS);
			}
		}

		//check to see if the song reaches its end
		if (songposition > songLength || songposition > endTime)
		{
			SongCompleted();
		}
	}

	void SongCompleted()
    {
		songStarted = false;
		SongCompletedEvent?.Invoke();
	}

	IEnumerator CountDown()
	{
		//wait until audio data loaded
		yield return new WaitUntil(() => songLayer.clip.loadState == AudioDataLoadState.Loaded);

		countDownCanvas.SetActive(false);

		StartSong();
	}

	void OnDestroy()
	{
		PlayingUIController.LostEvent -= SongCompleted;
		songLayer.clip.UnloadAudioData();
	}
}
