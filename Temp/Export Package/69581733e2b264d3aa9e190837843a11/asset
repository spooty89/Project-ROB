
// Require a character controller to be attached to the same game object
@script RequireComponent(CharacterController)

public var idleAnimation : AnimationClip;
public var walkAnimation : AnimationClip;
public var runAnimation : AnimationClip;
public var jumpPoseAnimation : AnimationClip;
public var jumpPoseAfterApexAnimation : AnimationClip;
public var doubleJumpPoseAnimation : AnimationClip;
public var wallSlideAnimation : AnimationClip;

public var walkMaxAnimationSpeed : float = 1.0;
public var trotMaxAnimationSpeed : float = 0.7;
public var runMaxAnimationSpeed : float = 1.4;
public var jumpAnimationSpeed : float = 1.0;
public var landAnimationSpeed : float = 1.0;
public var jumpAfterApexAnimationSpeed : float = 1.0;
public var doubleJumpAnimationSpeed : float = 1.0;
public var wallSlideAnimationSpeed : float = 1.0;

private var _animation : Animation;

enum customCharacterStateBackup {
	Idle = 0,
	Walking = 1,
	Trotting = 2,
	Running = 3,
	Jumping = 4,
	Jumping_After_Apex = 5,
	Double_Jumping = 6,
	Wall_Sliding = 7,
}

private var _characterState : customCharacterStateBackup;

var boxcollider : BoxCollider;

// The speed when walking
var walkSpeed = 3.0;
// after trotAfterSeconds of walking we trot with trotSpeed
var trotSpeed = 5.0;
// when pressing "Fire3" button (cmd) we start running
var runSpeed = 8.0;

var inAirControlAcceleration = 4.0;

// How high do we jump when pressing jump and letting go immediately
var jumpHeight = 2.0;

// The gravity for the character
var gravity = 16.0;
// The gravity in controlled descent mode
var speedSmoothing = 10.0;
var rotateSpeed = 450.0;
var trotAfterSeconds = 1.0;

var canJump = true;

private var jumpRepeatTime = 0.05;
private var jumpTimeout = 0.15;
private var groundedTimeout = 0.25;

// The camera doesnt start following the target immediately but waits for a split second to avoid too much waving around.
private var lockCameraTimer = 0.0;

// The current move direction in x-z
private var moveDirection = Vector3.zero;
// The current vertical speed
private var verticalSpeed = 0.0;
// The current x-z move speed
private var moveSpeed = 0.0;

// The last collision flags returned from controller.Move
private var collisionFlags : CollisionFlags; 

// Are we jumping? (Initiated with jump button and not grounded yet)
private var jumping = false;
private var doublejumping = false;
private var jumpingReachedApex = false;
private var wallSliding = false;
private var wallContact = false;
private var hasWallJumped = false;

// Are we moving backwards (This locks the camera to not do a 180 degree spin)
private var movingBack = false;
// Is the user pressing any keys?
private var isMoving = false;
// When did the user start walking (Used for going into trot after a while)
private var walkTimeStart = 0.0;
// Last time the jump button was clicked down
private var lastJumpButtonTime = -10.0;
// Last time we performed a jump
private var lastJumpTime = -1.0;


// the height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
private var lastJumpStartHeight = 0.0;


private var inAirVelocity = Vector3.zero;

private var lastGroundedTime = 0.0;


private var isControllable = true;

function Awake ()
{
	moveDirection = transform.TransformDirection(Vector3.forward);
	
	_animation = GetComponent(Animation);
	if(!_animation)
		Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");
	
	/*
public var idleAnimation : AnimationClip;
public var walkAnimation : AnimationClip;
public var runAnimation : AnimationClip;
public var jumpPoseAnimation : AnimationClip;	
	*/
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
	if(!jumpPoseAnimation && canJump) {
		_animation = null;
		Debug.Log("No jump animation found and the character has canJump enabled. Turning off animations.");
	}
	if(!jumpPoseAfterApexAnimation && canJump) {
		_animation = null;
		Debug.Log("No jump after apex animation found and the character has canJump enabled. Turning off animations.");
	}
	if(!doubleJumpPoseAnimation && canJump) {
		_animation = null;
		Debug.Log("No double jump animation found and the character has canJump enabled. Turning off animations.");
	}
	if(!wallSlideAnimation && canJump) {
		_animation = null;
		Debug.Log("No wall sliding animation found and the character has canJump enabled. Turning off animations.");
	}
			
}


