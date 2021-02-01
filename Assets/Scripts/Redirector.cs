using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Redirector : MonoBehaviour
{
    private void Start()
    {
        var lastLevel = PlayerPrefs.GetInt("lastLevel", 0);
        SceneManager.LoadScene(lastLevel == 0 ? "Tutorial" : "MainMenu");
    }
}