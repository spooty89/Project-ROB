var rob:ROB;

// Animations to be played
public var idleAnimation : AnimationClip;
public var walkAnimation : AnimationClip;
public var runAnimation : AnimationClip;
public var jumpPoseAnimation : AnimationClip;
public var jumpPoseAfterApexAnimation : AnimationClip;
public var doubleJumpAnimation : AnimationClip;
public var wallSlideAnimation : AnimationClip;
public var rollAnimation : AnimationClip;
public var climbIdlePoseAnimation : AnimationClip;
public var climbVerticalAnimation : AnimationClip;
public var climbHorizontalAnimation : AnimationClip;
public var hangIdlePoseAnimation : AnimationClip;
public var hangMoveAnimation : AnimationClip;
public var freefallPoseAnimation : AnimationClip;
public var heavyLandPoseAnimation : AnimationClip;

// Speed at which each animation will be played
public var idleAnimationSpeed : float = 1.0;
public var walkMaxAnimationSpeed : float = 1.0;
public var trotMaxAnimationSpeed : float = 0.7;
public var runMaxAnimationSpeed : float = 1.4;
public var jumpAnimationSpeed : float = 1.0;
public var landAnimationSpeed : float = 1.0;
public var jumpAfterApexAnimationSpeed : float = 1.0;
public var doubleJumpAnimationSpeed : float = 1.0;
public var wallSlideAnimationSpeed : float = 1.0;
public var rollAnimationSpeed : float = 0.8;
public var climbIdleAnimationSpeed : float = 1.0;
public var climbVerticalAnimationSpeed : float = 1.0;
public var climbHorizontalAnimationSpeed : float = 1.0;
public var hangIdleAnimationSpeed : float = 1.0;
public var hangMoveAnimationSpeed : float = 1.5;
public var freefallAnimationSpeed : float = 1.0;
public var heavyLandAnimationSpeed : float = 1.0;
private var _animation : Animation;

function Awake ()
{
	rob = new ROB(transform);
	_animation = GetComponent(Animation);
	if(!_animation)
		Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");
	
	// Check for all animations
	if(!idleAnimation) {
		_animation = null;
		Debug.Log("No idle animation found. Turning off animations.");
	}
	if(!walkAnimation) {
		_animation = null;
		Debug.Log("No walk animation found. Turning off animations.");
	}
	if(!runAnimation) {
		_animation = null;
		Debug.Log("No run animation found. Turning off animations.");
	}
	if(!jumpPoseAnimation && rob.canJump) {
		_animation = null;
		Debug.Log("No jump animation found and the character has canJump enabled. Turning off animations.");
	}
	if(!jumpPoseAfterApexAnimation) {
		_animation = null;
		Debug.Log("No jump after apex animation found. Turning off animations.");
	}
	if(!doubleJumpAnimation && rob.canJump) {
		_animation = null;
		Debug.Log("No double jump animation found and the character has canJump enabled. Turning off animations.");
	}
	if(!wallSlideAnimation) {
		_animation = null;
		Debug.Log("No wall sliding animation found. Turning off animations.");
	}
	
	rob.moveDirection = transform.forward;
}

// Update the current state of the game
function Update() {
	if (!rob.isControllable) {		// Kill all inputs if not controllable (here by default, may be useful for in-game cutscenes?)
		Input.ResetInputAxes();
	}
	rob.UpdateSmoothedMovementDirection();	// Smooth the player movement
	rob.ApplyGravity ();	// Apply gravity
	rob.MovementHandler();	// Handle movement
	rob.InputHandler();		// Handle user input (comes after movement, else jumping from ground is irregular)
	AnimationHandler();		// Handle animations
}

