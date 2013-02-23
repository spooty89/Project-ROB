@script RequireComponent(CharacterController)

public class ROB{
	private var walkSpeed : float = 3.0;	// The speed when walking
	private var trotSpeed : float = 5.0;	// After trotAfterSeconds of walking we trot with trotSpeed
	private var runSpeed : float = 8.0;		// When pressing Shift button we start running
	
	private var jumpAcceleration : float = 2.0;			// Acceleration from jumping
	private var doubleJumpAcceleration : float = 1.0;	// from double jumping
	
	private var jumpHeight : float = 1.5;			// How high we jump when pressing jump and letting go immediately
	private var doubleJumpHeight : float = 0.75;	// How high we jump when we double jump
	
	private var gravity : float = 17.0;				// The gravity for the character
	private var wallSlidingGravity : float = 3.2;	// Wall sliding reduces gravity's effect on character
	private var speedSmoothing : float = 10.0;
	private var rotateSpeed : float = 900.0;
	private var inAirRotateSpeed : float = 450.0;
	private var trotAfterSeconds : float = 1.0;
	private var timeAfterJumpLimitRotate : float = 0.9;
	public var canJump = true;
	private var rollingTimeout : float = 0.7;
	
	private var jumpRepeatTime : float = 0.05;
	private var jumpTimeout : float = 0.15;
	private var heavyLandingTimeout : float = 0.75;
	private var heavyLandingTime : float = 0.0;
	private var rollingTime : float = 0.0;
	
	// The current move direction in x-z
	public var moveDirection : Vector3 = Vector3.zero;
	// The current vertical speed
	public var verticalSpeed : float = 0.0;
	private var previousVerticalSpeed : float = 0.0;
	// The current x-z move speed
	public var moveSpeed : float = 0.0;
	
	// The last collision flags returned from controller.Move
	public var collisionFlags : CollisionFlags; 
	
	//private var lastWallSlideObject = "";
	
	// Are we jumping? (Initiated with jump button and not grounded yet)
	public var jumping = false;
	public var doubleJumping = false;
	public var jumpingReachedApex = false;
	public var wallSliding = false;
	public var wallContact = false;
	public var hangContact = false;
	public var numClimbContacts : int = 0;
	public var numHangContacts : int = 0;
	public var wasWallSliding = false;
	public var isFreeFalling = false;
	public var climbing = false;
	public var climbContact = false;
	public var hanging = false;
	public var hangingContact = false;
	public var decending = false;
	public var heavyLanding = false;
	public var rolling = false;
	
	// Is the user pressing any keys?
	public var isMoving = false;
	// When did the user start walking (Used for going into trot after a while)
	public var walkTimeStart : float = 0.0;
	// Last time we performed a jump
	public var lastJumpTime : float = -1.0;
	// The height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
	public var lastJumpStartHeight : float = 0.0;
	
	// Direction to face when wall sliding and jumping from wall
	public var wallFacing : Vector3 = Vector3.zero;
	public var wallRight : Vector3 = Vector3.zero;
	
	public var inAirVelocity : Vector3 = Vector3.zero;
	
	public var hookShotHit : RaycastHit;
	
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
	
	public var _characterState : customCharacterState;
	public var isControllable = true;
	public var transform : Transform;
	
	function ROB(trans : Transform){
		this.transform = trans;
	}
	
