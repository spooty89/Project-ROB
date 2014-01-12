using UnityEngine;
using System.Collections;

public class TransitionState : StateClass {
	[HideInInspector]
	public int timer = 0;
	[HideInInspector]
	private int curStage = -1;	
	private Vector3 curDirection;
	private float curVelocity;
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
			timer = 0;
			initialized = true;
		}
		else{
			// step the transition
			timer++;
		}
		if(timer > _Player.curTransitionBox.curCond.duration){
				// exit transition
			_Player.transform.Translate(_Player.curTransitionBox.curCond.displacement);
			_Player.transitioning = false;
			timer = 0;
			initialized = false;
			stateChange(_Player.curTransitionBox.curCond.finalState);
			return;
		}
		InputHandler();
		MovementHandler();
	}
	
	private void InputHandler(){
		// ignore user input
	}
	
	private void MovementHandler(){
		_Player.moveSpeed = 0;
	}
	
	
	public override void surroundingCollisionHandler()
	{
		
	}
	
	
	public override void TriggerEnterHandler(Collider other)
	{
		
	}
	
	
	public override void TriggerExitHandler(Collider other)
	{
		
	}
}
