using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using System;
using GoogleMobileAds.Api;

public class PlayingUIController : MonoBehaviour
{
	//save score
	private float lastHealthScore = 1f;
	private float newHealthScore = 1f;
	private Coroutine healthScoreCoroutine;
	private Coroutine shakeCoroutine;
	// song number for every level
	public int nextLevel;

	//pause scene
	public GameObject pauseButton, pauseScene;
	public GameObject winLayer, lostLayer;

	//health bar
	public Image healthBarTop, healthBarBottom;

	//countdown elements
	public Image fadePanel, endFadePanel;
	private bool lostSceneShowed;

	//cameras
	public CinemachineVirtualCamera playCam;
	public CinemachineDollyCart dollyCart;
	public GameObject towParticles;

	//ads counter
	private InterstitialAd interstitial;
	private bool willShowAds;
	private int closeAction;

	private void OnDestroy()
	{
		interstitial.Destroy();
	}

	private void Start()
	{
		var today = DateTime.Now.DayOfYear;
		var lastDay = PlayerPrefs.GetInt("dateToday", today);
		if (lastDay == today)
		{
			var adsCounter = PlayerPrefs.GetInt("adsCounterToday", 0);
			PlayerPrefs.SetInt("adsCounterToday", adsCounter + 1);
			if (adsCounter == 0 || adsCounter == 2 || adsCounter == 6 || adsCounter == 10)
			{
				willShowAds = true;
				RequestInterstitial();
			}
		}
		else
		{
			PlayerPrefs.SetInt("dateToday", DateTime.Now.DayOfYear);
			PlayerPrefs.SetInt("adsCounterToday", 1);
			willShowAds = true;
			RequestInterstitial();
		}

		StartCoroutine(CameraRoutine());
	}

	private void RequestInterstitial()
	{
#if UNITY_ANDROID
		//test ads
		//string adUnitId = "ca-app-pub-3940256099942544/1033173712";
        
		//my id
		const string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#else
        string adUnitId = "unexpected_platform";
#endif

		// Initialize an InterstitialAd.
		interstitial = new InterstitialAd(adUnitId);

		// Called when the ad is closed.
		interstitial.OnAdClosed += HandleOnAdClosed;

		// Create an empty ad request.
		var request = new AdRequest.Builder().Build();
		// Load the interstitial with the request.
		interstitial.LoadAd(request);
	}

	private void HandleOnAdClosed(object sender, EventArgs args)
	{
		TakeAction(closeAction);
	}

	private void ShowAds()
	{
		if (interstitial.IsLoaded())
		{
			interstitial.Show();
		}
		else
		{
			TakeAction(closeAction);
		}
	}

	public void ScoreDown(int track)
	{
		if (healthScoreCoroutine != null) StopCoroutine(healthScoreCoroutine);
		newHealthScore -= 0.05f;

		//lost
		if (newHealthScore < 0f && !lostSceneShowed)
		{
			//ShowLostScene();
			//Conductor.paused = true;
			//lostSceneShowed = true;
		}

		healthScoreCoroutine = StartCoroutine(HealthBarUpdate(lastHealthScore, newHealthScore));

		if (track >= 2) return;
		if (shakeCoroutine != null) return;
		shakeCoroutine = StartCoroutine(ShakeRoutine(track));
	}

	private IEnumerator CameraRoutine()
	{
		var elapsedTime = 0f;
		while (elapsedTime < 1f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(1f, 0f, elapsedTime / 1f);
			var c = new Color(0, 0, 0, a);
			fadePanel.color = c;
			yield return null;
		}
		pauseButton.SetActive(true);
		yield return new WaitWhile(() => Conductor.nextTrack == 4);
		playCam.Priority = 20;
		yield return new WaitForSeconds(2f);
		towParticles.SetActive(false);
	}

	private IEnumerator PauseFade()
	{
		pauseButton.SetActive(false);
		dollyCart.m_Speed = 0f;
		playCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0f;
		var elapsedTime = 0f;
		while (elapsedTime < 0.3f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(0f, 0.85f, elapsedTime / 0.3f);
			var c = new Color(0, 0, 0, a);
			fadePanel.color = c;
			yield return null;
		}
		pauseScene.SetActive(true);
	}

