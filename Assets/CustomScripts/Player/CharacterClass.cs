using UnityEngine;

public class CharacterClass : MonoBehaviour
{
	public float gravity = 17.0f,
					rotateSpeed = (float)900.0,
					inAirRotateSpeed = (float)450.0,
					speedSmoothing = (float)10.0,
					jumpHeight = (float)1.5,				// How high we jump when pressing jump and letting go immediately
					jumpAcceleration = (float)2.0,			// Acceleration from jumping
					doubleJumpHeight = (float)1.5,			// How high we jump when we double jump
					doubleJumpAcceleration = (float)1.0;	// from double jumping
	[HideInInspector]
	public float verticalSpeed = (float)0.0,		// The current vertical speed
					moveSpeed = (float)0.0,			// The current x-z move speed
					rotationModifier = (float)1.0,
					rotationModifierBuildTime = (float)0.0f;
	[HideInInspector]
	public Vector3 moveDirection = Vector3.zero,	// The current move direction in x-z
					inAirVelocity = Vector3.zero,
					wallFacing = Vector3.zero,
					wallRight = Vector3.zero;			
	[HideInInspector]
	public bool jumping = false,
				doubleJumping = false,
				jumpingReachedApex = false,
				climbContact = false,
				climbing = false,
				hangContact = false,
				hanging = false,
				wallSliding = false,
				transitioning = false;
	[HideInInspector]
	public int numHangContacts = 0,
				numClimbContacts = 0;
	[HideInInspector]
	public string currentState;
	[HideInInspector]
	public TransitionBox curTransitionBox;
	public stateChangeEvent stateChange;
	private bool build = false;

	public CharacterController controller;
	
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
		build = true;
	}

	private void Awake ()
	{
		controller = GetComponent<CharacterController>();
	}

	void Update()
	{
		Vector3 movement = moveDirection * moveSpeed + new Vector3 (0, verticalSpeed, 0) + inAirVelocity;		// Calculate actual motion
		movement *= Time.deltaTime;			// Base degree of applied movement on time since last frame
		controller.Move(movement);

		if( build )
		{
			rotationModifier += Time.deltaTime / rotationModifierBuildTime;
			if( rotationModifier > 1f )
			{
				rotationModifier = 1f;
				build = false;
			}
		}
	}
	
	public float CalculateJumpVerticalSpeed (float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * targetJumpHeight * gravity);
	}
	
	public bool IsGrounded () {
		if ( (controller.collisionFlags & CollisionFlags.CollidedBelow) != 0)
		{
			jumping = false;
			doubleJumping = false;
			jumpingReachedApex = false;
			return true;
		}
		else
		{
			return false;
		}
	}
	
	
	public void OnTriggerEnter(Collider other)
	{
	    if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
	        climbContact = true;						// Set climb contact to true
	        numClimbContacts += 1;						// Keep track of how many climb boxes player is currently in
		}
	    if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
	        hangContact = true;							// Set hang contact to true
	        numHangContacts += 1;						// Keep track of how many hang boxes player is currently in
		}
		if(other.gameObject.CompareTag("TransitionBox")){
			TransitionBox transBox = (TransitionBox)other.gameObject.GetComponent("TransitionBox");
			TransitionEnterCondition curCond = transBox.matchEnterCondition(this);
			if(curCond != null){
				transitioning = true;
				curTransitionBox = transBox;
				curTransitionBox.curCond = curCond;
			}
		}
	}
}

