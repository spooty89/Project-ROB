using UnityEngine;

public class ClimbState : StateClass
{
	public float walljumpRotationModifier = 1f,
					walljumpRotModBuildTime = 0f,
					walljumpSpeed = 5f;
	private bool isMoving;
	private float v, h;
	private Vector3 wallTargetDirection = Vector3.zero;

	protected override void Awake()
	{
		base.Awake();
	}

	void OnEnable()
	{
		transform.rotation = Quaternion.LookRotation(_cc.wallBack, _cc.wallUp);
		_cc.jumping.jumpBoost = 0f;
		_cc.targetDirection = Vector3.zero;
		_cc.movement.updateVelocity = Vector3.zero;
		wallTargetDirection = Vector3.zero;
		_cc.moveDirection.y = 0f;
		_cc.moveSpeed = 0f;
		_cc.climbing = true;
		_cc.inAirVelocity = Vector3.zero;
		_cc.jumping.jumping = true;
		_cc.jumping.doubleJumping = false;

		enabled = true;
	}
	
	public override void Run()
	{
		if( enabled )
		{
			if( _cc.getInput && _cc.canControl )
				InputHandler();
			if( enabled )
				MovementHandler();
		}
	}
	
	private void InputHandler()
	{	
		v = Input.GetAxisRaw("Vertical");
		h = Input.GetAxisRaw("Horizontal");
		
		isMoving = Mathf.Abs (h) > 0.05f || Mathf.Abs (v) > 0.05f;

			
		if (Input.GetButtonDown("Jump"))
		{
			ApplyJump();
		}

		if( Input.GetButtonDown( "Interact" ) )
		{
			_cc.movement.velocity = Vector3.down;
			enabled = false;
			stateChange("jump_after_apex");
			_cc.jumpingReachedApex = true;
		}
	}
	
	private void MovementHandler()
	{
		if ( isMoving )
		{
			Transform cameraTransform = Camera.main.transform;
			
			// Forward vector relative to the camera along the x-z plane	
			Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
			forward.y = 0.0f;
			forward = forward.normalized;
			// Right vector relative to the camera
			// Always orthogonal to the forward vector
			Vector3 right = new Vector3(forward.z, 0, -forward.x);
			_cc.targetDirection = h * right + v * forward;
			
			if( Vector3.Angle( cameraTransform.forward, transform.forward) > 110f )
				h *= -1;
			wallTargetDirection = h * _cc.wallRight + v * _cc.wallUp;
			if( wallTargetDirection != Vector3.zero )
			{
				_cc.moveSpeed = 2.0f * Mathf.Min( wallTargetDirection.magnitude, 1f );
				if( _cc.useController )
				{
					_cc.moveSpeed *= Mathf.Lerp(1f, 1.5f, _cc.moveSpeed / 2f);
                }
                else
				{
					if (Input.GetButton("Shift"))
						_cc.moveSpeed *= 1.5f;
				}
				_cc.movement.updateVelocity = wallTargetDirection * _cc.moveSpeed;
			}
			if ( v != 0f) {			// If one of the up/down buttons is pressed
				if( v > 0)
				{
					_cc.SetCurrentState("climb_wall_up");
					CheckAbove();
				}
				else
					_cc.SetCurrentState("climb_wall_down");
			}
			if ( h != 0f && enabled)			// If one of the left/right buttons is pressed
			{
				if( Mathf.Abs(h) > Mathf.Abs(v) )
				{
					if( h > 0 )
						_cc.SetCurrentState("climb_wall_right");
					else
						_cc.SetCurrentState("climb_wall_left");
				}
			}
			
			if(Vector3.Angle(_cc.wallFacing, _cc.targetDirection) < 60f)
			{
				_cc.jumping.jumpBoost = (90f - Vector3.Angle(_cc.wallFacing, _cc.targetDirection)) / 180f;
			}
			else
				_cc.jumping.jumpBoost = 0f;
		}
		else
		{
			_cc.targetDirection = Vector3.zero;
			wallTargetDirection = Vector3.zero;
			_cc.movement.updateVelocity = Vector3.zero;
			_cc.moveSpeed = 0.0f;
			_cc.moveDirection = transform.forward;
			_cc.SetCurrentState("climb_wall_idle");
			_cc.jumping.jumpBoost = 0f;
		}
		/*if(_cc.transitioning){
			stateChange("transition");
			return;
		}*/
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if( _cc.getInput )
		{
			if(_cc.IsGrounded() && wallTargetDirection.y < -0.1f)
			{
				_cc.moveDirection = _cc.wallFacing;
				stateChange("idle");
			}
		}
	}

	
	public override void surroundingCollisionHandler()
	{
		if(enabled && _cc.vCollider.vertical)
		{
			if( Vector3.Angle( transform.forward, _cc.wallFacing ) > 44f )		// If we aren't going around outside corners
			{
				transform.rotation = Quaternion.LookRotation( _cc.wallBack );
			}
		}
	}
    
    
    public override void TriggerEnterHandler(Collider other)
	{
		
	}

	
	public void ApplyJump ()
	{
		if(!CheckAbove())
		{
			enabled = false;
			if( wallTargetDirection != Vector3.zero )
				transform.rotation = Quaternion.LookRotation( _cc.targetDirection );
			else
				transform.rotation = Quaternion.LookRotation( _cc.wallFacing );
			_cc.movement.velocity.y = 0f;
			_cc.movement.updateVelocity = transform.forward;
			_cc.moveSpeed = ( walljumpSpeed * (1 + _cc.jumping.jumpBoost) );
			_cc.inputMoveDirection = transform.forward * _cc.moveSpeed;
			_cc.setRotationModiferAndBuild( walljumpRotationModifier, walljumpRotModBuildTime );
			_cc.jumping.lastButtonDownTime = Time.time;
			_cc.movement.updateVelocity = _cc.ApplyInputVelocityChange( _cc.movement.updateVelocity );
			_cc.movement.updateVelocity = _cc.ApplyJumping( _cc.movement.updateVelocity, _cc.jumping.baseHeight );
			_cc.wallSlideDirection = (int)WallDirections.neither;
			stateChange("double_jump");
			_cc.aimEnabled = false;
			CoRoutine.AfterWait( GetComponent<AnimationSetup>().animations.Find( a => a.name == "double_jump" ).animationClip.length,() => 
			                    {
				_cc.aimEnabled = true;
			});
		}
	}


	public override void TriggerExitHandler(Collider other)
	{
		if (_cc.numClimbContacts <= 0) {				// If the player is not in any climb boxes
			stateChange("jump_after_apex");
			enabled = false;
			_cc.numClimbContacts = 0;
			_cc.climbContact = false;						// Set climb contact to false
		}
	}

	bool CheckAbove()
	{
		if( _cc.IsTouchingCeiling() && _cc.numHangContacts > 0)
		{
			enabled = false;
			transform.rotation = Quaternion.LookRotation( _cc.wallFacing, transform.up );
			_cc.moveDirection = _cc.wallFacing;
			_cc.delayInput( .5f );
			stateChange("hang_idle");
			return true;
		}
		else
			return false;
	}

	void OnDisable()
	{
		isMoving = false;
		_cc.climbing = false;
		wallTargetDirection = Vector3.zero;
		//_cc.targetDirection = Vector3.zero;
	}
}

