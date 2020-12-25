using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conductor : MonoBehaviour
{
	public enum Rank { PERFECT, GOOD, BAD, MISS, WASTE };

	//keypress action
	public delegate void KeyDownAction(int trackNumber, Rank rank);
	public static event KeyDownAction KeyDownEvent;

	//song completion
	public delegate void SongCompletedAction();
	public static event SongCompletedAction SongCompletedEvent;
	public delegate void SpaceJump();
	public static event SpaceJump SpaceJumpEvent;

	public static float[] dueToNextNote;
	public static int[] nextNoteAnim;
	private float songLength;

	//if the whole game is paused
	public static bool paused = true;
	private bool songStarted = false;

	public static float pauseTimeStamp = -1f; //negative means not managed
	private float pausedTime = 0f;

	private SongInfo songInfo;

	//song to play without selecting it from main menu
	public SongInfo developmentSong;

	public static int fullNoteCounts;

	//z axis belongs to meteor object
	public float startLineY, finishLineY, removeLineY, meteorStartLineZ, meteorFinishLineZ;

	public float badOffsetY, goodOffsetY, perfectOffsetY;

	//current song position and remaining time
	public static float songposition;

	//index for each tracks
	private int[] trackNextIndices;

	//queue, saving the MusicNodes which currently on screen
	private Queue<MusicNode>[] queueForTracks;

	//keep a reference of the sound tracks
	private SongInfo.Track[] tracks;

	private float dsptimesong;

	//buttons state
	private bool leftButton, rightButton;

	public static float BeatsShownOnScreen = 4f, tempo = 1f;

	//count down canvas
	public GameObject countDownCanvas;

#if UNITY_EDITOR || UNITY_STANDALONE
    private KeyCode[] keybindings;
#endif

	//total tracks
	private int len = 2;

	private Coroutine nextPunchWait;

	//audio related stuff
	public AudioSource firstLayer;
	public AudioSource secondLayer;
	private float secondLayerVol;
	private float effectLength = 0.21f;
	private float timeToVolumeUp = 0f;
	private float timeToMute = 0f;
	
	
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
		if (track == 0) leftButton = true;
		if (track == 1) rightButton = true;
	}

	void BeatOnHit(float beatTime)
	{
		timeToVolumeUp = beatTime;
		timeToMute = beatTime + effectLength;
	}
	
	void PlayerInputted(int trackNumber)
	{
		if (nextPunchWait != null) return;	
		
		if (queueForTracks[trackNumber].Count != 0)
		{
			//peek the node in the queue
			MusicNode frontNode = queueForTracks[trackNumber].Peek();

			float offsetY = Mathf.Abs(frontNode.gameObject.transform.position.y - finishLineY);

			if (offsetY < perfectOffsetY) //perfect hit
			{
				frontNode.PerfectHit();
				KeyDownEvent?.Invoke(trackNumber, Rank.PERFECT);
				queueForTracks[trackNumber].Dequeue();
				BeatOnHit(frontNode.beat);
				nextPunchWait = StartCoroutine(Wait(0.1f));
			}
			else if (offsetY < goodOffsetY) //good hit
			{
				frontNode.GoodHit();
				KeyDownEvent?.Invoke(trackNumber, Rank.GOOD);
				queueForTracks[trackNumber].Dequeue();
				BeatOnHit(frontNode.beat);
				nextPunchWait = StartCoroutine(Wait(0.1f));
			}
			else if (offsetY < badOffsetY) //bad hit
			{
				frontNode.BadHit();
				KeyDownEvent?.Invoke(trackNumber, Rank.BAD);
				queueForTracks[trackNumber].Dequeue();
				BeatOnHit(frontNode.beat);
				nextPunchWait = StartCoroutine(Wait(0.1f));
			}
			
			//wasted (empty) hit
			else 
			{
				KeyDownEvent?.Invoke(trackNumber, Rank.WASTE);
				nextPunchWait = StartCoroutine(Wait(0.5f));
			}
		}
		else 
		{
			KeyDownEvent?.Invoke(trackNumber, Rank.WASTE);
			nextPunchWait = StartCoroutine(Wait(0.5f));
		}
	}

	IEnumerator Wait(float s)
	{
		yield return new WaitForSeconds(s);
		nextPunchWait = null;
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

		tempo = songInfo.tempo;
		BeatsShownOnScreen = songInfo.beatsShownOnScreen;
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
		queueForTracks = new Queue<MusicNode>[len];
		dueToNextNote = new float[len];
		nextNoteAnim = new int[len];
		for (int i = 0; i < len; i++)
		{
			trackNextIndices[i] = 0;
			queueForTracks[i] = new Queue<MusicNode>();
			dueToNextNote[i] = -1;
			nextNoteAnim[i]= 0;
		}

		tracks = songInfo.tracks; //keep a reference of the tracks

		//initialize audioSource
		firstLayer.clip = songInfo.song;

		//load Audio data
		firstLayer.clip.LoadAudioData();
		secondLayer.clip.LoadAudioData();

		firstLayer.volume = 1f;
		secondLayer.volume = 0f;

		//start countdown
		StartCoroutine(CountDown());
	}

	void StartSong()
	{
		//get dsptime
		dsptimesong = (float)AudioSettings.dspTime;

		//play song
		firstLayer.Play();
		secondLayer.Play();

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
				firstLayer.Pause();
				secondLayer.Pause();
			}

			return;
		}
		else if (pauseTimeStamp > 0f) //resume not managed
		{
			pausedTime += (float)AudioSettings.dspTime - pauseTimeStamp;
			firstLayer.Play();
			secondLayer.Play();
			pauseTimeStamp = -1f;
		}

		//player input control
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(keybindings[0])) leftButton = true;
		if (Input.GetKeyDown(keybindings[1])) rightButton = true; 
