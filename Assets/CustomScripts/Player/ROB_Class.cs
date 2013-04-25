using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class ROB
	{
		private float walkSpeed = (float)4.0;	// The speed when walking
		private float trotSpeed = (float)5.0;	// After trotAfterSeconds of walking we trot with trotSpeed
		private float runSpeed = (float)8.0;	// When pressing Shift button we start running
		
		public bool canJump = true;
		private float jumpAcceleration = (float)2.0;			// Acceleration from jumping
		private float doubleJumpAcceleration = (float)1.0;		// from double jumping
		private float jumpHeight = (float)1.5;					// How high we jump when pressing jump and letting go immediately
		private float doubleJumpHeight = (float)0.75;			// How high we jump when we double jump
		
		private float gravity = (float)17.0;				// The gravity for the character
		private float wallSlidingGravity = (float)3.2;		// Wall sliding reduces gravity's effect on character
		
		private float speedSmoothing = (float)10.0;
		private float rotateSpeed = (float)900.0;
		private float inAirRotateSpeed = (float)450.0;
		
		private float trotAfterSeconds = (float)1.0;
		private float timeAfterJumpLimitRotate = (float)0.9;
		private float rollingTimeout = (float)0.7;
		private float jumpRepeatTime = (float)0.05;
		private float rollingTime = (float)0.0;
		private float heavyLandingTimeout = (float)0.75;
		private float heavyLandingTime = (float)0.0;
		
		public Vector3 moveDirection = Vector3.zero;	// The current move direction in x-z
		public float verticalSpeed = (float)0.0;				// The current vertical speed
		private float previousVerticalSpeed = (float)0.0;
		public float moveSpeed = (float)0.0;					// The current x-z move speed
		
		// The last collision flags returned from controller.Move
		public CollisionFlags collisionFlags; 
		
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
		// When did the user start walking (Used for going into trot after a while)
		public float walkTimeStart = (float)0.0;
		// Last time we performed a jump
		public float lastJumpTime = (float)-1.0;
		// The height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
		public float lastJumpStartHeight = (float)0.0;
		
		// Direction to face when wall sliding and jumping from wall
		public Vector3 wallFacing = Vector3.zero;
		public Vector3 wallRight = Vector3.zero;
		
		public Vector3 inAirVelocity = Vector3.zero;
		
		public enum customCharacterState {
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
		
		public customCharacterState _characterState;
		public bool isControllable = true;
		public Transform transform;
		
		public Transform upDownAim;
		public Transform hand;
		
		public ROB(Transform trans){
			this.transform = trans;
		}
		
		public float CalculateJumpVerticalSpeed (float targetJumpHeight)
		{
			// From the jump height and gravity we deduce the upwards speed 
			// for the character to reach at the apex.
			return Mathf.Sqrt(2 * targetJumpHeight * gravity);
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
			
			isMoving = Mathf.Abs (h) > 0.1 || Mathf.Abs (v) > 0.1;
			
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
					moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
					moveDirection = moveDirection.normalized;
					
				}
				
				// Smooth the speed based on the current target direction
				float curSmooth = speedSmoothing * Time.deltaTime;
				
				// Choose target speed
				//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
				float targetSpeed = Mathf.Min(targetDirection.magnitude, (float)1.0);
			
				if (!bouncing)
					_characterState = customCharacterState.Idle;
				
				// Pick speed modifier
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift) && !bouncing)
				{
					targetSpeed *= runSpeed;
					_characterState = customCharacterState.Running;
				}
				else if (Time.time - trotAfterSeconds > walkTimeStart && !bouncing)
				{
					targetSpeed *= trotSpeed;
					_characterState = customCharacterState.Trotting;
				}
				else if (!bouncing){
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
							moveDirection += Vector3.RotateTowards(moveDirection, targetDirection, (inAirRotateSpeed*(float)temp) * Mathf.Deg2Rad * Time.deltaTime, 1000);
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
		
		public void ApplyJumping ()
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
						moveSpeed = (float)8.0;
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
		
		public void ApplyGravity ()
		{
			if (isControllable)	// don't move player at all if not controllable.
			{
				float temp;
				
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
					if (!climbing && !bouncing)
						verticalSpeed = (float)0.0;
					bouncing = false;	
					
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
							moveDirection = DirectionOnWall();
							moveSpeed *= (moveDirection.magnitude/temp);
						}
						else {
							if (verticalSpeed > -0.3)
								verticalSpeed = -0.3f;
							_characterState = customCharacterState.Wall_Sliding;
							verticalSpeed -= wallSlidingGravity * Time.deltaTime;
						}
					}
					else if (climbing) {
						if (collisionFlags == 0) {
							climbing = false;
							transform.position.Set(transform.position.x + (wallFacing.x * (float)0.1), transform.position.y, transform.position.z + (wallFacing.z * (float)0.1));
						}
						else {
							if (verticalSpeed < -0.5)
								verticalSpeed += gravity * Time.deltaTime;
							else if (verticalSpeed > 0.5)
								verticalSpeed -= gravity * Time.deltaTime;
							if (Mathf.Abs(verticalSpeed) <= 0.5)
								verticalSpeed = (float)0.0;
						}
					}
					else if (hanging) {
						if (collisionFlags == 0) {
							hanging = false;
							hangContact = false;
						}
						else
							verticalSpeed = (float)0.1;
					}
					else if (!IsGrounded()) {
						if (verticalSpeed <= -1.0)
							_characterState = customCharacterState.Jumping_After_Apex;
						if (verticalSpeed > -15.0)
							verticalSpeed -= gravity * Time.deltaTime;
						if (verticalSpeed <= -15.0){
							verticalSpeed = (float)-15.0;
							_characterState = customCharacterState.Free_Falling;
						}
					}
				}
				previousVerticalSpeed = verticalSpeed;
			}
		}
		
		public Vector3 DirectionOnWall()
		{
			Vector3 returnAngle = Vector3.zero;
			if (wallFacing.x <= -0.1)
				returnAngle.z = (float)-1.0 + Mathf.Abs(wallFacing.z);
			else if (wallFacing.x >= 0.1)
				returnAngle.z = (float)1.0 - Mathf.Abs(wallFacing.z);
			
			if (wallFacing.z <= -0.1)
				returnAngle.x = (float)1.0 - Mathf.Abs(wallFacing.x);
			else if (wallFacing.z >= 0.1)
				returnAngle.x = (float)-1.0 + Mathf.Abs(wallFacing.x);
			
			returnAngle = returnAngle.normalized;
			
			return returnAngle;
		}
		
		// Handle secondary input functions
		public void InputHandler() {
			/*if (Input.GetButtonDown("Fire2")){
				if (aim) {
					moveDirection = transform.forward;
				}
				else {
					climbing = false;
					climbContact = false;
					hanging = false;
					hangContact = false;
					wallSliding = false;
				}
				aim = !aim;
			}*/
				
			if (Input.GetButtonDown ("Jump"))	// If jump button pressed
				ApplyJumping ();		// Apply jumping logic
			
			else if (Input.GetButton ("Interact"))		// If the interact butten is pressed,
			{
				if (climbing) {								// and player is climbing, release from wall and apply slight pushoff (to prevent instantly regrabbing the wall)
					moveDirection = wallFacing;
					moveSpeed = (float)1.0;
					climbing = false;
					jumping = true;
					numClimbContacts = 0;
				}
				else if (hanging)	{						// and player is hanging, release from ceiling
					hanging = false;
					hangingContact = false;
					numHangContacts = 0;
					verticalSpeed = -1;
				}
			}
			
			// If the player is climbing
			if (climbing) {
					_characterState = customCharacterState.Climbing_Idle;
				if (Input.GetButton ("Vertical")) {			// If one of the up/down buttons is pressed
					_characterState = customCharacterState.Climbing_Vertical;
					verticalSpeed = (float)2.0 * Input.GetAxis("Vertical");
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
					inAirVelocity = Vector3.zero;
					moveDirection = -wallFacing;
					moveSpeed = (float)0.1;
				}
				// If one of the left/right buttons is pressed
				if (Input.GetButton ("Horizontal"))
				{
					_characterState = customCharacterState.Climbing_Horizontal;
					moveSpeed = (float)2.0;
					moveDirection += new Vector3(wallRight.x * Input.GetAxis("Horizontal"), wallRight.y * 
									 Input.GetAxis("Horizontal"), wallRight.z * Input.GetAxis("Horizontal"));
					moveDirection = moveDirection.normalized;
					if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
						moveSpeed *= 2;
				}
				// If one of the left/right buttons is released
				if (Input.GetButtonUp ("Horizontal"))
				{
					inAirVelocity = Vector3.zero;
					moveDirection = -wallFacing;
					moveSpeed = (float)0.1;
				}
			}
		}
		
		public void MovementHandler() {
			// If rolling (and time since rolling began is less than rolling timeout)
			if (rolling && ((Time.time - rollingTime) < rollingTimeout)) {
				_characterState = customCharacterState.Rolling;
				if (moveSpeed < 5.0)							// If player speed is below minimum for roll, make minimum roll speed
					moveSpeed = (float)5.0;
			}
			else {
				rolling = false;
			}
			
			// Handle heavy landing (big fall w/o enough forward movement to be roll)
			if (heavyLanding && ((Time.time - heavyLandingTime) < heavyLandingTimeout)) {
				_characterState = customCharacterState.Heavy_Landing;
				verticalSpeed = (float)0.0;
				previousVerticalSpeed = (float)0.0;
				rolling = false;
			}
			else
				heavyLanding = false;
		
			// Set rotation to the move direction
			if (IsGrounded())
			{
				if (moveDirection == Vector3.zero) 
					moveDirection = wallFacing;
				
				if (aim) {
					transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
				}
				else
					transform.rotation = Quaternion.LookRotation(moveDirection);
				inAirVelocity = Vector3.zero;
				if(IsJumping() && !bouncing){
					jumping = false;
					doubleJumping = false;
				}
							
				if (climbing && decending) {		// If just reached ground from climbing downward
					climbing = false;
					decending = false;
					transform.position.Set(transform.position.x + (wallFacing.x * (float)0.1), transform.position.y, transform.position.z + (wallFacing.z * (float)0.1));
				}
					
			}	
			else
			{
				if (wallSliding)
					transform.rotation = Quaternion.LookRotation(wallFacing);
				else {
					if (aim) {
						transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
					}
					else
						transform.rotation = Quaternion.LookRotation(moveDirection);
				}
			}
		}
	
	
		public bool IsJumping () {
			return jumping;
		}
		
		public bool IsDoubleJumping () {
			return doubleJumping;
		}
		
		public bool IsGrounded () {
			return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
		}
		
		public void ChangePosition(Vector3 position) {
			transform.position += position;
		}
	}
}

