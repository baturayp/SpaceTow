using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using System;

public class PlayingUIController : MonoBehaviour
{
	//save score
	private float lastHealthScore = 1f;
	private float newHealthScore = 1f;
	private Coroutine healthScoreCoroutine;
	private Coroutine shakeCoroutine;

	//pause scene
	public GameObject pauseButton, pauseScene;

	//health bar
	public Image healthBarTop, healthBarBottom;

	//countdown elements
	public Image fadePanel;

	//cameras
	public CinemachineVirtualCamera playCam;
	public GameObject towParticles;

	private void Start()
	{
		StartCoroutine(CameraRoutine());
	}

	public void FadeDown()
	{
		StartCoroutine(FadeRoutine());
	}

	public void ScoreDown(int track)
	{
		if (healthScoreCoroutine != null) StopCoroutine(healthScoreCoroutine);
		newHealthScore -= 0.05f;
		healthScoreCoroutine = StartCoroutine(HealthBarUpdate(lastHealthScore, newHealthScore));
		if (track < 2)
		{
			if (shakeCoroutine != null) return;
			shakeCoroutine = StartCoroutine(ShakeRoutine(track));
		}
	}

	private IEnumerator FadeRoutine()
	{
		var elapsedTime = 0f;
		while (elapsedTime < 2f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(1f, 0f, elapsedTime / 2f);
			var c = new Color(0, 0, 0, a);
			fadePanel.color = c;
			yield return null;
		}
		pauseButton.SetActive(true);
		StartCoroutine(CountRoutine());
	}

	private IEnumerator CountRoutine()
	{
		for (var i = 3; i >=1; i--)
		{
			yield return new WaitWhile(() => Conductor.paused);
			yield return new WaitForSeconds(1f);
		}
	}

	private IEnumerator CameraRoutine()
	{
		yield return new WaitForSeconds(5f);
		playCam.Priority = 20;
		yield return new WaitForSeconds(3f);
		towParticles.SetActive(false);
	}

	private IEnumerator PauseFade()
	{
		pauseButton.SetActive(false);
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
		while (elapsedTime < 0.2)
		{
			elapsedTime += Time.deltaTime;
			var s = Mathf.SmoothStep(0f, track == 0 ? -2 : 2, elapsedTime / 0.2f);
			playCam.m_Lens.Dutch = s;
			yield return null;
		}
		elapsedTime = 0.0f;
		while (elapsedTime < 0.2)
		{
			elapsedTime += Time.deltaTime;
			var s = Mathf.SmoothStep(track == 0 ? -2 : 2, 0f, elapsedTime / 0.2f);
			playCam.m_Lens.Dutch = s;
			yield return null;
		}
		playCam.m_Lens.Dutch = 0f;
		shakeCoroutine = null;
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
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void HomeButtonOnClick(int scene)
	{
		SceneManager.LoadSceneAsync("MainMenu");
	}
}