#endif

		//calculate songposition
		songposition = (float)(AudioSettings.dspTime - dsptimesong - pausedTime) * firstLayer.pitch - (songInfo.songOffset);

		CheckInput();

		if (songposition > timeToVolumeUp)
		{
			if (secondLayerVol != 1f) secondLayer.volume = 1f; secondLayerVol = 1f;
		}
		if (songposition > timeToMute)
		{
			if (secondLayerVol != 0f) secondLayer.volume = 0f; secondLayerVol = 0f;
		}

		//check if need to instantiate new nodes
		float beatToShow = songposition + (BeatsShownOnScreen / tempo);

		//loop the tracks for new MusicNodes
		for (int i = 0; i < len; i++)
		{
			int nextIndex = trackNextIndices[i];
			SongInfo.Track currTrack = tracks[i];

			if (nextIndex < currTrack.notes.Length && currTrack.notes[nextIndex].dueTo < beatToShow)
			{
				SongInfo.Note currNote = currTrack.notes[nextIndex];

				//get a new node
				MusicNode musicNode = MusicNodePool.instance.GetNode(startLineY, finishLineY, removeLineY, meteorStartLineZ, meteorFinishLineZ, currNote.dueTo, i);

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
				dueToNextNote[i] = -1;
				nextNoteAnim[i] = 0;
				continue;
			}

			MusicNode currNode = queueForTracks[i].Peek();

			//upcoming notes
			dueToNextNote[i] = currNode.beat - songposition;
			nextNoteAnim[i] = currNode.meteorPos;

			if (currNode.transform.position.y <= finishLineY)
			{
				queueForTracks[i].Dequeue();
				KeyDownEvent?.Invoke(i, Rank.MISS);
			}
		}

		float endTime = songInfo.endTime;

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
		yield return new WaitUntil(() => firstLayer.clip.loadState == AudioDataLoadState.Loaded);
		yield return new WaitUntil(() => secondLayer.clip.loadState == AudioDataLoadState.Loaded);

		countDownCanvas.SetActive(false);

		StartSong();
	}

	void OnDestroy()
	{
		PlayingUIController.LostEvent -= SongCompleted;
		firstLayer.clip.UnloadAudioData();
		secondLayer.clip.UnloadAudioData();
	}
}
