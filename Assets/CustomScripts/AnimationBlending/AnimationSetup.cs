using UnityEngine;
using System.Collections.Generic;

public class AnimationSetup : MonoBehaviour
{
	public List<AnimationClass> animations;
	
	void Awake()
	{
		Animation animation = gameObject.GetComponent<Animation>();

		foreach(AnimationClass ac in animations)
		{
			ac.Setup( animation );
		}
	}
}