function UpdateSmoothedMovementDirection ()
{
	var cameraTransform = Camera.main.transform;
	var grounded = IsGrounded();
	
	// Forward vector relative to the camera along the x-z plane	
	var forward = cameraTransform.TransformDirection(Vector3.forward);
	forward.y = 0;
	forward = forward.normalized;

	// Right vector relative to the camera
	// Always orthogonal to the forward vector
	var right = Vector3(forward.z, 0, -forward.x);

	var v = Input.GetAxisRaw("Vertical");
	var h = Input.GetAxisRaw("Horizontal");

	// Are we moving backwards or looking backwards
	if (v < -0.2)
		movingBack = true;
	else
		movingBack = false;
	
	var wasMoving = isMoving;
	isMoving = Mathf.Abs (h) > 0.1 || Mathf.Abs (v) > 0.1;
		
	// Target direction relative to the camera
	var targetDirection = h * right + v * forward;
	
	// Grounded controls
	if (grounded)
	{
		// Lock camera for short period when transitioning moving & standing still
		lockCameraTimer += Time.deltaTime;
		if (isMoving != wasMoving)
			lockCameraTimer = 0.0;

		// We store speed and direction seperately,
		// so that when the character stands still we still have a valid forward direction
		// moveDirection is always normalized, and we only update it if there is user input.
		if (targetDirection != Vector3.zero)
		{
			// If we are really slow, just snap to the target direction
			if (moveSpeed < walkSpeed * 0.9 && grounded)
			{
				moveDirection = targetDirection.normalized;
			}
			// Otherwise smoothly turn towards it
			else
			{
				moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
				
				moveDirection = moveDirection.normalized;
			}
		}
		
		// Smooth the speed based on the current target direction
		var curSmooth = speedSmoothing * Time.deltaTime;
		
		// Choose target speed
		//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
		var targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0);
	
		_characterState = customCharacterStateBackup.Idle;
		
		// Pick speed modifier
		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
		{
			targetSpeed *= runSpeed;
			_characterState = customCharacterStateBackup.Running;
		}
		else if (Time.time - trotAfterSeconds > walkTimeStart)
		{
			targetSpeed *= trotSpeed;
			_characterState = customCharacterStateBackup.Trotting;
		}
		else
		{
			targetSpeed *= walkSpeed;
			_characterState = customCharacterStateBackup.Walking;
		}
		
		moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);
		
		// Reset walk time start when we slow down
		if (moveSpeed < walkSpeed * 0.3)
			walkTimeStart = Time.time;
	}
	// In air controls
	else
	{
		// Lock camera while in air
		if (IsJumping())
			lockCameraTimer = 0.0;

		if (isMoving)
			inAirVelocity += targetDirection.normalized * Time.deltaTime * inAirControlAcceleration;
	}
	

		
}


function ApplyJumping ()
{
	// Prevent jumping too fast after each other
	if ((lastJumpTime + jumpRepeatTime > Time.time) && !IsDoubleJumping())
		return;

	if (IsGrounded() || !IsDoubleJumping() || (wallSliding && !hasWallJumped)) {
		// Jump
		// - Only when pressing the button down
		// - With a timeout so you can press the button slightly before landing		
		if (canJump && Time.time < lastJumpButtonTime + jumpTimeout) {
			verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
			SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
			if(wallSliding && !hasWallJumped) {
				hasWallJumped = true;
				wallSliding = false;
        		wallContact = false;
			}
		}
	}
	/*Debug.Log("wallSliding" + wallSliding);
	Debug.Log("hasWallJumped" + hasWallJumped);*/
}


