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
	private float v, h, jumpBoost;

	
	protected override void Awake()
	{
		base.Awake();
	}


	void OnEnable()
	{
		jumpBoost = 0f;
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
		if (Input.anyKey)
		{
			v = Input.GetAxisRaw("Vertical");
			h = Input.GetAxisRaw("Horizontal");
			
			isMoving = Mathf.Abs (h) > 0.05f || Mathf.Abs (v) > 0.05f;

			if( Input.GetButtonDown( "Jump" ) )
			{
				ApplyJump();
			}
		}
		else
		{
			v = 0f;
			h = 0f;
			isMoving = false;
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
		
			transform.rotation = Quaternion.LookRotation(_cc.wallFacing);
	
			Vector3 right = new Vector3(forward.z, 0, -forward.x);		// Right vector relative to the camera
			Vector3 targetDirection = forward;				// Always orthogonal to the forward vector
			targetDirection = h * right + v * forward;
			
			if(Vector3.Angle(_cc.wallFacing, targetDirection) > 60f)
			{
				if(Vector3.Angle(_cc.wallRight, targetDirection) < 30f)
				{
					Debug.Log("here");
					_cc.movement.updateVelocity = _cc.wallRight;
					//_cc.movement.updateVelocity.y = _cc.movement.velocity.y;
				}
				else if(Vector3.Angle(_cc.wallLeft, targetDirection) < 30f)
				{
					_cc.movement.updateVelocity = _cc.wallLeft;
					//_cc.movement.updateVelocity.y = _cc.movement.velocity.y;
				}
				else
					_cc.movement.updateVelocity = _cc.movement.velocity;
					//_cc.inputMoveDirection = new Vector3( _cc.movement.velocity.x, 0, _cc.movement.velocity.z );
					
				//_cc.movement.updateVelocity -= _cc.wallFacing;
				jumpBoost = 0f;
			}
			else
			{
					_cc.movement.updateVelocity = _cc.movement.velocity;
				//_cc.inputMoveDirection = new Vector3( _cc.movement.velocity.x, 0, _cc.movement.velocity.z );
				jumpBoost = Vector3.Angle(_cc.wallFacing, targetDirection) / 90f;
			}
		}
		else
		{
					_cc.movement.updateVelocity = _cc.movement.velocity;
			//_cc.inputMoveDirection = new Vector3( _cc.movement.velocity.x, 0, _cc.movement.velocity.z );
			if(_cc.inAirVelocity.magnitude > 0f)
			{
				_cc.inAirVelocity += _cc.movement.updateVelocity.normalized * Time.deltaTime * _cc.jumpAcceleration;
			}
		}
		/*
		if( Mathf.Abs(Vector3.Angle( _cc.moveDirection, _cc.wallRight)) > 90f )
		{
			_cc.moveDirection = _cc.wallLeft;
			_cc.wallSlideDirection = WallDirections.left;
		}
		else
		{
			_cc.moveDirection = _cc.wallRight;
			_cc.wallSlideDirection = WallDirections.right;
		}*/

		/*if (_cc.verticalSpeed > -5.0f)
		{
			_cc.verticalSpeed -= (_cc.gravity/2f) * Time.deltaTime;
		}
		else
		{
			_cc.verticalSpeed = -5.0f;
		}*/
		
		_cc.inputMoveDirection = new Vector3( _cc.movement.updateVelocity.x, 0, _cc.movement.updateVelocity.z ).normalized * _cc.moveSpeed;
		_cc.movement.updateVelocity = _cc.movement.velocity;
		_cc.movement.updateVelocity = _cc.ApplyInputVelocityChange( _cc.movement.updateVelocity );
		_cc.movement.updateVelocity = _cc.ApplyGravity( _cc.movement.updateVelocity, _cc.movement.gravity, 5f );

		/*if( _cc.controller.collisionFlags == 0 && !_cc.sTrigger.vertical)
		{
			//Debug.Log("here");
			stateChange("jump_after_apex");
		}*/
	}
	
	
	public void ApplyJump ()
	{
		justJumped = true;
		transform.rotation = Quaternion.LookRotation( _cc.wallFacing );
		_cc.movement.velocity = Vector3.zero;
		_cc.movement.updateVelocity = _cc.wallFacing;
		_cc.moveSpeed = ( walljumpSpeed * (1 + jumpBoost) );
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
		if(_cc.IsGrounded() && _cc.verticalSpeed < 0.0f)
		{
			_cc.moveSpeed = Vector3.Magnitude( _cc.inAirVelocity );
			_cc.wallSlideDirection = (int)WallDirections.neither;
			stateChange("idle");
		}
	}
	
	
	public override void surroundingCollisionHandler()
	{
		float y = _cc.movement.velocity.y;
		_cc.movement.velocity.y = 0;
		if(!_cc.vCollider.vertical)
		{
			stateChange("jump_after_apex");
		}
		else if( Vector3.Angle( _cc.movement.velocity, _cc.wallFacing ) > 89f )		// If we aren't going around outside corners
		{
			if( _cc.wallSlideDirection == (int)WallDirections.left )
			{
				setDirection( _cc.wallLeft, y );
			}
			else if( _cc.wallSlideDirection == (int)WallDirections.right )
			{
				setDirection( _cc.wallRight, y );
			}
			else
			{
				float rightDiff = Mathf.Abs(Vector3.Angle( _cc.movement.velocity, _cc.wallRight));
				float leftDiff = Mathf.Abs(Vector3.Angle( _cc.movement.velocity, _cc.wallLeft));
				if( leftDiff < rightDiff )
				{
					if( leftDiff < 90f )
					{
						Debug.Log("left");
						_cc.movement.updateVelocity = _cc.wallLeft;
						_cc.movement.updateVelocity.y = y;
						_cc.movement.velocity.y = y;
						_cc.wallSlideDirection = (int)WallDirections.left;
					}
				}
				else
				{
					if( rightDiff < 90f )
					{
						Debug.Log("right");
						_cc.movement.updateVelocity = _cc.wallRight;
						_cc.movement.updateVelocity.y = y;
						_cc.movement.velocity.y = y;
						_cc.wallSlideDirection = (int)WallDirections.right;
					}
				}
			}
		}
	}
	
	
	public override void topCollisionHandler()
	{
        
    }
    
    
    void setDirection( Vector3 wallDir, float y )
	{
		if( Mathf.Abs(Vector3.Angle( _cc.movement.velocity, wallDir)) < 89f)
		{
			_cc.movement.updateVelocity = wallDir;
			_cc.movement.updateVelocity.y = y;
			_cc.movement.velocity.y = y;
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