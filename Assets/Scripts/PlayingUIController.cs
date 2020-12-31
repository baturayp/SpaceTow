using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class PlayingUIController : MonoBehaviour
{
	//score texts
	public GameObject scoreText;

	//save score
	private int currLiveCount = 45, currPerfection = 0, fullNoteCounts;

	//fade in-out layer
	public Image fadeInOut;

	//pause scene
	public GameObject pauseButton, pauseScene;
	public ParticleSystem[] particleSys;

	//win scene
	private const float DelayBetweenElements = 0.2f;

	//inform conductor when lost before song completed
	public delegate void LostAction();
	public static event LostAction LostEvent;

	//show ads event
	// public delegate void ShowAdsAction();
	// public static event ShowAdsAction ShowAdsEvent;

	void Start()
	{
		//length of all notes
		fullNoteCounts = Conductor.fullNoteCounts;

		//register to events
		Conductor.KeyDownEvent += BeatOnHit;
		Conductor.SongCompletedEvent += SongCompleted;

		//show ad
		//IntersititialAd.AdsShownEvent += GotoNextScene;
	}


    void OnDestroy()
	{
		//unregister from events
		Conductor.KeyDownEvent -= BeatOnHit;
		Conductor.SongCompletedEvent -= SongCompleted;
		
		//show ad
		//IntersititialAd.AdsShownEvent -= GotoNextScene;
	}

	//called by event
	void BeatOnHit(int trackNumber, float beat, Conductor.Rank rank)
	{
		if (rank == Conductor.Rank.PERFECT)
		{
			currPerfection += 2;
		}
		else if (rank == Conductor.Rank.GOOD)
		{
			currPerfection++;
		}
		else if (rank == Conductor.Rank.BAD)
		{
			//do something
		}
		else if (rank == Conductor.Rank.MISS)
		{
			//loss
			currLiveCount--;
		}

		//invoke lose when no more lives left
		if (currLiveCount == 0) LostEvent?.Invoke();

		UpdateScoreUI();
	}

	void UpdateScoreUI()
	{;
		scoreText.GetComponent<TMPro.TextMeshProUGUI>().text = currLiveCount.ToString();
	}

	void StopParticles()
	{
		int ps = particleSys.Length;
		for (int i = 0; i < ps; i++)
		{
			particleSys[i].Pause();
		}
	}
	void StartParticles()
	{
		int ps = particleSys.Length;
		for (int i = 0; i < ps; i++)
		{
			particleSys[i].Play();
		}
	}

	public void PauseButtonOnClick()
	{
		//display pause scene
		pauseScene.SetActive(true);
		pauseButton.SetActive(false);
		StopParticles();
		Conductor.paused = true;
	}

	//pause scene
	public void ResumeButtonOnClick()
	{
		//disable pause scene
		pauseScene.SetActive(false);
		pauseButton.SetActive(true);
		StartParticles();
		Conductor.paused = false;
	}

	public void RetryButtonOnClick()
	{
		//reload current scene
		SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
	}

	public void HomeButtonOnClick()
	{
		StartCoroutine(ScreenFadeIn(false, true));
	}


	void SongCompleted()
	{
		StartCoroutine(ScreenFadeIn(true, false));
	}

	public void NextButtonOnClick()
    {
		int currSongNumber = SongInfoMessenger.Instance.currSongNumber;
		SongCollection collection = SongInfoMessenger.Instance.currentCollection;
		int collLength = collection.songSets.Length;
		if (currSongNumber < collLength - 1)
		{
			int curr = currSongNumber + 1;
			SongInfoMessenger.Instance.currentSong = collection.songSets[curr].song;
			SongInfoMessenger.Instance.currSongNumber = curr;
			//ShowAdsEvent?.Invoke();
			GotoNextScene();
		}
		else 
		{
			int curr = 0;
			SongInfoMessenger.Instance.currentSong = collection.songSets[curr].song;
			SongInfoMessenger.Instance.currSongNumber = curr;
			//ShowAdsEvent?.Invoke();
			GotoNextScene();
		}
	}

	public void GotoNextScene()
    {
		SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
	}

	IEnumerator ScreenFadeIn(bool finish, bool returnMain)
	{
		float elapsedTime = 0.0f;
		Color c = fadeInOut.color;
		while (elapsedTime < 0.6f)
		{
			elapsedTime += Time.deltaTime;
			c.a = 0.0f + Mathf.Clamp01(elapsedTime / 0.6f);
			fadeInOut.color = c;
			yield return null;
		}
		
		//song completed, slight fade while camera focusing to finish scene
		if (finish) 
		{
			StartCoroutine(ShowWinScene());
			StartCoroutine(ScreenFadeOut()); 
		}

		//exit to home button pressed, just fade in
		if (returnMain)
        {
			SceneManager.LoadSceneAsync("MainMenu");
		}
	}

	IEnumerator ScreenFadeOut()
    {
		float elapsedTime = 0.0f;
		Color c = fadeInOut.color;
		while (elapsedTime < 0.6f)
		{
			elapsedTime += Time.deltaTime;
			c.a = 1.0f - Mathf.Clamp01(elapsedTime / 0.6f);
			fadeInOut.color = c;
			yield return null;
		}
	}

	IEnumerator ShowWinScene()
	{
		// //hide scene elements and start to show finish items
		// pauseButton.SetActive(false);
		// score.SetActive(false);

		// //save progress
		// if (currLiveCount > 0)
        // {
		// 	int currSongNumber = SongInfoMessenger.Instance.currSongNumber;
		// 	int currCollNumber = SongInfoMessenger.Instance.currCollNumber;
		// 	string stringCollNumber = currCollNumber.ToString();
		// 	SongCollection collection = SongInfoMessenger.Instance.currentCollection;
		// 	int collLength = collection.songSets.Length;
		// 	int currProgress = PlayerPrefs.GetInt(stringCollNumber, 1);
		// 	if (currProgress - 1 == currSongNumber && collLength != currProgress)
		// 	{
		// 		PlayerPrefs.SetInt(stringCollNumber, currProgress + 1);
		// 	}
		// 	PlayerPrefs.Save();
		// }

		// yield return new WaitForSeconds(DelayBetweenElements);
		
		// finishedControls.SetActive(true);

		// if (currLiveCount > 0)
        // {
		// 	finishedSuccessState.SetActive(true);
		// 	nextButton.SetActive(true);
        // }
        // else
        // {
		// 	finishedFailState.SetActive(true);
		// 	nextButton.SetActive(false);
        // }

		yield return null;
	}
}