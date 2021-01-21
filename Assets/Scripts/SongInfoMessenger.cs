using UnityEngine;
using System;

//selected song info passed along scenes

public class SongInfoMessenger : MonoBehaviour
{
    public static SongInfoMessenger instance = null;

    [NonSerialized] public SongInfo currentSong;
    [NonSerialized] public int currSongNumber;

    private void Start()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);
    }
}