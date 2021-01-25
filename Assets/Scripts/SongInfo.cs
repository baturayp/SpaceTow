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

	[Header("Meteor appear offsets")]
	public Tempo[] appearTime;

	[Header("Just for reference, notes populated automatically, edit them from NoteEditor")]
	public Track[] tracks = new Track[4];

	[Header("When to finish scene shows up, in seconds")]
	public float endTime;

	[System.Serializable]
	public class JsonData
	{
		public string name;
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
	}

	// {note class}
	[System.Serializable]
	public class Note
	{
		public float dueTo;
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
		public float offsetVal;
	}

	private static JsonData FromJson(string json)
	{
		var jsonData = JsonUtility.FromJson<JsonData>(json);
		return jsonData;
	}

	public void OnEnable()
	{
		var jsonData = FromJson(json.ToString());
		var jsonNotes = jsonData.notes.ToList();
		var notes = new List<Note>();
		songOffset = jsonData.offset == 0 ? 0 : 1f - (44100f - jsonData.offset) / 44100f;
		if (string.IsNullOrEmpty(songTitle)) { songTitle = jsonData.name; }

		foreach (var jsonNote in jsonNotes)
		{
			notes.Add(ToAsset(jsonNote, jsonData.BPM, jsonNote.block));
		}

		var track0 = notes.Where(note => note.track == 0).ToList();
		tracks[0].notes = track0.ToArray();
		var track1 = notes.Where(note => note.track == 1).ToList();
		tracks[1].notes = track1.ToArray();
		var track2 = notes.Where(note => note.track == 2).ToList();
		tracks[2].notes = track2.ToArray();
		var track3 = notes.Where(note => note.track == 3).ToList();
		tracks[3].notes = track3.ToArray();
	}

	private static Note ToAsset(JsonNote note, int bpm, int track)
    {
		var noteAsset = new Note
		{
			dueTo = track >= 2 ? (float)note.num / note.LPB / (bpm / 60f) + 0.1f : (float)note.num / note.LPB / (bpm / 60f),
			track = track
		};
		return noteAsset;
    }
}