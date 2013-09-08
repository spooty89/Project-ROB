using System;
using UnityEngine;

public class ROB : MonoBehaviour
{
	private CharacterClass _Player;
	private float walkSpeed = (float)4.0;	// The speed when walking
	//private float trotSpeed = (float)5.0;	// After trotAfterSeconds of walking we trot with trotSpeed
	private float runSpeed = (float)8.0;	// When pressing Shift button we start running
	
	public bool canJump = true;
	private float jumpAcceleration = (float)2.0;			// Acceleration from jumping
	private float doubleJumpAcceleration = (float)1.0;		// from double jumping
	private float jumpHeight = (float)1.5;					// How high we jump when pressing jump and letting go immediately
	private float doubleJumpHeight = (float)0.75;			// How high we jump when we double jump
	
	private float wallSlidingGravity = (float)3.2;		// Wall sliding reduces gravity's effect on character
	
	private float speedSmoothing = (float)10.0;
	private float rotateSpeed = (float)900.0;
	private float inAirRotateSpeed = (float)450.0;
	
	//private float trotAfterSeconds = (float)1.0;
	private float timeAfterJumpLimitRotate = (float)0.9;
	private float rollingTimeout = (float)0.7;
	private float jumpRepeatTime = (float)0.05;
	private float rollingTime = (float)0.0;
	private float heavyLandingTimeout = (float)0.75;
	private float heavyLandingTime = (float)0.0;
	
	private float previousVerticalSpeed = (float)0.0;
	
	//private var lastWallSlideObject = "";
	
	// Are we jumping? (Initiated with jump button and not grounded yet)
	public bool jumping = false;
	public bool doubleJumping = false;
	public bool jumpingReachedApex = false;
	public bool wallSliding = false;
	public bool wallContact = false;
	public bool hangContact = false;
	public int numClimbContacts = 0;
	public int numHangContacts = 0;
	public bool wasWallSliding = false;
	public bool isFreeFalling = false;
	public bool climbing = false;
	public bool climbContact = false;
	public bool hanging = false;
	public bool hangingContact = false;
	public bool decending = false;
	public bool heavyLanding = false;
	public bool rolling = false;
	public bool bouncing = false;
	public bool aim = false;
	
	// Is the user pressing any keys?
	public bool isMoving = false;
	// Last time we performed a jump
	public float lastJumpTime = (float)-1.0;
	// The height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
	public float lastJumpStartHeight = (float)0.0;
	
	public bool isControllable = true;
	
	public Transform upDownAim;
	public Transform hand;
	
	private void Start()
	{
		_Player = GetComponent<CharacterClass>();
	}
	
