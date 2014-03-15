using UnityEngine;

public class GroundedState : StateClass
{
	private bool isMoving;//, sliding = false;
	private float v, h;
	public float walkSpeed = 4.0f;	// The speed when walking
	public float runSpeed = 8.0f;	// When pressing Shift button we start running
	public float minRollSpeed = 0.5f;
	public float slopeJumpRotationModifier, slopeJumpRotModBuildTime;
	private Vector3 surfaceUp = Vector3.up;

	// Get necessary information right away
	protected override void Awake()
	{
		base.Awake();
	}

	// Every time this class is enabled, run this
	void OnEnable()
	{
		if( _cc.TooSteep() && _cc.sliding.onSlideSurface && _cc.groundNormal != Vector3.zero )
		{
			_cc.movement.velocity = Vector3.zero;
			transform.forward = new Vector3(_cc.groundNormal.x, 0, _cc.groundNormal.z);
		}
		enabled = true;
		_cc.inAirVelocity = Vector3.zero;
		_cc.jumping.jumping = false;
		_cc.jumping.doubleJumping = false;
		_cc.jumping.lastButtonDownTime = -100;
		_cc.aimEnabled = true;
	}

	// While this class is running, perform these tasks every frame
	public override void Run()
	{
		if( enabled )
		{
			if( _cc.getInput && _cc.canControl )
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
		if( !_cc.rolling )
		{
			if( Input.GetButtonDown( "Jump" ) )
				ApplyJump();
			else
				_cc.inputJump = false;

			if( Input.GetButtonDown( "Interact" ) )
			{
				animation[ "roll" ].time = 0f;
				_cc.SetCurrentState( "roll" );
				_cc.rolling = true;
				float currentHeight = _cc.controller.height;
				float duration = animation[ "roll" ].length/animation[ "roll" ].speed;
				new CoRoutine( _cc.adjustControllerHeight( _cc.controller.height/2, duration/2 ), () =>
					new CoRoutine(_cc.adjustControllerHeight( currentHeight, duration/2 )) );
				CoRoutine.AfterWait( duration, () => _cc.rolling = false );
			}

			if( Input.GetButton( "Aim" ) )
				_cc.stateChange( "aim_idle" );
		}
	}
	
	
	private void MovementHandler()
	{	
		Transform cameraTransform = Camera.main.transform;
		
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);		// Forward vector relative to the camera along the x-z plane	
		forward.y = surfaceUp.z;
		forward = forward.normalized;
	

		Vector3 right = new Vector3(forward.z, 0, -forward.x);		// Right vector relative to the camera
		_cc.targetDirection = h * right + v * forward;					// Target direction relative to the camera
		
	
		// We store speed and direction seperately,
		// so that when the character stands still we still have a valid forward direction
		// moveDirection is always normalized, and we only update it if there is user input.
		if (_cc.targetDirection != Vector3.zero)
		{
			_cc.moveDirection = Vector3.RotateTowards( transform.forward, _cc.targetDirection, 			// Smoothly turn towards the target direction
															_cc.rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
			_cc.moveDirection = _cc.moveDirection.normalized;
		}
		
		float curSmooth = _cc.speedSmoothing * Time.deltaTime;			// Smooth the speed based on the current target direction
		float targetSpeed = Mathf.Min(_cc.targetDirection.magnitude, 1.0f);	//* Support analog input but insure you cant walk faster diagonally than just f/b/l/r
	
		if( _cc.rolling )
		{
			transform.rotation = Quaternion.LookRotation(new Vector3(_cc.moveDirection.x, 0.0f, _cc.moveDirection.z));
			// Pick speed modifier
			if (!isMoving)
				targetSpeed = minRollSpeed;
			if( _cc.useController )
			{
				targetSpeed *= Mathf.Lerp(walkSpeed*1.1f, runSpeed*1.1f, targetSpeed);
			}
			else
			{
				if (Input.GetButton("Shift"))
				{
					targetSpeed *= runSpeed;
				}
				else
				{
					targetSpeed *= walkSpeed;
				}
			}
		}
		else if (!isMoving)
		{
			_cc.SetCurrentState("idle");
		}
		else{
			transform.rotation = Quaternion.LookRotation(new Vector3(_cc.moveDirection.x, 0.0f, _cc.moveDirection.z));
			// Pick speed modifier
			if( _cc.useController )
			{
				targetSpeed *= Mathf.Lerp(walkSpeed, runSpeed, targetSpeed);
				if( targetSpeed <= walkSpeed )
					_cc.SetCurrentState("walk");
				else
					_cc.SetCurrentState("run");
			}
			else
			{
				if (Input.GetButton("Shift"))
				{
					targetSpeed *= runSpeed;
					_cc.SetCurrentState("run");
				}
				else
				{
					targetSpeed *= walkSpeed;
					_cc.SetCurrentState("walk");
				}
			}
		}
		
		_cc.moveSpeed = Mathf.Lerp(_cc.moveSpeed, targetSpeed, curSmooth);

		_cc.inputMoveDirection = transform.forward * _cc.moveSpeed;
		_cc.movement.updateVelocity = _cc.movement.velocity;
		_cc.movement.updateVelocity = _cc.ApplyInputVelocityChange( _cc.movement.updateVelocity );
		
		if (!_cc.IsGrounded())
		{
			stateChange("jump_after_apex");
		}
	}

	
	
	private void ApplyJump ()
	{
		if( !_cc.rolling )
		{
			enabled = false;
			_cc.inputJump = true;
			_cc.jumping.lastButtonDownTime = Time.time;
			if( _cc.TooSteep() )
			{
				_cc.setRotationModiferAndBuild( slopeJumpRotationModifier, slopeJumpRotModBuildTime );
				//transform.forward = new Vector3( _cc.groundNormal.x, 0, _cc.groundNormal.z );
				_cc.jumping.actualJumpSpeedBuffer = .0f;
				_cc.movement.updateVelocity = _cc.ApplyJumping( _cc.groundNormal * _cc.moveSpeed/2, _cc.jumping.doubleJumpBaseHeight/2 );
				stateChange("double_jump");
				_cc.jumping.doubleJumping = true;
			}
			else
			{
				_cc.movement.updateVelocity = _cc.ApplyJumping( _cc.movement.velocity );
				stateChange("jump");
			}
		}
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_cc.IsGrounded())
		{
			RaycastHit rcHit;
			if( Physics.Raycast( transform.position, (hit.point - transform.position).normalized,
			                    out rcHit, Vector3.Distance(hit.point, transform.position) + .5f ) &&
			   					rcHit.normal.y > 0.8f )
			{
				surfaceUp = hit.normal;
				//Debug.DrawRay( hit.point, rcHit.normal, Color.white, 5 );
			}
		}
	}
	
	
	public override void surroundingCollisionHandler()
	{
		if (_cc.climbContact &&  Vector3.Angle( new Vector3(_cc.targetDirection.x, 0, _cc.targetDirection.z), new Vector3(_cc.wallFacing.x, 0, _cc.wallFacing.z) ) > 100f ) {// If player is within climb triggerBox
			transform.rotation = Quaternion.LookRotation(_cc.wallBack, _cc.wallUp);
			stateChange("climb_wall_idle");
		}
	}
    
    
    public override void TriggerEnterHandler(Collider other)
	{
		
	}
	
	
	public override void TriggerExitHandler(Collider other)
	{
		
	}
}