using UnityEngine;

public class MusicClip
{
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

