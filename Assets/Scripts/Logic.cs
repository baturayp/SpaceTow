﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Logic : MonoBehaviour
{
	private const float MAXSwipeTime = 0.5f;
	private const float MINSwipeDistance = 0.10f;
	private Vector2 startPos;
	private float startTime;

	//stars
	public MeshRenderer staticStars;
	private Material starsMat;
	private Coroutine routine;

	//camera selection
	public GameObject[] cams;
	public GameObject[] planets;
	public Image[] uiElements;
	public Text[] texts;
	private int len, cur;
	public Color[] colors;
	public Color[] uiColors;
	public Image leftButton, rightButton, songInfoFrame, songBkg, playBtn;
	public Sprite playIcon, lockedIcon;
	public Toggle effectsToggle;
	public Text songTitle, artistName;
	public Image fadeLayer;
	public Animator lockAnimator;
	private int lastColor;
	private int lastLevel;
	private readonly string[] scenes = { "Chapel", "Beach", "Barn", "Toxic", "Station" };
	private readonly string[] tracks = { "Twelve Days", "Fright Night Twist", "Born Barnstormers", "Run!", "Mystica" };
	private readonly string[] artists = { "Alexander Nakarada", "Bryan Teoh", "Brian Boyko", "Komiku", "Alexander Nakarada" };
	private static readonly int NoiseColor = Shader.PropertyToID("_NoiseColor");
	private static readonly int MainColor = Shader.PropertyToID("_MainColor");

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
		StartCoroutine(FadeRoutine());
	}

	private void Start()
	{
		len = cams.Length;
		lastLevel = PlayerPrefs.GetInt("lastLevel", 1);
		lastColor = cur = lastLevel - 1;
		starsMat = staticStars.material;
		starsMat.SetColor(NoiseColor, colors[0]);
		SetCurrent(cur);

		for (int i = 0; i < 5; i++)
		{
			if (i > cur)
			{
				foreach (Transform child in planets[i].transform)
				{
					if (child.GetComponent<MeshRenderer>())
					{
						var cl = new Color (0.1f,0.1f,0.1f,1);
						child.GetComponent<MeshRenderer>().material.SetColor(MainColor, cl);
					}
				}
			}
		}

		//get effects pref
		effectsToggle.isOn = PlayerPrefs.GetInt("punchEffects", 1) == 1;
	}

	public void Update()
	{
		if (Input.touches.Length <= 0) return;
		var t = Input.GetTouch(0);
		switch (t.phase)
		{
			case TouchPhase.Began:
				startPos = new Vector2(t.position.x / Screen.width, t.position.y / Screen.width);
				startTime = Time.time;
				break;
			case TouchPhase.Ended when Time.time - startTime > MAXSwipeTime:
				return;
			case TouchPhase.Ended:
			{
				var endPos = new Vector2(t.position.x / Screen.width, t.position.y / Screen.width);
				var swipe = new Vector2(endPos.x - startPos.x, endPos.y - startPos.y);
				if (swipe.magnitude < MINSwipeDistance) // Too short swipe
					return;
				if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
				{ // Horizontal swipe
					if (swipe.x > 0) SwipedRight();
					else SwipedLeft();
				}
				break;
			}
		}
	}

	public void SwipedRight()
	{
		if (routine != null) return;
		for (var i = 0; i < len; i++) 
			cams[i].SetActive(false);
		cur = --cur % len;
		if (cur < 0) cur = len - 1;
		cams[cur].SetActive(true);
		routine = StartCoroutine(SetColor());
	}

	public void SwipedLeft()
	{
		if (routine != null) return;
		for (var i = 0; i < len; i++) 
			cams[i].SetActive(false);
		cur = ++cur % len;
		cams[cur].SetActive(true);
		routine = StartCoroutine(SetColor());
	}

	public void LevelSelect()
	{
		if (cur < lastLevel)
		{
			StartCoroutine(LevelSelectRoutine());
		}
		else lockAnimator.Play("lockIcon", 0);
	}

	public void EffectsToggle(bool tg)
	{
		PlayerPrefs.SetInt("punchEffects", tg ? 1 : 0);
	}

	private void SetCurrent(int cr)
	{
		var color = colors[cr];
		starsMat.SetColor(NoiseColor, color);
		SetUIColors(uiColors[cr]);
		for (int i = 0; i < len; i++)
			cams[i].SetActive(false);
		cams[cr].SetActive(true);
		songTitle.text = tracks[cr];
		artistName.text = "by " + artists[cr];
		rightButton.color =
				leftButton.color =
				songInfoFrame.color =
				playBtn.color =
				new Color(uiColors[cr].r, uiColors[cr].g, uiColors[cr].b, 1f);
		songBkg.color = new Color(uiColors[cr].r, uiColors[cr].g, uiColors[cr].b, 0.3f);
		songTitle.color = artistName.color = new Color(uiColors[cr].r, uiColors[cr].g, uiColors[cr].b, 1f);
		foreach (var text in texts)
		{
			text.color = uiColors[cr];
		}
	}

	private IEnumerator SetColor()
	{
		StartCoroutine(SongSelectionUI());
		var elapsedTime = 0f;
		while (elapsedTime < 1f)
		{
			elapsedTime += Time.deltaTime;
			//set skybox color
			var r = Color.Lerp(colors[lastColor], colors[cur], elapsedTime / 1f);
			starsMat.SetColor(NoiseColor, r);
			//set ui colors
			var u = Color.Lerp(uiColors[lastColor], uiColors[cur], elapsedTime / 1f);
			SetUIColors(u);
			yield return null;
		}
		lastColor = cur;
		routine = null;
	}

	private IEnumerator SongSelectionUI()
	{
		var elapsedTime = 0f;
		while (elapsedTime < 0.1f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(1f, 0f, elapsedTime / 0.1f);
			var b = Mathf.Lerp(0.3f, 0f, elapsedTime / 0.1f);
			rightButton.color = 
				leftButton.color = 
				songInfoFrame.color =
				playBtn.color =
				new Color(uiColors[lastColor].r, uiColors[lastColor].g, uiColors[lastColor].b, a);
			songBkg.color = new Color(uiColors[lastColor].r, uiColors[lastColor].g, uiColors[lastColor].b, b);
			songTitle.color = artistName.color = new Color(uiColors[lastColor].r, uiColors[lastColor].g, uiColors[lastColor].b, a);
			yield return null;
		}

		songTitle.text = tracks[cur];
		artistName.text = "by " + artists[cur];
		if (lastLevel > cur) playBtn.sprite = playIcon;
		else playBtn.sprite = lockedIcon;

		yield return new WaitForSeconds(0.6f);

		elapsedTime = 0f;
		while (elapsedTime < 0.15f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(0f, 1f, elapsedTime / 0.15f);
			var b = Mathf.Lerp(0f, 0.3f, elapsedTime / 0.15f);
			rightButton.color =
				leftButton.color =
				songInfoFrame.color =
				playBtn.color =
				new Color(uiColors[cur].r, uiColors[cur].g, uiColors[cur].b, a);
			songBkg.color = new Color(uiColors[cur].r, uiColors[cur].g, uiColors[cur].b, b);
			songTitle.color = artistName.color = new Color(uiColors[cur].r, uiColors[cur].g, uiColors[cur].b, a);
			yield return null;
		}

		foreach (var text in texts)
		{
			text.color = uiColors[cur];
		}
	}

	private void SetUIColors(Color col)
	{
		var length = uiElements.Length;
		for (var i = 0; i < length; i++)
		{
			uiElements[i].color = col;
		}
		uiElements[length - 1].color = new Color(col.r, col.g, col.b, 0.4f);
	}

	private IEnumerator FadeRoutine()
	{
		var elapsedTime = 0f;
		while (elapsedTime < 1f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(1f, 0f, elapsedTime / 1f);
			var c = new Color(0, 0, 0, a);
			fadeLayer.color = c;
			yield return null;
		}
	}

	private IEnumerator LevelSelectRoutine()
	{
		var elapsedTime = 0f;
		while (elapsedTime < 0.5f)
		{
			elapsedTime += Time.deltaTime;
			var a = Mathf.Lerp(0f, 1f, elapsedTime / 0.5f);
			var c = new Color(0, 0, 0, a);
			fadeLayer.color = c;
			yield return null;
		}
		SceneManager.LoadScene(scenes[cur]);
	}
}