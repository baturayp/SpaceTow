using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Logic : MonoBehaviour
{
	private const float MAX_SWIPE_TIME = 0.5f;
	private const float MIN_SWIPE_DISTANCE = 0.10f;
	public static bool swipedRight = false;
	public static bool swipedLeft = false;
	public static bool swipedUp = false;
	public static bool swipedDown = false;
	private Vector2 startPos;
	private float startTime;

	//stars
	public MeshRenderer staticStars;
	private Material starsMat;
	private Coroutine routine;

	//camera selection
	public GameObject[] cams;
	public Image[] uiElements;
	private int len, cur;
	public Color[] colors;
	public Color[] uiColors;
	private int lastColor;
	private string[] scenes = { "Beach", "Station", "Chapel", "OrangeCyan", "GreenYellow" };

	private void Start()
	{
		len = cams.Length;
		lastColor = cur = 0;
		starsMat = staticStars.material;
		starsMat.SetColor("_NoiseColor", colors[0]);
	}

	public void Update()
	{
		swipedRight = false;
		swipedLeft = false;
		swipedUp = false;
		swipedDown = false;

		if (Input.touches.Length > 0)
		{
			Touch t = Input.GetTouch(0);
			if (t.phase == TouchPhase.Began)
			{
				startPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width);
				startTime = Time.time;
			}

			if (t.phase == TouchPhase.Ended)
			{
				if (Time.time - startTime > MAX_SWIPE_TIME) // press too long
					return;

				Vector2 endPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width);

				Vector2 swipe = new Vector2(endPos.x - startPos.x, endPos.y - startPos.y);

				if (swipe.magnitude < MIN_SWIPE_DISTANCE) // Too short swipe
					return;

				if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
				{ // Horizontal swipe
					if (swipe.x > 0) swipedRight = true;
					else swipedLeft = true;
				}
				else
				{ // Vertical swipe
					if (swipe.y > 0) swipedUp = true;
					else swipedDown = true;
				}
			}
		}

        //use arrows to trigger swipe
		swipedDown = swipedDown || Input.GetKeyDown(KeyCode.DownArrow);
		swipedUp = swipedUp || Input.GetKeyDown(KeyCode.UpArrow);
		swipedRight = swipedRight || Input.GetKeyDown(KeyCode.RightArrow);
		swipedLeft = swipedLeft || Input.GetKeyDown(KeyCode.LeftArrow);

		if (swipedRight) SwipedRight();
		if (swipedLeft) SwipedLeft();
	}

	public void SwipedRight()
	{
		for (int i = 0; i < len; i++) 
			cams[i].SetActive(false);
		cur = --cur % len;
		if (cur < 0) cur = len - 1;
		cams[cur].SetActive(true);
		if (routine != null) StopCoroutine(routine);
		routine = StartCoroutine(SetColor(cur));
	}

	public void SwipedLeft()
	{
		for (int i = 0; i < len; i++) 
			cams[i].SetActive(false);
		cur = ++cur % len;
		cams[cur].SetActive(true);
		if (routine != null) StopCoroutine(routine);
		routine = StartCoroutine(SetColor(cur));
	}

	public void LevelSelect()
	{
		SceneManager.LoadScene(scenes[cur]);
	}

	private IEnumerator SetColor(int cur)
	{
		var elapsedTime = 0f;
		while (elapsedTime < 1f)
		{
			elapsedTime += Time.deltaTime;
			//set skybox color
			var r = Color.Lerp(colors[lastColor], colors[cur], elapsedTime / 1f);
			starsMat.SetColor("_NoiseColor", r);
			//set ui colors
			var u = Color.Lerp(uiColors[lastColor], uiColors[cur], elapsedTime / 1f);
			SetUIColors(u);
			yield return null;
		}
		lastColor = cur;
		routine = null;
	}

	private void SetUIColors(Color col)
	{
		var length = uiElements.Length;
		for (int i = 0; i < length; i++)
		{
			uiElements[i].color = col;
		}
		uiElements[length - 1].color = new Color(col.r, col.g, col.b, 0.4f);
	}
}