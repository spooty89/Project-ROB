using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AnimationBlender : MonoBehaviour
{
	AnimationSetup _animationSetup;
	List<AnimationClip> animationsToBlend;
	List<AnimationClass> blendInList, blendOutList;
	bool blend, blendIn, blendOut;
	
	void Awake()
	{
		_animationSetup = GetComponent<AnimationSetup>();
		animationsToBlend = new List<AnimationClip>();
		blendInList = new List<AnimationClass>();
		blendOutList = new List<AnimationClass>();
		blend = false;
		blendIn = false;
		blendOut = false;
	}
	
	void Update()
	{
		if(blend)
		{
			if(blendIn)
			{
				List<AnimationClass> rmBlendIn = new List<AnimationClass>();
				foreach(AnimationClass ac in blendInList)
				{
					if( ac.blend )
					{
						if(animation[ ac.animationClip.name ].weight < ac.weight )
						{
							ac.delta += Time.deltaTime;
							animation[ ac.animationClip.name ].weight = Mathf.Lerp( 0, ac.weight, ac.delta/ac.toWeightDuration );
						}
						else
						{
							animation[ ac.animationClip.name ].weight = ac.weight;
							rmBlendIn.Add(ac);
						}
					}
				}
				foreach(AnimationClass ac in rmBlendIn)
				{
					blendInList.Remove(ac);
				}
				if(blendInList.Count == 0)
					blendIn = false;
			}
			
			if(blendOut)
			{
				List<AnimationClass> rmBlendOut = new List<AnimationClass>();
				foreach(AnimationClass ac in blendOutList)
				{
					if( ac.blend )
					{
						if(animation[ ac.animationClip.name ].weight > 0.01f )
						{
							ac.delta += Time.deltaTime;
							animation[ ac.animationClip.name ].weight = Mathf.Lerp( ac.weight, 0, ac.delta/ac.fromWeightDuration );
						}
						else
						{
							animation[ ac.animationClip.name ].time = 0;
							animation[ ac.animationClip.name ].weight = 0;
							ac.blend = false;
							rmBlendOut.Add(ac);
						}
					}
				}
				foreach(AnimationClass ac in rmBlendOut)
				{
					blendOutList.Remove(ac);
					animationsToBlend.Remove(ac.animationClip);
				}
				if(blendOutList.Count == 0)
					blendOut = false;
				if(animationsToBlend.Count == 0)
					blend = false;
			}
			
			foreach(AnimationClip ac in animationsToBlend)
			{
				if( ac != null )
				{
					AnimationClass animClass = _animationSetup.animations.First( a => (a.animationClip.Equals(ac)));
					if(animClass.blend)
					{
						animation.Blend( ac.name );
					}
				}
			}
		}
	}
	
	
	/* Start Blending functions */
	public void StartBlending(List<AnimationClip> anim, float customTime)
	{
		foreach(AnimationClip ac in anim)
		{
			AnimationClass animationClass = _animationSetup.animations.First( a => a.animationClip.Equals(ac));
			animationClass.time = customTime;
			animation[ ac.name ].time = animationClass.time;
			new CoRoutine(delayAndBuildToWeight(animationClass));
		}
	}
	
	public void StartBlending(List<AnimationClip> anim, float customTime, float customStartTime, float customTimeToWeight)
	{
		foreach(AnimationClip ac in anim)
		{
			AnimationClass animationClass = _animationSetup.animations.First( a => a.animationClip.Equals(ac));
			animationClass.toWeightStartTime = customStartTime;
			animationClass.toWeightDuration = customTimeToWeight;
			animationClass.time = customTime;
			animation[ ac.name ].time = animationClass.time;
			new CoRoutine(delayAndBuildToWeight(animationClass));
		}
	}
	
	public void StartBlending(List<AnimationClip> anim, List<float> customTimes, float customStartTime, float customTimeToWeight)
	{
		int i = 0;
		foreach(AnimationClip ac in anim)
		{
			AnimationClass animationClass = _animationSetup.animations.First( a => a.animationClip.Equals(ac));
			animationClass.toWeightStartTime = customStartTime + 0.1f * i;
			animationClass.toWeightDuration = customTimeToWeight;
			animationClass.time = customTimes.ElementAt( i );
			animation[ animationClass.animationClip.name ].time = animationClass.time;
			new CoRoutine(delayAndBuildToWeight(animationClass));
			i++;
		}
	}
	
	IEnumerator delayAndBuildToWeight(AnimationClass animationClass)
	{
		animation[ animationClass.animationClip.name ].weight = 0;
		CoRoutine.AfterWait( animationClass.toWeightStartTime, () =>
        {
			if( animationsToBlend.FirstOrDefault(animClass => animClass.name.Equals(animationClass.animationClip.name)) == null )
				animationsToBlend.Add( animationClass.animationClip );
			blend = true;
			blendIn = true;
			animationClass.blend = true;
			animationClass.delta = 0;
			blendInList.Add( animationClass );
		});
		
		yield return null;
	}
	
	
	/* Stop Blending functions */
	public void StopBlending(List<AnimationClip> anim)
	{
		foreach(AnimationClip ac in anim)
		{
			if( animationsToBlend.FirstOrDefault(animClass => animClass.name.Equals(ac.name)) == null )
				animationsToBlend.Add( ac );
			
			AnimationClass animationClass = _animationSetup.animations.First( a => a.animationClip.Equals(ac));
			new CoRoutine(delayAndBuildToZero(animationClass));
		}
	}
	
	public void StopBlending(List<AnimationClip> anim, float customTime)
	{
		foreach(AnimationClip ac in anim)
		{
			if( animationsToBlend.FirstOrDefault(animClass => animClass.name.Equals(ac.name)) == null )
				animationsToBlend.Add( ac );
			
			AnimationClass animationClass = _animationSetup.animations.First( a => a.animationClip.Equals(ac));
			animationClass.time = customTime;
			animation[ ac.name ].time = animationClass.time;
			new CoRoutine(delayAndBuildToZero(animationClass));
		}
	}
	
	public void StopBlending(List<AnimationClip> anim, float customStartTime, float customWeightTime)
	{
		foreach(AnimationClip ac in anim)
		{
			if( animationsToBlend.FirstOrDefault(animClass => animClass.name.Equals(ac.name)) == null )
				animationsToBlend.Add( ac );
			
			AnimationClass animationClass = _animationSetup.animations.First( a => a.animationClip.Equals(ac));
			animationClass.fromWeightStartTime = customStartTime;
			animationClass.fromWeightDuration = customWeightTime;
			animation[ ac.name ].time = animationClass.time;
			new CoRoutine(delayAndBuildToZero(animationClass));
		}
	}
	
	public void StopBlending(List<AnimationClip> anim, float customTime, float customStartTime, float customWeightTime)
	{
		foreach(AnimationClip ac in anim)
		{
			if( animationsToBlend.FirstOrDefault(animClass => animClass.name.Equals(ac.name)) == null )
				animationsToBlend.Add( ac );
			
			AnimationClass animationClass = _animationSetup.animations.First( a => a.animationClip.Equals(ac));
			animationClass.fromWeightStartTime = customStartTime;
			animationClass.fromWeightDuration = customWeightTime;
			animationClass.time = customTime;
			animation[ ac.name ].time = animationClass.time;
			new CoRoutine(delayAndBuildToZero(animationClass));
		}
	}
	
	public void StopBlending(List<AnimationClip> anim, List<float> customTimes, float customStartTime, float customWeightTime)
	{
		int i = 0;
		foreach(AnimationClip ac in anim)
		{
			if( animationsToBlend.FirstOrDefault(animClass => animClass.name.Equals(ac.name)) == null )
				animationsToBlend.Add( ac );
			
			AnimationClass animationClass = _animationSetup.animations.First( a => a.animationClip.Equals(ac));
			animationClass.fromWeightStartTime = customStartTime;
			animationClass.fromWeightDuration = customWeightTime;
			animationClass.time = customTimes.ElementAt( i );
			animation[ ac.name ].time = animationClass.time;
			new CoRoutine(delayAndBuildToZero(animationClass));
			i++;
		}
	}
	
	IEnumerator delayAndBuildToZero(AnimationClass animationClass)
	{
		animation[ animationClass.animationClip.name ].weight = animationClass.weight;
		blend = true;
		CoRoutine.AfterWait( animationClass.fromWeightStartTime, () =>
        {
			animationClass.blend = true;
			blendOut = true;
			animationClass.delta = 0;
			blendOutList.Add( animationClass );
		});
		
		yield return null;
	}
}