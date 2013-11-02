using System;
using UnityEngine;

[Serializable]
public class TransitionEnterCondition
{
	public StateClass state;
	public int duration;
	public Vector3 facingDirection;
	public Animation animation;
	public Vector3 displacement;
	public string finalState;
	
	public TransitionEnterCondition (Vector3 facing, int duration, Vector3 displacement, string finalState)
	{
		this.facingDirection = facing;
		this.displacement = displacement;
		this.duration = duration;
		this.finalState = finalState;
	}
	
	public bool matchCondition(CharacterClass _Player){
		if(Vector3.Dot(_Player.moveDirection, facingDirection) > 0 &&
			_Player.currentState.Equals("walk")){
			return true;
		}
		return false;
	}
}