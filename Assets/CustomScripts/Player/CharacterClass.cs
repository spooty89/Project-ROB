using UnityEngine;
using System.Collections;

public delegate void collisionEvent( );

public enum WallDirections : int
{
	left = 1, right = 2, neither = 3
}

public class CharacterClass : MonoBehaviour
{
	public bool canControl = true,
				useController = false;
	
	[System.Serializable]
	public class CharacterMovement {
		// Curve for multiplying speed based on slope (negative = downwards)
		public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0));
		
		// How fast does the character change speeds?  Higher is faster.
		public float maxGroundAcceleration = 30.0f;
		public float maxAirAcceleration = 20.0f;
		
		// The gravity for the character
		public float gravity = 10.0f;
		public float maxFallSpeed = 20.0f;
		
		// The last collision flags returned from controller.Move
		[HideInInspector]
		public CollisionFlags collisionFlags; 
		
		// We will keep track of the character's current velocity,
		[HideInInspector]
		public Vector3 velocity,
					   updateVelocity;
		
		// This keeps track of our current velocity while we're not grounded
		[HideInInspector]
		public Vector3 frameVelocity = Vector3.zero;
		
		[HideInInspector]
		public Vector3 hitPoint = Vector3.zero;
		
		[HideInInspector]
		public Vector3 lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
	}

	public enum MovementTransferOnJump {
		None, // The jump is not affected by velocity of floor at all.
		InitTransfer, // Jump gets its initial velocity from the floor, then gradualy comes to a stop.
		PermaTransfer, // Jump gets its initial velocity from the floor, and keeps that velocity until landing.
		PermaLocked // Jump is relative to the movement of the last touched floor and will move together with that floor.
	}
	
	[System.Serializable]
	// We will contain all the jumping related variables in one helper class for clarity.
	public class CharacterJumping {
		// Can the character jump?
		public bool enabled = true;
		
		// How high do we jump when pressing jump and letting go immediately
		public float baseHeight = 1.0f;
		
		// We add extraHeight units (meters) on top when holding the button down longer while jumping
		public float extraHeight = 4.1f;
		
		// How much does the character jump out perpendicular to the surface on walkable surfaces?
		// 0 means a fully vertical jump and 1 means fully perpendicular.
		public float perpAmount = 0.0f;
		
		// How much does the character jump out perpendicular to the surface on too steep surfaces?
		// 0 means a fully vertical jump and 1 means fully perpendicular.
		public float steepPerpAmount = 0.5f;

		public float minJumpSpeedBuffer = 1.0f;
		
		// Are we jumping? (Initiated with jump button and not grounded yet)
		// To see if we are just in the air (initiated by jumping OR falling) see the grounded variable.
		[HideInInspector]
		public bool jumping = false;

		[HideInInspector]
		public bool doubleJumping = false;
		
		[HideInInspector]
		public bool holdingJumpButton = false;
		
		// the time we jumped at (Used to determine for how long to apply extra jump power after jumping.)
		[HideInInspector]
		public float lastStartTime = 0.0f;
		
		[HideInInspector]
		public float lastButtonDownTime = -100f;
		
		[HideInInspector]
		public Vector3 jumpDir = Vector3.up;
		
		[HideInInspector]
		public float jumpBoost = 0f;

		[HideInInspector]
		public float actualJumpSpeedBuffer = 1.0f;

		[HideInInspector]
		public bool justJumped = false;
	}
	
	[System.Serializable]
	public class CharacterMovingPlatform {
		public bool enabled = true;
		
		public MovementTransferOnJump movementTransfer = MovementTransferOnJump.PermaTransfer;
		
		[HideInInspector]
		public Transform hitPlatform;
		
		//[HideInInspector]
		public Transform activePlatform;
		
		[HideInInspector]
		public Vector3 activeLocalPoint;
		
		[HideInInspector]
		public Vector3 activeGlobalPoint;
		
		[HideInInspector]
		public Quaternion activeLocalRotation;
		
		[HideInInspector]
		public Quaternion activeGlobalRotation;
		
		[HideInInspector]
		public Matrix4x4 lastMatrix;
		
		[HideInInspector]
		public Vector3 platformVelocity;
		
		[HideInInspector]
		public bool newPlatform;

		//[HideInInspector]
		public bool overRide;
		
		[HideInInspector]
		public bool overRideTrigger;
	}
	
	[System.Serializable]
	public class CharacterSliding {
		// Does the character slide on too steep surfaces?
		public bool enabled = true;
		
		// How fast does the character slide on steep surfaces?
		public float slidingSpeed = 15f;
		
		// How much can the player control the sliding direction?
		// If the value is 0.5 the player can slide sideways with half the speed of the downwards sliding speed.
		public float sidewaysControl = 1.0f;
		
		// How much can the player influence the sliding speed?
		// If the value is 0.5 the player can speed the sliding up to 150% or slow it down to 50%.
		public float speedControl = 0.4f;
		
		[HideInInspector]
		public float slopeMultiplier = 1f;

		[HideInInspector]
		public bool onSlideSurface;
	}

	public CharacterMovement movement;
	public CharacterJumping jumping;
	public CharacterMovingPlatform movingPlatform;
	public CharacterSliding sliding;
	

	[HideInInspector]
	public bool grounded = true,
				inputJump = false;
	[HideInInspector]
	public Vector3 targetDirection = Vector3.zero,
				   inputMoveDirection = Vector3.zero,
				   groundNormal = Vector3.zero;
	
	private Vector3 lastGroundNormal = Vector3.zero;
	private Transform tr;
	[HideInInspector]
	public CharacterController controller;

	void Awake () {
		controller = GetComponent<CharacterController>();
		tr = transform;
		vCollider = transform.GetComponentInChildren<verticalCollider>();
		vCollider.wallNormal = wallNormalChangeHandler;
	}
	
	public void UpdateFunction () {
		// Moving platform support
		Vector3 moveDistance = Vector3.zero;
		if (MoveWithPlatform()) {
			Vector3 newGlobalPoint = movingPlatform.activePlatform.TransformPoint(movingPlatform.activeLocalPoint);
			moveDistance = (newGlobalPoint - movingPlatform.activeGlobalPoint);
			if (moveDistance != Vector3.zero)
				controller.Move(moveDistance);
			
			// Support moving platform rotation as well:
			Quaternion newGlobalRotation = movingPlatform.activePlatform.rotation * movingPlatform.activeLocalRotation;
			Quaternion rotationDiff = newGlobalRotation * Quaternion.Inverse(movingPlatform.activeGlobalRotation);
			
			var yRotation = rotationDiff.eulerAngles.y;
			if (yRotation != 0) {
				// Prevent rotation of the local up vector
				tr.Rotate(0, yRotation, 0);
			}
		}
		
		// Save lastPosition for velocity calculation.
		Vector3 lastPosition = tr.position;
		
		// We always want the movement to be framerate independent.  Multiplying by Time.deltaTime does this.
		Vector3 currentMovementOffset = movement.updateVelocity * Time.deltaTime;
		
		// Find out how much we need to push towards the ground to avoid loosing grouning
		// when walking down a step or over a sharp change in slope.
		float pushDownOffset = Mathf.Max(controller.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);
		if (grounded && !climbing)
			currentMovementOffset -= pushDownOffset * Vector3.up;
		
		// Reset variables that will be set by collision function
		movingPlatform.hitPlatform = null;
		groundNormal = Vector3.zero;
		
		// Move our character!
		movement.collisionFlags = controller.Move (currentMovementOffset);
		
		movement.lastHitPoint = movement.hitPoint;
		lastGroundNormal = groundNormal;
		
		if (movingPlatform.enabled && movingPlatform.activePlatform != movingPlatform.hitPlatform) {
			if (movingPlatform.hitPlatform != null) {
				movingPlatform.activePlatform = movingPlatform.hitPlatform;
				movingPlatform.lastMatrix = movingPlatform.hitPlatform.localToWorldMatrix;
				movingPlatform.newPlatform = true;
				/*if(movingPlatform.overRide && movingPlatform.overRideTrigger)
				{
					movingPlatform.overRideTrigger = false;
					new CoRoutine( SubtractNewPlatformVelocity(), () => {
						//movingPlatform.overRide = false;
					});
				}*/
			}
		}
		
		// Calculate the velocity based on the current and previous position.  
		// This means our velocity will only be the amount the character actually moved as a result of collisions.
		Vector3 oldHVelocity = new Vector3(movement.updateVelocity.x, 0, movement.updateVelocity.z);
		movement.velocity = (tr.position - lastPosition) / Time.deltaTime;
		Vector3 newHVelocity = new Vector3(movement.velocity.x, 0, movement.velocity.z);
		
		// The CharacterController can be moved in unwanted directions when colliding with things.
		// We want to prevent this from influencing the recorded velocity.
		if (oldHVelocity == Vector3.zero) {
			movement.velocity = new Vector3(0, movement.velocity.y, 0);
		}
		else {
			float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
			movement.velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + movement.velocity.y * Vector3.up;
		}
		
		if (movement.velocity.y < movement.updateVelocity.y - 0.001) {
			if (movement.velocity.y < 0) {
				// Something is forcing the CharacterController down faster than it should.
				// Ignore this
				movement.velocity.y = movement.updateVelocity.y;
			}
			else {
				// The upwards movement of the CharacterController has been blocked.
				// This is treated like a ceiling collision - stop further jumping here.
				jumping.holdingJumpButton = false;
			}
		}
		
		/*if( movingPlatform.overRide && movingPlatform.overRideTrigger )
		{
			Debug.Log("!");
			//movingPlatform.activePlatform = movingPlatform.hitPlatform;
			//movingPlatform.lastMatrix = movingPlatform.hitPlatform.localToWorldMatrix;
			movingPlatform.overRideTrigger = false;
			new CoRoutine( SubtractNewPlatformVelocity(), () => {
				movingPlatform.overRide = false;
			});
		}
		// We were grounded but just lost grounding
		else*/ if (grounded && !IsGroundedTest()) {
			grounded = false;
			
			// Apply inertia from platform
			if (movingPlatform.enabled &&
			    (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
			 movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
			    ) {
				Debug.Log("#");
				movement.frameVelocity = movingPlatform.platformVelocity;
				movement.velocity += movingPlatform.platformVelocity;
				movingPlatform.activePlatform = null;
				//movingPlatform.overRide = true;
			}
			
			SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
			// We pushed the character down to ensure it would stay on the ground if there was any.
			// But there wasn't so now we cancel the downwards offset to make the fall smoother.
			tr.position += pushDownOffset * Vector3.up;
		}
		// We were not grounded but just landed on something
		else if (!grounded && IsGroundedTest()) {
			grounded = true;
			jumping.jumping = false;
			jumping.doubleJumping = false;
			//movingPlatform.overRide = false;
			new CoRoutine( SubtractNewPlatformVelocity() );
			
			SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);
		}
		
		// Moving platforms support
		if (MoveWithPlatform()) {
			// Use the center of the lower half sphere of the capsule as reference point.
			// This works best when the character is standing on moving tilting platforms. 
			movingPlatform.activeGlobalPoint = tr.position + Vector3.up * (controller.center.y - controller.height*0.5f + controller.radius);
			movingPlatform.activeLocalPoint = movingPlatform.activePlatform.InverseTransformPoint(movingPlatform.activeGlobalPoint);
			
			// Support moving platform rotation as well:
			movingPlatform.activeGlobalRotation = tr.rotation;
			movingPlatform.activeLocalRotation = Quaternion.Inverse(movingPlatform.activePlatform.rotation) * movingPlatform.activeGlobalRotation; 
		}
	}
	
	public Vector3 ApplyInputVelocityChange(Vector3 velocity) {	
		if (!canControl)
			inputMoveDirection = Vector3.zero;
		
		// Find desired velocity
		Vector3 desiredVelocity = velocity;
		if (grounded && TooSteep() && sliding.enabled) {
			if( Vector3.Angle( new Vector3( transform.forward.x, 0f, transform.forward.z ), new Vector3(groundNormal.x, 0, groundNormal.z) ) > 90f )
			{
				var movementSlopeAngle = Mathf.Asin(movement.velocity.normalized.y)  * Mathf.Rad2Deg;
				sliding.slopeMultiplier -= Time.deltaTime * ( 1 - groundNormal.y );
				if( desiredVelocity.magnitude < 0.1f || sliding.slopeMultiplier < 0f)
				{
					sliding.slopeMultiplier = 1f;
					movement.velocity = Vector3.zero;
					// The direction we're sliding in
					desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
					transform.forward = desiredVelocity;
					//desiredVelocity *= sliding.slidingSpeed;
					movement.updateVelocity = desiredVelocity;
				}
				else
				{
					desiredVelocity = inputMoveDirection * sliding.slopeMultiplier;
				}
			}
			else
			{
				// The direction we're sliding in
				inputMoveDirection += new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
				inputMoveDirection.Normalize();
				if(inputMoveDirection == Vector3.zero)
					inputMoveDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
				transform.forward = inputMoveDirection;

				float magnitude = desiredVelocity.magnitude;
				desiredVelocity = inputMoveDirection;
				// Find the input movement direction projected onto the sliding direction
				var projectedMoveDir = Vector3.Project(inputMoveDirection, desiredVelocity);
				// Add the sliding direction, the spped control, and the sideways control vectors
				desiredVelocity = desiredVelocity + projectedMoveDir * sliding.speedControl + (inputMoveDirection - projectedMoveDir) * sliding.sidewaysControl;
				// Multiply with the sliding speed
				desiredVelocity *= Mathf.Lerp(magnitude, sliding.slidingSpeed, speedSmoothing * Time.deltaTime);
			}
		}
		else
		{
			sliding.slopeMultiplier = 1f;
			desiredVelocity = GetDesiredHorizontalVelocity();
		}
		
		if (movingPlatform.enabled && movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer) {
			desiredVelocity += movement.frameVelocity;
			desiredVelocity.y = 0;
		}
		
		if (grounded)
			desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);
		else
			velocity.y = 0;
		
		// Enforce max velocity change
		float maxVelocityChange = GetMaxAcceleration(grounded) * Time.deltaTime;
		Vector3 velocityChangeVector = (desiredVelocity - velocity);
		if (velocityChangeVector.sqrMagnitude > maxVelocityChange * maxVelocityChange) {
			velocityChangeVector = velocityChangeVector.normalized * maxVelocityChange;
		}
		// If we're in the air and don't have control, don't apply any velocity change at all.
		// If we're on the ground and don't have control we do apply it - it will correspond to friction.
		if (grounded || canControl)
			velocity += velocityChangeVector;
		
		if (grounded) {
			// When going uphill, the CharacterController will automatically move up by the needed amount.
			// Not moving it upwards manually prevent risk of lifting off from the ground.
			// When going downhill, DO move down manually, as gravity is not enough on steep hills.
			velocity.y = Mathf.Min(velocity.y, 0);
		}
		
		return velocity;
	}

	public Vector3 ApplyGravity(Vector3 velocity) {
		return ApplyGravity( velocity, movement.gravity, movement.maxFallSpeed );
	}

	public Vector3 ApplyGravity(Vector3 velocity, float gravity)
	{
		return ApplyGravity( velocity, gravity, movement.maxFallSpeed );
	}

	public Vector3 ApplyGravity(Vector3 velocity, float gravity, float maxFallSpeed) {
		
		if (!inputJump || !canControl) {
			jumping.holdingJumpButton = false;
			jumping.lastButtonDownTime = -100;
		}
		
		if (inputJump && jumping.lastButtonDownTime < 0 && canControl)
			jumping.lastButtonDownTime = Time.time;

		velocity.y = movement.velocity.y - gravity * Time.deltaTime;
		
		// When jumping up we don't apply gravity for some time when the user is holding the jump button.
		// This gives more control over jump height by pressing the button longer.
		if (jumping.jumping && jumping.holdingJumpButton) {
			// Calculate the duration that the extra jump force should have effect.
			// If we're still less than that duration after the jumping time, apply the force.
			if (Time.time < jumping.lastStartTime + jumping.extraHeight / CalculateJumpVerticalSpeed(jumping.baseHeight)) {
				// Negate the gravity we just applied, except we push in jumpDir rather than jump upwards.
				velocity += jumping.jumpDir * gravity * Time.deltaTime;
			}
		}
		// Make sure we don't fall any faster than maxFallSpeed. This gives our character a terminal velocity.
		velocity.y = Mathf.Max (velocity.y, -maxFallSpeed);
		return velocity;
	}
	
	public Vector3 ApplyJumping(Vector3 velocity)
	{
		return ApplyJumping( velocity, jumping.baseHeight );
	}
	
	public Vector3 ApplyJumping(Vector3 velocity, float height) {
		// Jump only if the jump button was pressed down in the last 0.2 seconds.
		// We use this check instead of checking if it's pressed down right now
		// because players will often try to jump in the exact moment when hitting the ground after a jump
		// and if they hit the button a fraction of a second too soon and no new jump happens as a consequence,
		// it's confusing and it feels like the game is buggy.
		jumping.justJumped = true;
		if (jumping.enabled && canControl && (Time.time - jumping.lastButtonDownTime < 0.2)) {
			grounded = false;
			if( !jumping.jumping )
				jumping.jumping = true;
			else
				jumping.doubleJumping = true;
			jumping.lastStartTime = Time.time;
			jumping.lastButtonDownTime = -100;
			jumping.holdingJumpButton = true;
			
			// Calculate the jumping direction
			if (TooSteep() && !(wallSliding || jumping.doubleJumping || climbing) )
			{
				jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.steepPerpAmount);
			}
			else
				jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.perpAmount);
			
			// Apply the jumping force to the velocity. Cancel any vertical velocity first.
			velocity.y = 0;
			velocity += jumping.jumpDir * CalculateJumpVerticalSpeed (height);
			
			// Apply inertia from platform
			if (movingPlatform.enabled &&
			    (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
			 	movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
			    ) {
					if( !jumping.doubleJumping )
					{
						movement.frameVelocity = movingPlatform.platformVelocity;
						velocity += movingPlatform.platformVelocity;
						movingPlatform.activePlatform = null;
						//movingPlatform.overRide = true;
					}
			}
			
			SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
		}
		else {
			jumping.holdingJumpButton = false;
		}

		return velocity;
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if ((hit.normal.y > 0 && hit.normal.y > groundNormal.y && (hit.moveDirection.y < 0)) && !movingPlatform.overRide) {
			//Debug.Log("-");
			if ((hit.point - movement.lastHitPoint).sqrMagnitude > 0.001 || lastGroundNormal == Vector3.zero)
			{
				//Debug.Log("1");
				groundNormal = hit.normal;
			}
			else
			{
				//Debug.Log("2");
				groundNormal = lastGroundNormal;
			}
		}
			
			sliding.onSlideSurface = hit.collider.gameObject.CompareTag( "slide" );
			movingPlatform.hitPlatform = hit.collider.transform;
			movement.hitPoint = hit.point;
			movement.frameVelocity = Vector3.zero;
		/*}
		else if( hit.collider.gameObject != vCollider.gameObject ) {
			Debug.Log("+");
			if( movingPlatform.overRide )
				movingPlatform.overRideTrigger = true;
			sliding.onSlideSurface = hit.collider.gameObject.CompareTag( "slide" );
			movingPlatform.hitPlatform = hit.collider.transform;
			movement.hitPoint = hit.point;
			movement.frameVelocity = Vector3.zero;
		}*/
	}
	
	IEnumerator SubtractNewPlatformVelocity () {
		// When landing, subtract the velocity of the new ground from the character's velocity
		// since movement in ground is relative to the movement of the ground.
		if (movingPlatform.enabled &&
		    (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
		 		movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
		    ) {
			// If we landed on a new platform, we have to wait for two FixedUpdates
			// before we know the velocity of the platform under the character
			if (movingPlatform.newPlatform) {
				Transform platform = movingPlatform.activePlatform;
				yield return new WaitForFixedUpdate();
				yield return new WaitForFixedUpdate();
			}
			movement.velocity -= movingPlatform.platformVelocity;
		}
	}

	private bool MoveWithPlatform() {
		return (
			movingPlatform.enabled
			//&& (movingPlatform.platformPush || grounded || movingPlatform.movementTransfer == MovementTransferOnJump.PermaLocked)
			&& movingPlatform.activePlatform != null
			);
	}
	
	private Vector3 GetDesiredHorizontalVelocity() {
		// Find desired velocity
		Vector3 desiredLocalDirection = tr.InverseTransformDirection(inputMoveDirection);
		float maxSpeed = desiredLocalDirection == Vector3.zero ? 0 : moveSpeed;
		if (grounded) {
			// Modify max speed on slopes based on slope speed multiplier curve
			var movementSlopeAngle = Mathf.Asin(movement.velocity.normalized.y)  * Mathf.Rad2Deg;
			maxSpeed *= movement.slopeSpeedMultiplier.Evaluate(movementSlopeAngle);
		}
		return tr.TransformDirection(desiredLocalDirection * maxSpeed);
	}
	
	private Vector3 AdjustGroundVelocityToNormal( Vector3 hVelocity, Vector3 groundNormal) {
		Vector3 sideways = Vector3.Cross(Vector3.up, hVelocity);
		return Vector3.Cross(sideways, groundNormal).normalized * hVelocity.magnitude;
	}
	
	private bool IsGroundedTest () {
		return (groundNormal.y > 0.01);
	}
	
	float GetMaxAcceleration ( bool grounded ) {
		// Maximum acceleration on ground and in air
		if (grounded)
			return movement.maxGroundAcceleration;
		else
			return movement.maxAirAcceleration;
	}
	
	public float CalculateJumpVerticalSpeed (float targetJumpHeight) {
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt (2 * targetJumpHeight * movement.gravity);
	}
	
	bool IsJumping () {
		return jumping.jumping;
	}
	
	bool IsSliding () {
		return (grounded && sliding.enabled && TooSteep() && sliding.onSlideSurface);
	}
	
	public bool IsTouchingCeiling () {
		return (movement.collisionFlags & CollisionFlags.CollidedAbove) != 0;
	}
	
	public bool IsGrounded () {
		return grounded;
	}
	
	public bool TooSteep () {
		return (groundNormal.y <= Mathf.Cos( Mathf.Min(controller.slopeLimit-15, 60f) * Mathf.Deg2Rad) && sliding.onSlideSurface);
	}
	
	Vector3 GetDirection () {
		return inputMoveDirection;
	}
	
	void SetControllable (bool controllable) {
		canControl = controllable;
	}
	
	void SetVelocity (Vector3 velocity) {
		grounded = false;
		movement.velocity = Vector3.zero;
		movement.updateVelocity = velocity;
		movement.frameVelocity = Vector3.zero;
		SendMessage("OnExternalVelocity");
	}

	public LayerMask layerMask;
	public float    rotateSpeed = (float)900.0,
					inAirRotateSpeed = (float)450.0,
					speedSmoothing = (float)10.0,
					doubleJumpHeight = (float)1.5;			// How high we jump when we double jump
	[HideInInspector]
	public float    moveSpeed = (float)0.0,			// The current x-z move speed
					rotationModifier = (float)1.0,
					rotationModifierBuildTime = (float)0.0f;
	[HideInInspector]
	public Vector3  moveDirection = Vector3.zero,	// The current move direction in x-z
					inAirVelocity = Vector3.zero,
					wallFacing = Vector3.zero,
					wallRight = Vector3.zero,
					wallLeft = Vector3.zero,
					wallUp = Vector3.zero,
					wallBack = Vector3.zero,
					oldWallFacing = Vector3.zero,
					oldwallRight = Vector3.zero,
					oldwallLeft = Vector3.zero,
					oldwallUp = Vector3.zero,
					oldwallBack = Vector3.zero;			
	[HideInInspector]
	public bool jumpingReachedApex = false,
				climbContact = false,
				climbing = false,
				hangContact = false,
				hanging = false,
				wallSliding = false,
				transitioning = false,
				getInput = true,
				aimEnabled = true,
				rolling = false;
	[HideInInspector]
	public int  numHangContacts = 0,
				numClimbContacts = 0,
				wallSlideDirection = (int)WallDirections.neither;
	[HideInInspector]
	public string currentState;
	[HideInInspector]
	public TransitionBox curTransitionBox;
	public stateChangeEvent stateChange;
	[HideInInspector]
	public verticalCollider vCollider;
	public collisionEvent surroundingCollision;

	private CoRoutine buildRotation;
	
	public string GetCurrentState()
	{
		return currentState;
	}
	public void SetCurrentState( string state )
	{
		currentState = state;
	}

	public void setRotationModiferAndBuild( float buildFrom, float duration )
	{
		rotationModifier = buildFrom;
		rotationModifierBuildTime = duration / (1f - buildFrom);
		if(buildRotation != null)
			buildRotation.Stop();
		buildRotation = new CoRoutine( buildRotationModifier() );
	}

	private IEnumerator buildRotationModifier()
	{
		while( rotationModifier < 1f )
		{
			rotationModifier += Time.deltaTime/rotationModifierBuildTime;
			if( rotationModifier >= 1f )
			{
				rotationModifier = 1f;
				break;
			}
			yield return null;
		}
	}

	// Get wall contact information based on the contact point's normal vector
	public void wallNormalChangeHandler( Vector3 newWallNormal, Vector3 point, GameObject obj )
	{
		oldWallFacing = wallFacing;			// Store the old values in case you need to revert back to them
		oldwallLeft = wallLeft;
		oldwallRight = wallRight;
		oldwallUp = wallUp;
		oldwallBack = wallBack;

		wallFacing = newWallNormal;			// Set wallFacing equal to the contact normal (points out toward player)
		wallRight = Vector3.Cross( wallFacing, transform.up );		// Cross multiply wallFacing with the player's up vector to get wallRight
		wallUp = Vector3.Cross( wallRight, wallFacing );		// Cross multiply wallFacing with wallLeft to get wallUp
		wallBack = Vector3.Cross(wallRight, wallUp);		// Just look back from wallUp to get wallDown;
		wallLeft = Vector3.Cross(wallBack, wallUp);		// Just look back from wallRight to get wallLeft;

		surroundingCollision();
	}

	public void getOldWallNormal()			// Restore the old wall vector values
	{
		wallFacing = oldWallFacing;
		wallLeft = oldwallLeft;
		wallRight = oldwallRight;
		wallUp = oldwallUp;
		wallBack = oldwallBack;
	}

	public void delayInput( float delay )
	{
		getInput = false;
		CoRoutine.AfterWait(delay, () => getInput = true);
	}
}