// Player has come in contact with a surface
function OnControllerColliderHit (hit : ControllerColliderHit )
{
	if (Mathf.Abs(hit.normal.y) > 0.3) {					// If surface of contact is relatively horizontal
		if (rob.hangContact && (hit.normal.y < -0.1)) {				// If surface is above and player is within hang triggerBox
			if (!rob.hanging) {											// If player isn't already hanging, set necessary variables
				rob.hanging = true;
				rob.wallSliding = false;
				rob.wasWallSliding = false;
				rob.climbing = false;
				rob.jumping = false;
				rob.doubleJumping = false;
				rob._characterState = rob.customCharacterState.Hanging_Idle;
				rob.inAirVelocity = Vector3.zero;
			}
		}
		else {													// Else, if surface is below and/or player is not within hang triggerBox
			rob.hanging = false;
			if (rob.climbing && rob.decending) {							// If player just reached flat ground after climbing downward
				rob.transform.position.x += (rob.wallFacing.x * 0.1);
				rob.transform.position.z += (rob.wallFacing.z * 0.1);
				rob.moveSpeed = 0.0;
				rob.climbing = false;
				rob.decending = false;
			}
		}
	}
	else {													// If surface of contact is relatively vertical
		rob.wallContact = true;
		if (rob.climbContact) {										// If player is within climb triggerBox
			rob.wallFacing = hit.normal;
			rob.wallRight = rob.DirectionOnWall(rob.moveDirection);
			rob.moveDirection = -rob.wallFacing;
			rob.transform.rotation = Quaternion.LookRotation(rob.moveDirection);
			if (!rob.climbing) {										// If player isn't already climbing, set necessary variables
				rob.climbing = true;
				rob.hanging = false;
				rob._characterState = rob.customCharacterState.Climbing_Idle;
				if (rob.IsGrounded())										// If player walks into climb surface (rather than jumping)
					rob.verticalSpeed = 1.0;									// need to get them off ground (otherwise, rapid switch between climb and grounded states)
				rob.wallSliding = false;
				rob.wasWallSliding = false;
				rob.moveSpeed = 1.0;
				rob.inAirVelocity = Vector3.zero;
				rob.jumping = false;
				rob.doubleJumping = false;
			}
    	}
    	else {								// Else, if player is against non-climb wall surface
	    	rob.climbing = false;
			rob.wallFacing = hit.normal;
			if (rob.verticalSpeed < 0.0) {		// If player has reached their jump apex or is simply falling	
				if (!rob.wallSliding){				// If player is not already wallSliding, set necessary variables
					rob.wallSliding = true;
	    			rob._characterState = rob.customCharacterState.Wall_Sliding;
	    			rob.jumping = false;
	    			rob.doubleJumping = false;
				}
			}
		}
	}
}

// If the player enters a triggerBox
function OnTriggerEnter(other:Collider)
{
    if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
        rob.climbContact = true;
        rob.numClimbContacts += 1;
	}
    if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
        rob.hangContact = true;
        rob.numHangContacts += 1;
	}
}

// If the player exits a triggerBox
function OnTriggerExit(other:Collider)
{
    if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
    	rob.numClimbContacts -= 1;
    	if (rob.numClimbContacts == 0) {					// If the player has exited all triggerBoxes with "Climb" tags
			rob.climbContact = false;
			rob.climbing = false;
		}
	}
    if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
    	rob.numHangContacts -= 1;
    	if (rob.numHangContacts == 0) {						// If the player has exited all triggerBoxes with "Hang" tags
			rob.hangContact = false;
			rob.hanging = false;
		}
	}
}

