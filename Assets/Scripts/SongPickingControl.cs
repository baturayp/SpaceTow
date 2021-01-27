using UnityEngine;
using System.Collections;

public class SongPickingControl : MonoBehaviour
{
	[Header("Setting objects")]
	public GameObject settingsButton;
	public GameObject settingsLayer;
	public GameObject aboutLayer;
	public static bool _settingsIsActive = false;
	private static bool _secondBoardIsActive = false;

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

	private void Awake()
	{
		//check if song messenger exists (if player returned from other scenes)
		if (SongInfoMessenger.instance != null)
		{
			//Placeholder. Do something here.
		}
		//if not exist, create new SongInfoMessenger
		else
		{
			Instantiate(songInfoMessengerPrefab);
		}
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
		while (elapsedTime < 0.2f)
		{
			elapsedTime += Time.deltaTime;
			aboutLayer.transform.localScale = new Vector3(0 + 5 * elapsedTime, 0 + 5 * elapsedTime, 1);
			yield return null;
		}
		aboutLayer.transform.localScale = new Vector3(1, 1, 1);
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
		else if (_settingsIsActive) FlipToFirstFromSettings();
		else FlipToSettingsFromFirst();
	}

	public void FlipToSettingsFromFirst()
	{
		StartCoroutine(FlipAnimCoroutine( firstBoardTransform, Vector3.zero, horizontalFlip, FlipAnimationDuration, firstBoardTransform.pivot, topEdgePivot));
		StartCoroutine(FlipAnimCoroutine( settingsBoardTransform, horizontalFlip, Vector3.zero, FlipAnimationDuration, settingsBoardTransform.pivot, bottomEdgePivot));
		_settingsIsActive = true;
		aboutLayer.SetActive(false);
		settingsLayer.SetActive(true);
	}

	public void FlipToFirstFromSettings()
	{
		StartCoroutine(FlipAnimCoroutine( settingsBoardTransform, Vector3.zero, horizontalFlip, FlipAnimationDuration, settingsBoardTransform.pivot, bottomEdgePivot));
		StartCoroutine(FlipAnimCoroutine( firstBoardTransform, horizontalFlip, Vector3.zero, FlipAnimationDuration, firstBoardTransform.pivot, topEdgePivot));
		_settingsIsActive = false;
	}

	public void AboutToggle()
	{
		StartCoroutine(AboutAnim());
	}
}