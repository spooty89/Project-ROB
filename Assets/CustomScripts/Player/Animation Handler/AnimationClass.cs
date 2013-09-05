using UnityEngine;
using System;

[Serializable]
public class AnimationClass
{
	public string name;
	public AnimationClip clip;
	public float speed;
	public WrapMode wrap;
	public float crossfade;
	public string state = "";
	
	public AnimationClass(AnimationClip animationClip)
	{
		new AnimationClass( animationClip, 1.0f );
	}
	
	public AnimationClass(AnimationClip animationClip, float animationSpeed)
	{
		new AnimationClass( animationClip, animationSpeed, WrapMode.Default, 0.5f );
	}
	
	public AnimationClass(AnimationClip animationClip, float animationSpeed, float crossFade)
	{
		new AnimationClass( animationClip, animationSpeed, WrapMode.Default, crossFade );
	}
	
	public AnimationClass(AnimationClip animationClip, float animationSpeed, WrapMode wrapMode)
	{
		new AnimationClass( animationClip, animationSpeed, wrapMode, 0.5f );
	}
	
	public AnimationClass(AnimationClip animationClip, float animationSpeed, WrapMode wrapMode, float crossFade)
	{
		name = animationClip.name;
		clip = animationClip;
		speed = animationSpeed;
		wrap = wrapMode;
		crossfade = crossFade;
	}
	
	public AnimationClass(string animationName, float animationSpeed, WrapMode wrapMode)
	{
		new AnimationClass( animationName, animationSpeed, wrapMode, 0.5f, "" );
	}
	
	public AnimationClass(string animationName, float animationSpeed, WrapMode wrapMode, float crossFade, string stateName)
	{
		name = animationName;
		speed = animationSpeed;
		wrap = wrapMode;
		crossfade = crossFade;
		state = stateName;
	}
}



