using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    public Text messageText;
    public Color textColor;
    public GameObject tapIcon, swipeIcon, nextIcon;
    public Image fadePanel;
    private bool[] messageViewed = new bool[3];
    private bool tapped, swiped;
    private Coroutine blink;

    private void Start()
    {
        // var nextLevel = PlayerPrefs.GetInt("lastLevel", 0);
        // if (nextLevel > 0)
        // {
        //     SceneManager.LoadScene(nextLevel);
        // }
    }

    public void OnNextButton()
    {
        //PlayerPrefs.SetInt("lastLevel", 1);
        SceneManager.LoadScene("MainMenu");
    }

    private void Update()
    {
        tapped = false; swiped = false;

        if (Conductor.songposition > 2.95f)
        {
            if (!messageViewed[0]) ShowMessage(0, "Single tap to punch\nwhen a meteor glows red");
        }

        if (Conductor.songposition > 6.95f)
        {
            if (!messageViewed[1]) ShowMessage(1, "Swipe to avoid objects\nthat are not meteors");
        }

        if (Conductor.songposition > 10f)
        {
            StartCoroutine(FadeRoutine());
        }

        if (Input.touches.Length > 0)
		{
			var t = Input.GetTouch(0);
			switch (t.phase)
			{
				case TouchPhase.Began:
					tapped = true;
					break;
				case TouchPhase.Moved:
				{
					if (t.deltaPosition.x > 20) swiped = true;
					break;
				}
			}
		}
        if (Input.GetKeyDown(KeyCode.Space)) tapped = swiped = true;
    }

    private void ShowMessage(int message, string text)
    {
        messageViewed[message] = true;
        StartCoroutine(MessageRoutine(message, text));
    }

    private IEnumerator MessageRoutine(int message, string text)
    {
        var elapsedTime = 0f;
        Conductor.paused = true;
        messageText.text = text;
        elapsedTime = 0f;
        while (elapsedTime < 0.3f)
        {
            elapsedTime += Time.deltaTime;
            var v = Mathf.Lerp(0f, 1f, elapsedTime / 0.3f);
            var c = new Color (textColor.r, textColor.g, textColor.b, v);
            messageText.color = c;
            yield return null;
        }
        
        if (message == 0) 
        {
            blink = StartCoroutine(TapRoutine());
            yield return new WaitUntil(() => tapped);
            if (blink != null) StopCoroutine(blink);
            tapIcon.SetActive(false);
            Conductor.punchedFromTutorial = true;
        }
        
        if (message == 1) 
        {
            blink = StartCoroutine(SwipeRoutine());
            yield return new WaitUntil(() => swiped);
            if (blink != null) StopCoroutine(blink);
            swipeIcon.SetActive(false);
            Conductor.swipedFromTutorial = true;
        }
        
        Conductor.paused = false;
        elapsedTime = 0f;
        while (elapsedTime < 0.3f)
        {
            elapsedTime += Time.deltaTime;
            var v = Mathf.Lerp(1f, 0f, elapsedTime / 0.3f);
            var c = new Color (textColor.r, textColor.g, textColor.b, v);
            messageText.color = c;
            yield return null;
        }
    }

    private IEnumerator TapRoutine()
    {
        var times = 0;
        while (times < 4)
        {
            times++;
            tapIcon.SetActive(true);
            yield return new WaitForSeconds(0.25f);
            tapIcon.SetActive(false);
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(2f);
        blink = StartCoroutine(TapRoutine());
    }

    private IEnumerator SwipeRoutine()
    {
        swipeIcon.SetActive(true);
        var elapsedTime = 0f;
        while (elapsedTime < 2.9f)
        {
            elapsedTime += Time.deltaTime;
            var p = Mathf.Repeat(elapsedTime * 50, 75);
            swipeIcon.transform.localPosition = new Vector3(p, 0, 0);
            yield return null;
        }
        swipeIcon.SetActive(false);
        yield return new WaitForSeconds(2f);
        blink = StartCoroutine(SwipeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        nextIcon.SetActive(false);
        var elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            var a = Mathf.Lerp(0f, 1f, elapsedTime / 1f);
            fadePanel.color = new Color (0, 0, 0, a);
            yield return null;
        }
        OnNextButton();
    }
}
