using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Redirector : MonoBehaviour
{
    private readonly string[] scenes = { "Tutorial", "Beach", "Toxic", "Station", "Chapel", "Barn" };

    private void Start()
    {
        //var lastLevel = PlayerPrefs.GetInt("lastLevel", 0);
    }
}