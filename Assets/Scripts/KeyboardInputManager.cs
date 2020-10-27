using UnityEngine;

public class KeyboardInputManager : MonoBehaviour
{
    public static KeyboardInputManager instance;
    private KeyCode[] keys;
    public enum KeyBindings {Track1 = 0, Track2, Track3, Pause};
    private const string Default1 = "A";
    private const string Default2 = "S";
    private const string Default3 = "D";
    private const string DefaultPause = "P";

    public KeyCode GetKeyCode(KeyBindings keyBinding)
    {
        return keys[(int)keyBinding];
    }

    void Awake()
    {
        //singleton
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        keys = new KeyCode[4];
        keys[(int)KeyBindings.Track1] = (KeyCode)System.Enum.Parse(typeof(KeyCode), Default1);
        keys[(int)KeyBindings.Track2] = (KeyCode)System.Enum.Parse(typeof(KeyCode), Default2);
        keys[(int)KeyBindings.Track3] = (KeyCode)System.Enum.Parse(typeof(KeyCode), Default3);
        keys[(int)KeyBindings.Pause] = (KeyCode)System.Enum.Parse(typeof(KeyCode), DefaultPause);
    }
}
