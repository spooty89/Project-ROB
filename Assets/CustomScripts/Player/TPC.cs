using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TPC: MonoBehaviour
{
	private CharacterClass _Player;
	private Animation _animation;
	private Dictionary<string, AnimationClass> _animations;
	private CharacterController controller;
	private StateClass[] scArray;
	private StateClass stateClass;
	public string lastState;
	
	private void Awake ()
	{
		_Player = GetComponent<CharacterClass>();
		controller = GetComponent<CharacterController>();
		
		_Player.moveDirection = transform.forward;	// Initialize player's move direction to the direction rob is initially facing
		stateChangeSetup();
		animationSetup();
	}
	
	
	// Update the current state of the game
	void Update() {
		stateClass.Run();
		AnimationHandler();		// Handle animations
	}
	
	
	void stateChangeSetup()
	{
		scArray = GetComponents<StateClass>();
		foreach(StateClass sc in scArray)
		{
			sc.stateChange = stateChangeHandler;
		}
		_Player.stateChange = stateChangeHandler;
	}
	
	
	void animationSetup()
	{
		_animation = GetComponent<Animation>();		// Get the character's animations
		_animations = new Dictionary<string, AnimationClass>();

		foreach( AnimationClass aClass in GetComponent<AnimationSetup>().animations )
		{
			_animations.Add( aClass.animationClip.name, aClass );
		}

		lastState = "";
		stateChangeHandler("idle");
	}
	
	
	// Player has come in contact with a surface
	private void OnControllerColliderHit (ControllerColliderHit hit)
	{
		((StateClass)GetComponent( _animations[_Player.GetCurrentState()].state )).CollisionHandler(hit);
	}
	
	void OnCollisionEnter(Collision hit)
	{
		//Debug.Log("here");
		//((StateClass)GetComponent( animations[_Player.GetCurrentState()].state )).CollisionHandler( hit );
	}
	
	public void OnTriggerExit(Collider other)
	{
		if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
			_Player.numClimbContacts -= 1;						// Keep track of how many climb boxes player is currently in
			if (_Player.numClimbContacts <= 0 && !_Player.doubleJumping) {				// If the player is not in any climb boxes
				_Player.stateChange("jump_after_apex");
				_Player.numClimbContacts = 0;
				_Player.climbContact = false;						// Set climb contact to false
				_Player.climbing = false;							// Set climbing to false
			}
		}
	}
	
	
	private void stateChangeHandler( string state )
	{
		foreach(StateClass sc in scArray)
		{
			sc.enabled = false;
		}
		stateClass = (StateClass)GetComponent(_animations[state].state);
		stateClass.enabled = true;
		_Player.SetCurrentState( state );
	}
	
	
	// Handle animation states
	private void AnimationHandler() {
		Vector3 movement = _Player.moveDirection * _Player.moveSpeed + new Vector3 (0, _Player.verticalSpeed, 0) + _Player.inAirVelocity;		// Calculate actual motion
		movement *= Time.deltaTime;			// Base degree of applied movement on time since last frame
		//transform.position += movement;
		_Player.collisionFlags = controller.Move(movement);
		if(!_Player.GetCurrentState().Equals(lastState))
		{
			_animation[_animations[_Player.GetCurrentState()].name].speed = _animations[_Player.GetCurrentState()].speed;
			_animation[_animations[_Player.GetCurrentState()].name].wrapMode = _animations[_Player.GetCurrentState()].wrapMode;
			_animation.CrossFade(_animations[_Player.GetCurrentState()].name, _animations[_Player.GetCurrentState()].crossfade);
			lastState = _Player.GetCurrentState();
		}
	}
}