using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public delegate void stateChangeEvent( string state );

public class TPC: MonoBehaviour
{
	private CharacterClass _Player;
	private Animation _animation;
	private Dictionary<string, AnimationClass> _animations;
	private CharacterController controller;
	private StateClass stateClass;
	public string lastState;
	
	private void Awake ()
	{
		_Player = GetComponent<CharacterClass>();
		controller = GetComponent<CharacterController>();
		
		_Player.moveDirection = transform.forward;	// Initialize player's move direction to the direction rob is initially facing
		stateChangeSetup();
	}

	private void Start()
	{
		animationSetup();
	}
	
	
	// Update the current state of the game
	void Update() {
		if( Input.GetButtonDown( "Reload" ) )
		{
			Application.LoadLevel( Application.loadedLevel );
		}
		if( Input.GetButtonDown( "Quit" ) )
		{
			Application.Quit();
        }
		stateClass.Run();
		AnimationHandler();		// Handle animations
	}
	
	
	void stateChangeSetup()
	{
		StateClass[] scArray = GetComponents<StateClass>();
		foreach(StateClass sc in scArray)
		{
			sc.stateChange = stateChangeHandler;
		}
		_Player.surroundingCollision = surroundingCollisionHandler;
		_Player.topCollision = topCollisionHandler;
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
		if( hit.collider.gameObject != _Player.vCollider.gameObject )
			((StateClass)GetComponent( _animations[_Player.GetCurrentState()].state )).CollisionHandler(hit);
	}


	private void surroundingCollisionHandler()
	{
		((StateClass)GetComponent( _animations[_Player.GetCurrentState()].state )).surroundingCollisionHandler();
	}
	
	
	private void topCollisionHandler()
	{
		((StateClass)GetComponent( _animations[_Player.GetCurrentState()].state )).topCollisionHandler();
    }
	
	
	public void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
			_Player.climbContact = true;						// Set climb contact to true
			_Player.numClimbContacts += 1;						// Keep track of how many climb boxes player is currently in
		}
		else if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
			_Player.hangContact = true;							// Set hang contact to true
			_Player.numHangContacts += 1;						// Keep track of how many hang boxes player is currently in
		}
		else if(other.gameObject.CompareTag("TransitionBox")){
			TransitionBox transBox = (TransitionBox)other.gameObject.GetComponent("TransitionBox");
			TransitionEnterCondition curCond = transBox.matchEnterCondition(_Player);
			if(curCond != null){
				_Player.transitioning = true;
				_Player.curTransitionBox = transBox;
				_Player.curTransitionBox.curCond = curCond;
			}
		}
		
		((StateClass)GetComponent( _animations[_Player.GetCurrentState()].state )).TriggerEnterHandler(other);
	}


	public void OnTriggerExit(Collider other)
	{
		if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
			_Player.numClimbContacts -= 1;						// Keep track of how many climb boxes player is currently in
			if (_Player.numClimbContacts <= 0) {				// If the player is not in any climb boxes
				_Player.numClimbContacts = 0;
				_Player.climbContact = false;						// Set climb contact to false
				_Player.climbing = false;							// Set climbing to false
			}
		}
		
		if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Climb" tag
			_Player.numHangContacts -= 1;						// Keep track of how many climb boxes player is currently in
			if (_Player.numHangContacts <= 0) {				// If the player is not in any climb boxes
				_Player.numHangContacts = 0;
				_Player.hangContact = false;						// Set climb contact to false
				_Player.hanging = false;							// Set climbing to false
			}
		}
		
		((StateClass)GetComponent( _animations[_Player.GetCurrentState()].state )).TriggerExitHandler(other);
	}


	private void stateChangeHandler( string state )
	{
		StateClass[] scArray = GetComponents<StateClass>();
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
		if(!_Player.GetCurrentState().Equals(lastState))
		{
			_animation[_animations[_Player.GetCurrentState()].name].speed = _animations[_Player.GetCurrentState()].speed;
			_animation[_animations[_Player.GetCurrentState()].name].wrapMode = _animations[_Player.GetCurrentState()].wrapMode;
			_animation.CrossFade(_animations[_Player.GetCurrentState()].name, _animations[_Player.GetCurrentState()].crossfade);
			lastState = _Player.GetCurrentState();
		}
	}
}