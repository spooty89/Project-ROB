using UnityEngine;
using System.Collections;

public class TransitionState : StateClass {
	[HideInInspector]
	public int timer = 0;
	[HideInInspector]
	public int curDuration = -1;
	private int curStage = -1;	
	private Vector3 curDirection;
	private float curVelocity;
	private TransitionAnimSequence animSeq;
	[HideInInspector]
	public bool initialized = false;
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
	}
	
	public override void Run(){		
		if(!initialized){
			// set initial states
			_Player.jumping = false;
			_Player.climbing = false;
			curStage = 0;
			animSeq = ((TransitionAnimSequence)_Player.curTransitionBox.GetComponent("TransitionAnimSequence"));
			curDuration = animSeq.durations[0];
			curDirection = animSeq.directions[0];
			curVelocity = animSeq.velocities[0];
			timer = 0;
			initialized = true;
		}
		else{
			// step the transition
			timer++;
		}
		if(timer > curDuration){
			// step to the next transition stage
			curStage++;
			if(curStage < animSeq.durations.Length){
				curDuration = animSeq.durations[curStage];
				curDirection = animSeq.directions[curStage];
				curVelocity = animSeq.velocities[curStage];
				timer = 0;
			}
			else{
				// exit transition
				_Player.transitioning = false;
				_Player.SetCurrentState("free_fall");
				timer = 0;
				initialized = false;
				return;
			}
		}
		InputHandler();
		MovementHandler();
	}
	
	private void InputHandler(){
		// ignore user input
	}
	
	private void MovementHandler(){
		// TODO: fix this.
		_Player.moveDirection = curDirection;
		_Player.verticalSpeed = curDirection.y;
		_Player.moveSpeed = curVelocity;
	}
}
