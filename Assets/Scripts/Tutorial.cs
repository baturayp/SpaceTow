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
    private bool message0, message1;
    private bool tapped, swiped;
    private Coroutine blink;
    public Conductor conductor;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }
    
    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(StartRoutine());
    }

    public void OnNextButton()
    {
        StartCoroutine(SkipRoutine());
    }

    private void Update()
    {
        tapped = false; swiped = false;

        if (Conductor.songposition > 2.95f)
        {
            if (!message0) ShowMessage(0, "Single tap to punch\nwhen a meteor glows red");
        }

        if (Conductor.songposition > 6.8f)
        {
            if (!message1) ShowMessage(1, "Swipe to avoid objects\nthat are not meteors");
        }

        if (Conductor.songposition > 10f)
        {
            StartCoroutine(SkipRoutine());
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
        switch (message)
        {
            case 0:
                message0 = true;
                break;
            case 1:
                message1 = true;
                break;
        }

        StartCoroutine(MessageRoutine(message, text));
    }

    private IEnumerator MessageRoutine(int message, string text)
    {
        var elapsedTime = 0f;
        Conductor.paused = true;
        messageText.text = text;
        while (elapsedTime < 0.3f)
        {
            elapsedTime += Time.deltaTime;
            var v = Mathf.Lerp(0f, 1f, elapsedTime / 0.3f);
            var c = new Color (textColor.r, textColor.g, textColor.b, v);
            messageText.color = c;
            yield return null;
        }

        switch (message)
        {
            case 0:
            {
                blink = StartCoroutine(TapRoutine());
                yield return new WaitUntil(() => tapped);
                if (blink != null) StopCoroutine(blink);
                tapIcon.SetActive(false);
                Conductor.paused = false;
                conductor.Inputted();
                break;
            }
            case 1:
            {
                blink = StartCoroutine(SwipeRoutine());
                yield return new WaitUntil(() => swiped);
                if (blink != null) StopCoroutine(blink);
                swipeIcon.SetActive(false);
                Conductor.paused = false;
                conductor.Avoid(3);
                break;
            }
        }

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
    
    private IEnumerator StartRoutine()
    {
        var elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            var a = Mathf.Lerp(1f, 0f, elapsedTime / 1f);
            fadePanel.color = new Color (0, 0, 0, a);
            yield return null;
        }
        nextIcon.SetActive(true);
    }

    private IEnumerator SkipRoutine()
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
        SceneManager.LoadScene("MainMenu");
    }
}