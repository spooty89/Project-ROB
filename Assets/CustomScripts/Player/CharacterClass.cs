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
					moveSpeed = (float)0.0;			// The current x-z move speed
	[HideInInspector]
	public Vector3 moveDirection = Vector3.zero,	// The current move direction in x-z
					inAirVelocity = Vector3.zero,
					wallFacing = Vector3.zero,
					wallRight = Vector3.zero;
	[HideInInspector]
	public CollisionFlags collisionFlags;			// The last collision flags returned from controller.Move
	[HideInInspector]
	public bool jumping = false,
				doubleJumping = false,
				jumpingReachedApex = false,
				climbContact = false,
				climbing = false,
				hangContact = false,
				hanging = false,
				wallSliding = false;
	[HideInInspector]
	public int numHangContacts = 0,
				numClimbContacts = 0;
	[HideInInspector]
	public string currentState;
	
	
	
	public string GetCurrentState()
	{
		return currentState;
	}
	public void SetCurrentState( string state )
	{
		currentState = state;
	}
	
	
	public float CalculateJumpVerticalSpeed (float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * targetJumpHeight * gravity);
	}
	
	public bool IsGrounded () {
		if ((collisionFlags & CollisionFlags.CollidedBelow) != 0)
		{
			jumping = false;
			doubleJumping = false;
			jumpingReachedApex = false;
		}
		return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
	}
	
	
	private void OnTriggerEnter(Collider other)
	{
		//ContactObjectInterface i = (ContactObjectInterface)other.gameObject.GetComponent(typeof(ContactObjectInterface));
		
		//if(i != null)
		//	i.contact(rob);
		
	    if(other.gameObject.CompareTag("Climb")) {    	// If the triggerBox has a "Climb" tag
	        climbContact = true;						// Set climb contact to true
	        numClimbContacts += 1;						// Keep track of how many climb boxes player is currently in
		}
	    if(other.gameObject.CompareTag("Hang")) {    	// If the triggerBox has a "Hang" tag
	        hangContact = true;							// Set hang contact to true
	        numHangContacts += 1;						// Keep track of how many hang boxes player is currently in
		}
		/*if(other.gameObject.CompareTag("Token")) {
			GetComponent<ROBgui>().tokens++;
			Destroy(other.gameObject);
		}*/
	}
}

