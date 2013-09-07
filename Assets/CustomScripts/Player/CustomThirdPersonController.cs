using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public delegate void stateChangeEvent(string state);

public class CustomThirdPersonController : MonoBehaviour
{
	public ROB rob;
	private CharacterClass _Player;
	public Dictionary<string, AnimationClass> animations;
	
	public string state, lastState;
	public bool loadAnimationInfo;
	
	private Animation _animation;
	private GameObject contactObject;
	private CharacterController controller;
	
	public bool enable;
	
	void animationSetup()
	{
		_animation = GetComponent<Animation>();		// Get the character's animations
		animations = new Dictionary<string, AnimationClass>();
		
		if(loadAnimationInfo)
			AnimationInfoImporter.Load();
		foreach( AnimationState aState in _animation )
		{
			AnimationClass tempAC = animations.FirstOrDefault (i => i.Key == aState.clip.name).Value;
			if(tempAC == null)
				animations.Add(aState.clip.name, new AnimationClass(aState.clip, 1.0f, aState.wrapMode == WrapMode.Default ? WrapMode.Loop : aState.wrapMode));
			else
			{
				tempAC.clip = aState.clip;
			}
		}
		lastState = "";
		_Player.currentState = "idle";
	}
	
	private void Start ()
	{
		_Player = GetComponent<CharacterClass>();
		rob = GetComponent<ROB>();
		controller = GetComponent<CharacterController>();
		
		this.enable = true;
		_Player.moveDirection = rob.transform.forward;	// Initialize player's move direction to the direction rob is initially facing
		animationSetup();
	}
	
	// Update the current state of the game
	void Update() {
		if(GetComponent( animations[_Player.currentState].state ))
			((StateClass)GetComponent( animations[_Player.currentState].state )).Run();
		if (_Player.collisionFlags == 0) {		// If rob is no longer in contact with 
			if(contactObject != null)				// Useful for when no object has been contacted
				contactObject.SendMessage("noContact", SendMessageOptions.DontRequireReceiver);
		}
		AnimationHandler();		// Handle animations
	}
	
	// Player has come in contact with a surface
	private void OnControllerColliderHit (ControllerColliderHit hit)
	{
		if(!hit.gameObject.CompareTag("Barrier")) {
			// Apply the motion of the object player is standing/hanging/climbing on to the player
			if ((hit.normal.y > 0.0) || rob.hanging || rob.climbing)
			{
				contactObject = hit.gameObject;
				hit.gameObject.SendMessage("transferSpeed", rob.transform, SendMessageOptions.DontRequireReceiver);
			}
				
			if (Mathf.Abs(hit.normal.y) > 0.5) {
				/*if(hit.gameObject.CompareTag("Bouncy") && (_Player.verticalSpeed <= 0)){    
					rob.bouncing = true;
					rob.collisionFlags=0;
					rob.jumping=true;
					rob.doubleJumping = false;
					state = "jumping";	
				    rob.verticalSpeed = (float)20.0;
				}*/					// If surface of contact is relatively horizontal
				if (rob.hangContact && (hit.normal.y < -0.1) && !rob.aim) {				// If surface is above and player is within hang triggerBox
					if (!rob.hanging) {											// If player isn't already hanging, set necessary variables
						rob.hanging = true;
						rob.wallSliding = false;
						rob.wasWallSliding = false;
						rob.climbing = false;
						rob.jumping = false;
						rob.doubleJumping = false;
						_Player.inAirVelocity = Vector3.zero;
					}
				}
				else {													// Else, if surface is below and/or player is not within hang triggerBox
					rob.hanging = false;
					if (rob.climbing && rob.decending && hit.normal.y > 0.0) {							// If player just reached flat ground after climbing downward
						rob.transform.position = new Vector3(rob.transform.position.x + (_Player.wallFacing.x * (float)0.2), rob.transform.position.y, rob.transform.position.z + (_Player.wallFacing.z * (float)0.2));
						rob.transform.forward = _Player.wallFacing;
						_Player.moveDirection = rob.transform.forward;
						_Player.moveSpeed = (float)0.0;
						rob.climbing = false;
						rob.decending = false;
					}
				}
			}
			else {													// If surface of contact is relatively vertical
				rob.wallContact = true;
				if (!rob.aim) {
					if (rob.climbContact && Vector3.Angle(hit.normal, transform.forward) > 150f) {										// If player is within climb triggerBox
						_Player.wallFacing = hit.normal;
						//rob.wallRight = rob.DirectionOnWall();
						_Player.moveDirection = -_Player.wallFacing;
						rob.transform.rotation = Quaternion.LookRotation(_Player.moveDirection);
						_Player.wallRight = rob.transform.right;
						GetComponent<ClimbState>().enabled = true;
						rob.enabled = false;
						//Debug.Log(rob.wallRight);
						if (!rob.climbing) {										// If player isn't already climbing, set necessary variables
							_Player.currentState = "climb_idle";
							rob.climbing = true;
							rob.hanging = false;
							//rob._characterState = AssemblyCSharp.ROB.customCharacterState.Climbing_Idle;
							if (rob.IsGrounded())										// If player walks into climb surface (rather than jumping)
								_Player.verticalSpeed = (float)1.0;									// need to get them off ground (otherwise, rapid switch between climb and grounded states)
							rob.wallSliding = false;
							rob.wasWallSliding = false;
							_Player.moveSpeed = (float)1.0;
							_Player.inAirVelocity = Vector3.zero;
							rob.jumping = false;
							rob.doubleJumping = false;
						}
			    	}
			    	else {								// Else, if player is against non-climb wall surface
				    	rob.climbing = false;
						_Player.wallFacing = hit.normal;
						if (_Player.verticalSpeed < -0.4) {		// If player has reached their jump apex or is simply falling	
							if (!rob.wallSliding){				// If player is not already wallSliding, set necessary variables
								rob.wallSliding = true;
				    			_Player.currentState = "wall_slide";
				    			rob.jumping = true;
				    			rob.doubleJumping = false;
							}
						}
					}
				}
			}
		}
	}
	
