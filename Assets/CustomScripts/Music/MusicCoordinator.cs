using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicCoordinator : MonoBehaviour
{
	public List<MusicClip> musicClips;
	public bool playOnStart;

	void Awake()
	{
		foreach( MusicClip musicClip in musicClips )
		{
			musicClip.source = gameObject.AddComponent<AudioSource>();
			musicClip.source.clip = musicClip.musicClip;
			musicClip.source.volume = musicClip.begin.volume;
		}
	}

	void Start()
	{
		if( playOnStart )
		{
			StartPlay();
		}
	}

	public void StartPlay()
	{
		foreach( MusicClip musicClip in musicClips )
		{
			if( !musicClip.source.isPlaying )
				musicClip.source.Play();
			if( musicClip.musicType == MusicClip.MusicType.levelMusic )
				CoRoutine.AfterWait( musicClip.begin.startTime, () =>
	            {
					if( musicClip.fadeCoRoutine != null ) musicClip.fadeCoRoutine.Stop();
					musicClip.fadeCoRoutine = new CoRoutine( fadeToDesiredVolume( musicClip.source, musicClip.begin ) );
				} );
		}
	}
	
	public void StopPlay()
	{
		foreach( MusicClip musicClip in musicClips )
		{
			if( musicClip.source.isPlaying )
			{
				CoRoutine.AfterWait( musicClip.end.startTime, () =>
				{
					if( musicClip.fadeCoRoutine != null ) musicClip.fadeCoRoutine.Stop();
					musicClip.fadeCoRoutine = new CoRoutine( fadeToDesiredVolume( musicClip.source, musicClip.end ), () => musicClip.source.Stop() );
				} );
			}
		}
	}

	public void StartBattle()
	{
		foreach( MusicClip musicClip in musicClips.FindAll( mC => mC.musicType == MusicClip.MusicType.battleMusic ) )
		{
			CoRoutine.AfterWait( musicClip.begin.startTime, () =>
			{
				if( musicClip.fadeCoRoutine != null ) musicClip.fadeCoRoutine.Stop();
				musicClip.fadeCoRoutine = new CoRoutine( fadeToDesiredVolume( musicClip.source, musicClip.begin ) );
			} );
		}
	}
	
	public void StopBattle()
	{
		foreach( MusicClip musicClip in musicClips.FindAll( mC => mC.musicType == MusicClip.MusicType.battleMusic ) )
		{
			CoRoutine.AfterWait( musicClip.end.startTime, () =>
			{
				if( musicClip.fadeCoRoutine != null ) musicClip.fadeCoRoutine.Stop();
				musicClip.fadeCoRoutine = new CoRoutine( fadeToDesiredVolume( musicClip.source, musicClip.end ) );
			} );
		}
	}

	IEnumerator fadeToDesiredVolume( AudioSource source, MusicFade fade )
	{
		Vector2 currentVolume = new Vector2( source.volume, 0 );
		Vector2 desiredVolume = new Vector2( fade.desiredVolume, 0 );

		while( currentVolume.y != desiredVolume.y )
		{
			currentVolume = Vector2.MoveTowards( currentVolume, desiredVolume, fade.duration * Time.deltaTime );
			source.volume = currentVolume.y;
			yield return null;
		}
	}
}

