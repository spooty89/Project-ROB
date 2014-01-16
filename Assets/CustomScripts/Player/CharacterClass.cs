using UnityEngine;

public delegate void surroundingCollisionEvent( );

public enum WallDirections : int
{
	left = 1, right = 2, neither = 3
}

public class CharacterClass : MonoBehaviour
{

	public float    gravity = 17.0f,
					rotateSpeed = (float)900.0,
					inAirRotateSpeed = (float)450.0,
					speedSmoothing = (float)10.0,
					jumpHeight = (float)1.5,				// How high we jump when pressing jump and letting go immediately
					jumpAcceleration = (float)2.0,			// Acceleration from jumping
					doubleJumpHeight = (float)1.5,			// How high we jump when we double jump
					doubleJumpAcceleration = (float)1.0,	// from double jumping
					maxWallInteractAngle = 90f;
	[HideInInspector]
	public float    verticalSpeed = (float)0.0,		// The current vertical speed
					moveSpeed = (float)0.0,			// The current x-z move speed
					rotationModifier = (float)1.0,
					rotationModifierBuildTime = (float)0.0f;
	[HideInInspector]
	public Vector3  moveDirection = Vector3.zero,	// The current move direction in x-z
					inAirVelocity = Vector3.zero,
					wallFacing = Vector3.zero,
					wallRight = Vector3.zero,
					wallLeft = Vector3.zero;			
	[HideInInspector]
	public bool jumping = false,
				doubleJumping = false,
				jumpingReachedApex = false,
				climbContact = false,
				climbing = false,
				hangContact = false,
				hanging = false,
				wallSliding = false,
				wallSlideRight = false,
				transitioning = false,
				build = false;
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
	public CharacterController controller;
	[HideInInspector]
	public surroundingTrigger sTrigger;
	public surroundingCollisionEvent surroundingCollision;
	
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
		sTrigger = transform.GetComponentInChildren<surroundingTrigger>();
		sTrigger.wallNormal = wallNormalChangeHandler;
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

	// Get wall contact information based on the contact point's normal vector
	public void wallNormalChangeHandler( Vector3 newWallNormal )
	{
		if( wallSlideDirection == (int)WallDirections.right )
		{
			//Debug.Log("right");
			if( Vector3.Angle( wallRight, newWallNormal ) > 90f )
			{
				//Debug.Log("here");
				wallFacing = newWallNormal;			// Set wallFacing equal to the contact normal (points out toward player)
				wallRight = Vector3.Cross( wallFacing, transform.up );		// Cross multiply wallFacing with the player's up vector to get wallRight
				wallLeft = Quaternion.LookRotation(wallRight, transform.up) * Vector3.back;
			}
		}
		else if( wallSlideDirection == (int)WallDirections.left )
		{
			//Debug.Log("left");
			if( Vector3.Angle( wallLeft, newWallNormal ) > 90f )
			{
				//Debug.Log("here");
				wallFacing = newWallNormal;			// Set wallFacing equal to the contact normal (points out toward player)
				wallRight = Vector3.Cross( wallFacing, transform.up );		// Cross multiply wallFacing with the player's up vector to get wallRight
				wallLeft = Quaternion.LookRotation(wallRight, transform.up) * Vector3.back;
			}
		}
		else
		{
			//Debug.Log("neither here");
			wallFacing = newWallNormal;			// Set wallFacing equal to the contact normal (points out toward player)
			wallRight = Vector3.Cross( wallFacing, transform.up );		// Cross multiply wallFacing with the player's up vector to get wallRight
			wallLeft = Quaternion.LookRotation(wallRight, transform.up) * Vector3.back;
		}
		//Debug.Log("Character - right: " + wallRight + ", left: " + wallLeft );
		surroundingCollision();
	}
	
	public float CalculateJumpVerticalSpeed (float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * targetJumpHeight * gravity);
	}
	
	public bool IsGrounded () {
		if ((controller.collisionFlags & CollisionFlags.CollidedBelow) != 0)
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

