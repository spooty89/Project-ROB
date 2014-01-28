using UnityEngine;

public class HangState : StateClass
{
	private bool isMoving;
	private float v, h;
	private float walkSpeed = 2.0f;	// The speed when walking
	private float runSpeed = 3.0f;	// When pressing Shift button we start running
	private Vector3 surfaceUp = Vector3.up;

	
	protected override void Awake()
	{
		base.Awake();
	}
	
	void OnEnable()
	{
		v = 0;
		h = 0;
		isMoving = false;
		_cc.movement.updateVelocity = Vector3.zero;
		_cc.moveSpeed = 0f;
		_cc.hanging = true;
	}


	public override void Run()
	{
		if( _cc.getInput )
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
				_cc.verticalSpeed = -0.1f;
				stateChange("jump_after_apex");
				_cc.hanging = false;							// Set climbing to false
				_cc.jumpingReachedApex = true;
			}
		}
	}
	
	
	private void MovementHandler()
	{
		/*if(_cc.transitioning){
			stateChange("transition");
			return;
		}*/		
		_cc.inAirVelocity = Vector3.zero;
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
			_cc.moveDirection = Vector3.RotateTowards(_cc.moveDirection, targetDirection, 			// Smoothly turn towards the target direction
															_cc.rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
			_cc.moveDirection = _cc.moveDirection.normalized;
		}
		
		float curSmooth = _cc.speedSmoothing * Time.deltaTime;			// Smooth the speed based on the current target direction
		float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);	//* Support analog input but insure you cant walk faster diagonally than just f/b/l/r
	
		if (!isMoving)
		{
			_cc.SetCurrentState("hang_idle");
			_cc.moveSpeed = 0;
		}
		else{
			_cc.SetCurrentState("hang_move");
			// Pick speed modifier
			if (Input.GetButton("Shift"))
			{
				targetSpeed *= runSpeed;
			}
			else
			{
				targetSpeed *= walkSpeed;
			}
			_cc.moveSpeed = Mathf.Lerp(_cc.moveSpeed, targetSpeed, curSmooth);
		}

		
		transform.rotation = Quaternion.LookRotation(new Vector3(_cc.moveDirection.x, 0.0f, _cc.moveDirection.z));
		
		_cc.moveSpeed = Mathf.Lerp(_cc.moveSpeed, targetSpeed, curSmooth);
		
		_cc.inputMoveDirection = transform.forward * _cc.moveSpeed;
		_cc.movement.updateVelocity = _cc.movement.velocity;
		_cc.movement.updateVelocity = _cc.ApplyInputVelocityChange( _cc.movement.updateVelocity );
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if((_cc.controller.collisionFlags & CollisionFlags.CollidedAbove) != 0)
			surfaceUp = -hit.normal;
	}
	
	
	public override void surroundingCollisionHandler()
	{
		if (_cc.getInput && _cc.climbContact && Vector3.Angle( _cc.moveDirection, _cc.wallFacing ) > 100f ) {// If player is within climb triggerBox
			transform.rotation = Quaternion.Euler( _cc.wallBack );

			_cc.delayInput( 0.5f );
			stateChange("climb_wall_idle");
		}
	}
	
	
	public override void topCollisionHandler()
	{

    }
    
    
    public override void TriggerEnterHandler(Collider other)
	{
		
	}
	
	
	public override void TriggerExitHandler(Collider other)
	{
		if (_cc.numHangContacts <= 0) {				// If the player is not in any climb boxes
			_cc.verticalSpeed = -0.1f;
			_cc.stateChange("jump_after_apex");
			_cc.numHangContacts = 0;
			_cc.hangContact = false;						// Set climb contact to false
		}
	}

	void OnDisable()
	{
		_cc.hanging = false;
	}
}



