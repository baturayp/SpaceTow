using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Redirector : MonoBehaviour
{
    private readonly string[] scenes = { "Tutorial", "Chapel", "Beach", "Barn", "Toxic", "Station" };
    private static bool _alreadyStarted;

    private void Start()
    {
        var lastLevel = PlayerPrefs.GetInt("lastLevel", 0);
        if (!_alreadyStarted && lastLevel == 0)
        {
            SceneManager.LoadScene("Tutorial");
            _alreadyStarted = true;
        }
        else if (!_alreadyStarted && lastLevel > 0)
        {
            SceneManager.LoadScene("MainMenu");
            _alreadyStarted = true;
        }
        else
        {
            SceneManager.LoadScene(lastLevel < 5 ? scenes[lastLevel] : "MainMenu");
        }
    }
}