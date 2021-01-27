using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Redirector : MonoBehaviour
{
    private readonly string[] scenes = { "Tutorial", "Chapel", "Beach", "Barn", "Toxic", "Station" };

    private void Start()
    {
        SceneManager.LoadScene("MainMenu");
    }
}