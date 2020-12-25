using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Song Info")]
public class SongInfo : ScriptableObject
{
	[Header( "Set relevant song audio clip" )]
	public AudioClip song;

	[Header( "Set relevant song json" )]
	public TextAsset json;

	public string songTitle;

	[HideInInspector]
	public float songOffset;
	[HideInInspector]
	public float bpm;

	[Header("Initial default tempo 1 for normal, 0.5 half, 2 double etc")]
	public float tempo = 1f;

	[Header("Just for reference, notes populated automatically, edit them from NoteEditor")]
	public Track[] tracks = new Track[2];

	[Header("Adjusts how much notes appears on screen in advance. 4 is a fair default.")]
	public float beatsShownOnScreen = 4f;
	private int totalHits;

	[Header("When to finish scene shows up, in seconds")]
	public float endTime;

	//get the total hits of the song
	public int TotalHitCounts()
	{
		totalHits = 0;
		foreach (Track track in tracks)
		{
			foreach (Note note in track.notes)
			{
				totalHits += 1;
			}
		}

		return totalHits;
	}

	[System.Serializable]
	public class JsonData
	{
		public string name;
		public int maxBlock;
		public int BPM;
		public int offset;
		public JsonNote[] notes;
	}

	[System.Serializable]
	public class JsonNote
	{
		public int LPB;
		public int num;
		public int block;
		public int type;
		public int length;
		public int times;
	}

	// {note class}
	[System.Serializable]
	public class Note
	{
		public float dueTo;
		//public int manyTimes;
		//public float duration;
		public int track;
	}

	[System.Serializable]
	public class Track
	{
		public Note[] notes;
	}
	
	// you can change tempo over time if needed
	[System.Serializable]
	public class Tempo
	{
		public float startTime;
		public float tempoVal;
	}
	// {note class}


	public static JsonData FromJson(string json)
	{
		JsonData jsonData = JsonUtility.FromJson<JsonData>(json);
		return jsonData;
	}

	public void OnEnable()
	{
		JsonData jsonData = FromJson(json.ToString());
		List<JsonNote> jsonNotes = jsonData.notes.ToList();
		List<Note> notes = new List<Note>();
		songOffset = jsonData.offset == 0 ? 0 : (1f - ((44100f - jsonData.offset) / 44100f));
		bpm = jsonData.BPM;
		if (songTitle == null || songTitle == "") { songTitle = jsonData.name; }

		foreach (JsonNote jsonNote in jsonNotes)
		{
			notes.Add(ToAsset(jsonNote, jsonData.BPM, jsonNote.block));
		}

		var track0 = notes.Where(note => note.track == 0).ToList();
		tracks[0].notes = track0.ToArray();
		var track1 = notes.Where(note => note.track == 1).ToList();
		tracks[1].notes = track1.ToArray();
	}

	static Note ToAsset(JsonNote note, int BPM, int track)
    {
		var noteAsset = new Note
		{
			dueTo = (float)(note.num) / (note.LPB) / (BPM / 60f),
			track = track,
		};
		return noteAsset;
    }
}
