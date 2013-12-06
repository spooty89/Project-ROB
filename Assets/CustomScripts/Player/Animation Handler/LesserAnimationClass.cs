using UnityEngine;
using System;

[Serializable]
public class LesserAnimationClass
{
	public string name;
	public AnimationClip clip;
	public float speed;
	public WrapMode wrap;
	public float crossfade;
	public string state = "";
	
	public LesserAnimationClass(AnimationClip animationClip)
	{
		new LesserAnimationClass( animationClip, 1.0f );
	}
	
	public LesserAnimationClass(AnimationClip animationClip, float animationSpeed)
	{
		new LesserAnimationClass( animationClip, animationSpeed, WrapMode.Default, 0.5f );
	}
	
	public LesserAnimationClass(AnimationClip animationClip, float animationSpeed, float crossFade)
	{
		new LesserAnimationClass( animationClip, animationSpeed, WrapMode.Default, crossFade );
	}
	
	public LesserAnimationClass(AnimationClip animationClip, float animationSpeed, WrapMode wrapMode)
	{
		new LesserAnimationClass( animationClip, animationSpeed, wrapMode, 0.5f );
	}
	
	public LesserAnimationClass(AnimationClip animationClip, float animationSpeed, WrapMode wrapMode, float crossFade)
	{
		name = animationClip.name;
		clip = animationClip;
		speed = animationSpeed;
		wrap = wrapMode;
		crossfade = crossFade;
	}
	
	public LesserAnimationClass(string animationName, float animationSpeed, WrapMode wrapMode)
	{
		new LesserAnimationClass( animationName, animationSpeed, wrapMode, 0.5f, "" );
	}
	
	public LesserAnimationClass(string animationName, float animationSpeed, WrapMode wrapMode, float crossFade, string stateName)
	{
		name = animationName;
		speed = animationSpeed;
		wrap = wrapMode;
		crossfade = crossFade;
		state = stateName;
	}
}



