using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Conductor : MonoBehaviour
{
	//if the whole game is paused
	public static bool paused;
	private bool songStarted;

	public static float pauseTimeStamp = -1f; //negative means not managed
	private float pausedTime;

	private SongInfo songInfo;
	private float songLength;

	//song to play without selecting it from main menu
	public SongInfo developmentSong;

	//z axis belongs to meteor object
	public float meteorStartLineZ, meteorFinishLineZ;

	public const float HitOffset = 0.15f;
	private const float BackOffset = 0.10f;

	//current song position and remaining time
	public static float songposition;

	//index for each tracks
	private int[] trackNextIndices;

	//queue, saving the MusicNodes which currently on screen
	private Queue<MusicNode> beatQueue;

	//keep a reference of the sound tracks
	private SongInfo.Track[] tracks;
	public SpaceMan spaceMan;
	public PlayingUIController uiController;
	private float dsptimesong;
	private float endTime;
	private int appearTimeLength;
	public static float appearTime;
	private static int _nextTrack;

	//total tracks
	private const int Len = 4;
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
	private static float _avoidMoveWait;

	//play punch sound at exact time
	private void PunchEffect(float offset)
	{
		effectLayer.PlayOneShot(offset < 0.05f ? longPunchClip : shortPunchClip);
	}

	//wait until
	private IEnumerator Wait(float until)
	{
		yield return new WaitUntil(() => songposition > until);
		nextPunchWait = null;
	}

	//avoid from obstacles
	private void Avoid(int trackNumber)
	{
		if (trackNumber != avoidPos)
		{
			avoidPos = trackNumber;
			spaceMan.Avoid(trackNumber);
		}
		_avoidMoveWait = songposition + 0.5f;
	}

	private void Inputted()
	{
		if (nextPunchWait != null) return;
		if (beatQueue.Count != 0)
		{
			//peek the node in the queue
			var frontNode = beatQueue.Peek();

			//do a nudge gesture when incoming node is an obstacle
			if (frontNode.trackNumber > 1)
			{
				spaceMan.Empty();
				return;
			}
			var offset = frontNode.beat - songposition;
			switch (offset > 0)
			{
				//success hit
				//incoming target is near but not inside the offset
				case true when offset < HitOffset:
					spaceMan.Punch(frontNode.objPos, frontNode.trackNumber, offset, true);
					frontNode.Score(true);
					beatQueue.Dequeue();
					PunchEffect(offset);
					nextPunchWait = StartCoroutine(Wait(frontNode.beat));
					break;
				//delay offset
				case true when offset < HitOffset * 1.25:
					spaceMan.Punch(frontNode.objPos, frontNode.trackNumber, offset, false);
					nextPunchWait = StartCoroutine(Wait(songposition + 0.15f));
					break;
				default:
				{
					if (offset < 0f && offset > -0.07f)
					{
						spaceMan.Punch(frontNode.objPos, frontNode.trackNumber, offset, true);
						frontNode.Score(true);
						beatQueue.Dequeue();
						PunchEffect(Mathf.Abs(offset));
					}
					else if (offset < 0f && offset > 0 - BackOffset)
					{
						spaceMan.DelayedPunch(frontNode.objPos, frontNode.trackNumber);
						frontNode.Score(true);
						beatQueue.Dequeue();
						PunchEffect(Mathf.Abs(offset));
					}
					//incoming target is too far
					else
					{
						spaceMan.IsTooFar(frontNode.trackNumber);
						nextPunchWait = StartCoroutine(Wait(songposition + 0.15f));
					}
					break;
				}
			}
		}

		//no target in sight, empty attack
		else 
		{
			if (avoidPos != 0) return;
			spaceMan.Empty();
			nextPunchWait = StartCoroutine(Wait(songposition + 0.15f));
		}
	}

	private void Start()
	{
		//reset static variables
		paused = true;
		pauseTimeStamp = -1f;

		//get the song info from messenger
		//songInfo = SongInfoMessenger.Instance.currentSong;
		songInfo = developmentSong;

		appearTime = 2f;
		appearTimeLength = songInfo.appearTime.Length;

		songLength = songInfo.song.length;

		//initialize arrays
		trackNextIndices = new int[Len];
		beatQueue = new Queue<MusicNode>();
		for (var i = 0; i < Len; i++)
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

	private void StartSong()
	{
		//get dsptime
		dsptimesong = (float)AudioSettings.dspTime;
		//play song
		songLayer.Play();
		//unpause
		paused = false;
		songStarted = true;
	}

	private void Update()
	{
		//for count down
		if (!songStarted) return;

		//for pausing
		if (paused)
		{
			if (!(pauseTimeStamp < 0f)) return;
			pauseTimeStamp = (float)AudioSettings.dspTime;
			songLayer.Pause();
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

		for (var i = 0; i < appearTimeLength; i++)
		{
			if(songposition >= songInfo.appearTime[i].startTime) appearTime = songInfo.appearTime[i].offsetVal;
		}

		//check if need to instantiate new nodes
		var beatToShow = songposition + appearTime;

		//loop the tracks for new MusicNodes
		for (var i = 0; i < Len; i++)
		{
			var nextIndex = trackNextIndices[i];
			var currTrack = tracks[i];

			if (nextIndex >= currTrack.notes.Length || !(currTrack.notes[nextIndex].dueTo < beatToShow)) continue;
			var currNote = currTrack.notes[nextIndex];
			//get a new node
			var musicNode = MusicNodePool.instance.GetNode(meteorStartLineZ, meteorFinishLineZ, currNote.dueTo, i);
			//enqueue
			beatQueue.Enqueue(musicNode);
			//update the next index
			trackNextIndices[i]++;
		}

		//take reference of next target and its track number
		if (beatQueue.Count != 0)
		{
			var currNode = beatQueue.Peek();
			_nextTrack = currNode.trackNumber;

			//meteors
			if (currNode.trackNumber < 2)
			{
				if (currNode.beat < songposition - BackOffset)
				{
					beatQueue.Dequeue();
					uiController.ScoreDown(currNode.trackNumber);
				}
			}

			//obstacles
			else
			{
				if (currNode.beat < songposition)
				{
					//success avoid
					if (avoidPos == currNode.trackNumber)
					{
						currNode.Score(true);
						beatQueue.Dequeue();
						effectLayer.PlayOneShot(obstacleSuccessClip);
					}
					//got hit
					else
					{
						beatQueue.Dequeue();
						uiController.ScoreDown(currNode.trackNumber);
						spaceMan.GotHit(currNode.trackNumber);
						effectLayer.PlayOneShot(obstacleMissClip);
					}
				}
			}
		}

		//no incoming targets
		if (beatQueue.Count == 0) _nextTrack = 4;

		//touch controls
		if (Input.touches.Length > 0)
		{
			var t = Input.GetTouch(0);
			switch (t.phase)
			{
				case TouchPhase.Began when _nextTrack < 2:
					Inputted();
					break;
				case TouchPhase.Moved:
				{
					if (t.deltaPosition.x > 20 && _nextTrack > 1) Avoid(3);
					if (t.deltaPosition.x < -20 && _nextTrack > 1) Avoid(2);
					break;
				}
				case TouchPhase.Stationary when _nextTrack > 1:
				{
					_avoidMoveWait += 0.1f;
					break;
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow)) Avoid(2);
		if (Input.GetKeyDown(KeyCode.RightArrow)) Avoid(3);
		if (Input.GetKeyDown(KeyCode.Space)) Inputted();

		if (songposition > _avoidMoveWait)
		{
			if (avoidPos != 0) spaceMan.AvoidBack(avoidPos);
			avoidPos = 0;
			_avoidMoveWait = 0f;
		}

		//check to see if the song reaches its end
		if (songposition > songLength || songposition > endTime)
		{
			SongCompleted();
		}
	}

	private void SongCompleted()
    {
		songStarted = false;
    }

	private IEnumerator CountDown()
	{
		//wait until audio data loaded
		yield return new WaitUntil(() => songLayer.clip.loadState == AudioDataLoadState.Loaded);
		uiController.FadeDown();
		StartSong();
	}

	private void OnDestroy()
	{
		songLayer.clip.UnloadAudioData();
	}
}