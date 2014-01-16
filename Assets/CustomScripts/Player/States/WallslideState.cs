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
		if( _Player == null )
		{
			_Player = GetComponent<CharacterClass>();
		}
	}


	void OnEnable()
	{
		justJumped = false;
		getInput = false;
		_Player.wallSlideDirection = (int)WallDirections.neither;
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
		/*if (Input.anyKey)
		{
			v = Input.GetAxisRaw("Vertical");
			h = Input.GetAxisRaw("Horizontal");
			
			isMoving = Mathf.Abs (h) > 0.05f || Mathf.Abs (v) > 0.05f;
			*/
			if( Input.GetButtonDown( "Jump" ) )
			{
				ApplyJump();
			}
		/*}
		else
		{
			v = 0f;
			h = 0f;
			isMoving = false;
		}*/
	}
	
	private void MovementHandler()
	{
		if(_Player.transitioning){
			stateChange("transition");
			return;
		}	
		/*if( isMoving )
		{
			Transform cameraTransform = Camera.main.transform;
			
			Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);		// Forward vector relative to the camera along the x-z plane
			forward = forward.normalized;
		
	
			Vector3 right = new Vector3(forward.z, 0, -forward.x);		// Right vector relative to the camera
			Vector3 targetDirection = transform.forward;				// Always orthogonal to the forward vector
			targetDirection = h * right + v * forward;
			
			if(Vector3.Angle(_Player.wallFacing, targetDirection) > 90f)
			{
				if(Vector3.Angle(_Player.wallRight, targetDirection) < 90f)
				{
					_Player.moveDirection = _Player.wallRight;
				}
				else
				{
					_Player.moveDirection = new Vector3(-_Player.wallFacing.z, _Player.wallFacing.y, _Player.wallFacing.x);
				}
				_Player.moveDirection -= _Player.wallFacing;
			}
			else
			{
				stateChange("jump_after_apex");
				_Player.moveDirection = targetDirection;
			}
			_Player.inAirVelocity += _Player.moveDirection.normalized * Time.deltaTime * 0.01f;
		}
		else
		{
			if(_Player.inAirVelocity.magnitude > 0f)
			{
				_Player.inAirVelocity += _Player.moveDirection.normalized * Time.deltaTime * _Player.jumpAcceleration;
			}
		}*/
		/*
		if( Mathf.Abs(Vector3.Angle( _Player.moveDirection, _Player.wallRight)) > 90f )
		{
			_Player.moveDirection = _Player.wallLeft;
			_Player.wallSlideDirection = WallDirections.left;
		}
		else
		{
			_Player.moveDirection = _Player.wallRight;
			_Player.wallSlideDirection = WallDirections.right;
		}*/
		transform.rotation = Quaternion.LookRotation(_Player.wallFacing);

		if (_Player.verticalSpeed > -5.0f)
		{
			_Player.verticalSpeed -= (_Player.gravity/2f) * Time.deltaTime;
		}
		else
		{
			_Player.verticalSpeed = -5.0f;
		}

		/*if( _Player.controller.collisionFlags == 0 && !_Player.sTrigger.vertical)
		{
			//Debug.Log("here");
			stateChange("jump_after_apex");
		}*/
	}
	
	
	public void ApplyJump ()
	{
		justJumped = true;
		_Player.moveDirection = _Player.wallFacing;
		_Player.moveSpeed = walljumpSpeed;
		_Player.verticalSpeed = _Player.CalculateJumpVerticalSpeed (_Player.doubleJumpHeight);
		_Player.setRotationModiferAndBuild( walljumpRotationModifier, walljumpRotModBuildTime );
		_Player.doubleJumping = true;
		_Player.wallSlideDirection = (int)WallDirections.neither;
		stateChange("double_jump");
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_Player.IsGrounded() && _Player.verticalSpeed < 0.0f)
		{
			_Player.moveSpeed = Vector3.Magnitude( _Player.inAirVelocity );
			_Player.wallSlideDirection = (int)WallDirections.neither;
			stateChange("idle");
		}
	}
	
	
	public override void surroundingCollisionHandler()
	{
		
		if(!_Player.sTrigger.vertical)
		{
			Debug.Log("jump transition");
			stateChange("jump_after_apex");
		}
		else
		{
			if( _Player.wallSlideDirection == (int)WallDirections.left )
			{
				if( Mathf.Abs(Vector3.Angle( _Player.moveDirection, _Player.wallLeft)) < 150f )
				{
					_Player.moveDirection = _Player.wallLeft;
					transform.rotation = Quaternion.LookRotation(_Player.wallFacing);
				}
			}
			else if( _Player.wallSlideDirection == (int)WallDirections.right )
			{
				if( Mathf.Abs(Vector3.Angle( _Player.moveDirection, _Player.wallRight)) < 150f )
				{
					_Player.moveDirection = _Player.wallRight;
					transform.rotation = Quaternion.LookRotation(_Player.wallFacing);
				}
			}
			else
			{
				float rightDiff = Mathf.Abs(Vector3.Angle( _Player.moveDirection, _Player.wallRight));
				float leftDiff = Mathf.Abs(Vector3.Angle( _Player.moveDirection, _Player.wallLeft));
				if( leftDiff < rightDiff )
				{
					if( leftDiff < 90f )
					{
						_Player.moveDirection = _Player.wallLeft;
						_Player.wallSlideDirection = (int)WallDirections.left;
					}
				}
				else
				{
					if( rightDiff < 90f )
					{
						_Player.moveDirection = _Player.wallRight;
						_Player.wallSlideDirection = (int)WallDirections.right;
					}
				}
			}
		}
	}
	
	
	public override void TriggerEnterHandler(Collider other)
	{
		if (_Player.climbContact) {// If player is within climb triggerBox
			//_Player.moveDirection = -_Player.wallFacing;
			transform.rotation = Quaternion.LookRotation(_Player.wallFacing);
			Vector3 rotation = transform.rotation.eulerAngles;
			rotation = new Vector3( rotation.x, rotation.y + 180, rotation.z );
			transform.rotation = Quaternion.Euler( rotation );
			
			stateChange("climb_wall_idle");
			_Player.climbing = true;
			//_Player.moveSpeed = 1.0f;
			_Player.inAirVelocity = Vector3.zero;
			_Player.jumping = false;
			_Player.doubleJumping = false;
		}
	}
	
	
	public override void TriggerExitHandler(Collider other)
	{
		
	}
}