function ApplyGravity ()
{
	if (isControllable)	// don't move player at all if not controllable.
	{
		// Apply gravity
		var jumpButton = Input.GetButton("Jump");
		
		
		// When we reach the apex of the jump we send out a message
		if ((IsJumping() || IsDoubleJumping()) && !jumpingReachedApex && verticalSpeed <= 0.0)
		{
			jumpingReachedApex = true;
			_characterState = customCharacterStateBackup.Jumping_After_Apex;
			SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
		}
	
		if (IsGrounded ()) {
			wallSliding = false;
			wallContact = false;
			hasWallJumped = false;
			jumpingReachedApex = false;
			verticalSpeed = 0.0;
		}
		else {
			if (wallSliding)
				verticalSpeed -= gravity * Time.deltaTime * .2;
			else
				verticalSpeed -= gravity * Time.deltaTime;
		}
	}
}

function CalculateJumpVerticalSpeed (targetJumpHeight : float)
{
	// From the jump height and gravity we deduce the upwards speed 
	// for the character to reach at the apex.
	return Mathf.Sqrt(2 * targetJumpHeight * gravity);
}

function DidJump ()
{
	if(IsJumping()) {
		doublejumping = true;
		_characterState = customCharacterStateBackup.Double_Jumping;
	}
	else {
		jumping = true;
		_characterState = customCharacterStateBackup.Jumping;
	}
	
	jumpingReachedApex = false;
	lastJumpTime = Time.time;
	lastJumpStartHeight = transform.position.y;
	lastJumpButtonTime = -10;
}

