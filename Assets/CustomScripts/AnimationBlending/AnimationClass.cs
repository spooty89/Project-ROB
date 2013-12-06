using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AnimationClass
{
	public string name;
	public string state;
	public AnimationClip animationClip;
	public List<GameObject> affectedJoints;
	public float speed = 1f, 
				 weight = 1f, 
				 time = 0f,
				 toWeightStartTime = 0f,
				 toWeightDuration = 0f,
				 fromWeightStartTime = 0f,
				 fromWeightDuration = 0f,
				 parentingTime = 0f;
	public ParentingClass parent;
	public AnimationBlendMode animationBlendMode = AnimationBlendMode.Blend;
	public WrapMode wrapMode = WrapMode.Default;
	public float crossfade = 0f;
	public int layer = 1;
	[HideInInspector]
	public bool blend = false;
	[HideInInspector]
	public float delta = 0;
	
	public void Setup( Animation animatedCharacter )
	{
		if(affectedJoints.Count == 0)
		{
			affectedJoints.Add( animatedCharacter.gameObject );
		}
		else foreach(GameObject joint in affectedJoints)
		{
        	animatedCharacter[ animationClip.name ].AddMixingTransform( joint.transform );
		}

        animatedCharacter[ animationClip.name ].speed = speed;
        animatedCharacter[ animationClip.name ].weight = weight;
        animatedCharacter[ animationClip.name ].wrapMode = wrapMode;
        animatedCharacter[ animationClip.name ].layer = layer;
        animatedCharacter[ animationClip.name ].blendMode = animationBlendMode;
	}
}