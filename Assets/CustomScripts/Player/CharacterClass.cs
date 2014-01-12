using UnityEngine;

public class CharacterClass : MonoBehaviour
{
	public float    gravity = 17.0f,
					rotateSpeed = (float)900.0,
					inAirRotateSpeed = (float)450.0,
					speedSmoothing = (float)10.0,
					jumpHeight = (float)1.5,				// How high we jump when pressing jump and letting go immediately
					jumpAcceleration = (float)2.0,			// Acceleration from jumping
					doubleJumpHeight = (float)1.5,			// How high we jump when we double jump
					doubleJumpAcceleration = (float)1.0;	// from double jumping
	[HideInInspector]
	public float    verticalSpeed = (float)0.0,		// The current vertical speed
					moveSpeed = (float)0.0,			// The current x-z move speed
					rotationModifier = (float)1.0,
					rotationModifierBuildTime = (float)0.0f;
	[HideInInspector]
	public Vector3  moveDirection = Vector3.zero,	// The current move direction in x-z
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
				transitioning = false,
				build = false;
	[HideInInspector]
	public int  numHangContacts = 0,
				numClimbContacts = 0;
	[HideInInspector]
	public string currentState;
	[HideInInspector]
	public TransitionBox curTransitionBox;
	public stateChangeEvent stateChange;
	[HideInInspector]
	public CharacterController controller;
	[HideInInspector]
	public SurroundingTrigger surroundingTrigger;
	
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
		surroundingTrigger = transform.GetComponentInChildren<SurroundingTrigger>();
		surroundingTrigger.wallNormal = wallNormalChangeHandler;
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

	public void wallNormalChangeHandler( Vector3 newWallNormal )
	{
		wallFacing = newWallNormal;
		wallRight = Vector3.Cross( wallFacing, transform.up );
	}
	
	public float CalculateJumpVerticalSpeed (float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * targetJumpHeight * gravity);
	}
	
	public bool IsGrounded () {
		if ( surroundingTrigger.horizontalUp || (controller.collisionFlags & CollisionFlags.CollidedBelow) != 0)
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
}

