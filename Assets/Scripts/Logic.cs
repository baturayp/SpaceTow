using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Logic : MonoBehaviour
{
    private SongInfo currSong;
    public SongCollection[] songCollections;

    public void SelectSong()
    {
        currSong = songCollections[0].songSets[0].song;
        SongInfoMessenger.Instance.currentSong = currSong;
        SongInfoMessenger.Instance.currentCollection = songCollections[0];
        SongInfoMessenger.Instance.currSongNumber = 0;
        SongInfoMessenger.Instance.currCollNumber = 0;
        SceneManager.LoadSceneAsync(1);
    }
}
