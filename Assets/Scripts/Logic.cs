using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
	}
}