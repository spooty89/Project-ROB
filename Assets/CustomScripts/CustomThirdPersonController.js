
// Require a character controller to be attached to the same game object
@script RequireComponent(CharacterController)

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

public var idleAnimationSpeed : float = 1.0;
public var walkMaxAnimationSpeed : float = 1.0;
public var trotMaxAnimationSpeed : float = 0.7;
public var runMaxAnimationSpeed : float = 1.4;
public var jumpAnimationSpeed : float = 1.0;
public var landAnimationSpeed : float = 1.0;
public var jumpAfterApexAnimationSpeed : float = 1.0;
public var doubleJumpAnimationSpeed : float = 1.0;
public var wallSlideAnimationSpeed : float = 1.0;
public var rollAnimationSpeed : float = 1.0;
public var climbIdleAnimationSpeed : float = 1.0;
public var climbVerticalAnimationSpeed : float = 1.0;
public var climbHorizontalAnimationSpeed : float = 1.0;
public var hangIdleAnimationSpeed : float = 1.0;
public var hangMoveAnimationSpeed : float = 1.0;
public var freefallAnimationSpeed : float = 1.0;
public var heavyLandAnimationSpeed : float = 1.0;

private var _animation : Animation;

enum customCharacterState {
	Idle = 0,
	Walking = 1,
	Trotting = 2,
	Running = 3,
	Jumping = 4,
	Jumping_After_Apex = 5,
	Double_Jumping = 6,
	Wall_Sliding = 7,
	Climbing_Idle = 8,
	Climbing_Vertical = 9,
	Climbing_Horizontal = 10,
	Hanging_Idle = 11,
	Hanging_Move = 12,
	Free_Falling = 13,
	Heavy_Landing = 14,
	Rolling = 15,
}

private var _characterState : customCharacterState;

// The speed when walking
var walkSpeed = 3.0;
// after trotAfterSeconds of walking we trot with trotSpeed
var trotSpeed = 5.0;
// when pressing "Fire3" button (cmd) we start running
var runSpeed = 8.0;

// Acceleration from jumping
var jumpAcceleration = 2.0;
// from double jumping
var doubleJumpAcceleration = 1.0;

// How high we jump when pressing jump and letting go immediately
var jumpHeight = 2.0;
// How high we jump when we double jump
var doubleJumpHeight = 1.0;

// The gravity for the character
var gravity = 16.0;
// Wall sliding reduces gravity's effect on character
var wallSlidingGravity = 3.2;
// The gravity in controlled descent mode
var speedSmoothing = 10.0;
var rotateSpeed = 450.0;
var inAirRotateSpeed = 100.0;
var trotAfterSeconds = 1.0;
var timeAfterJumpLimitRotate = 0.5;

var canJump = true;

private var jumpRepeatTime = 0.05;
private var jumpTimeout = 0.15;
private var heavyLandingTimeout = 0.75;
private var heavyLandingTime = 0.0;
var rollingTimeout = 0.75;
private var rollingTime = 0.0;
private var sprintTimeout = 0.4;
private var sprintTime = 0.0;

// The camera doesnt start following the target immediately but waits for a split second to avoid too much waving around.
private var lockCameraTimer = 0.0;

// The current move direction in x-z
private var moveDirection = Vector3.zero;
// The current vertical speed
private var verticalSpeed = 0.0;
private var previousVerticalSpeed = 0.0;
// The current x-z move speed
private var moveSpeed = 0.0;

// The last collision flags returned from controller.Move
private var collisionFlags : CollisionFlags; 

//private var lastWallSlideObject = "";

// Are we jumping? (Initiated with jump button and not grounded yet)
private var jumping = false;
private var doubleJumping = false;
private var jumpingReachedApex = false;
private var wallSliding = false;
private var wallContact = false;
private var hangContact = false;
private var numClimbContacts = 0;
private var numHangContacts = 0;
private var wasWallSliding = false;
private var isFreeFalling = false;
private var flying = false;
private var climbing = false;
private var climbContact = false;
private var hanging = false;
private var hangingContact = false;
private var decending = false;
private var heavyLanding = false;
private var rolling = false;

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
// The height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
private var lastJumpStartHeight = 0.0;

