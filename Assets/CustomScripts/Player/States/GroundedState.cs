using UnityEngine;

public class GroundedState : StateClass
{
	private bool isMoving;//, sliding = false;
	private float v, h;
	private float walkSpeed = 4.0f;	// The speed when walking
	private float runSpeed = 8.0f;	// When pressing Shift button we start running
	private Vector3 surfaceUp = Vector3.up;

	// Get necessary information right away
	protected override void Awake()
	{
		base.Awake();
	}

	// Every time this class is enabled, run this
	void OnEnable()
	{
		enabled = true;
		_Player.inAirVelocity = Vector3.zero;
	}

	// While this class is running, perform these tasks every frame
	public override void Run()
	{
		if( enabled )
		{
			if( _Player.getInput )
				InputHandler();
			if( enabled )				// Check again, in case input disabled this script
				MovementHandler();
		}
	}
	
	// Get the user's input, make sense of it
	private void InputHandler()
	{
		v = Input.GetAxisRaw("Vertical");
		h = Input.GetAxisRaw("Horizontal");
		
		isMoving = Mathf.Abs (h) > 0.05f || Mathf.Abs (v) > 0.05f;

		if( Input.GetButtonDown( "Jump" ) )
			ApplyJump();
		if( Input.GetButton( "Aim" ) )
			_Player.stateChange( "aim_idle" );
	}
	
	
	private void MovementHandler()
	{	
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
			_Player.SetCurrentState("idle");
		}
		else{
			// Pick speed modifier
			if (Input.GetButton("Shift"))
			{
				targetSpeed *= runSpeed;
				_Player.SetCurrentState("run");
			}
			else
			{
				targetSpeed *= walkSpeed;
				_Player.SetCurrentState("walk");
			}
		}
		
		_Player.moveSpeed = Mathf.Lerp(_Player.moveSpeed, targetSpeed, curSmooth);
		
		transform.rotation = Quaternion.LookRotation(new Vector3(_Player.moveDirection.x, 0.0f, _Player.moveDirection.z));
		
		if (!_Player.IsGrounded())
		{
			Debug.Log("here");
			stateChange("jump_after_apex");
		}
	}

	
	
	private void ApplyJump ()
	{
		enabled = false;
		_Player.verticalSpeed = _Player.CalculateJumpVerticalSpeed (_Player.jumpHeight);
		stateChange("jump");
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_Player.IsGrounded())
		{
			RaycastHit rcHit;
			if( Physics.Raycast( transform.position, (hit.point - transform.position).normalized,
			                    out rcHit, Vector3.Distance(hit.point, transform.position) + .5f ) )
			{
				if( rcHit.normal.y > 0.8f)
				{
					surfaceUp = hit.normal;
					Debug.DrawRay( hit.point, rcHit.normal, Color.white, 5 );
				}
			}

			//surfaceUp = hit.normal;

		}
	}
	
	
	public override void surroundingCollisionHandler()
	{
		if (_Player.climbContact && Vector3.Angle( _Player.moveDirection, _Player.wallFacing ) > 100f ) {// If player is within climb triggerBox
			transform.rotation = Quaternion.Euler( Quaternion.Euler(_Player.wallFacing) * Vector3.back );
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
		
	}
}