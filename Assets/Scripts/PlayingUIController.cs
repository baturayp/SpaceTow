using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class PlayingUIController : MonoBehaviour
{
	//save score
	private float lastHealthScore = 1f;
	private float newHealthScore = 1f;
	private Coroutine healthScoreCoroutine;

	//pause scene
	public GameObject pauseButton, pauseScene;

	//health bar
	public Image healthBarTop, healthBarBottom;

	public void ScoreDown()
	{
		if (healthScoreCoroutine != null) StopCoroutine(healthScoreCoroutine);
		newHealthScore -= 0.05f;
		healthScoreCoroutine = StartCoroutine(HealthBarUpdate(lastHealthScore, newHealthScore));
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

	public void PauseButtonOnClick()
	{
		pauseScene.SetActive(true);
		pauseButton.SetActive(false);
		Conductor.paused = true;
	}

	public void ResumeButtonOnClick()
	{
		pauseScene.SetActive(false);
		pauseButton.SetActive(true);
		Conductor.paused = false;
	}

	public void RetryButtonOnClick()
	{
		SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
	}

	public void HomeButtonOnClick(int scene)
	{
		SceneManager.LoadSceneAsync(scene);
	}
}