// Direction to face when wall sliding and jumping from wall
private var wallFacing = Vector3.zero;
private var wallRight = Vector3.zero;

private var inAirVelocity = Vector3.zero;

private var hookShotHit : RaycastHit;

private var isControllable = true;


function Awake ()
{
	moveDirection = transform.TransformDirection(Vector3.forward);
	
	_animation = GetComponent(Animation);
	if(!_animation)
		Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");
	
	// Check for various animations
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
	if(!jumpPoseAfterApexAnimation) {
		_animation = null;
		Debug.Log("No jump after apex animation found. Turning off animations.");
	}
	if(!doubleJumpAnimation && canJump) {
		_animation = null;
		Debug.Log("No double jump animation found and the character has canJump enabled. Turning off animations.");
	}
	if(!wallSlideAnimation) {
		_animation = null;
		Debug.Log("No wall sliding animation found. Turning off animations.");
	}
			
}


function UpdateSmoothedMovementDirection ()
{
	var cameraTransform = Camera.main.transform;
	var grounded = IsGrounded();
	
	// Forward vector relative to the camera along the x-z plane	
	var forward = cameraTransform.TransformDirection(Vector3.forward);
	if (!flying)
		forward.y = 0.0;
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
	
	var targetDirection = transform.forward;
	// Target direction relative to the camera
	if (!climbing)
		targetDirection = h * right + v * forward;
	
	// Grounded controls
	if ((grounded || flying || hanging) && !climbing)
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
			/*if (moveSpeed < walkSpeed * 0.9 && grounded)
			{
				moveDirection = targetDirection.normalized;
			}
			// Otherwise smoothly turn towards it
			else
			{*/
				moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
				
				moveDirection = moveDirection.normalized;
			//}
			
		}
		
		// Smooth the speed based on the current target direction
		var curSmooth = speedSmoothing * Time.deltaTime;
		if (flying)
			curSmooth /= 5;
		
		// Choose target speed
		//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
		var targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0);
	
		_characterState = customCharacterState.Idle;
		
		// Pick speed modifier
		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
		{
			if (flying)
				targetSpeed *= 3;
			targetSpeed *= runSpeed;
			_characterState = customCharacterState.Running;
		}
		else if (Time.time - trotAfterSeconds > walkTimeStart)
		{
			if (flying)
				targetSpeed *= 2;
			targetSpeed *= trotSpeed;
			_characterState = customCharacterState.Trotting;
		}
		else
		{
			if (flying)
				targetSpeed *= 2;
			targetSpeed *= walkSpeed;
			_characterState = customCharacterState.Walking;
		}
		
		if (hanging) {
			if (targetSpeed > 0.0) {
				//Debug.Log("am");
				_characterState = customCharacterState.Hanging_Move;
			}
			else
				_characterState = customCharacterState.Hanging_Idle;
		}
		
		moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);
		
		// Reset walk time start when we slow down
		if (moveSpeed < walkSpeed * 0.3)
			walkTimeStart = Time.time;
	}
	// In air controls
	else// if (IsJumping() || IsDoubleJumping())
	{
		if (targetDirection != Vector3.zero)
		{
			if (wasWallSliding) 
			{
				if ((Time.time - lastJumpTime) <= timeAfterJumpLimitRotate) {
					var temp = (1.0 / timeAfterJumpLimitRotate) * (Time.time - lastJumpTime);
					moveDirection += Vector3.RotateTowards(moveDirection, targetDirection, (inAirRotateSpeed*temp) * Mathf.Deg2Rad * Time.deltaTime, 1000);
				}
				else
					wasWallSliding = false;
			}
			else
				moveDirection += Vector3.RotateTowards(moveDirection, targetDirection, inAirRotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
				
			moveDirection = moveDirection.normalized;
		}
		// Lock camera while in air
		if (IsJumping())
			lockCameraTimer = 0.0;

		if (isMoving) {
			if (doubleJumping)
				inAirVelocity += targetDirection.normalized * Time.deltaTime * jumpAcceleration;
			else
				inAirVelocity += targetDirection.normalized * Time.deltaTime * doubleJumpAcceleration;
		}
	}	
}


