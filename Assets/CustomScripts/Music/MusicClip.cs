using UnityEngine;

[System.Serializable]
public class MusicClip
{
	[HideInInspector]
	public string name = "Clip";

	public AudioClip musicClip;
	public MusicFade begin, end;
	public MusicType musicType;
	[HideInInspector]
	public AudioSource source;
	[HideInInspector]
	public CoRoutine fadeCoRoutine;

	public enum MusicType : int
	{
		levelMusic, battleMusic
	}
}

