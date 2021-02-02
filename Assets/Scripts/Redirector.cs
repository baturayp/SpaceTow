using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Redirector : MonoBehaviour
{
    public Texture2D loadingIcon;
    private void Start()
    {
        var lastLevel = PlayerPrefs.GetInt("lastLevel", 0);
        SceneManager.LoadScene(lastLevel == 0 ? "Tutorial" : "MainMenu");
    }

    private void OnGUI ()
	{
		GUI.DrawTexture (new Rect (Screen.width / 2 - 59, Screen.height / 2 - 75, 118, 150), loadingIcon);
	}
}