function ApplyJumping ()
{
	// Prevent jumping too fast after each other
	if ((lastJumpTime + jumpRepeatTime > Time.time) && !IsDoubleJumping())
		return;

	if (IsGrounded() || !IsDoubleJumping() || wallSliding){
		// Jump
		// - Only when pressing the button down
		// - With a timeout so you can press the button slightly before landing		
		if (canJump && Time.time < lastJumpButtonTime + jumpTimeout) {
			if (!IsDoubleJumping())
				verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
			else
				verticalSpeed = CalculateJumpVerticalSpeed (doubleJumpHeight);
			SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
			if(wallSliding || climbing){
				inAirVelocity = Vector3.zero;
				jumpingReachedApex = false;
				wasWallSliding = true;
				/* Carry portion of moment on wall into jump (too unpredictable for player)
				if (SurfaceNormalContactAngleDiff() > 1.0) {
					DirectionOnWall();
					moveDirection *= 0.5;
					moveDirection += (wallFacing * (SurfaceNormalContactAngleDiff()/2.0));
					moveDirection.Normalize();
				}
				else*/
					//moveDirection *= 0.1;
				moveDirection = wallFacing;
				moveDirection = moveDirection.normalized;
				moveSpeed = 8.0;
				wallSliding = false;
        		wallContact = false;
        		//Debug.Log("jumping?");
        		climbing = false;
        		//climbContact = false;
			}
		}
	}
}


function ApplyGravity ()
{
	if (isControllable)	// don't move player at all if not controllable.
	{
		// Apply gravity
		var jumpButton = Input.GetButton("Jump");
		var temp;
		
		// When we reach the apex of the jump we send out a message
		if ((IsJumping() || IsDoubleJumping()) && !jumpingReachedApex && verticalSpeed <= 0.0)
		{
			jumpingReachedApex = true;
			_characterState = customCharacterState.Jumping_After_Apex;
			SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
		}
	
		if (IsGrounded ()) {
			wallSliding = false;
			wallContact = false;
			hangContact = false;
			jumpingReachedApex = false;
			if (!climbing)
				verticalSpeed = 0.0;
			
			if ((previousVerticalSpeed < -10.0) && (moveSpeed >= runSpeed/1.5)) {
				rolling = true;
				//Debug.Log("Previous Vertical Speed : " + previousVerticalSpeed);
				rollingTime = Time.time;
				_characterState = customCharacterState.Rolling;
			}
			else if (previousVerticalSpeed <= -25.0) {
				//Debug.Log("heavy landing : " + previousVerticalSpeed);
				heavyLanding = true;
				heavyLandingTime = Time.time;
				_characterState = customCharacterState.Heavy_Landing;
			}
		}
		else {
			if (wallSliding) {
				if (collisionFlags == 0) {
					wallSliding = false;
					wallContact = false;
					hangContact = false;
					verticalSpeed -= gravity * Time.deltaTime;
					_characterState = customCharacterState.Jumping_After_Apex;
					temp = moveDirection.normalized.magnitude;
					moveDirection = DirectionOnWall(moveDirection);
					moveSpeed *= (moveDirection.magnitude/temp);
				}
				else
					verticalSpeed -= wallSlidingGravity * Time.deltaTime;
			}
			else if (climbing) {
				if (collisionFlags == 0) {
					//Debug.Log("climbing false");
					climbing = false;
			transform.position.x += (wallFacing.x * 0.1);
			transform.position.z += (wallFacing.z * 0.1);
					//climbContact = false;
					/*verticalSpeed -= gravity * Time.deltaTime;
					_characterState = customCharacterState.Jumping_After_Apex;
					temp = moveDirection.normalized.magnitude;
					moveDirection = DirectionOnWall();
					moveSpeed *= (moveDirection.magnitude/temp);*/
				}
				else {
					if (verticalSpeed < -0.5)
						verticalSpeed += gravity * Time.deltaTime;
					else if (verticalSpeed > 0.5)
						verticalSpeed -= gravity * Time.deltaTime;
					if (Mathf.Abs(verticalSpeed) <= 0.5) {
						//Debug.Log("not moving");
						verticalSpeed = 0.0;
						}
				}
			}
			else if (hanging) {
				if (collisionFlags == 0) {
					//Debug.Log("climbing false");
					hanging = false;
					hangContact = false;
					/*verticalSpeed -= gravity * Time.deltaTime;
					_characterState = customCharacterState.Jumping_After_Apex;
					temp = moveDirection.normalized.magnitude;
					moveDirection = DirectionOnWall();
					moveSpeed *= (moveDirection.magnitude/temp);*/
				}
				else {
					verticalSpeed = 1.0;
				}
			}
			else {
				if (verticalSpeed < -1.0){
					_characterState = customCharacterState.Jumping_After_Apex;
					}
				if (verticalSpeed > -40.0)
					verticalSpeed -= gravity * Time.deltaTime;
				if (verticalSpeed < -15.0) {
					//Debug.Log("freefalling");
					_characterState = customCharacterState.Free_Falling;
				}
			}
		}
		previousVerticalSpeed = verticalSpeed;
	}
}

