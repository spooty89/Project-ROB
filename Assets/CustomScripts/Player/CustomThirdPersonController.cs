/*using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class CustomThirdPersonController : MonoBehaviour
{
	private CharacterClass _Player;
	public Dictionary<string, LesserAnimationClass> animations;
	
	public string lastState;
	public bool loadAnimationInfo;
	
	private Animation _animation;
	private CharacterController controller;
	private StateClass[] scArray;
	private StateClass stateClass;
	
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
	
	
	void animationSetup()
	{
		_animation = GetComponent<Animation>();		// Get the character's animations
		animations = new Dictionary<string, LesserAnimationClass>();
		
		//if(loadAnimationInfo)
		//	AnimationInfoImporter.Load();
		foreach( AnimationState aState in _animation )
		{
			LesserAnimationClass tempAC = animations.FirstOrDefault (i => i.Key == aState.clip.name).Value;
			if(tempAC == null)
				animations.Add(aState.clip.name, new LesserAnimationClass(aState.clip, 1.0f, aState.wrapMode == WrapMode.Default ? WrapMode.Loop : aState.wrapMode));
			else
			{
				tempAC.clip = aState.clip;
			}
		}
		lastState = "";
		stateChangeHandler("idle");
	}
	
	
	void stateChangeSetup()
	{
		scArray = GetComponents<StateClass>();
		foreach(StateClass sc in scArray)
		{
			sc.stateChange = stateChangeHandler;
		}
	}
	
	
	// Player has come in contact with a surface
	private void OnControllerColliderHit (ControllerColliderHit hit)
	{
		((StateClass)GetComponent( animations[_Player.GetCurrentState()].state )).CollisionHandler(hit);
	}
	
	void OnCollisionEnter(Collision hit)
	{
		//((StateClass)GetComponent( animations[_Player.GetCurrentState()].state )).CollisionHandler( hit );
	}
	
	
	private void stateChangeHandler( string state )
	{
		foreach(StateClass sc in scArray)
		{
			sc.enabled = false;
		}
		stateClass = (StateClass)GetComponent(animations[state].state);
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
			_animation[animations[_Player.GetCurrentState()].name].speed = animations[_Player.GetCurrentState()].speed;
			_animation[animations[_Player.GetCurrentState()].name].wrapMode = animations[_Player.GetCurrentState()].wrap;
			_animation.CrossFade(animations[_Player.GetCurrentState()].name, animations[_Player.GetCurrentState()].crossfade);
			lastState = _Player.GetCurrentState();
		}
	}
}*/