using UnityEngine;

public class WallslideState : StateClass
{
	public float inputDelay = 0f,
					walljumpRotationModifier = 1f,
					walljumpRotModBuildTime = 0f,
					walljumpSpeed = 5f;
	private bool isMoving,
				 getInput = false,
				 justJumped = false;
	private float v, h;

	
	protected override void Awake()
	{
		base.Awake();
	}


	void OnEnable()
	{
		_cc.jumping.jumpBoost = 0f;
		_cc.wallSliding = true;
		justJumped = false;
		getInput = false;
		_cc.jumping.jumping = true;
		_cc.wallSlideDirection = (int)WallDirections.neither;
		CoRoutine.AfterWait(inputDelay, () => getInput = true);
			//Debug.Log("wallslideState");
	}

	public override void Run()
	{
		if( getInput )
		{
			InputHandler();
		}
		if( !justJumped)
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
	}
	
	private void MovementHandler()
	{
		if(_cc.transitioning){
			stateChange("transition");
			return;
		}	
		if( isMoving )
		{
			Transform cameraTransform = Camera.main.transform;
			
			Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);		// Forward vector relative to the camera along the x-z plane
			forward = forward.normalized;
		
			transform.rotation = Quaternion.LookRotation(_cc.wallFacing, _cc.wallUp);
	
			Vector3 right = new Vector3(forward.z, 0, -forward.x);		// Right vector relative to the camera
			_cc.targetDirection = h * right + v * forward;

			if(Vector3.Angle(_cc.wallRight, _cc.targetDirection) < 90f)
			{
				_cc.wallSlideDirection = (int)WallDirections.right;
				_cc.movement.updateVelocity = _cc.wallRight;
			}
			else// if(Vector3.Angle(_cc.wallLeft, targetDirection) < 60f)
			{
				_cc.wallSlideDirection = (int)WallDirections.left;
				_cc.movement.updateVelocity = _cc.wallLeft;
			}
			_cc.jumping.jumpBoost = 0f;
			
			if(Vector3.Angle(_cc.wallFacing, _cc.targetDirection) < 60f)
			{
				_cc.jumping.jumpBoost = (90f - Vector3.Angle(_cc.wallFacing, _cc.targetDirection)) / 90f;
			}
		}
		
		_cc.inputMoveDirection = new Vector3( _cc.movement.updateVelocity.x, 0, _cc.movement.updateVelocity.z ).normalized * Mathf.Max(_cc.moveSpeed, 2f);
		Debug.Log(_cc.inputMoveDirection);
		_cc.movement.updateVelocity = _cc.movement.velocity;
		_cc.movement.updateVelocity = _cc.ApplyInputVelocityChange( _cc.movement.updateVelocity );
		_cc.movement.updateVelocity = _cc.ApplyGravity( _cc.movement.updateVelocity, _cc.movement.gravity, 5f );
	}
	
	
	public void ApplyJump ()
	{
		justJumped = true;
		transform.rotation = Quaternion.LookRotation( _cc.wallFacing );
		_cc.movement.velocity = Vector3.zero;
		_cc.movement.updateVelocity = _cc.wallFacing;
		_cc.moveSpeed = ( walljumpSpeed * (1 + _cc.jumping.jumpBoost) );
		_cc.inputMoveDirection = transform.forward * _cc.moveSpeed;
		//_cc.verticalSpeed = _cc.CalculateJumpVerticalSpeed (_cc.doubleJumpHeight);
		_cc.setRotationModiferAndBuild( walljumpRotationModifier, walljumpRotModBuildTime );
		_cc.jumping.lastButtonDownTime = Time.time;
		_cc.movement.updateVelocity = _cc.ApplyInputVelocityChange( _cc.movement.updateVelocity );
		_cc.movement.updateVelocity = _cc.ApplyJumping( _cc.movement.updateVelocity, _cc.doubleJumpHeight );
		_cc.wallSlideDirection = (int)WallDirections.neither;
		stateChange("double_jump");
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_cc.IsGrounded())
		{
			_cc.moveSpeed = Vector3.Magnitude( _cc.inAirVelocity );
			_cc.wallSlideDirection = (int)WallDirections.neither;
			stateChange("idle");
		}
	}
	
	
	public override void surroundingCollisionHandler()
	{
		Vector3 dir = new Vector3( _cc.movement.velocity.x, 0, _cc.movement.velocity.z );
		if(!_cc.vCollider.vertical)
		{
			stateChange("jump_after_apex");
		}
		else if( Vector3.Angle( dir, _cc.wallFacing ) > 89f )		// If we aren't going around outside corners
		{
			if( _cc.wallSlideDirection == (int)WallDirections.left )
			{
				setDirection( _cc.wallLeft );
			}
			else if( _cc.wallSlideDirection == (int)WallDirections.right )
			{
				setDirection( _cc.wallRight );
			}
			else
			{
				float rightDiff = Mathf.Abs(Vector3.Angle( dir, _cc.wallRight));
				float leftDiff = Mathf.Abs(Vector3.Angle( dir, _cc.wallLeft));
				if( leftDiff < rightDiff )
				{
					if( leftDiff < 90f )
					{
						Debug.Log("left");
						_cc.movement.updateVelocity = _cc.wallLeft;
						_cc.wallSlideDirection = (int)WallDirections.left;
					}
				}
				else
				{
					if( rightDiff < 90f )
					{
						Debug.Log("right");
						_cc.movement.updateVelocity = _cc.wallRight;
						_cc.wallSlideDirection = (int)WallDirections.right;
					}
				}
			}
		}
	}
	
	
	public override void topCollisionHandler()
	{
        
    }
    
    
    void setDirection( Vector3 wallDir )
	{
		if( Mathf.Abs(Vector3.Angle( _cc.movement.velocity, wallDir)) < 89f)
		{
			_cc.movement.updateVelocity = wallDir;
		}
		else
		{
			_cc.getOldWallNormal();
			_cc.moveSpeed = 0f;
		}
		transform.rotation = Quaternion.LookRotation(_cc.wallFacing);
	}
	
	
	public override void TriggerEnterHandler(Collider other)
	{
		if (_cc.climbContact) {// If player is within climb triggerBox
			stateChange("climb_wall_idle");
			_cc.climbing = true;
			_cc.inAirVelocity = Vector3.zero;
			_cc.jumping.jumping = false;
			_cc.jumping.doubleJumping = false;
		}
	}
	
	
	public override void TriggerExitHandler(Collider other)
	{
		
	}
}