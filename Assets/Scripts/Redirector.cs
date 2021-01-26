using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Redirector : MonoBehaviour
{
    private readonly string[] scenes = { "Tutorial", "Chapel", "Beach", "Barn", "Toxic", "Station" };

    private void Start()
    {
        var lastLevel = PlayerPrefs.GetInt("lastLevel", 0);
        if (lastLevel < 5) SceneManager.LoadScene(scenes[lastLevel]);
        else SceneManager.LoadScene("MainMenu");
    }
}