// Determine the direction of the player against the surface (for climbing side to side and wallsliding)
function DirectionOnWall(dir)
{
	var returnAngle : Vector3 = Vector3.zero;
	
	if (dir.x <= -0.1)
		returnAngle.x = -(1.0 - Mathf.Abs(wallFacing.x));
	else if (dir.x >= 0.1)
		returnAngle.x = 1.0 - Mathf.Abs(wallFacing.x);
		
	if (dir.z <= -0.1)
		returnAngle.z = -(1.0 - Mathf.Abs(wallFacing.z));
	else if (dir.z >= 0.1)
		returnAngle.z = 1.0 - Mathf.Abs(wallFacing.z);
	
	returnAngle = returnAngle.normalized;
	
	return returnAngle;
}

function CalculateJumpVerticalSpeed (targetJumpHeight : float)
{
	// From the jump height and gravity we deduce the upwards speed 
	// for the character to reach at the apex.
	return Mathf.Sqrt(2 * targetJumpHeight * gravity);
}

// Handle the jump/double-jump actions
function DidJump ()
{
	if(IsJumping()) {
		doubleJumping = true;
		_characterState = customCharacterState.Double_Jumping;
	}
	else {
		jumping = true;
		_characterState = customCharacterState.Jumping;
	}
	
	jumpingReachedApex = false;
	lastJumpTime = Time.time;
	lastJumpStartHeight = transform.position.y;
	lastJumpButtonTime = -10;
}

