using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public delegate void stateChangeEvent( string state );
public delegate void statusChangeEvent();

public class TPC: MonoBehaviour
{
	private CharacterClass _cc;
	private Animation _animation;
	private Dictionary<string, AnimationClass> _animations;
	private CharacterController controller;
	private StateClass stateClass;
	public string lastState;
	public bool useFixedUpdate = true;
	
	private void Awake ()
	{
		_cc = GetComponent<CharacterClass>();
		controller = GetComponent<CharacterController>();
		
		_cc.moveDirection = transform.forward;	// Initialize player's move direction to the direction rob is initially facing
		stateChangeSetup();
	}

	private void Start()
	{
		animationSetup();
	}
	
	
	// Update the current state of the game
	void UpdateFunction() {
		if( Input.GetButtonDown( "Reload" ) )
		{
			Application.LoadLevel( Application.loadedLevel );
		}
		if( Input.GetButtonDown( "Quit" ) )
		{
			Application.Quit();
        }
		stateClass.Run();
		_cc.UpdateFunction();
		AnimationHandler();		// Handle animations
	}
	
	void FixedUpdate () {
		if (_cc.movingPlatform.enabled) {
			if (_cc.movingPlatform.activePlatform != null) {
				if (!_cc.movingPlatform.newPlatform) {
					Vector3 lastVelocity = _cc.movingPlatform.platformVelocity;
					_cc.movingPlatform.platformVelocity = ( _cc.movingPlatform.activePlatform.localToWorldMatrix.MultiplyPoint3x4(_cc.movingPlatform.activeLocalPoint)
															- _cc.movingPlatform.lastMatrix.MultiplyPoint3x4(_cc.movingPlatform.activeLocalPoint) ) / Time.deltaTime;
				}
				_cc.movingPlatform.lastMatrix = _cc.movingPlatform.activePlatform.localToWorldMatrix;
				_cc.movingPlatform.newPlatform = false;
			}
			else {
				_cc.movingPlatform.platformVelocity = Vector3.zero;	
			}
		}
		if (useFixedUpdate)
			UpdateFunction();
	}
	
	void Update () {
		if (!useFixedUpdate)
			UpdateFunction();
	}
	
	
	void stateChangeSetup()
	{
		StateClass[] scArray = GetComponents<StateClass>();
		foreach(StateClass sc in scArray)
		{
			sc.stateChange = stateChangeHandler;
		}
		_cc.surroundingCollision = surroundingCollisionHandler;
		_cc.stateChange = stateChangeHandler;
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
		if( hit.collider.gameObject != _cc.vCollider.gameObject )
			((StateClass)GetComponent( _animations[_cc.GetCurrentState()].state )).CollisionHandler(hit);
	}


	private void surroundingCollisionHandler()
	{
		((StateClass)GetComponent( _animations[_cc.GetCurrentState()].state )).surroundingCollisionHandler();
	}
	
	
	public void OnTriggerEnter(Collider other)
	{
		other.SendMessage("triggerEnter", gameObject, SendMessageOptions.DontRequireReceiver );
		if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
			_cc.climbContact = true;						// Set climb contact to true
			_cc.numClimbContacts += 1;						// Keep track of how many climb boxes player is currently in
		}
		else if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
			_cc.hangContact = true;							// Set hang contact to true
			_cc.numHangContacts += 1;						// Keep track of how many hang boxes player is currently in
		}
		else if(other.gameObject.CompareTag("TransitionBox")){
			TransitionBox transBox = (TransitionBox)other.gameObject.GetComponent("TransitionBox");
			TransitionEnterCondition curCond = transBox.matchEnterCondition(_cc);
			if(curCond != null){
				_cc.transitioning = true;
				_cc.curTransitionBox = transBox;
				_cc.curTransitionBox.curCond = curCond;
			}
		}
		
		((StateClass)GetComponent( _animations[_cc.GetCurrentState()].state )).TriggerEnterHandler(other);
	}


	public void OnTriggerExit(Collider other)
	{
		other.SendMessage("triggerExit", gameObject, SendMessageOptions.DontRequireReceiver );
		if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
			_cc.numClimbContacts -= 1;						// Keep track of how many climb boxes player is currently in
			if (_cc.numClimbContacts <= 0) {				// If the player is not in any climb boxes
				_cc.numClimbContacts = 0;
				_cc.climbContact = false;						// Set climb contact to false
				_cc.climbing = false;							// Set climbing to false
			}
		}
		
		if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Climb" tag
			_cc.numHangContacts -= 1;						// Keep track of how many climb boxes player is currently in
			if (_cc.numHangContacts <= 0) {				// If the player is not in any climb boxes
				_cc.numHangContacts = 0;
				_cc.hangContact = false;						// Set climb contact to false
				_cc.hanging = false;							// Set climbing to false
			}
		}
		
		((StateClass)GetComponent( _animations[_cc.GetCurrentState()].state )).TriggerExitHandler(other);
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
		_cc.SetCurrentState( state );
	}
	
	
	// Handle animation states
	private void AnimationHandler() {
		if(!_cc.GetCurrentState().Equals(lastState))
		{
			_animation[_animations[_cc.GetCurrentState()].name].speed = _animations[_cc.GetCurrentState()].speed;
			_animation[_animations[_cc.GetCurrentState()].name].wrapMode = _animations[_cc.GetCurrentState()].wrapMode;
			_animation.CrossFade(_animations[_cc.GetCurrentState()].name, _animations[_cc.GetCurrentState()].crossfade);
			lastState = _cc.GetCurrentState();
		}
	}
}