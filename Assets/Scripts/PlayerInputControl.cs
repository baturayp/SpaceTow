using UnityEngine;

public class PlayerInputControl : MonoBehaviour
{
    public delegate void KeyDownAction(int trackNumber);
    public static event KeyDownAction KeyDownEvent;

    //in unity editor & standalone, input by keyboard
#if UNITY_EDITOR || UNITY_STANDALONE
    private KeyCode[] keybindings;
#endif
    //cache the number of tracks
    private int trackLength;

    void Start()
    {
        trackLength = 2;
#if UNITY_EDITOR || UNITY_STANDALONE
        keybindings = new KeyCode[2];
        keybindings[0] = (KeyCode)System.Enum.Parse(typeof(KeyCode), "A");
        keybindings[1] = (KeyCode)System.Enum.Parse(typeof(KeyCode), "D");
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
        }
#endif
    }

    void Inputted(int i)
    {
        //inform Conductor and other interested classes
        KeyDownEvent?.Invoke(i);
    }
}