	public float CalculateJumpVerticalSpeed (float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * targetJumpHeight * _Player.gravity);
	}

	public void UpdateSmoothedMovementDirection ()
	{
		Transform cameraTransform = Camera.main.transform;
		
		// Forward vector relative to the camera along the x-z plane	
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = (float)0.0;
		forward = forward.normalized;
	
		// Right vector relative to the camera
		// Always orthogonal to the forward vector
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
	
		float v = Input.GetAxisRaw("Vertical");
		float h = Input.GetAxisRaw("Horizontal");
		
		isMoving = Mathf.Abs (h) > 0.05f || Mathf.Abs (v) > 0.05f;
		
		Vector3 targetDirection = transform.forward;
		// Target direction relative to the camera
		if (!climbing)
			targetDirection = h * right + v * forward;
		
		// Grounded controls
		if ((IsGrounded() || hanging) && !climbing)
		{
	
			// We store speed and direction seperately,
			// so that when the character stands still we still have a valid forward direction
			// moveDirection is always normalized, and we only update it if there is user input.
			if (targetDirection != Vector3.zero)
			{
				// Smoothly turn towards the target direction
				_Player.moveDirection = Vector3.RotateTowards(_Player.moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
				_Player.moveDirection = _Player.moveDirection.normalized;
				
			}
			
			// Smooth the speed based on the current target direction
			float curSmooth = speedSmoothing * Time.deltaTime;
			
			// Choose target speed
			//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
			float targetSpeed = Mathf.Min(targetDirection.magnitude, (float)1.0);
		
			if (!isMoving)
			{
				if (hanging) {
					_Player.SetCurrentState("hang_idle");
				}
				else if(climbing)
				{
					_Player.SetCurrentState("climb_idle");	
				}
				else
				{
					_Player.SetCurrentState("idle");
				}
			}
			else{
				// Pick speed modifier
				if (Input.GetKey (KeyCode.LeftShift))
				{
					targetSpeed *= runSpeed;
					_Player.SetCurrentState("run");
				}
				else
				{
					targetSpeed *= walkSpeed;
					_Player.SetCurrentState("walk");
				}			
				if (hanging) {
					_Player.SetCurrentState("hang_move");
				}
			}
			
			_Player.moveSpeed = Mathf.Lerp(_Player.moveSpeed, targetSpeed, curSmooth);
		}
		// In air controls
		else
		{
			if (targetDirection != Vector3.zero)
			{
				if (wasWallSliding) 
				{
					if ((Time.time - lastJumpTime) <= timeAfterJumpLimitRotate) {
						var temp = (1.0 / timeAfterJumpLimitRotate) * (Time.time - lastJumpTime);
						_Player.moveDirection += Vector3.RotateTowards(_Player.moveDirection, targetDirection, (inAirRotateSpeed*(float)temp) * Mathf.Deg2Rad * Time.deltaTime, 1000);
					}
					else
						wasWallSliding = false;
				}
				else
					_Player.moveDirection += Vector3.RotateTowards(_Player.moveDirection, targetDirection, inAirRotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
					
				_Player.moveDirection = _Player.moveDirection.normalized;
			}
	
			if (isMoving) {
				if (doubleJumping)
					_Player.inAirVelocity += targetDirection.normalized * Time.deltaTime * jumpAcceleration;
				else
					_Player.inAirVelocity += targetDirection.normalized * Time.deltaTime * doubleJumpAcceleration;
			}
		}	
	}
	
	public void ApplyJumping ()
	{
		// Prevent jumping too fast after each other
		if ((lastJumpTime + jumpRepeatTime > Time.time) && !doubleJumping || rolling)
			return;
		
		if (IsGrounded() || !doubleJumping || wallSliding){
			// Jump
			// - Only when pressing the button down
			// - With a timeout so you can press the button slightly before landing	
			if (!jumping || !doubleJumping){
				if (!doubleJumping)
					_Player.verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
				else
					_Player.verticalSpeed = CalculateJumpVerticalSpeed (doubleJumpHeight);
				DidJump();
				
				if(wallSliding || climbing){
					_Player.inAirVelocity = Vector3.zero;
					jumpingReachedApex = false;
					wasWallSliding = true;
					_Player.moveDirection = _Player.wallFacing;
					_Player.moveDirection = _Player.moveDirection.normalized;
					_Player.moveSpeed = (float)8.0;
					wallSliding = false;
	        		wallContact = false;
	        		climbing = false;
				}
			}
		}
	}
	
	// Handle the jump/double-jump actions
	private void DidJump ()
	{
		if(jumping) {
			doubleJumping = true;
			_Player.SetCurrentState("double_jump");
		}
		else {
			jumping = true;
			_Player.SetCurrentState("jump");
		}
		
		jumpingReachedApex = false;
		lastJumpTime = Time.time;
		lastJumpStartHeight = transform.position.y;
	}
	
	public void ApplyGravity ()
	{
		if (isControllable)	// don't move player at all if not controllable.
		{
			float temp;
			
			// When we reach the apex of the jump we send out a message
			if ((jumping || doubleJumping) && !jumpingReachedApex && _Player.verticalSpeed <= 0.0 && !climbing)
			{
				jumpingReachedApex = true;
				_Player.SetCurrentState("jump_after_apex");
			}
		
			if (IsGrounded ()) {
				wallSliding = false;
				wallContact = false;
				hangContact = false;
				jumpingReachedApex = false;
				if (!climbing && !bouncing)
					_Player.verticalSpeed = (float)0.0;
				bouncing = false;	
				
				if ((previousVerticalSpeed < -10.0) && (_Player.moveSpeed >= runSpeed/1.5)) {
					rolling = true;
					rollingTime = Time.time;
					_Player.SetCurrentState("roll");
				}
				else if (previousVerticalSpeed <= -25.0) {
					heavyLanding = true;
					heavyLandingTime = Time.time;
					_Player.SetCurrentState("heavy_land");
				}
			}
			else {
				if (wallSliding) {
					if (_Player.collisionFlags == 0) {
						wallSliding = false;
						wallContact = false;
						hangContact = false;
						_Player.verticalSpeed -= _Player.gravity * Time.deltaTime;
						_Player.SetCurrentState("jump_after_apex");
						temp = _Player.moveDirection.normalized.magnitude;
						_Player.moveDirection = DirectionOnWall();
						_Player.moveSpeed *= (_Player.moveDirection.magnitude/temp);
					}
					else {
						if (_Player.verticalSpeed > -0.3)
							_Player.verticalSpeed = -0.3f;
						_Player.SetCurrentState("wall_slide");
						_Player.verticalSpeed -= wallSlidingGravity * Time.deltaTime;
					}
				}
				else if (climbing) {
					if (_Player.collisionFlags == 0) {
						climbing = false;
						transform.position.Set(transform.position.x + (_Player.wallFacing.x * (float)0.1), transform.position.y, transform.position.z + (_Player.wallFacing.z * (float)0.1));
					}
					else {
						if (_Player.verticalSpeed < -0.5)
							_Player.verticalSpeed += _Player.gravity * Time.deltaTime;
						else if (_Player.verticalSpeed > 0.5)
							_Player.verticalSpeed -= _Player.gravity * Time.deltaTime;
						if (Mathf.Abs(_Player.verticalSpeed) <= 0.5)
						{
							_Player.verticalSpeed = (float)0.0;
								_Player.SetCurrentState("climb_idle");
						}
					}
				}
				else if (hanging) {
					/*if (_Player.collisionFlags == 0) {
						hanging = false;
						hangContact = false;
					}
					else*/
						_Player.verticalSpeed = (float)0.1;
				}
				else if (!IsGrounded()) {
					if (_Player.verticalSpeed <= -1.0)
						_Player.SetCurrentState("jump_after_apex");
					if (_Player.verticalSpeed > -15.0)
						_Player.verticalSpeed -= _Player.gravity * Time.deltaTime;
					if (_Player.verticalSpeed <= -15.0){
						_Player.verticalSpeed = (float)-15.0;
						_Player.SetCurrentState("free_fall");
					}
				}
			}
			previousVerticalSpeed = _Player.verticalSpeed;
		}
	}
	
	public Vector3 DirectionOnWall()
	{
		Vector3 returnAngle = Vector3.zero;
		if (_Player.wallFacing.x <= -0.1)
			returnAngle.z = (float)-1.0 + Mathf.Abs(_Player.wallFacing.z);
		else if (_Player.wallFacing.x >= 0.1)
			returnAngle.z = (float)1.0 - Mathf.Abs(_Player.wallFacing.z);
		
		if (_Player.wallFacing.z <= -0.1)
			returnAngle.x = (float)1.0 - Mathf.Abs(_Player.wallFacing.x);
		else if (_Player.wallFacing.z >= 0.1)
			returnAngle.x = (float)-1.0 + Mathf.Abs(_Player.wallFacing.x);
		
		returnAngle = returnAngle.normalized;
		
		return returnAngle;
	}
	
	// Handle secondary input functions
	public void InputHandler() {
		if (Input.GetButtonDown ("Jump"))	// If jump button pressed
			ApplyJumping ();		// Apply jumping logic
		
		else if (Input.GetButton ("Interact"))		// If the interact butten is pressed,
		{
			if (climbing) {								// and player is climbing, release from wall and apply slight pushoff (to prevent instantly regrabbing the wall)
				_Player.moveDirection = _Player.wallFacing;
				_Player.moveSpeed = (float)1.0;
				climbing = false;
				jumping = true;
				numClimbContacts = 0;
			}
			else if (hanging)	{						// and player is hanging, release from ceiling
				hanging = false;
				hangingContact = false;
				numHangContacts = 0;
				_Player.verticalSpeed = -1;
			}
		}
			
		/*if (Input.GetButtonDown("Fire2")){
			aim = !aim;
			if(aim)
				_Player.currentState = "aim";
		}*/
		
		// If the player is climbing
		/*if (climbing) {
				_Player.currentState = "climb_idle";
			if (Input.GetButton ("Vertical")) {			// If one of the up/down buttons is pressed
				_Player.currentState = "climb_vertical";
				_Player.verticalSpeed = (float)2.0 * Input.GetAxis("Vertical");
				if (_Player.verticalSpeed < 0.0)				// If moving down
					decending = true;
				else
					decending = false;
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))		// Climb faster
					_Player.verticalSpeed *= 2;
			}
			// If one of the up/down buttons is released
			if (Input.GetButtonUp ("Vertical"))
			{
				_Player.currentState = "climb_idle";
				_Player.inAirVelocity = Vector3.zero;
				_Player.moveDirection = -_Player.wallFacing;
				_Player.moveSpeed = (float)0.1;
			}
			// If one of the left/right buttons is pressed
			if (Input.GetButton ("Horizontal"))
			{
				_Player.currentState = "climb_horizontal";
				_Player.moveSpeed = (float)2.0;
				_Player.moveDirection += new Vector3(_Player.wallRight.x * Input.GetAxis("Horizontal"), _Player.wallRight.y * 
								 Input.GetAxis("Horizontal"), _Player.wallRight.z * Input.GetAxis("Horizontal"));
				_Player.moveDirection = _Player.moveDirection.normalized;
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
					_Player.moveSpeed *= 2;
			}
			// If one of the left/right buttons is released
			if (Input.GetButtonUp ("Horizontal"))
			{
				_Player.currentState = "climb_idle";
				_Player.inAirVelocity = Vector3.zero;
				_Player.moveDirection = -_Player.wallFacing;
				_Player.moveSpeed = (float)0.1;
			}
		}*/
	}
	
	public void MovementHandler() {
		// If rolling (and time since rolling began is less than rolling timeout)
		if (rolling && ((Time.time - rollingTime) < rollingTimeout)) {
			_Player.SetCurrentState("roll");
			if (_Player.moveSpeed < 5.0)							// If player speed is below minimum for roll, make minimum roll speed
				_Player.moveSpeed = (float)5.0;
		}
		else {
			rolling = false;
		}
		
		// Handle heavy landing (big fall w/o enough forward movement to be roll)
		if (heavyLanding && ((Time.time - heavyLandingTime) < heavyLandingTimeout)) {
			_Player.SetCurrentState("heavy_land");
			_Player.verticalSpeed = (float)0.0;
			previousVerticalSpeed = (float)0.0;
			rolling = false;
		}
		else
			heavyLanding = false;
	
		// Set rotation to the move direction
		if (IsGrounded())
		{
			if (_Player.moveDirection == Vector3.zero) 
				_Player.moveDirection = _Player.wallFacing;
			
			/*if (aim) {
				transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
			}
			else*/
				transform.rotation = Quaternion.LookRotation(_Player.moveDirection);
			_Player.inAirVelocity = Vector3.zero;
			if(jumping && !bouncing){
				jumping = false;
				doubleJumping = false;
			}
						
			if (climbing && decending) {		// If just reached ground from climbing downward
				climbing = false;
				decending = false;
				transform.position.Set(transform.position.x + (_Player.wallFacing.x * (float)0.1), transform.position.y, transform.position.z + (_Player.wallFacing.z * (float)0.1));
			}
				
		}	
		else
		{
			if (wallSliding)
				transform.rotation = Quaternion.LookRotation(_Player.wallFacing);
			else {
				/*if (aim) {
					transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
				}
				else*/
					transform.rotation = Quaternion.LookRotation(_Player.moveDirection);
			}
		}
	}
	
	public bool IsGrounded () {
		return (_Player.collisionFlags & CollisionFlags.CollidedBelow) != 0;
	}
	
	public void ChangePosition(Vector3 position) {
		transform.position += position;
	}
}