// Handle animation states
function AnimationHandler() {
	var movement = rob.moveDirection * rob.moveSpeed + Vector3 (0, rob.verticalSpeed, 0) + rob.inAirVelocity;		// Calculate actual motion
	movement *= Time.deltaTime;			// Base degree of applied movement on time since last frame
	var controller : CharacterController = transform.GetComponent(CharacterController);		// Move the controller
	rob.collisionFlags = controller.Move(movement);
	
	// ANIMATION sector
	if(_animation) {
		switch(rob._characterState)
		{
			// Player is jumping; play jumping animation
			case rob.customCharacterState.Jumping:
				_animation[jumpPoseAnimation.name].speed = jumpAnimationSpeed;		// Animation speed = jump animation speed
				_animation[jumpPoseAnimation.name].wrapMode = WrapMode.Loop;		// Loop the jump animation
				_animation.CrossFade(jumpPoseAnimation.name);
			break;
			// Player is double jumping; play double jumping animation
			case rob.customCharacterState.Double_Jumping:
				_animation[doubleJumpAnimation.name].speed = doubleJumpAnimationSpeed;
				_animation[doubleJumpAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(doubleJumpAnimation.name);
			break;
			// Player is decending in jump; play jump after apex animation
			case rob.customCharacterState.Jumping_After_Apex:
				_animation[jumpPoseAfterApexAnimation.name].speed = jumpAfterApexAnimationSpeed;
				_animation[jumpPoseAfterApexAnimation.name].wrapMode = WrapMode.Loop;
				_animation.CrossFade(jumpPoseAfterApexAnimation.name);	
			break;
			// Player is wallsliding; play wallsliding animation
			case rob.customCharacterState.Wall_Sliding:
				_animation[wallSlideAnimation.name].speed = wallSlideAnimationSpeed;
				_animation[wallSlideAnimation.name].wrapMode = WrapMode.Loop;
				_animation.CrossFade(wallSlideAnimation.name);	
			break;
			// Player is idle while climbing; play climbing idle animation
			case rob.customCharacterState.Climbing_Idle:
				_animation[climbIdlePoseAnimation.name].speed = -climbIdleAnimationSpeed;
				_animation[climbIdlePoseAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(climbIdlePoseAnimation.name);
			break;
			// Player is climbing vertically; play climbing vertical animation
			case rob.customCharacterState.Climbing_Vertical:
				_animation[climbVerticalAnimation.name].speed = rob.verticalSpeed/2;
				_animation[climbVerticalAnimation.name].wrapMode = WrapMode.Loop;
				_animation.CrossFade(climbVerticalAnimation.name);
			break;
			// Player is climbing horizontally; play climbing horizontal animation
			case rob.customCharacterState.Climbing_Horizontal:
				_animation[climbHorizontalAnimation.name].speed = climbHorizontalAnimationSpeed;
				_animation[climbHorizontalAnimation.name].wrapMode = WrapMode.Loop;
				_animation.CrossFade(climbHorizontalAnimation.name);
			break;
			// Player is idle while hanging; play hanging idle animation
			case rob.customCharacterState.Hanging_Idle:
				_animation[hangIdlePoseAnimation.name].speed = -hangIdleAnimationSpeed;
				_animation[hangIdlePoseAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(hangIdlePoseAnimation.name);
			break;
			// Player is moving while hanging; play hanging move animation
			case rob.customCharacterState.Hanging_Move:
				_animation[hangMoveAnimation.name].speed = Mathf.Clamp(rob.moveSpeed, 0.0, hangMoveAnimationSpeed);
				_animation[hangMoveAnimation.name].wrapMode = WrapMode.Loop;
				_animation.CrossFade(hangMoveAnimation.name);
			break;
			// Player is falling fast; play free falling animation
			case rob.customCharacterState.Free_Falling:
				_animation[freefallPoseAnimation.name].speed = freefallAnimationSpeed;
				_animation[freefallPoseAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(freefallPoseAnimation.name);
			break;
			// Player landed hard; play heavy landing animation
			case rob.customCharacterState.Heavy_Landing:
				_animation[heavyLandPoseAnimation.name].speed = heavyLandAnimationSpeed;
				_animation[heavyLandPoseAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(heavyLandPoseAnimation.name);
			break;
			// Player is rolling; play rolling animation
			case rob.customCharacterState.Rolling:
				_animation[rollAnimation.name].speed = rollAnimationSpeed;
				_animation[rollAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(rollAnimation.name);
			break;
			default:
				// If player is standing idle, play idle animation
				if(controller.velocity.sqrMagnitude < 0.1) {
					_animation[idleAnimation.name].speed = -idleAnimationSpeed;
					_animation[idleAnimation.name].wrapMode = WrapMode.ClampForever;
					_animation.CrossFade(idleAnimation.name);
				}
				// Else, player is moving
				else 
				{
					switch(rob._characterState)
					{
						// Player is running; play running animation
						case rob.customCharacterState.Running:
							_animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0, runMaxAnimationSpeed);
							_animation.CrossFade(runAnimation.name);	
						break;
						// Player is trotting; play trotting animation
						case rob.customCharacterState.Trotting:
							_animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0, trotMaxAnimationSpeed);
							_animation.CrossFade(runAnimation.name);	
						break;
						// Player is walking; play walking animation
						default:
							_animation[walkAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0, walkMaxAnimationSpeed);
							_animation.CrossFade(walkAnimation.name);
						break;
					}
				}
			break;
		}
	}
	// ANIMATION sector
}