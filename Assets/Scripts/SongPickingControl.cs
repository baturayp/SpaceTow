using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SongPickingControl : MonoBehaviour
{
	[Header("Setting objects")]
	public GameObject settingsButton;
	public GameObject settingsLayer;
	public GameObject aboutLayer;
	public static bool settingsIsActive = false;
	public static bool _secondBoardIsActive = false;

	public GameObject[] aboutSongLayers;

	[Header("Board transforms")]
	//First Board
	public RectTransform firstBoardTransform;
	public RectTransform secondBoardTransform;
	public RectTransform settingsBoardTransform;

	//the messenger to pass through other scenes
	public GameObject songInfoMessengerPrefab;

	private const float FlipAnimationDuration = 0.25f;

	//rotation
	private readonly Vector3 verticalFlip = new Vector3(90f, 0f, 0f);
	private readonly Vector3 horizontalFlip = new Vector3(0f, 90f, 0f);

	//pivot
	private readonly Vector2 topEdgePivot = new Vector2(0.5f, 1f);
	private readonly Vector2 bottomEdgePivot = new Vector2(0.5f, 0f);

    private void Start()
    {
		settingsIsActive = false;
		_secondBoardIsActive = false;
    }

    //menu flip animations
    private static IEnumerator FlipAnimCoroutine(RectTransform rectT, Vector3 start, Vector3 end, float duration, Vector2 originalPivot, Vector2 animationPivot)
	{
		rectT.pivot = animationPivot;
		var i = 0f;
		while (i <= 1f)
		{
			i += Time.deltaTime / duration;
			rectT.transform.localRotation = Quaternion.Euler(Vector3.Lerp(start, end, i));
			yield return null;
		}
		rectT.pivot = originalPivot;
	}

	private IEnumerator AboutAnim()
	{
		var elapsedTime = 0.0f;
		aboutLayer.SetActive(true);
		settingsLayer.SetActive(false);
		while (elapsedTime < 0.15f)
		{
			elapsedTime += Time.deltaTime;
			aboutLayer.transform.localScale = new Vector3(0 + 9f * elapsedTime, 0 + 9f * elapsedTime, 1);
			yield return null;
		}
		elapsedTime = 0.0f;
		while (elapsedTime < 0.1f)
		{
			elapsedTime += Time.deltaTime;
			aboutLayer.transform.localScale = new Vector3(1.35f - 3.5f * elapsedTime, 1.35f - 3.5f * elapsedTime, 1);
			yield return null;
		}
		aboutLayer.transform.localScale = new Vector3(1, 1, 1);
	}

	private IEnumerator AboutSongsAnim()
	{
		aboutSongLayers[0].SetActive(false); aboutSongLayers[1].SetActive(false); aboutSongLayers[2].SetActive(false);
		aboutSongLayers[3].SetActive(false); aboutSongLayers[4].SetActive(false);
		aboutSongLayers[Logic.currentSong].SetActive(true);
		settingsLayer.SetActive(false);
		var elapsedTime = 0.0f;
		while (elapsedTime < 0.15f)
		{
			elapsedTime += Time.deltaTime;
			aboutSongLayers[Logic.currentSong].transform.localScale = new Vector3(0 + 9f * elapsedTime, 0 + 9f * elapsedTime, 1);
			yield return null;
		}
		elapsedTime = 0.0f;
		while (elapsedTime < 0.1f)
		{
			elapsedTime += Time.deltaTime;
			aboutSongLayers[Logic.currentSong].transform.localScale = new Vector3(1.35f - 3.5f * elapsedTime, 1.35f - 3.5f * elapsedTime, 1);
			yield return null;
		}
		aboutSongLayers[Logic.currentSong].transform.localScale = new Vector3(1, 1, 1);
	}

    public void FlipToFirstFromSong()
    {
        StartCoroutine(FlipAnimCoroutine( secondBoardTransform, Vector3.zero, verticalFlip, FlipAnimationDuration, secondBoardTransform.pivot, bottomEdgePivot));
        StartCoroutine(FlipAnimCoroutine( firstBoardTransform, verticalFlip, Vector3.zero, FlipAnimationDuration, firstBoardTransform.pivot, topEdgePivot));
		_secondBoardIsActive = false;
    }

    public void FlipToSongFromFirst()
    {
        StartCoroutine(FlipAnimCoroutine( firstBoardTransform, Vector3.zero, verticalFlip, FlipAnimationDuration, firstBoardTransform.pivot, topEdgePivot));
        StartCoroutine(FlipAnimCoroutine( secondBoardTransform, verticalFlip, Vector3.zero, FlipAnimationDuration, secondBoardTransform.pivot, bottomEdgePivot));
		_secondBoardIsActive = true;
    }

    public void SettingsButtonToggle()
	{
		if (_secondBoardIsActive) FlipToFirstFromSong();
		else if (settingsIsActive) FlipToFirstFromSettings();
		else FlipToSettingsFromFirst();
	}

	public void FlipToSettingsFromFirst()
	{
		StartCoroutine(FlipAnimCoroutine( firstBoardTransform, Vector3.zero, horizontalFlip, FlipAnimationDuration, firstBoardTransform.pivot, topEdgePivot));
		StartCoroutine(FlipAnimCoroutine( settingsBoardTransform, horizontalFlip, Vector3.zero, FlipAnimationDuration, settingsBoardTransform.pivot, bottomEdgePivot));
		settingsIsActive = true;
		aboutLayer.SetActive(false);
		aboutSongLayers[0].SetActive(false); aboutSongLayers[1].SetActive(false); aboutSongLayers[2].SetActive(false);
		aboutSongLayers[3].SetActive(false); aboutSongLayers[4].SetActive(false);
		settingsLayer.SetActive(true);
	}

	public void FlipToFirstFromSettings()
	{
		StartCoroutine(FlipAnimCoroutine( settingsBoardTransform, Vector3.zero, horizontalFlip, FlipAnimationDuration, settingsBoardTransform.pivot, bottomEdgePivot));
		StartCoroutine(FlipAnimCoroutine( firstBoardTransform, horizontalFlip, Vector3.zero, FlipAnimationDuration, firstBoardTransform.pivot, topEdgePivot));
		settingsIsActive = false;
	}

	public void VisitURL(string website)
	{
		Application.OpenURL(website);
	}

	public void TutorialButton()
	{
		SceneManager.LoadScene("Tutorial");
	}
	public void AboutToggle()
	{
		StartCoroutine(AboutAnim());
	}
	public void AboutSongsToggle()
	{
		StartCoroutine(AboutSongsAnim());
	}
}