using UnityEngine;
using System;

[Serializable]
public class AnimationClass
{
	public string name;
	public AnimationClip clip;
	public float speed;
	public WrapMode wrap;
	
	public AnimationClass()
	{
		name = "";
		clip = new AnimationClip();
		speed = 1.0f;
		wrap = WrapMode.Default;
	}
	
	public AnimationClass(AnimationClip ac)
	{
		name = ac.name;
		clip = ac;
		speed = 1.0f;
		wrap = WrapMode.Default;
	}
	
	public AnimationClass(AnimationClip ac, float s)
	{
		name = ac.name;
		clip = ac;
		speed = s;
		wrap = WrapMode.Default;
	}
	
	public AnimationClass(AnimationClip ac, float s, WrapMode w)
	{
		name = ac.name;
		clip = ac;
		speed = s;
		wrap = w;
	}
	
	public AnimationClass(string n, float s, WrapMode w)
	{
		name = n;
		speed = s;
		wrap = w;
	}
}



