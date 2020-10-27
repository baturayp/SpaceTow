using UnityEngine;
using System;

//selected song info passed along scenes

public class SongInfoMessenger : MonoBehaviour
{
    public static SongInfoMessenger Instance = null;

    [NonSerialized] public SongInfo currentSong;
    [NonSerialized] public SongCollection currentCollection;
    [NonSerialized] public int currSongNumber;
    [NonSerialized] public int currCollNumber;

    void Start()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
}
