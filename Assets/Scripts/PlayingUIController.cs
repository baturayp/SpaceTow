using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using System;
using UnityEngine.Advertisements;
using Lumpn.Matomo;

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

    //cameras
    public CinemachineVirtualCamera playCam;
    public CinemachineDollyCart dollyCart;
    public GameObject towParticles;
    [NonSerialized] public bool lostSceneShowed;

    private readonly string[] scenes = { "Tutorial", "Chapel", "Beach", "Barn", "Toxic", "Station" };

#if UNITY_ANDROID
    private readonly string gameId = "4010719";
    private readonly string platform = "Android";
#else
    private readonly string gameId = "4010718";
    private readonly string platform = "iOS";
#endif
    private readonly bool testMode = false;
    //matomo
    [SerializeField] private MatomoTrackerData trackerData;


    private void Start()
    {
        Advertisement.Initialize(gameId, testMode);
        Application.targetFrameRate = 60;
        StartCoroutine(CameraRoutine());
        MatomoStarted();
    }

    private void MatomoLost()
    {
        //matomo
        var tracker = trackerData.CreateTracker();
        var session = tracker.CreateSession(SystemInfo.deviceUniqueIdentifier);
        session.Record(SceneManager.GetActiveScene().name, platform + "Lost/End", Time.timeSinceLevelLoad);
    }

    private void MatomoWin()
    {
        //matomo
        var tracker = trackerData.CreateTracker();
        var session = tracker.CreateSession(SystemInfo.deviceUniqueIdentifier);
        session.Record(SceneManager.GetActiveScene().name, platform + "Win/End", Time.timeSinceLevelLoad);
    }

    private void MatomoRestart()
    {
        //matomo
        var tracker = trackerData.CreateTracker();
        var session = tracker.CreateSession(SystemInfo.deviceUniqueIdentifier);
        session.Record(SceneManager.GetActiveScene().name, platform + "/Restart", Time.timeSinceLevelLoad);
    }

    private void MatomoQuit()
    {
        //matomo
        var tracker = trackerData.CreateTracker();
        var session = tracker.CreateSession(SystemInfo.deviceUniqueIdentifier);
        session.Record(SceneManager.GetActiveScene().name, platform + "/Quit", Time.timeSinceLevelLoad);
    }

    private void MatomoStarted()
    {
        //matomo
        var tracker = trackerData.CreateTracker();
        var session = tracker.CreateSession(SystemInfo.deviceUniqueIdentifier);
        session.Record(SceneManager.GetActiveScene().name, platform + "/Start", 0);
    }

    public void ScoreDown(int track, float amount)
    {
        if (healthScoreCoroutine != null) StopCoroutine(healthScoreCoroutine);
        newHealthScore -= amount;

        //lost
        if (newHealthScore < 0f && !lostSceneShowed)
        {
            ShowLostScene();
            Conductor.paused = true;
            lostSceneShowed = true;
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

    private IEnumerator QuitFade()
    {
        pauseScene.SetActive(false);
        var elapsedTime = 0f;
        while (elapsedTime < 0.3f)
        {
            elapsedTime += Time.deltaTime;
            var a = Mathf.Lerp(0.85f, 1f, elapsedTime / 0.3f);
            var c = new Color(0, 0, 0, a);
            fadePanel.color = c;
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, 1f);
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
        TakeAction(0);
    }

    public void ShowWinScene()
    {
        MatomoWin();
        StartCoroutine(ShowWinRoutine());
    }

    public void ShowLostScene()
    {
        MatomoLost();
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
        MatomoRestart();
        TakeAction(1);
    }

    public void HomeButtonOnClick()
    {
        StartCoroutine(QuitFade());
        MatomoQuit();
        TakeAction(2);
    }

    public void TakeAction(int action)
    {
        Advertisement.Show();
        switch (action)
        {
            case 0:
                SceneManager.LoadScene(scenes[nextLevel]);
                break;
            case 1:
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            case 2:
                SceneManager.LoadScene("MainMenu");
                break;
        }
    }
}