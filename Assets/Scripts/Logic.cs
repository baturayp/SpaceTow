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
	public Image leftButton, rightButton, songInfoFrame, songBkg, playBtn;
	public Text songTitle, artistName;
	private int lastColor;
	private readonly string[] scenes = { "Beach", "Toxic", "Station", "Chapel", "Barn" };
	private readonly string[] tracks = { "Fright Night Twist", "Run!", "Mystica", "Twelve Days", "Born Barnstormers" };
	private readonly string[] artists = { "Bryan Teoh", "Komiku", "Alexander Nakarada", "Alexander Nakarada", "Brian Boyko" };

	private void Start()
	{
		len = cams.Length;
		lastColor = cur = 0;
		starsMat = staticStars.material;
		starsMat.SetColor("_NoiseColor", colors[0]);
		SetCurrent(cur);
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
		if (routine != null) return;
		for (int i = 0; i < len; i++) 
			cams[i].SetActive(false);
		cur = --cur % len;
		if (cur < 0) cur = len - 1;
		cams[cur].SetActive(true);
		routine = StartCoroutine(SetColor(cur));
	}

	public void SwipedLeft()
	{
		if (routine != null) return;
		for (int i = 0; i < len; i++) 
			cams[i].SetActive(false);
		cur = ++cur % len;
		cams[cur].SetActive(true);
		routine = StartCoroutine(SetColor(cur));
	}

	public void LevelSelect()
	{
		SceneManager.LoadScene(scenes[cur]);
	}

	private void SetCurrent(int cr)
	{
		var color = colors[cr];
		starsMat.SetColor("_NoiseColor", color);
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
		songTitle.color = artistName.color = new Color(1, 1, 1, 1f);
	}

	private IEnumerator SetColor(int cur)
	{
		StartCoroutine(SongSelectionUI());
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
			songTitle.color = artistName.color = new Color(1, 1, 1, a);
			yield return null;
		}

		songTitle.text = tracks[cur];
		artistName.text = "by " + artists[cur];

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
			songTitle.color = artistName.color = new Color(1, 1, 1, a);
			yield return null;
		}
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