// Update the current state of the game
function Update() {
	
	if (!isControllable) {		// Kill all inputs if not controllable (here by default, may be useful for in-game cutscenes?)
		Input.ResetInputAxes();
	}
	
	if ((Input.GetKeyDown (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) 
		&& (moveSpeed > 1.0)) {								// Handle sprinting (not a thing right now, may be implemented in the future)
		if ((Time.time - sprintTime) <= sprintTimeout) {		// If two shift presses fall within the timeout, sprinting
			sprint = true;
		}
		else {													// Else, get time of the current shift press and not sprinting
			sprint = false;
			sprintTime = Time.time;
		}
	}
	
	if (Input.GetButtonDown ("Jump") && !flying) {	// If jump button pressed, get time of jump
		lastJumpButtonTime = Time.time;
	}
	
	else if (Input.GetButtonDown ("Fly")) {		// If fly button pressed, apply flying values to appropriate variables (doesn't matter too much; just for fast testing)
		flying = !flying;
		verticalSpeed = 0.0;
		moveSpeed = 0.0;
		if (wallSliding) {							// If fly button pressed while wall sliding
			moveDirection = DirectionOnWall(moveDirection);
			transform.rotation = Quaternion.LookRotation(moveDirection);
			wallSliding = false;
			wasWallSliding = false;
		}	
		else if (climbing) {						// If fly button pressed while climbing
			moveDirection = wallFacing;
			moveSpeed = 1.0;
			climbing = false;
			jumping = true;
		}
		else {
			moveDirection = Vector3(moveDirection.x, 0.0, moveDirection.z);
			moveDirection = moveDirection.normalized;
			transform.rotation = Quaternion.LookRotation(moveDirection);
		}
		inAirVelocity = Vector3(0.0, 0.0, 0.0);
		jumping = false;
		doubleJumping = false;
	}
	
	else if (Input.GetButton ("Interact"))		// If the interact butten is pressed,
	{
		if (climbing) {								// and player is climbing, release from wall and apply slight pushoff (to prevent instantly regrabbing the wall)
			moveDirection = wallFacing;
			moveSpeed = 1.0;
			climbing = false;
			jumping = true;
		}
		else if (hanging) {							// and player is hanging, release from ceiling
			hanging = false;
		}
	}
	
	// If the player is climbing
	if (climbing) {
		if (Input.GetButton ("Vertical")) {			// If one of the up/down buttons is pressed
			_characterState = customCharacterState.Climbing_Vertical;
			verticalSpeed = 2.0 * Input.GetAxis("Vertical");
			if (verticalSpeed < 0.0)				// If moving down
				decending = true;
			else
				decending = false;
			if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))		// Climb faster
				verticalSpeed *= 2;
		}
		// If one of the up/down buttons is released
		if (Input.GetButtonUp ("Vertical"))
		{
			_characterState = customCharacterState.Climbing_Idle;
			inAirVelocity = Vector3.zero;
			moveDirection = -wallFacing;
			moveSpeed = 0.1;
		}
		// If one of the left/right buttons is pressed
		if (Input.GetButton ("Horizontal"))
		{
			if (_characterState == customCharacterState.Climbing_Idle)
				_characterState = customCharacterState.Climbing_Horizontal;
			moveSpeed = 2.0;
			moveDirection += Vector3(wallRight.x * Input.GetAxis("Horizontal"), wallRight.y * 
							 Input.GetAxis("Horizontal"), wallRight.z * Input.GetAxis("Horizontal"));
			moveDirection = moveDirection.normalized;
			if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
				moveSpeed *= 2;
		}
		// If one of the left/right buttons is released
		if (Input.GetButtonUp ("Horizontal"))
		{
			if (_characterState == customCharacterState.Climbing_Horizontal)
				_characterState = customCharacterState.Climbing_Idle;
			inAirVelocity = Vector3.zero;
			moveDirection = -wallFacing;
			moveSpeed = 0.1;
		}
	}

	// Smooth the player movement
	UpdateSmoothedMovementDirection();
	
	// If player isn't flying
	if (!flying){
		ApplyGravity ();		// Apply gravity
		ApplyJumping ();		// Apply jumping logic
	}
	
	var movement = moveDirection * moveSpeed + Vector3 (0, verticalSpeed, 0) + inAirVelocity;		// Calculate actual motion
	movement *= Time.deltaTime;			// Base degree of applied movement on time since last frame

	// If rolling (and time since rolling began is less than rolling timeout)
	if (rolling && ((Time.time - rollingTime) < rollingTimeout)) {
		_characterState = customCharacterState.Rolling;
		if (moveSpeed < 5.0)							// If player speed is below minimum for roll, make minimum roll speed
			moveSpeed = 5.0;
	}
	else {
		rolling = false;
	}
	
	// Handle heavy landing (big fall w/o enough forward movement to be roll)
	if (heavyLanding && ((Time.time - heavyLandingTime) < heavyLandingTimeout)) {
		_characterState = customCharacterState.Heavy_Landing;
		movement = Vector3.zero;
		verticalSpeed = 0.0;
		previousVerticalSpeed = 0.0;
		rolling = false;
	}
	else
		heavyLanding = false;
	
	// Move the controller
	var controller : CharacterController = GetComponent(CharacterController);
	collisionFlags = controller.Move(movement);
	
	// ANIMATION sector
	if(_animation) {
		switch(_characterState)
		{
			// Player is jumping; play jumping animation
			case customCharacterState.Jumping:
				_animation[jumpPoseAnimation.name].speed = jumpAnimationSpeed;		// Animation speed = jump animation speed
				_animation[jumpPoseAnimation.name].wrapMode = WrapMode.Loop;		// Loop the jump animation
				_animation.CrossFade(jumpPoseAnimation.name);
			break;
			case customCharacterState.Double_Jumping:
				_animation[doubleJumpAnimation.name].speed = doubleJumpAnimationSpeed;
				_animation[doubleJumpAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(doubleJumpAnimation.name);
			break;
			case customCharacterState.Jumping_After_Apex:
				_animation[jumpPoseAfterApexAnimation.name].speed = jumpAfterApexAnimationSpeed;
				_animation[jumpPoseAfterApexAnimation.name].wrapMode = WrapMode.Loop;
				_animation.CrossFade(jumpPoseAfterApexAnimation.name);	
			break;
			case customCharacterState.Wall_Sliding:
				_animation[wallSlideAnimation.name].speed = wallSlideAnimationSpeed;
				_animation[wallSlideAnimation.name].wrapMode = WrapMode.Loop;
				_animation.CrossFade(wallSlideAnimation.name);	
			break;
			case customCharacterState.Climbing_Idle:
				_animation[climbIdlePoseAnimation.name].speed = -climbIdleAnimationSpeed;
				_animation[climbIdlePoseAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(climbIdlePoseAnimation.name);
			break;
			case customCharacterState.Climbing_Vertical:
				_animation[climbVerticalAnimation.name].speed = verticalSpeed/2;
				_animation[climbVerticalAnimation.name].wrapMode = WrapMode.Loop;
				_animation.CrossFade(climbVerticalAnimation.name);
			break;
			case customCharacterState.Climbing_Horizontal:
				_animation[climbHorizontalAnimation.name].speed = climbHorizontalAnimationSpeed;
				_animation[climbHorizontalAnimation.name].wrapMode = WrapMode.Loop;
				_animation.CrossFade(climbHorizontalAnimation.name);
			break;
			case customCharacterState.Hanging_Idle:
				_animation[hangIdlePoseAnimation.name].speed = -hangIdleAnimationSpeed;
				_animation[hangIdlePoseAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(hangIdlePoseAnimation.name);
			break;
			case customCharacterState.Hanging_Move:
				_animation[hangMoveAnimation.name].speed = Mathf.Clamp(moveSpeed, 0.0, hangMoveAnimationSpeed);
				_animation[hangMoveAnimation.name].wrapMode = WrapMode.Loop;
				_animation.CrossFade(hangMoveAnimation.name);
			break;
			case customCharacterState.Free_Falling:
				_animation[freefallPoseAnimation.name].speed = freefallAnimationSpeed;
				_animation[freefallPoseAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(freefallPoseAnimation.name);
			break;
			case customCharacterState.Heavy_Landing:
				_animation[heavyLandPoseAnimation.name].speed = heavyLandAnimationSpeed;
				_animation[heavyLandPoseAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(heavyLandPoseAnimation.name);
			break;
			case customCharacterState.Rolling:
				_animation[rollAnimation.name].speed = rollAnimationSpeed;
				_animation[rollAnimation.name].wrapMode = WrapMode.ClampForever;
				_animation.CrossFade(rollAnimation.name);
			break;
			default:
				if(controller.velocity.sqrMagnitude < 0.1) {
					_animation[idleAnimation.name].speed = -idleAnimationSpeed;
					_animation[idleAnimation.name].wrapMode = WrapMode.ClampForever;
					_animation.CrossFade(idleAnimation.name);
				}
				else 
				{
					switch(_characterState)
					{
						case customCharacterState.Running:
							_animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0, runMaxAnimationSpeed);
							_animation.CrossFade(runAnimation.name);	
						break;
						case customCharacterState.Trotting:
							_animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0, trotMaxAnimationSpeed);
							_animation.CrossFade(runAnimation.name);	
						break;
						case customCharacterState.Walking:
							_animation[walkAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0, walkMaxAnimationSpeed);
							_animation.CrossFade(walkAnimation.name);	
						break;
						default:
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
		if (moveDirection == Vector3.zero) 
			moveDirection = wallFacing;
		transform.rotation = Quaternion.LookRotation(moveDirection);
		
		inAirVelocity = Vector3.zero;
		if (climbing && decending) {		// If just reached ground from climbing downward
			climbing = false;
			decending = false;
			transform.position.x += (wallFacing.x * 0.1);		// Move the player away
			transform.position.z += (wallFacing.z * 0.1);		// from the climb surface
		}
		if (IsJumping()) {					// If just landed from jump
			jumping = false;
			doubleJumping = false;
			SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
		}
			
	}	
	else
	{
		if (wallSliding)
			transform.rotation = Quaternion.LookRotation(wallFacing);
		else 
			transform.rotation = Quaternion.LookRotation(moveDirection);
	}
}

// Player has come in contact with a surface
function OnControllerColliderHit (hit : ControllerColliderHit )
{
	if (Mathf.Abs(hit.normal.y) > 0.3) {					// If surface of contact is relatively horizontal
		if (hangContact && (hit.normal.y < -0.1)) {				// If surface is above and player is within hang triggerBox
			if (!hanging) {											// If player isn't already hanging, set necessary variables
				hanging = true;
				wallSliding = false;
				wasWallSliding = false;
				climbing = false;
				jumping = false;
				doubleJumping = false;
				_characterState = customCharacterState.Hanging_Idle;
				inAirVelocity = Vector3.zero;
			}
		}
		else {													// Else, if surface is below and/or player is not within hang triggerBox
			hanging = false;
			if (climbing && decending) {							// If player just reached flat ground after climbing downward
				transform.position.x += (wallFacing.x * 0.1);
				transform.position.z += (wallFacing.z * 0.1);
				moveSpeed = 0.0;
				climbing = false;
				decending = false;
			}
		}
	}
	else {													// If surface of contact is relatively vertical
		wallContact = true;
		if (climbContact) {										// If player is within climb triggerBox
			wallFacing = hit.normal;
			wallRight = DirectionOnWall(wallFacing);
			moveDirection = -wallFacing;
			transform.rotation = Quaternion.LookRotation(moveDirection);
			if (!climbing) {										// If player isn't already climbing, set necessary variables
				climbing = true;
				hanging = false;
				_characterState = customCharacterState.Climbing_Idle;
				if (IsGrounded())										// If player walks into climb surface (rather than jumping)
					verticalSpeed = 1.0;									// need to get them off ground (otherwise, rapid switch between climb and grounded states)
				wallSliding = false;
				wasWallSliding = false;
				moveSpeed = 1.0;
				inAirVelocity = Vector3.zero;
				jumping = false;
				doubleJumping = false;
			}
    	}
    	else {													// Else, if player is against non-climb wall surface
	    	climbing = false;
			wallFacing = hit.normal;
			if (HasJumpReachedApex() || (verticalSpeed < -1.0)) {	// If player has reached their jump apex or is simply falling	
				if (!wallSliding){										// If player is not already wallSliding, set necessary variables
					wallSliding = true;
	    			_characterState = customCharacterState.Wall_Sliding;
	    			jumping = false;
	    			doubleJumping = false;
				}
			}
		}
	}
}

// These functions just return the truth value of their implied variables
function GetSpeed () {
	return moveSpeed;
}

function IsJumping () {
	return jumping;
}

function IsDoubleJumping () {
	return doubleJumping;
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

function Reset ()
{
	gameObject.tag = "Player";
}

// If the player enters a triggerBox
function OnTriggerEnter(other:Collider)
{
    if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
        climbContact = true;
        numClimbContacts += 1;
	}
    if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
        hangContact = true;
        numHangContacts += 1;
	}
}

// If the player exits a triggerBox
function OnTriggerExit(other:Collider)
{
    if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
    	numClimbContacts -= 1;
    	if (numClimbContacts == 0) {					// If the player has exited all triggerBoxes with "Climb" tags
			climbContact = false;
			climbing = false;
		}
	}
    if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
    	numHangContacts -= 1;
    	if (numHangContacts == 0) {						// If the player has exited all triggerBoxes with "Hang" tags
			hangContact = false;
			hanging = false;
		}
	}
}