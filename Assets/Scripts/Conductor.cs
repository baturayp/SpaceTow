using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Conductor : MonoBehaviour
{
	public enum Rank { PERFECT, GOOD, BAD, MISS, WASTE };

	//keypress action
	public delegate void KeyDownAction(int trackNumber, Rank rank);
	public static event KeyDownAction KeyDownEvent;

	//song completion
	public delegate void SongCompletedAction();
	public static event SongCompletedAction SongCompletedEvent;

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

	[Header("Node spawn points")]
	public float[] trackSpawnPosX;

	//z axis belongs to meteor object
	public float startLineY, finishLineY, removeLineY, meteorStartLineZ, meteorFinishLineZ;

	public float badOffsetY, goodOffsetY, perfectOffsetY;

	//current song position and remaining time
	public static float songposition, remainingTime;

	//index for each tracks
	private int[] trackNextIndices;

	//queue, saving the MusicNodes which currently on screen
	private Queue<MusicNode>[] queueForTracks;

	//keep a reference of the sound tracks
	private SongInfo.Track[] tracks;

	private float dsptimesong;

	public static float BeatsShownOnScreen = 4f, tempo = 1f;

	//count down canvas
	public GameObject countDownCanvas, countDownText;

	//total tracks
	private int len;
	private AudioSource AudioSource { get { return GetComponent<AudioSource>(); } }

	void PlayerInputted(int trackNumber)
	{
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
			}
			else if (offsetY < goodOffsetY) //good hit
			{
				frontNode.GoodHit();
				KeyDownEvent?.Invoke(trackNumber, Rank.GOOD);
				queueForTracks[trackNumber].Dequeue();
			}
			else if (offsetY < badOffsetY) //bad hit
			{
				frontNode.BadHit();
				KeyDownEvent?.Invoke(trackNumber, Rank.BAD);
				queueForTracks[trackNumber].Dequeue();
			}
			//wasted (empty) hit
			else 
			{
				KeyDownEvent?.Invoke(trackNumber, Rank.WASTE);
			}
		}
		else 
		{
			KeyDownEvent?.Invoke(trackNumber, Rank.WASTE);
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

		tempo = songInfo.tempo;
		BeatsShownOnScreen = songInfo.beatsShownOnScreen;
		fullNoteCounts = songInfo.TotalHitCounts();

		//listen to player input
		PlayerInputControl.KeyDownEvent += PlayerInputted;

		//listen playing ui for lost state
		PlayingUIController.LostEvent += SongCompleted;

		songLength = songInfo.song.length;

		//initialize arrays
		len = trackSpawnPosX.Length;
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

		//toggle game objects
		SetGameObjects(true);
		StartCoroutine(AnimationCoroutine());

		//initialize audioSource
		AudioSource.clip = songInfo.song;

		//load Audio data
		AudioSource.clip.LoadAudioData();

		AudioSource.volume = PlayerPrefs.GetFloat("Volume", 1f);

#if UNITY_EDITOR
		countDownCanvas.SetActive(false);
		StartSong();
#endif
#if UNITY_IOS || UNITY_ANDROID
		//start countdown
		StartCoroutine(CountDown());
#endif
	}

	IEnumerator AnimationCoroutine()
	{
		yield return new WaitForSeconds(0.001f);
		SetGameObjects(false);
	}

	void StartSong()
	{
		//get dsptime
		dsptimesong = (float)AudioSettings.dspTime;

		//play song
		AudioSource.Play();

		SetGameObjects(true);

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
				AudioSource.Pause();
				SetGameObjects(false);
			}

			return;
		}
		else if (pauseTimeStamp > 0f) //resume not managed
		{
			pausedTime += (float)AudioSettings.dspTime - pauseTimeStamp;
			AudioSource.Play();
			SetGameObjects(true);

			pauseTimeStamp = -1f;
		}

		//calculate songposition
		songposition = (float)(AudioSettings.dspTime - dsptimesong - pausedTime) * AudioSource.pitch - (songInfo.songOffset);

		//remaining time
		remainingTime = songInfo.endTime - songposition;

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
				MusicNode musicNode = MusicNodePool.instance.GetNode(trackSpawnPosX[i], startLineY, finishLineY, removeLineY, meteorStartLineZ, meteorFinishLineZ, currNote.dueTo, i);

				//enqueue
				queueForTracks[i].Enqueue(musicNode);

				//update the next index
				trackNextIndices[i]++;
			}
		}

		//set dynamic tempo when applicable
		if(songInfo.dynamicTempo != null && songInfo.dynamicTempo.Length > 0)
		{
			int length = songInfo.dynamicTempo.Length;
			for (int i = 0; i < length; i++)
			{
				if (songposition > songInfo.dynamicTempo[i].startTime)
				{
					tempo = songInfo.dynamicTempo[i].tempoVal;
				}
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

			//single note
			if (currNode.transform.position.y <= finishLineY - goodOffsetY)
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
		yield return new WaitForSeconds(1f);

		for (int i = 3; i >= 1; i--)
		{
			countDownText.GetComponent<TMPro.TextMeshProUGUI>().text = i.ToString();
			yield return new WaitForSeconds(1f);
		}

		//wait until audio data loaded
		yield return new WaitUntil(() => AudioSource.clip.loadState == AudioDataLoadState.Loaded);

		countDownCanvas.SetActive(false);

		StartSong();
	}

	void OnDestroy()
	{
		PlayerInputControl.KeyDownEvent -= PlayerInputted;
		PlayingUIController.LostEvent -= SongCompleted;
		AudioSource.clip.UnloadAudioData();
	}

	void SetGameObjects(bool state)
	{
		//animated objects that needs to paused when song paused
	}
}
