using UnityEngine;
using System.Collections;

public class SongPickingControl : MonoBehaviour
{
	[Header("Setting objects")]
	public GameObject settingsButton;
	public GameObject settingsLayer;
	public GameObject aboutLayer;
	public static bool settingsIsActive = false;
	public static bool secondBoardIsActive = false;

	[Header("Board transforms")]
	//First Board
	public RectTransform firstBoardTransform;
	public RectTransform secondBoardTransform;
	public RectTransform settingsBoardTransform;

	//the messenger to pass through other scenes
	public GameObject songInfoMessengerPrefab;

	//animation
	private RotationAnimation flipInAnimation;
	private RotationAnimation flipOutAnimation;
	private const float FlipAnimationDuration = 0.25f;

	//rotation
	private Vector3 verticalFlip = new Vector3(90f, 0f, 0f);
	private Vector3 horizontalFlip = new Vector3(0f, 90f, 0f);

	//pivot
	private Vector2 topEdgePivot = new Vector2(0.5f, 1f);
	private Vector2 bottomEdgePivot = new Vector2(0.5f, 0f);

	void Awake()
	{
		//init animation
		flipInAnimation = new RotationAnimation();
		flipOutAnimation = new RotationAnimation();

		//check if song messenger exists (if player returned from other scenes)
		if (SongInfoMessenger.Instance != null)
		{
			//Placeholder. Do something here.
		}
		//if not exist, create new SongInfoMessenger
		else
		{
			Instantiate(songInfoMessengerPrefab);
		}
	}

	void Start()
	{
		//LogicScript.OpenSongBoardEvent += FlipToSongFromFirst;
	}

    void OnDestroy()
    {
		//LogicScript.OpenSongBoardEvent -= FlipToSongFromFirst;
	}


    /*menu transition and animations
	they can be simplified !*/
    public void FlipToFirstFromSong()
    {
        StartCoroutine(flipOutAnimation.AnimationCoroutine(
            secondBoardTransform,
            Vector3.zero,
            verticalFlip,
            FlipAnimationDuration,
            secondBoardTransform.pivot,
            bottomEdgePivot
        ));
        StartCoroutine(flipInAnimation.AnimationCoroutine(
            firstBoardTransform,
            verticalFlip,
            Vector3.zero,
            FlipAnimationDuration,
            firstBoardTransform.pivot,
            topEdgePivot
        ));
		secondBoardIsActive = false;
    }

    public void FlipToSongFromFirst()
    {
        StartCoroutine(flipOutAnimation.AnimationCoroutine(
            firstBoardTransform,
            Vector3.zero,
            verticalFlip,
            FlipAnimationDuration,
            firstBoardTransform.pivot,
            topEdgePivot
        ));
        StartCoroutine(flipInAnimation.AnimationCoroutine(
            secondBoardTransform,
            verticalFlip,
            Vector3.zero,
            FlipAnimationDuration,
            secondBoardTransform.pivot,
            bottomEdgePivot
        ));
		secondBoardIsActive = true;
    }

    public void SettingsButtonToggle()
	{
		if (secondBoardIsActive)
        {
			FlipToFirstFromSong();
        }
		else if (settingsIsActive)
		{
			FlipToFirstFromSettings();
		}
		else
		{
			FlipToSettingsFromFirst();
		}
	}

	public void FlipToSettingsFromFirst()
	{
		StartCoroutine(flipOutAnimation.AnimationCoroutine(
			firstBoardTransform,
			Vector3.zero,
			horizontalFlip,
			FlipAnimationDuration,
			firstBoardTransform.pivot,
			topEdgePivot
		));
		StartCoroutine(flipInAnimation.AnimationCoroutine(
			settingsBoardTransform,
			horizontalFlip,
			Vector3.zero,
			FlipAnimationDuration,
			settingsBoardTransform.pivot,
			bottomEdgePivot
		));
		settingsIsActive = true;
		aboutLayer.SetActive(false);
		settingsLayer.SetActive(true);
	}

	public void FlipToFirstFromSettings()
	{
		StartCoroutine(flipOutAnimation.AnimationCoroutine(
			settingsBoardTransform,
			Vector3.zero,
			horizontalFlip,
			FlipAnimationDuration,
			settingsBoardTransform.pivot,
			bottomEdgePivot
		));
		StartCoroutine(flipInAnimation.AnimationCoroutine(
			firstBoardTransform,
			horizontalFlip,
			Vector3.zero,
			FlipAnimationDuration,
			firstBoardTransform.pivot,
			topEdgePivot
		));
		settingsIsActive = false;
	}

	public void AboutToggle()
	{
		StartCoroutine(AboutAnim());
	}

	IEnumerator AboutAnim()
	{
		float elapsedTime = 0.0f;
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
}
