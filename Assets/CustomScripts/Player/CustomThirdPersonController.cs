using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class CustomThirdPersonController : MonoBehaviour
	{
		public ROB rob;
		
		// Animations to be played
		public AnimationClip idleAnimation;
		public AnimationClip walkAnimation;
		public AnimationClip runAnimation;
		public AnimationClip jumpPoseAnimation;
		public AnimationClip jumpPoseAfterApexAnimation;
		public AnimationClip doubleJumpAnimation;
		public AnimationClip wallSlideAnimation;
		public AnimationClip rollAnimation;
		public AnimationClip climbIdlePoseAnimation;
		public AnimationClip climbVerticalAnimation;
		public AnimationClip climbHorizontalAnimation;
		public AnimationClip hangIdlePoseAnimation;
		public AnimationClip hangMoveAnimation;
		public AnimationClip freefallPoseAnimation;
		public AnimationClip heavyLandPoseAnimation;
		public AnimationClip aimAnimation;
		public AnimationClip shootAnimation;
		
		// Speed at which each animation will be played
		public float idleAnimationSpeed = (float)1.0;
		public float walkMaxAnimationSpeed = (float)1.0;
		public float trotMaxAnimationSpeed = (float)0.7;
		public float runMaxAnimationSpeed = (float)1.4;
		public float jumpAnimationSpeed = (float)1.0;
		public float landAnimationSpeed = (float)1.0;
		public float jumpAfterApexAnimationSpeed = (float)1.0;
		public float doubleJumpAnimationSpeed = (float)1.0;
		public float wallSlideAnimationSpeed = (float)1.0;
		public float rollAnimationSpeed = (float)0.8;
		public float climbIdleAnimationSpeed = (float)1.0;
		public float climbVerticalAnimationSpeed = (float)1.0;
		public float climbHorizontalAnimationSpeed = (float)1.0;
		public float hangIdleAnimationSpeed = (float)1.0;
		public float hangMoveAnimationSpeed = (float)1.5;
		public float freefallAnimationSpeed = (float)1.0;
		public float heavyLandAnimationSpeed = (float)1.0;
		public float aimAnimationSpeed = (float)1.0;
		public float shootAnimationSpeed = (float)1.0;
		private Animation _animation;
		private GameObject contactObject;
		private CharacterController controller;
		
		private void Awake ()
		{
			rob = new ROB(transform);
			_animation = GetComponent<Animation>();		// Get the character's animations
			controller = GetComponent<CharacterController>();
			
			// If there are no animations for the character
			if(!_animation)
				Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");
			
			// Check for all animations
			if(!idleAnimation || !walkAnimation || !runAnimation || (!jumpPoseAnimation && rob.canJump) || 
				!jumpPoseAfterApexAnimation || (!doubleJumpAnimation && rob.canJump) || !wallSlideAnimation ||
				!rollAnimation || !climbIdlePoseAnimation || !climbVerticalAnimation || !climbHorizontalAnimation ||
				!hangIdlePoseAnimation || !hangMoveAnimation || !freefallPoseAnimation || !heavyLandPoseAnimation) {
				_animation = null;
				Debug.Log("Animation missing. Turning off animations.");
			}

			rob.moveDirection = rob.transform.forward;	// Initialize player's move direction to the direction rob is initially facing
		}
		
		// Update the current state of the game
		private void Update() {
			if (!rob.isControllable) {		// Kill all inputs if not controllable (here by default, may be useful for in-game cutscenes?)
				Input.ResetInputAxes();
			}
			if (rob.collisionFlags == 0) {		// If rob is no longer in contact with 
				try{				// Try-catch useful in beginning, when no object has been contacted
					contactObject.SendMessage("noContact", SendMessageOptions.DontRequireReceiver);
				}catch (Exception E){};
			}
			
			if (Input.GetButtonDown("Fire2")){
				rob.aim = !rob.aim;
			}
			
			rob.UpdateSmoothedMovementDirection();	// Smooth the player movement
			rob.ApplyGravity ();	// Apply gravity
			rob.MovementHandler();	// Handle movement
			rob.InputHandler();		// Handle user input (comes after movement, else jumping from ground is irregular)
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
					if(hit.gameObject.CompareTag("Bouncy") && (rob.verticalSpeed <= 0)){    
						rob.bouncing = true;
						rob.collisionFlags=0;
						rob.jumping=true;
						rob.doubleJumping = false;
						rob._characterState = AssemblyCSharp.ROB.customCharacterState.Jumping;	
					    rob.verticalSpeed = (float)20.0;
					}					// If surface of contact is relatively horizontal
					if (rob.hangContact && (hit.normal.y < -0.1) && !rob.aim) {				// If surface is above and player is within hang triggerBox
						if (!rob.hanging) {											// If player isn't already hanging, set necessary variables
							rob.hanging = true;
							rob.wallSliding = false;
							rob.wasWallSliding = false;
							rob.climbing = false;
							rob.jumping = false;
							rob.doubleJumping = false;
							//rob._characterState = AssemblyCSharp.ROB.customCharacterState.Hanging_Idle;
							rob.inAirVelocity = Vector3.zero;
						}
					}
					else {													// Else, if surface is below and/or player is not within hang triggerBox
						rob.hanging = false;
						if (rob.climbing && rob.decending && hit.normal.y > 0.0) {							// If player just reached flat ground after climbing downward
							rob.transform.position = new Vector3(rob.transform.position.x + (rob.wallFacing.x * (float)0.2), rob.transform.position.y, rob.transform.position.z + (rob.wallFacing.z * (float)0.2));
							rob.transform.forward = rob.wallFacing;
							rob.moveDirection = rob.transform.forward;
							Debug.Log("here");
							rob.moveSpeed = (float)0.0;
							rob.climbing = false;
							rob.decending = false;
						}
					}
				}
				else {													// If surface of contact is relatively vertical
					rob.wallContact = true;
					if (!rob.aim) {
						if (rob.climbContact) {										// If player is within climb triggerBox
							rob.wallFacing = hit.normal;
							//rob.wallRight = rob.DirectionOnWall();
							rob.moveDirection = -rob.wallFacing;
							rob.transform.rotation = Quaternion.LookRotation(rob.moveDirection);
							rob.wallRight = rob.transform.right;
							//Debug.Log(rob.wallRight);
							if (!rob.climbing) {										// If player isn't already climbing, set necessary variables
								rob.climbing = true;
								rob.hanging = false;
								//rob._characterState = AssemblyCSharp.ROB.customCharacterState.Climbing_Idle;
								if (rob.IsGrounded())										// If player walks into climb surface (rather than jumping)
									rob.verticalSpeed = (float)1.0;									// need to get them off ground (otherwise, rapid switch between climb and grounded states)
								rob.wallSliding = false;
								rob.wasWallSliding = false;
								rob.moveSpeed = (float)1.0;
								rob.inAirVelocity = Vector3.zero;
								rob.jumping = false;
								rob.doubleJumping = false;
							}
				    	}
				    	else {								// Else, if player is against non-climb wall surface
					    	rob.climbing = false;
							rob.wallFacing = hit.normal;
							if (rob.verticalSpeed < -0.3) {		// If player has reached their jump apex or is simply falling	
								if (!rob.wallSliding){				// If player is not already wallSliding, set necessary variables
									rob.wallSliding = true;
					    			rob._characterState = AssemblyCSharp.ROB.customCharacterState.Wall_Sliding;
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
		    if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
		        rob.climbContact = true;						// Set climb contact to true
		        rob.numClimbContacts += 1;						// Keep track of how many climb boxes player is currently in
			}
		    if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
		        rob.hangContact = true;							// Set hang contact to true
		        rob.numHangContacts += 1;						// Keep track of how many hang boxes player is currently in
			}
			if(other.gameObject.CompareTag("Token")) {
				GetComponent<ROBgui>().tokens++;
				Destroy(other.gameObject);
			}
			if(other.gameObject.CompareTag("Triforce")) {
				rob.transform.forward = other.gameObject.transform.forward;
				rob.transform.position = new Vector3(other.gameObject.transform.position.x, other.gameObject.transform.position.y-.75f, other.gameObject.transform.position.z);
				rob.inAirVelocity = Vector3.zero;
				rob.verticalSpeed = 0.0f;
				rob._characterState = AssemblyCSharp.ROB.customCharacterState.Idle;
				rob.moveSpeed = 0.0f;
				other.gameObject.transform.position = new Vector3(other.gameObject.transform.position.x, other.gameObject.transform.position.y + 1, other.gameObject.transform.position.z);
				other.gameObject.GetComponent<CapsuleCollider>().enabled = false;
				Camera.main.GetComponent<CustomCameraController>().enabled = false;
				Camera.main.transform.position = GameObject.FindGameObjectWithTag("FinalCamera").transform.position;
				Camera.main.transform.LookAt(other.gameObject.transform.position);
				GetComponent<ROBgui>().gameFinished = true;
				rob.isControllable = false;
			}
		}
		
		// If the player exits a triggerBox
		private void OnTriggerExit(Collider other)
		{
		    if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
		    	rob.numClimbContacts -= 1;						// Keep track of how many climb boxes player is currently in
		    	if (rob.numClimbContacts <= 0) {				// If the player is not in any climb boxes
					rob.numClimbContacts = 0;
					rob.climbContact = false;						// Set climb contact to false
					rob.climbing = false;							// Set climbing to false
				}
			}
		    if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
		    	rob.numHangContacts -= 1;						// Keep track of how many climb boxes player is currently in
		    	if (rob.numHangContacts <= 0) {					// If the player has exited all triggerBoxes with "Hang" tags
					rob.numHangContacts = 0;
					rob.hangContact = false;						// Set hang contact to false
					rob.hanging = false;							// Set hanging to false
				}
			}
		}
		
		// Handle animation states
		private void AnimationHandler() {
			Vector3 movement = rob.moveDirection * rob.moveSpeed + new Vector3 (0, rob.verticalSpeed, 0) + rob.inAirVelocity;		// Calculate actual motion
			movement *= Time.deltaTime;			// Base degree of applied movement on time since last frame
			rob.collisionFlags = controller.Move(movement);
			
			// ANIMATION sector
			if(_animation) {
				if (rob.aim) {
					_animation[aimAnimation.name].speed = aimAnimationSpeed;		// Animation speed = jump animation speed
					_animation[aimAnimation.name].wrapMode = WrapMode.Loop;		// Loop the jump animation
					_animation.CrossFade(aimAnimation.name);
				}
				else {
					switch(rob._characterState)
					{
						// Player is jumping; play jumping animation
						case AssemblyCSharp.ROB.customCharacterState.Jumping:
							_animation[jumpPoseAnimation.name].speed = jumpAnimationSpeed;		// Animation speed = jump animation speed
							_animation[jumpPoseAnimation.name].wrapMode = WrapMode.Loop;		// Loop the jump animation
							_animation.CrossFade(jumpPoseAnimation.name);
						break;
						// Player is double jumping; play double jumping animation
						case AssemblyCSharp.ROB.customCharacterState.Double_Jumping:
							_animation[doubleJumpAnimation.name].speed = doubleJumpAnimationSpeed;
							_animation[doubleJumpAnimation.name].wrapMode = WrapMode.ClampForever;
							_animation.CrossFade(doubleJumpAnimation.name);
						break;
						// Player is decending in jump; play jump after apex animation
						case AssemblyCSharp.ROB.customCharacterState.Jumping_After_Apex:
							_animation[jumpPoseAfterApexAnimation.name].speed = jumpAfterApexAnimationSpeed;
							_animation[jumpPoseAfterApexAnimation.name].wrapMode = WrapMode.Loop;
							_animation.CrossFade(jumpPoseAfterApexAnimation.name);	
						break;
						// Player is wallsliding; play wallsliding animation
						case AssemblyCSharp.ROB.customCharacterState.Wall_Sliding:
							_animation[wallSlideAnimation.name].speed = wallSlideAnimationSpeed;
							_animation[wallSlideAnimation.name].wrapMode = WrapMode.Loop;
							_animation.CrossFade(wallSlideAnimation.name);	
						break;
						// Player is idle while climbing; play climbing idle animation
						case AssemblyCSharp.ROB.customCharacterState.Climbing_Idle:
							_animation[climbIdlePoseAnimation.name].speed = -climbIdleAnimationSpeed;
							_animation[climbIdlePoseAnimation.name].wrapMode = WrapMode.ClampForever;
							_animation.CrossFade(climbIdlePoseAnimation.name);
						break;
						// Player is climbing vertically; play climbing vertical animation
						case AssemblyCSharp.ROB.customCharacterState.Climbing_Vertical:
							_animation[climbVerticalAnimation.name].speed = rob.verticalSpeed/2;
							_animation[climbVerticalAnimation.name].wrapMode = WrapMode.Loop;
							_animation.CrossFade(climbVerticalAnimation.name);
						break;
						// Player is climbing horizontally; play climbing horizontal animation
						case AssemblyCSharp.ROB.customCharacterState.Climbing_Horizontal:
							_animation[climbHorizontalAnimation.name].speed = climbHorizontalAnimationSpeed;
							_animation[climbHorizontalAnimation.name].wrapMode = WrapMode.Loop;
							_animation.CrossFade(climbHorizontalAnimation.name);
						break;
						// Player is idle while hanging; play hanging idle animation
						case AssemblyCSharp.ROB.customCharacterState.Hanging_Idle:
							_animation[hangIdlePoseAnimation.name].speed = hangIdleAnimationSpeed;
							_animation[hangIdlePoseAnimation.name].wrapMode = WrapMode.Loop;
							_animation.CrossFade(hangIdlePoseAnimation.name);
						break;
						// Player is moving while hanging; play hanging move animation
						case AssemblyCSharp.ROB.customCharacterState.Hanging_Move:
							_animation[hangMoveAnimation.name].speed = Mathf.Clamp(rob.moveSpeed, (float)0.0, hangMoveAnimationSpeed);
							_animation[hangMoveAnimation.name].wrapMode = WrapMode.Loop;
							_animation.CrossFade(hangMoveAnimation.name);
						break;
						// Player is falling fast; play free falling animation
						case AssemblyCSharp.ROB.customCharacterState.Free_Falling:
							_animation[freefallPoseAnimation.name].speed = freefallAnimationSpeed;
							_animation[freefallPoseAnimation.name].wrapMode = WrapMode.Loop;
							_animation.CrossFade(freefallPoseAnimation.name);
						break;
						// Player landed hard; play heavy landing animation
						case AssemblyCSharp.ROB.customCharacterState.Heavy_Landing:
							_animation[heavyLandPoseAnimation.name].speed = heavyLandAnimationSpeed;
							_animation[heavyLandPoseAnimation.name].wrapMode = WrapMode.ClampForever;
							_animation.CrossFade(heavyLandPoseAnimation.name);
						break;
						// Player is rolling; play rolling animation
						case AssemblyCSharp.ROB.customCharacterState.Rolling:
							_animation[rollAnimation.name].speed = rollAnimationSpeed;
							_animation[rollAnimation.name].wrapMode = WrapMode.ClampForever;
							_animation.CrossFade(rollAnimation.name);
						break;
						default:
							// If player is standing idle, play idle animation
							if(controller.velocity.sqrMagnitude < 0.1) {
								_animation[idleAnimation.name].speed = idleAnimationSpeed;
								_animation[idleAnimation.name].wrapMode = WrapMode.Loop;
								_animation.CrossFade(idleAnimation.name);
								/*if (rob.movingPlatformContact) {
									rob.moveDirection *= Vector3.Angle(movingPlatform.transform.localEulerAngles, movingPlatform.gameObject.SendMessage("noContact", SendMessageOptions.DontRequireReceiver).retPrevRot);
									rob.transform.rotation = Quaternion.LookRotation(rob.moveDirection);
								}*/
							}
							// Else, player is moving
							else 
							{
								switch(rob._characterState)
								{
									// Player is running; play running animation
									case AssemblyCSharp.ROB.customCharacterState.Running:
										_animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, (float)0.0, runMaxAnimationSpeed);
										_animation.CrossFade(runAnimation.name);	
									break;
									// Player is trotting; play trotting animation
									case AssemblyCSharp.ROB.customCharacterState.Trotting:
										_animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, (float)0.0, trotMaxAnimationSpeed);
										_animation.CrossFade(runAnimation.name);	
									break;
									// Player is walking; play walking animation
									case AssemblyCSharp.ROB.customCharacterState.Walking:
										_animation[walkAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, (float)0.0, walkMaxAnimationSpeed);
										_animation.CrossFade(walkAnimation.name);
									break;
									default:
										// If player is standing idle, play idle animation
										_animation[idleAnimation.name].speed = -idleAnimationSpeed;
										_animation[idleAnimation.name].wrapMode = WrapMode.ClampForever;
										_animation.CrossFade(idleAnimation.name);
									break;
								}
							}
						break;
					}
				}
			}
			// ANIMATION sector
		}
	}
}