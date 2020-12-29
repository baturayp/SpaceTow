using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Conductor : MonoBehaviour
{
	public enum Rank { PERFECT, GOOD, BAD, MISS, WASTE };

	//keypress action
	public delegate void KeyDownAction(int trackNumber, float beat, Rank rank);
	public static event KeyDownAction KeyDownEvent;

	//song completion
	public delegate void SongCompletedAction();
	public static event SongCompletedAction SongCompletedEvent;
	public delegate void SpaceJump();
	public static event SpaceJump SpaceJumpEvent;
	public static int[] nextNoteAnim;
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

	public float badOffset, goodOffset, perfectOffset;

	//current song position and remaining time
	public static float songposition;

	//index for each tracks
	private int[] trackNextIndices;

	//queue, saving the MusicNodes which currently on screen
	private Queue<MusicNode>[] queueForTracks;

	//keep a reference of the sound tracks
	private SongInfo.Track[] tracks;

	private float dsptimesong;
	private float endTime;

	//buttons state
	private bool leftButton, rightButton;

	private int appearTimeLength;
	public static float appearTime;

	//count down canvas
	public GameObject countDownCanvas;

#if UNITY_EDITOR || UNITY_STANDALONE
	private KeyCode[] keybindings;
#endif

	//total tracks
	private readonly int len = 2;
	private Coroutine[] nextPunchWait;

	//audio related stuff
	public AudioSource songLayer;
	public AudioClip punchClip;


	//check if button(s) pressed inside update
	void CheckInput()
	{
		if (leftButton && rightButton) SpaceJumpEvent?.Invoke();
		if (leftButton && !rightButton) PlayerInputted(0);
		if (rightButton && !leftButton) PlayerInputted(1);
		rightButton = false; leftButton = false;
	}

	//buttons inside the scene
	public void ButtonPressed(int track)
	{
		leftButton |= track == 0;
		rightButton |= track == 1;
	}

	IEnumerator BeatOnHit(float beatTime)
	{
		yield return new WaitUntil(() => songposition >= beatTime);
		songLayer.PlayOneShot(punchClip);
	}
	IEnumerator Wait(float until, int track)
	{
		yield return new WaitUntil(() => songposition > until);
		nextPunchWait[track] = null;
	}
	
	void PlayerInputted(int trackNumber)
	{
		if (nextPunchWait[trackNumber] != null) return;
		
		if (queueForTracks[trackNumber].Count != 0)
		{
			//peek the node in the queue
			MusicNode frontNode = queueForTracks[trackNumber].Peek();

			float offset = Mathf.Abs(frontNode.beat - songposition);

			if (offset < perfectOffset) //perfect hit
			{
				frontNode.PerfectHit();
				KeyDownEvent?.Invoke(trackNumber, frontNode.beat, Rank.PERFECT);
				queueForTracks[trackNumber].Dequeue();
				StartCoroutine(BeatOnHit(frontNode.beat));
				nextPunchWait[trackNumber] = StartCoroutine(Wait(frontNode.beat, trackNumber));
			}
			else if (offset < goodOffset) //good hit
			{
				frontNode.GoodHit();
				KeyDownEvent?.Invoke(trackNumber, frontNode.beat, Rank.GOOD);
				queueForTracks[trackNumber].Dequeue();
				StartCoroutine(BeatOnHit(frontNode.beat));
				nextPunchWait[trackNumber] = StartCoroutine(Wait(frontNode.beat, trackNumber));
			}
			else if (offset < badOffset) //bad hit
			{
				frontNode.BadHit();
				KeyDownEvent?.Invoke(trackNumber, frontNode.beat, Rank.BAD);
				queueForTracks[trackNumber].Dequeue();
				StartCoroutine(BeatOnHit(frontNode.beat));
				nextPunchWait[trackNumber] = StartCoroutine(Wait(frontNode.beat, trackNumber));
			}
			
			//wasted (empty) hit
			else 
			{
				KeyDownEvent?.Invoke(trackNumber, 0, Rank.WASTE);
				nextPunchWait[0] = StartCoroutine(Wait(songposition + 0.4f, 0));
				nextPunchWait[1] = StartCoroutine(Wait(songposition + 0.4f, 1));
			}
		}
		else 
		{
			KeyDownEvent?.Invoke(trackNumber, 0, Rank.WASTE);
			nextPunchWait[0] = StartCoroutine(Wait(songposition + 0.4f, 0));
			nextPunchWait[1] = StartCoroutine(Wait(songposition + 0.4f, 1));
		}
	}

	void Start()
	{
		//reset static variables
		paused = true;
		pauseTimeStamp = -1f;

		//set button states
		leftButton = false;
		rightButton = false;

		//display countdown canvas
		countDownCanvas.SetActive(true);

		//get the song info from messenger
		//songInfo = SongInfoMessenger.Instance.currentSong;
		songInfo = developmentSong;

		appearTime = 2f;
		appearTimeLength = songInfo.appearTime.Length;
		
		fullNoteCounts = songInfo.TotalHitCounts();

		//keyboard controls
#if UNITY_EDITOR || UNITY_STANDALONE
		keybindings = new KeyCode[2];
		keybindings[0] = (KeyCode)System.Enum.Parse(typeof(KeyCode), "A");
		keybindings[1] = (KeyCode)System.Enum.Parse(typeof(KeyCode), "D");
#endif

		//listen playing ui for lost state
		PlayingUIController.LostEvent += SongCompleted;

		songLength = songInfo.song.length;

		//initialize arrays
		trackNextIndices = new int[len];
		nextPunchWait = new Coroutine[len];
		queueForTracks = new Queue<MusicNode>[len];
		nextNoteAnim = new int[len];
		for (int i = 0; i < len; i++)
		{
			trackNextIndices[i] = 0;
			queueForTracks[i] = new Queue<MusicNode>();
			nextNoteAnim[i]= 0;
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

		//player input control
#if UNITY_EDITOR || UNITY_STANDALONE
		leftButton |= Input.GetKeyDown(keybindings[0]);
		rightButton |= Input.GetKeyDown(keybindings[1]);
#endif

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
				queueForTracks[i].Enqueue(musicNode);

				//update the next index
				trackNextIndices[i]++;
			}
		}

		//loop the queue to check if any of them reaches the finish line
		for (int i = 0; i < len; i++)
		{
			//empty queue, continue
			if (queueForTracks[i].Count == 0)
			{
				nextNoteAnim[i] = 0;
				continue;
			}

			MusicNode currNode = queueForTracks[i].Peek();

			//upcoming notes
			nextNoteAnim[i] = currNode.meteorPos;

			if (currNode.beat <= songposition)
			{
				queueForTracks[i].Dequeue();
				KeyDownEvent?.Invoke(i, 0, Rank.MISS);
			}
		}

		CheckInput();

		//check to see if the song reaches its end
		if (songposition > songLength || songposition > endTime)
		{
			SongCompleted();
		}

		//press spacebar to end a song when inside unity editor
#if UNITY_EDITOR
		if (Input.GetKeyDown("space"))
		{
			SongCompleted();
		}
#endif
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