	function CalculateJumpVerticalSpeed (targetJumpHeight : float) : float
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * targetJumpHeight * gravity);
	}

	function UpdateSmoothedMovementDirection ()
	{
		var cameraTransform : Transform = Camera.main.transform;
		
		// Forward vector relative to the camera along the x-z plane	
		var forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = 0.0;
		forward = forward.normalized;
	
		// Right vector relative to the camera
		// Always orthogonal to the forward vector
		var right = Vector3(forward.z, 0, -forward.x);
	
		var v = Input.GetAxisRaw("Vertical");
		var h = Input.GetAxisRaw("Horizontal");
		
		isMoving = Mathf.Abs (h) > 0.1 || Mathf.Abs (v) > 0.1;
		
		var targetDirection = transform.forward;
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
				moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
				moveDirection = moveDirection.normalized;
				
			}
			
			// Smooth the speed based on the current target direction
			var curSmooth = speedSmoothing * Time.deltaTime;
			
			// Choose target speed
			//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
			var targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0);
		
			_characterState = customCharacterState.Idle;
			
			// Pick speed modifier
			if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
			{
				targetSpeed *= runSpeed;
				_characterState = customCharacterState.Running;
			}
			else if (Time.time - trotAfterSeconds > walkTimeStart)
			{
				targetSpeed *= trotSpeed;
				_characterState = customCharacterState.Trotting;
			}
			else
			{
				targetSpeed *= walkSpeed;
				_characterState = customCharacterState.Walking;
			}
			
			if (hanging) {
				if (targetSpeed > 0.0) {
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
		else
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
			if (canJump && (!IsJumping() || !IsDoubleJumping())){
				if (!IsDoubleJumping())
					verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
				else
					verticalSpeed = CalculateJumpVerticalSpeed (doubleJumpHeight);
				DidJump();
				
				if(wallSliding || climbing){
					inAirVelocity = Vector3.zero;
					jumpingReachedApex = false;
					wasWallSliding = true;
					moveDirection = wallFacing;
					moveDirection = moveDirection.normalized;
					moveSpeed = 8.0;
					wallSliding = false;
	        		wallContact = false;
	        		climbing = false;
				}
			}
		}
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
	}
	
	function ApplyGravity ()
	{
		if (isControllable)	// don't move player at all if not controllable.
		{
			var temp;
			
			// When we reach the apex of the jump we send out a message
			if ((IsJumping() || IsDoubleJumping()) && !jumpingReachedApex && verticalSpeed <= 0.0 && !climbing)
			{
				jumpingReachedApex = true;
				_characterState = customCharacterState.Jumping_After_Apex;
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
					rollingTime = Time.time;
					_characterState = customCharacterState.Rolling;
				}
				else if (previousVerticalSpeed <= -25.0) {
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
						climbing = false;
						transform.position.x += (wallFacing.x * 0.1);
						transform.position.z += (wallFacing.z * 0.1);
					}
					else {
						if (verticalSpeed < -0.5)
							verticalSpeed += gravity * Time.deltaTime;
						else if (verticalSpeed > 0.5)
							verticalSpeed -= gravity * Time.deltaTime;
						if (Mathf.Abs(verticalSpeed) <= 0.5)
							verticalSpeed = 0.0;
					}
				}
				else if (hanging) {
					if (collisionFlags == 0) {
						hanging = false;
						hangContact = false;
					}
					else
						verticalSpeed = 1.0;
				}
				else if (!IsGrounded()) {
					if (verticalSpeed <= -1.0)
						_characterState = customCharacterState.Jumping_After_Apex;
					if (verticalSpeed > -15.0)
						verticalSpeed -= gravity * Time.deltaTime;
					if (verticalSpeed <= -15.0){
						verticalSpeed = -15.0;
						_characterState = customCharacterState.Free_Falling;
					}
				}
			}
			previousVerticalSpeed = verticalSpeed;
		}
	}
	
	function DirectionOnWall(dir) : Vector3
	{
		var returnAngle : Vector3 = Vector3.zero;
		if (wallFacing.x <= -0.1)
			returnAngle.z = -1.0 + Mathf.Abs(wallFacing.z);
		else if (wallFacing.x >= 0.1)
			returnAngle.z = 1.0 - Mathf.Abs(wallFacing.z);
		
		if (wallFacing.z <= -0.1)
			returnAngle.x = 1.0 - Mathf.Abs(wallFacing.x);
		else if (wallFacing.z >= 0.1)
			returnAngle.x = -1.0 + Mathf.Abs(wallFacing.x);
		
		returnAngle = returnAngle.normalized;
		
		return returnAngle;
	}
	
	// Handle secondary input functions
	function InputHandler() {
		if (Input.GetButtonDown ("Jump"))	// If jump button pressed
			ApplyJumping ();		// Apply jumping logic
		
		else if (Input.GetButton ("Interact"))		// If the interact butten is pressed,
		{
			if (climbing) {								// and player is climbing, release from wall and apply slight pushoff (to prevent instantly regrabbing the wall)
				moveDirection = wallFacing;
				moveSpeed = 1.0;
				climbing = false;
				jumping = true;
			}
			else if (hanging)							// and player is hanging, release from ceiling
				hanging = false;
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
	}
	
	function MovementHandler() {
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
			verticalSpeed = 0.0;
			previousVerticalSpeed = 0.0;
			rolling = false;
		}
		else
			heavyLanding = false;
	
		// Set rotation to the move direction
		if (IsGrounded())
		{
			if (moveDirection == Vector3.zero) 
				moveDirection = wallFacing;
				
			transform.rotation = Quaternion.LookRotation(moveDirection);
			inAirVelocity = Vector3.zero;
			jumping = false;
			doubleJumping = false;
			
			if (climbing && decending) {		// If just reached ground from climbing downward
				climbing = false;
				decending = false;
				transform.position.x += (wallFacing.x * 0.1);		// Move the player away
				transform.position.z += (wallFacing.z * 0.1);		// from the climb surface
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


	function IsJumping () {
		return jumping;
	}
	
	function IsDoubleJumping () {
		return doubleJumping;
	}
	
	function IsGrounded () {
		return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
	}
	
	function ChangePosition(position:Vector3) {
		transform.position += position;
	}
}
