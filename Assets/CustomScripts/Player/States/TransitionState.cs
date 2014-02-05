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
	
	protected override void Awake()
	{
		if( _cc == null )
		{
			_cc = GetComponent<CharacterClass>();
		}
	}


	public override void CollisionHandler(ControllerColliderHit hit)
	{
	}
	
	public override void Run(){		
		if(!initialized){
			// set initial states
			_cc.jumping.jumping = false;
			_cc.climbing = false;
			timer = 0;
			initialized = true;
		}
		else{
			// step the transition
			timer++;
		}
		if(timer > _cc.curTransitionBox.curCond.duration){
				// exit transition
			_cc.transform.Translate(_cc.curTransitionBox.curCond.displacement);
			_cc.transitioning = false;
			timer = 0;
			initialized = false;
			stateChange(_cc.curTransitionBox.curCond.finalState);
			return;
		}
		InputHandler();
		MovementHandler();
	}
	
	private void InputHandler(){
		// ignore user input
	}
	
	private void MovementHandler(){
		_cc.moveSpeed = 0;
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
