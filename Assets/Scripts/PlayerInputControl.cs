using UnityEngine;

public class PlayerInputControl : MonoBehaviour
{

    public delegate void InputtedAction(int trackNumber);
    public static event InputtedAction InputtedEvent;

    public delegate void KeyupAction(int trackNumber);
    public static event KeyupAction KeyupEvent;

    public BoxCollider2D[] tappingBoxes;

    //in unity editor & standalone, input by keyboard
#if UNITY_EDITOR || UNITY_STANDALONE
    private KeyCode[] keybindings;
    private KeyCode pauseKey;
#endif
    //cache the number of tracks
    private int trackLength;

    void Start()
    {
        trackLength = 2;
#if UNITY_EDITOR || UNITY_STANDALONE
        keybindings = new KeyCode[4];
        keybindings[0] = KeyboardInputManager.instance.GetKeyCode(KeyboardInputManager.KeyBindings.Track1);
        keybindings[1] = KeyboardInputManager.instance.GetKeyCode(KeyboardInputManager.KeyBindings.Track2);
        pauseKey = KeyboardInputManager.instance.GetKeyCode(KeyboardInputManager.KeyBindings.Pause);
#endif
    }

    void Update()
    {
        if (Conductor.paused) return;
        //keyboard input
#if UNITY_EDITOR || UNITY_STANDALONE
        for (int i = 0; i < trackLength; i++)
        {
            if (Input.GetKeyDown(keybindings[i]))
            {
                Inputted(i);
            }
            if (Input.GetKeyUp(keybindings[i]))
            {
                Keyup(i);
            }
        }
        if (Input.GetKeyDown(pauseKey))
        {
            FindObjectOfType<PlayingUIController>().PauseButtonOnClick();
        }
#endif

        //touch input
#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
        //check touch input
        foreach (Touch touch in Input.touches)
        {
            //tap down
            if (touch.phase == TouchPhase.Began)
            {
                //check if on a tapping sphere
                for (int i = 0; i < trackLength; i++)
                {
                    if (tappingBoxes[i].OverlapPoint(Camera.main.ScreenToWorldPoint(touch.position)))
                    {
                        Inputted(i);
                    }
                }
            }
            //tap ended
            if (touch.phase == TouchPhase.Ended)
            {
                //check if on a tapping sphere
                for (int i = 0; i < trackLength; i++)
                {
                    if (tappingBoxes[i].OverlapPoint(Camera.main.ScreenToWorldPoint(touch.position)))
                    {
                        Keyup(i);
                    }
                }
            }
        }
#endif
    }

    void Inputted(int i)
    {
        //inform Conductor and other interested classes
        InputtedEvent?.Invoke(i);
    }

    void Keyup(int i)
    {
        KeyupEvent?.Invoke(i);
    }
}
