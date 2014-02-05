using UnityEngine;

public class JumpState : StateClass
{
	private bool isMoving;
	private float v, h;


	void OnEnable()
	{
		_cc.jumping.jumping = true;
		_cc.jumpingReachedApex = false;
	}
	
	protected override void Awake()
	{
		base.Awake();
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

			if( Input.GetButtonDown( "Jump" ) )
			{
				ApplyJump();
			}
			if( Input.GetButton( "Jump" ) )
				_cc.inputJump = true;
			else
				_cc.inputJump = false;
	}
	
	
	private void MovementHandler()
	{
		if(_cc.transitioning){
			stateChange("transition");
			return;
		}
		// When we reach the apex
		if (_cc.movement.velocity.y < -0.5f)
		{
			_cc.jumpingReachedApex = true;
			_cc.SetCurrentState("jump_after_apex");
			/*if( _cc.movement.velocity.y == _cc.movement.maxFallSpeed){
				_cc.SetCurrentState("free_fall");
			}*/
		}
		
		
		Transform cameraTransform = Camera.main.transform;
		
		// Forward vector relative to the camera along the x-z plane	
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = 0.0f;
		forward = forward.normalized;
		// Right vector relative to the camera
		// Always orthogonal to the forward vector
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		_cc.targetDirection = h * right + v * forward;

		if (_cc.targetDirection != Vector3.zero) { // If there is any input, smoothly turn towards the target direction
			transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, _cc.targetDirection, _cc.rotationModifier * _cc.inAirRotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000));
		}

		float curSmooth = _cc.speedSmoothing * Time.deltaTime /2;			// Smooth the speed based on the current target direction
		float targetSpeed = Mathf.Min(_cc.targetDirection.magnitude, 1.0f);		//* Support analog input but insure you cant walk faster diagonally than just f/b/l/r
		targetSpeed *= Mathf.Max( _cc.moveSpeed, 2f);
		_cc.moveSpeed = Mathf.Lerp(_cc.moveSpeed, targetSpeed, curSmooth);
		_cc.inputMoveDirection = transform.forward * _cc.moveSpeed;

		_cc.movement.updateVelocity = _cc.movement.velocity;
		_cc.movement.updateVelocity = _cc.ApplyInputVelocityChange( _cc.movement.updateVelocity );
		_cc.movement.updateVelocity = _cc.ApplyGravity( _cc.movement.updateVelocity );
	}
	
	
	private void ApplyJump ()
	{
		if( !_cc.jumping.doubleJumping )
		{
			_cc.jumping.lastButtonDownTime = Time.time;
			_cc.movement.velocity = _cc.ApplyJumping( _cc.movement.velocity, _cc.doubleJumpHeight );
			_cc.SetCurrentState("double_jump");
			_cc.jumpingReachedApex = false;
			_cc.jumping.doubleJumping = true;
		}
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_cc.IsGrounded())
		{
			_cc.rotationModifier = 1f;
			stateChange("idle");
		}
		else if (_cc.hangContact && _cc.movement.velocity.y > 0f && Vector3.Angle(hit.normal, transform.up) > 100f) {// If player is within hang triggerBox;
				stateChange("hang_idle");
		}
	}
	
	
	public override void surroundingCollisionHandler()
	{
		wallInteract( );
	}
    
    
    public override void TriggerEnterHandler(Collider other)
	{
		
	}
	
	
	public override void TriggerExitHandler(Collider other)
	{
		
	}


	private void wallInteract( )
	{
		// If we aren't going around an outside corner
		if (_cc.movement.velocity.y < -0.1f)	
		{
			if (_cc.climbContact)
			{
				if( Vector3.Angle( new Vector3(_cc.targetDirection.x, 0, _cc.targetDirection.z), new Vector3(_cc.wallFacing.x, 0, _cc.wallFacing.z) ) > 91f ) {// If player is within climb triggerBox
					transform.rotation = Quaternion.LookRotation(_cc.wallBack, _cc.wallUp);
					stateChange("climb_wall_idle");
				}
			}
			else if ( Vector3.Angle( new Vector3(_cc.movement.velocity.x, 0, _cc.movement.velocity.z), new Vector3(_cc.wallFacing.x, 0, _cc.wallFacing.z) ) > 89f )
			{
				float rightDiff = Mathf.Abs(Vector3.Angle( _cc.movement.velocity, _cc.wallRight));
				float leftDiff = Mathf.Abs(Vector3.Angle( _cc.movement.velocity, _cc.wallLeft));
				if( leftDiff < rightDiff )
				{
					_cc.movement.updateVelocity = _cc.wallLeft;
					_cc.wallSlideDirection = (int)WallDirections.left;
					setWallMovement();
				}
				else
				{
					_cc.movement.updateVelocity = _cc.wallRight;
					_cc.wallSlideDirection = (int)WallDirections.right;
					setWallMovement();
				}
			}
		}
	}
	
	void setWallMovement()
	{
		_cc.moveSpeed *= (90f - Mathf.Abs(Vector3.Angle( _cc.movement.updateVelocity.normalized, transform.forward ))) / 90f;
		transform.rotation = Quaternion.LookRotation(_cc.wallFacing, _cc.wallUp);

		stateChange("wall_slide");
		_cc.wallSliding = true;
	}


	void OnDisable()
	{
		_cc.jumping.jumping = false;
		_cc.jumping.doubleJumping = false;
	}
}