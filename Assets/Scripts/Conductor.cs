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
	private readonly float waitOffset = 0.15f;
	private readonly float backOffset = 0.10f;

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
	public static int nextTrack;

	//count down canvas
	public GameObject countDownCanvas;

	//total tracks
	private readonly int len = 4;
	private Coroutine nextPunchWait;

	//audio related stuff
	public AudioSource songLayer;
	public AudioSource effectLayer;
	public AudioClip longPunchClip;
	public AudioClip shortPunchClip;
	public AudioClip obstacleSuccessClip;
	public AudioClip obstacleMissClip;

	//avoid movement values
	public static int avoidPos;
	public static float avoidMoveWait;


	//play punch sound at exact time
	void PunchEffect(float offset)
	{
		if (offset < 0.05f)
		{
			effectLayer.PlayOneShot(longPunchClip);
		}
		else
		{
			//effectLayer.time = Mathf.Abs(offset);
			//effectLayer.PlayScheduled(dsptimesong + pausedTime + beatTime);
			effectLayer.PlayOneShot(shortPunchClip);
		}
	}

	//wait until
	IEnumerator Wait(float until)
	{
		yield return new WaitUntil(() => songposition > until);
		nextPunchWait = null;
	}

	//avoid from obstacles
	void Avoid(int trackNumber)
	{
		if (trackNumber != avoidPos)
		{
			avoidPos = trackNumber;
			spaceMan.Avoid(trackNumber, songposition);
		}

		avoidMoveWait = songposition + 0.5f;
	}

	void PlayerInputted(int track)
	{
		if (nextPunchWait != null) return;
		
		if (beatQueue.Count != 0)
		{
			//peek the node in the queue
			MusicNode frontNode = beatQueue.Peek();

			//do a nudge gesture when incoming node is an obstacle
			if (frontNode.trackNumber > 1)
			{
				spaceMan.Nudge();
				return;
			}

			avoidMoveWait = 0;

			float offset = frontNode.beat - songposition;

			if (offset > 0 && offset < hitOffset) //success hit
			{
				spaceMan.Punch(frontNode.objPos, frontNode.trackNumber, frontNode.beat, true);
				ScoreEvent?.Invoke(Rank.HIT);
				frontNode.Score(true);
				beatQueue.Dequeue();
				PunchEffect(offset);
				//nextPunchWait = StartCoroutine(Wait(frontNode.beat));
			}

			//incoming target is near but not inside the offset
			else if (offset > 0 && offset < hitOffset * 1.25)
			{
				ScoreEvent?.Invoke(Rank.WASTE);
				spaceMan.Punch(frontNode.objPos, frontNode.trackNumber, songposition + waitOffset, false);
				//nextPunchWait = StartCoroutine(Wait(songposition + waitOffset));
			}

			//delay offset
			else if (offset < 0f && offset > 0 - backOffset)
			{
				spaceMan.DelayedPunch(frontNode.objPos, frontNode.trackNumber);
				ScoreEvent?.Invoke(Rank.HIT);
				frontNode.Score(true);
				beatQueue.Dequeue();
				PunchEffect(Mathf.Abs(offset));
			}

			//incoming target is too far
			else
			{
				ScoreEvent?.Invoke(Rank.WASTE);
				spaceMan.IsTooFar(frontNode.trackNumber);
				//nextPunchWait = StartCoroutine(Wait(songposition + waitOffset));
			}
		}

		//no target in sight, empty attack
		else 
		{
			ScoreEvent?.Invoke(Rank.WASTE);
			spaceMan.Empty();
			//nextPunchWait = StartCoroutine(Wait(songposition + waitOffset));
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

		//take reference of next target and its track number
		if (beatQueue.Count != 0)
		{
			MusicNode currNode = beatQueue.Peek();
			nextTrack = currNode.trackNumber;

			//meteors
			if (currNode.trackNumber < 2)
			{
				if (currNode.beat < songposition - backOffset)
				{
					beatQueue.Dequeue();
					ScoreEvent?.Invoke(Rank.MISS);
				}
			}

			//obstacles
			else
			{
				if (currNode.beat < songposition)
				{
					if (avoidPos == currNode.trackNumber)
					{
						currNode.Score(true);
						ScoreEvent?.Invoke(Rank.HIT);
						beatQueue.Dequeue();
						effectLayer.PlayOneShot(obstacleSuccessClip);
					}
					else
					{
						beatQueue.Dequeue();
						ScoreEvent?.Invoke(Rank.MISS);
						spaceMan.GotHit(currNode.trackNumber);
						effectLayer.PlayOneShot(obstacleMissClip);
					}
				}
			}
		}

		//no incoming targets
		if (beatQueue.Count == 0) nextTrack = -1;

		//touch controls
		if (Input.touches.Length > 0)
		{
			Touch t = Input.GetTouch(0);

			if (t.phase == TouchPhase.Began && nextTrack < 2)
			{
				PlayerInputted(0);
			}

			if (t.phase == TouchPhase.Moved && nextTrack > 1)
			{
				if (t.deltaPosition.x > 10) Avoid(3);
				if (t.deltaPosition.x < -10) Avoid(2);
			}
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow)) Avoid(2);
		if (Input.GetKeyDown(KeyCode.RightArrow)) Avoid(3);
		if (Input.GetKeyDown(KeyCode.Space) && nextTrack < 2) PlayerInputted(0);

		//reset player position within 0.5 seconds
		if (songposition > avoidMoveWait)
		{
			avoidPos = 0;
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
