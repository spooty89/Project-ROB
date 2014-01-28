using UnityEngine;

public class ClimbState : StateClass
{
	public float walljumpRotationModifier = 1f,
					walljumpRotModBuildTime = 0f,
					walljumpSpeed = 5f;
	private bool isMoving;
	private float v, h;
	public Vector3 targetDirection;

	protected override void Awake()
	{
		base.Awake();
	}

	void OnEnable()
	{
		transform.rotation = Quaternion.LookRotation(_cc.wallBack, _cc.wallUp);

		targetDirection = Vector3.zero;
		_cc.movement.updateVelocity = Vector3.zero;
		_cc.moveDirection.y = 0f;
		_cc.verticalSpeed = 0f;
		_cc.moveSpeed = 0f;
		_cc.climbing = true;
		_cc.inAirVelocity = Vector3.zero;
		_cc.jumping.jumping = false;
		_cc.jumping.doubleJumping = false;

		enabled = true;
	}
	
	public override void Run()
	{
		if( enabled )
		{
			if( _cc.getInput )
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
			enabled = false;
			transform.rotation = Quaternion.LookRotation( _cc.wallFacing, transform.up );
			_cc.moveDirection = _cc.wallFacing;
			_cc.moveSpeed = walljumpSpeed;
			_cc.setRotationModiferAndBuild( walljumpRotationModifier, walljumpRotModBuildTime );
			_cc.verticalSpeed = _cc.CalculateJumpVerticalSpeed( _cc.jumpHeight );
			_cc.jumping.lastButtonDownTime = Time.time;
			_cc.movement.updateVelocity = _cc.ApplyJumping( _cc.movement.velocity, _cc.jumpHeight );
			_cc.jumping.doubleJumping = true;
			_cc.climbing = false;
			stateChange("double_jump");
		}

		if( Input.GetButtonDown( "Interact" ) )
		{
			_cc.verticalSpeed = -0.1f;
			transform.rotation = Quaternion.LookRotation( _cc.wallFacing, transform.up );
			_cc.climbing = false;
			_cc.moveDirection = _cc.wallFacing;
			//_cc.transform.position += new Vector3(_cc.wallFacing.x * 0.25f, _cc.wallFacing.y * 0.25f, _cc.wallFacing.z * 0.25f);
			enabled = false;
			stateChange("jump_after_apex");
			_cc.jumpingReachedApex = true;
		}
	}
	
	private void MovementHandler()
	{
		if ( isMoving )
		{
			targetDirection = h * _cc.wallRight + v * _cc.wallUp;					// Target direction relative to the camera
			if( targetDirection != Vector3.zero )
			{
				_cc.moveSpeed = 2.0f * Mathf.Min( targetDirection.magnitude, 1f );
				if (Input.GetButton("Shift"))
					_cc.moveSpeed *= 1.5f;
				_cc.movement.updateVelocity = targetDirection * _cc.moveSpeed;
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
		}
		else
		{
			_cc.movement.updateVelocity = Vector3.zero;
			_cc.moveSpeed = 0.0f;
			_cc.moveDirection = transform.forward;
			_cc.SetCurrentState("climb_wall_idle");
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
			if(_cc.IsGrounded() && targetDirection.y < -0.1f)
			{
				_cc.moveDirection = _cc.wallFacing;
				targetDirection = Vector3.zero;
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
	
	
	public override void topCollisionHandler()
	{
    }
    
    
    public override void TriggerEnterHandler(Collider other)
	{
		
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

	void CheckAbove()
	{
		if( _cc.tCollider.horizontal && _cc.numHangContacts > 0)
		{
			enabled = false;
			transform.rotation = Quaternion.LookRotation( _cc.wallFacing, transform.up );
			Debug.Log( _cc.wallFacing );
			_cc.moveDirection = _cc.wallFacing;
			_cc.delayInput( .5f );
			stateChange("hang_idle");
		}
	}

	void OnDisable()
	{
		isMoving = false;
		_cc.climbing = false;
		targetDirection = Vector3.zero;
	}
}

