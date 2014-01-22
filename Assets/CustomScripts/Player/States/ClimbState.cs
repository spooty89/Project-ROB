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
		transform.rotation = Quaternion.LookRotation(_Player.wallBack, _Player.wallUp);

		targetDirection = Vector3.zero;
		_Player.moveDirection.y = 0f;
		_Player.verticalSpeed = 0f;
		_Player.moveSpeed = 0f;
		_Player.climbing = true;
		_Player.inAirVelocity = Vector3.zero;
		_Player.jumping = false;
		_Player.doubleJumping = false;

		enabled = true;
	}
	
	public override void Run()
	{
		if( enabled )
		{
			if( _Player.getInput )
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
			transform.rotation = Quaternion.LookRotation( _Player.wallFacing, transform.up );
			_Player.moveDirection = _Player.wallFacing;
			_Player.moveSpeed = walljumpSpeed;
			_Player.setRotationModiferAndBuild( walljumpRotationModifier, walljumpRotModBuildTime );
			_Player.verticalSpeed = _Player.CalculateJumpVerticalSpeed( _Player.jumpHeight );
			_Player.doubleJumping = true;
			_Player.climbing = false;
			stateChange("double_jump");
		}

		if( Input.GetButtonDown( "Interact" ) )
		{
			_Player.verticalSpeed = -0.1f;
			transform.rotation = Quaternion.LookRotation( _Player.wallFacing, transform.up );
			_Player.climbing = false;
			_Player.moveDirection = _Player.wallFacing;
			//_Player.transform.position += new Vector3(_Player.wallFacing.x * 0.25f, _Player.wallFacing.y * 0.25f, _Player.wallFacing.z * 0.25f);
			enabled = false;
			stateChange("jump_after_apex");
			_Player.jumpingReachedApex = true;
		}
	}
	
	private void MovementHandler()
	{
		if ( isMoving )
		{
			targetDirection = h * _Player.wallRight + v * _Player.wallUp;					// Target direction relative to the camera
			if( targetDirection != Vector3.zero )
				_Player.moveDirection = targetDirection;
			if ( v != 0f) {			// If one of the up/down buttons is pressed
				if( v > 0)
				{
					_Player.SetCurrentState("climb_wall_up");
					CheckAbove();
				}
				else
					_Player.SetCurrentState("climb_wall_down");
			}
			if ( h != 0f && enabled)			// If one of the left/right buttons is pressed
			{
				if( h > Mathf.Abs(v) )
					_Player.SetCurrentState("climb_wall_right");
				else if(Mathf.Abs(h) < Mathf.Abs(v))
					_Player.SetCurrentState("climb_wall_left");
			}
			_Player.moveSpeed = 2.0f * Mathf.Min( (Mathf.Abs(h)+Mathf.Abs(v)), 1f );
			if (Input.GetButton("Shift"))
				_Player.moveSpeed *= 1.5f;
		}
		else
		{
			targetDirection = Vector3.zero;
			_Player.moveSpeed = 0.0f;
			_Player.moveDirection = transform.forward;
			_Player.SetCurrentState("climb_wall_idle");
		}
		/*if(_Player.transitioning){
			stateChange("transition");
			return;
		}*/
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if( _Player.getInput )
		{
			if(_Player.IsGrounded() && targetDirection.y < -0.1f)
			{
				_Player.moveDirection = _Player.wallFacing;
				targetDirection = Vector3.zero;
				stateChange("idle");
			}
		}
	}

	
	public override void surroundingCollisionHandler()
	{
		if(enabled && _Player.vCollider.vertical)
		{
			if( Vector3.Angle( transform.forward, _Player.wallFacing ) > 44f )		// If we aren't going around outside corners
			{
				transform.rotation = Quaternion.LookRotation( _Player.wallBack );
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
		if (_Player.numClimbContacts <= 0) {				// If the player is not in any climb boxes
			stateChange("jump_after_apex");
			enabled = false;
			_Player.numClimbContacts = 0;
			_Player.climbContact = false;						// Set climb contact to false
		}
	}

	void CheckAbove()
	{
		if( _Player.tCollider.horizontal && _Player.numHangContacts > 0)
		{
			enabled = false;
			transform.rotation = Quaternion.LookRotation( _Player.wallFacing, transform.up );
			Debug.Log( _Player.wallFacing );
			_Player.moveDirection = _Player.wallFacing;
			_Player.delayInput( .5f );
			stateChange("hang_idle");
		}
	}

	void OnDisable()
	{
		isMoving = false;
		_Player.climbing = false;
		targetDirection = Vector3.zero;
	}
}