function Update() {
	
	if (!isControllable)
	{
		// kill all inputs if not controllable.
		Input.ResetInputAxes();
	}

	if (Input.GetButtonDown ("Jump"))
	{
		lastJumpButtonTime = Time.time;
	}

	UpdateSmoothedMovementDirection();
	
	// Apply gravity
	// - extra power jump modifies gravity
	// - controlledDescent mode modifies gravity
	ApplyGravity ();

	// Apply jumping logic
	ApplyJumping ();
	
	// Calculate actual motion
	var movement = moveDirection * moveSpeed + Vector3 (0, verticalSpeed, 0) + inAirVelocity;
	movement *= Time.deltaTime;
	
	// Move the controller
	var controller : CharacterController = GetComponent(CharacterController);
	collisionFlags = controller.Move(movement);
	
	// ANIMATION sector
	if(_animation) {
		switch(_characterState)
		{
			case customCharacterStateBackup.Jumping:
				_animation[jumpPoseAnimation.name].speed = jumpAnimationSpeed;
				_animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(jumpPoseAnimation.name);
			break;
			case customCharacterStateBackup.Double_Jumping:
				_animation[doubleJumpPoseAnimation.name].speed = doubleJumpAnimationSpeed;
				_animation[doubleJumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(doubleJumpPoseAnimation.name);
			break;
			case customCharacterStateBackup.Jumping_After_Apex:
				_animation[jumpPoseAfterApexAnimation.name].speed = -landAnimationSpeed;
				_animation[jumpPoseAfterApexAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(jumpPoseAfterApexAnimation.name);	
			break;
			case customCharacterStateBackup.Wall_Sliding:
				_animation[wallSlideAnimation.name].speed = -wallSlideAnimationSpeed;
				_animation[wallSlideAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(wallSlideAnimation.name);	
			break;
			default:
				if(controller.velocity.sqrMagnitude < 0.1) {
					_animation.CrossFade(idleAnimation.name);
				}
				else 
				{
					switch(_characterState)
					{
						case customCharacterStateBackup.Running:
							_animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0, runMaxAnimationSpeed);
							_animation.CrossFade(runAnimation.name);	
						break;
						case customCharacterStateBackup.Trotting:
							_animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0, trotMaxAnimationSpeed);
							_animation.CrossFade(runAnimation.name);	
						break;
						case customCharacterStateBackup.Walking:
							_animation[walkAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0, walkMaxAnimationSpeed);
							_animation.CrossFade(walkAnimation.name);	
						break;
					}
				}
			break;
		}
	}
	// ANIMATION sector
	
	// Set rotation to the move direction
	if (IsGrounded())
	{
		
		transform.rotation = Quaternion.LookRotation(moveDirection);
			
	}	
	else
	{
		var xzMove = movement;
		xzMove.y = 0;
		if (xzMove.sqrMagnitude > 0.001)
		{
			transform.rotation = Quaternion.LookRotation(xzMove);
		}
	}	
	
	// We are in jump mode but just became grounded
	if (IsGrounded())
	{
		lastGroundedTime = Time.time;
		inAirVelocity = Vector3.zero;
		if (IsJumping())
		{
			jumping = false;
			doublejumping = false;
			SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
		}
	}
}

function OnControllerColliderHit (hit : ControllerColliderHit )
{
	//Stuff in original; don't know what it's used for, but will keep till it's known to be useless
	/*Debug.DrawRay(hit.point, hit.normal);
	if (hit.moveDirection.y > 0.01) 
		return;*/
		
	if (hit.rigidbody) { //hit.normal.y < 0.707)){  //used to parse floors/tops of platforms
		//if (hit.gameObject.tag == "Platform") {
			if (Mathf.Abs(hit.normal.y) > 0.1) {
				/*if ((Mathf.Abs(hit.normal.x) > 0.1) || (Mathf.Abs(hit.normal.z) > 0.1))
					Debug.Log("Hit edge of: " + hit.gameObject.name);
				else
					Debug.Log("Hit top/bottom of: " + hit.gameObject.name);*/
			}
			else {
				wallContact = true;
				//Debug.Log("Hit side of: " + hit.gameObject.name);
				if (HasJumpReachedApex()) {
					
					wallSliding = true;
	        		_characterState = customCharacterStateBackup.Wall_Sliding;
	        		if (!hasWallJumped) {
	        			jumping = false;
	        			doublejumping = false;
	        		}
	        	}
			}
		/*}
		if (hit.gameObject.tag == "Wall") {
        	wallContact = true;
			Debug.Log("Hit wall: " + hit.gameObject.name);
			if (HasJumpReachedApex()) {
				wallSliding = true;
        		_characterState = customCharacterStateBackup.Wall_Sliding;
        		if (!hasWallJumped) {
        			jumping = false;
        			doublejumping = false;
        		}
        	}
		}*/
	}
		
}

function GetSpeed () {
	return moveSpeed;
}

function IsJumping () {
	return jumping;
}

function IsDoubleJumping () {
	return doublejumping;
}

function IsGrounded () {
	return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
}

function GetDirection () {
	return moveDirection;
}

function IsMovingBackwards () {
	return movingBack;
}

function GetLockCameraTimer () 
{
	return lockCameraTimer;
}

function IsMoving ()  : boolean
{
	return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5;
}

function HasJumpReachedApex ()
{
	return jumpingReachedApex;
}

function IsGroundedWithTimeout ()
{
	return lastGroundedTime + groundedTimeout > Time.time;
}

function Reset ()
{
	gameObject.tag = "Player";
}

/*function OnCollisionEnter(collision : Collision)
{
	for (var contact : ContactPoint in collision.contacts) {
		Debug.Log(collision.gameObject.name);
		Debug.DrawRay(contact.point, contact.normal, Color.white);
	}
}*/

function OnTriggerEnter(other:Collider)
{
    // if our trigger is touching an objec with tag "Player"
    if(other.gameObject.CompareTag("Wall"))
    {    
        wallContact = true;
        Debug.Log("Wall Contact");
	}
}

function OnTriggerExit(other:Collider)
{
    // if our trigger is touching an objec with tag "Player"

    if(other.gameObject.CompareTag("Wall"))
    {    
    		wallContact = false;
    		wallSliding = false;
			hasWallJumped = false;
            Debug.Log("No Wall Contact");
            if(Input.GetButton("Jump"))
            {
                //jump.canDoubleJump = false;
                } 
}
}
