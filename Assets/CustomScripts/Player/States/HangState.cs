using UnityEngine;

public class HangState : StateClass
{
	private bool isMoving;
	private float v, h;
	private float walkSpeed = 4.0f;	// The speed when walking
	private float runSpeed = 8.0f;	// When pressing Shift button we start running
	private Vector3 surfaceUp = Vector3.up;

	
	protected override void Awake()
	{
		base.Awake();
	}
	
	void OnEnable()
	{
		//Debug.Log("hangState");
	}


	public override void Run()
	{
		InputHandler();
		MovementHandler();
	}
	
	
	private void InputHandler()
	{
		v = Input.GetAxisRaw("Vertical");
		h = Input.GetAxisRaw("Horizontal");
		
		isMoving = Mathf.Abs (h) > 0.05f || Mathf.Abs (v) > 0.05f;
		
		if( Input.anyKey)
		{
			if( Input.GetButtonDown( "Interact" ) )
			{
				_Player.verticalSpeed = -0.1f;
				stateChange("jump_after_apex");
				_Player.hanging = false;							// Set climbing to false
				_Player.jumpingReachedApex = true;
			}
		}
	}
	
	
	private void MovementHandler()
	{
		if(_Player.transitioning){
			stateChange("transition");
			return;
		}		
		_Player.inAirVelocity = Vector3.zero;
		Transform cameraTransform = Camera.main.transform;
		
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);		// Forward vector relative to the camera along the x-z plane	
		forward.y = surfaceUp.z;
		forward = forward.normalized;
	

		Vector3 right = new Vector3(forward.z, 0, -forward.x);		// Right vector relative to the camera
		Vector3 targetDirection = transform.forward;				// Always orthogonal to the forward vector
		targetDirection = h * right + v * forward;					// Target direction relative to the camera
		
	
		// We store speed and direction seperately,
		// so that when the character stands still we still have a valid forward direction
		// moveDirection is always normalized, and we only update it if there is user input.
		if (targetDirection != Vector3.zero)
		{
			_Player.moveDirection = Vector3.RotateTowards(_Player.moveDirection, targetDirection, 			// Smoothly turn towards the target direction
															_Player.rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
			_Player.moveDirection = _Player.moveDirection.normalized;
		}
		
		float curSmooth = _Player.speedSmoothing * Time.deltaTime;			// Smooth the speed based on the current target direction
		float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);	//* Support analog input but insure you cant walk faster diagonally than just f/b/l/r
	
		if (!isMoving)
		{
			_Player.SetCurrentState("hang_idle");
		}
		else{
			_Player.SetCurrentState("hang_move");
			// Pick speed modifier
			if (Input.GetButton("Shift"))
			{
				targetSpeed *= runSpeed;
			}
			else
			{
				targetSpeed *= walkSpeed;
			}
		}
		
		_Player.moveSpeed = Mathf.Lerp(_Player.moveSpeed, targetSpeed, curSmooth);
		
		transform.rotation = Quaternion.LookRotation(new Vector3(_Player.moveDirection.x, 0.0f, _Player.moveDirection.z));
		
		if((_Player.controller.collisionFlags & CollisionFlags.Above) == 0)
		{
			_Player.verticalSpeed = -0.1f;
			stateChange("jump_after_apex");
		}
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if((_Player.controller.collisionFlags & CollisionFlags.CollidedAbove) != 0)
			surfaceUp = -hit.normal;
	}
	
	
	public override void surroundingCollisionHandler()
	{
		
	}
	
	
	public override void TriggerEnterHandler(Collider other)
	{
		
	}
	
	
	public override void TriggerExitHandler(Collider other)
	{
		if (_Player.numHangContacts <= 0) {				// If the player is not in any climb boxes
			_Player.verticalSpeed = -0.1f;
			_Player.stateChange("jump_after_apex");
			_Player.numHangContacts = 0;
			_Player.hangContact = false;						// Set climb contact to false
			_Player.hanging = false;							// Set climbing to false
		}
	}
}



