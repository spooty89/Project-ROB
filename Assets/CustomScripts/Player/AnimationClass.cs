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
	
	public AnimationClass(AnimationClip animationClip)
	{
		name = animationClip.name;
		clip = animationClip;
		speed = 1.0f;
		wrap = WrapMode.Default;
		crossfade = 0.5f;
	}
	
	public AnimationClass(AnimationClip animationClip, float animationSpeed)
	{
		name = animationClip.name;
		clip = animationClip;
		speed = animationSpeed;
		wrap = WrapMode.Default;
		crossfade = 0.5f;
	}
	
	public AnimationClass(AnimationClip animationClip, float animationSpeed, float crossFade)
	{
		name = animationClip.name;
		clip = animationClip;
		speed = animationSpeed;
		wrap = WrapMode.Default;
		crossfade = crossFade;
	}
	
	public AnimationClass(AnimationClip animationClip, float animationSpeed, WrapMode wrapMode)
	{
		name = animationClip.name;
		clip = animationClip;
		speed = animationSpeed;
		wrap = wrapMode;
		crossfade = 0.5f;
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
		name = animationName;
		speed = animationSpeed;
		wrap = wrapMode;
		crossfade = 0.5f;
	}
	
	public AnimationClass(string animationName, float animationSpeed, WrapMode wrapMode, float crossFade)
	{
		name = animationName;
		speed = animationSpeed;
		wrap = wrapMode;
		crossfade = crossFade;
	}
}