	// If the player enters a triggerBox
	private void OnTriggerEnter(Collider other)
	{
		ContactObjectInterface i = (ContactObjectInterface)other.gameObject.GetComponent(typeof(ContactObjectInterface));
		
		if(i != null)
			i.contact(rob);
		
	    if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
	        rob.climbContact = true;						// Set climb contact to true
	        rob.numClimbContacts += 1;						// Keep track of how many climb boxes player is currently in
		}
	    if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
			_Player.currentState = "hang_idle";
	        rob.hangContact = true;							// Set hang contact to true
	        rob.numHangContacts += 1;						// Keep track of how many hang boxes player is currently in
		}
		if(other.gameObject.CompareTag("Token")) {
			GetComponent<ROBgui>().tokens++;
			Destroy(other.gameObject);
		}
	}
	
	// If the player exits a triggerBox
	private void OnTriggerExit(Collider other)
	{
	    if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
	    	rob.numClimbContacts -= 1;						// Keep track of how many climb boxes player is currently in
	    	if (rob.numClimbContacts <= 0) {				// If the player is not in any climb boxes
				_Player.currentState = "jump_after_apex";
				rob.numClimbContacts = 0;
				rob.climbContact = false;						// Set climb contact to false
				rob.climbing = false;							// Set climbing to false
			}
		}
	    if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
	    	rob.numHangContacts -= 1;						// Keep track of how many climb boxes player is currently in
	    	if (rob.numHangContacts <= 0) {					// If the player has exited all triggerBoxes with "Hang" tags
				_Player.currentState = "idle";
				rob.numHangContacts = 0;
				rob.hangContact = false;						// Set hang contact to false
				rob.hanging = false;							// Set hanging to false
			}
		}
	}
	
	// Handle animation states
	private void AnimationHandler() {
		Vector3 movement = _Player.moveDirection * _Player.moveSpeed + new Vector3 (0, _Player.verticalSpeed, 0) + _Player.inAirVelocity;		// Calculate actual motion
		movement *= Time.deltaTime;			// Base degree of applied movement on time since last frame
		_Player.collisionFlags = controller.Move(movement);
		if(!_Player.currentState.Equals(lastState))
		{
			_animation[animations[_Player.currentState].name].speed = animations[_Player.currentState].speed;
			_animation[animations[_Player.currentState].name].wrapMode = animations[_Player.currentState].wrap;
			_animation.CrossFade(animations[_Player.currentState].name, animations[_Player.currentState].crossfade);
			lastState = _Player.currentState;
		}
	}
}