	private IEnumerator ResumeFade()
	{
		pauseScene.SetActive(false);
		var elapsedTime = 0f;
		while (elapsedTime < 0.3f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(0.85f, 0f, elapsedTime / 0.3f);
			var c = new Color(0, 0, 0, a);
			fadePanel.color = c;
			yield return null;
		}
		fadePanel.color = new Color(0, 0, 0, 0f);
		playCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 1f;
		dollyCart.m_Speed = 0.1f;
		pauseButton.SetActive(true);
		Conductor.paused = false;
	}

	private IEnumerator HealthBarUpdate(float from, float to)
	{
		var elapsedTime = 0.0f;
		while (elapsedTime < 0.2f)
		{
			elapsedTime += Time.deltaTime;
			var fillAm = Mathf.Lerp(from, to, elapsedTime / 0.2f);
			healthBarTop.fillAmount = fillAm;
			healthBarBottom.fillAmount = fillAm;
			lastHealthScore = fillAm;
			yield return null;
		}
		healthScoreCoroutine = null;
	}

	private IEnumerator ShakeRoutine(int track)
	{
		var elapsedTime = 0.0f;
		while (elapsedTime < 0.1)
		{
			elapsedTime += Time.deltaTime;
			var s = Mathf.SmoothStep(0f, track == 0 ? -2 : 2, elapsedTime / 0.1f);
			playCam.m_Lens.Dutch = s;
			yield return null;
		}
		elapsedTime = 0.0f;
		while (elapsedTime < 0.1)
		{
			elapsedTime += Time.deltaTime;
			var s = Mathf.SmoothStep(track == 0 ? -2 : 2, 0f, elapsedTime / 0.1f);
			playCam.m_Lens.Dutch = s;
			yield return null;
		}
		playCam.m_Lens.Dutch = 0f;
		shakeCoroutine = null;
	}

	private IEnumerator HomeRoutine()
	{
		var elapsedTime = 0f;
		while (elapsedTime < 0.5f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(0f, 1f, elapsedTime / 0.5f);
			var c = new Color(0, 0, 0, a);
			fadePanel.color = c;
			yield return null;
		}
		SceneManager.LoadScene("MainMenu");
	}

	private IEnumerator ShowWinRoutine()
	{
		var elapsedTime = 0f;
		while (elapsedTime < 0.3f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(0f, 1f, elapsedTime / 0.3f);
			var c = new Color(0, 0, 0, a);
			endFadePanel.color = c;
			yield return null;
		}
		winLayer.SetActive(true);

		var lastLevel = PlayerPrefs.GetInt("lastLevel", 1);
		if (lastLevel < nextLevel)
		{
			PlayerPrefs.SetInt("lastLevel", nextLevel);
			PlayerPrefs.Save();
		}

		elapsedTime = 0f;
		while (elapsedTime < 0.3f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(1f, 0f, elapsedTime / 0.3f);
			var c = new Color(0, 0, 0, a);
			endFadePanel.color = c;
			yield return null;
		}
	}

	private IEnumerator ShowLostRoutine()
	{
		var elapsedTime = 0f;
		while (elapsedTime < 0.3f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(0f, 1f, elapsedTime / 0.3f);
			var c = new Color(0, 0, 0, a);
			endFadePanel.color = c;
			yield return null;
		}
		lostLayer.SetActive(true);
		elapsedTime = 0f;
		while (elapsedTime < 0.3f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(1f, 0f, elapsedTime / 0.3f);
			var c = new Color(0, 0, 0, a);
			endFadePanel.color = c;
			yield return null;
		}
	}

	public void OnLastButton()
	{
		closeAction = 0;
		if (willShowAds) ShowAds();
		else TakeAction(0);
	}

	public void ShowWinScene()
	{
		StartCoroutine(ShowWinRoutine());
	}

	private void ShowLostScene()
	{
		StartCoroutine(ShowLostRoutine());
	}

	public void PauseButtonOnClick()
	{
		StartCoroutine(PauseFade());
		Conductor.paused = true;
	}

	public void ResumeButtonOnClick()
	{
		StartCoroutine(ResumeFade());
	}

	public void RetryButtonOnClick()
	{
		closeAction = 1;
		if (willShowAds) ShowAds();
		else TakeAction(1);
	}

	public void HomeButtonOnClick()
	{
		closeAction = 2;
		if (willShowAds) ShowAds();
		else TakeAction(2);
	}

	private void TakeAction(int action)
	{
		switch (action)
		{
			case 0:
				SceneManager.LoadScene("Redirector");
				break;
			case 1:
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				break;
			case 2:
				StartCoroutine(HomeRoutine());
				break;
		}
	}
}