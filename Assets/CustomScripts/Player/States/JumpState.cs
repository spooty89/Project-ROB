using UnityEngine;

public class JumpState : StateClass
{
	private bool isMoving;
	private float v, h;


	void OnEnable()
	{
		//Debug.Log("jumpState");
	}
	
	protected override void Awake()
	{
		if( _Player == null )
		{
			_Player = GetComponent<CharacterClass>();
		}
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
		
		if( Input.anyKey)
		{	
			if( Input.GetButtonDown( "Jump" ) )
			{
				ApplyJump();
			}
		}
	}
	
	
	private void MovementHandler()
	{
		if(_Player.transitioning){
			stateChange("transition");
			return;
		}
		_Player.moveDirection.y = 0.0f;
		if (_Player.verticalSpeed > -15.0)
			_Player.verticalSpeed -= _Player.gravity * Time.deltaTime;
		// When we reach the apex
		if (_Player.verticalSpeed < -0.5f)
		{
			/*if( _Player.sTrigger.vertical )
			{
				Debug.Log("here");
				wallInteract();
			}
			else
			{*/
			//if (!_Player.jumpingReachedApex)
			//{
				_Player.jumpingReachedApex = true;
				_Player.SetCurrentState("jump_after_apex");
			/*}
			else*/ if (_Player.verticalSpeed <= -15.0f){
				_Player.verticalSpeed = -15.0f;
				//_Player.SetCurrentState("free_fall");
				}
			//}
		}
		
		
		Transform cameraTransform = Camera.main.transform;
		
		// Forward vector relative to the camera along the x-z plane	
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = 0.0f;
		forward = forward.normalized;
	
		// Right vector relative to the camera
		// Always orthogonal to the forward vector
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		Vector3 targetDirection = transform.forward;
		// Target direction relative to the camera
		targetDirection = h * right + v * forward;
		
	
		// We store speed and direction seperately,
		// so that when the character stands still we still have a valid forward direction
		// moveDirection is always normalized, and we only update it if there is user input.
		if (targetDirection != Vector3.zero)
		{
			// Smoothly turn towards the target direction
			_Player.moveDirection = Vector3.RotateTowards(_Player.moveDirection, targetDirection, _Player.rotationModifier * _Player.inAirRotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
			_Player.moveDirection = _Player.moveDirection.normalized;
		}
		
		if (isMoving) {
			if (!_Player.doubleJumping)
				_Player.inAirVelocity += targetDirection.normalized * Time.deltaTime * _Player.jumpAcceleration;
			else
				_Player.inAirVelocity += targetDirection.normalized * Time.deltaTime * _Player.doubleJumpAcceleration;
		}
		
		transform.rotation = Quaternion.LookRotation(_Player.moveDirection);
	}
	
	
	private void ApplyJump ()
	{
		if( !_Player.doubleJumping )
		{
			_Player.verticalSpeed = _Player.CalculateJumpVerticalSpeed (_Player.doubleJumpHeight);
			_Player.SetCurrentState("double_jump");
			_Player.jumpingReachedApex = false;
			_Player.doubleJumping = true;
		}
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_Player.IsGrounded() || Vector3.Angle(hit.normal, transform.up) < 30f)
		{
			_Player.rotationModifier = 1f;
			stateChange("idle");
		}
		/*else if( _Player.sTrigger.vertical && Vector3.Angle(hit.normal, transform.forward) > _Player.maxWallInteractAngle && Mathf.Abs(hit.normal.y) <= 0.1f)
		{
			_Player.wallFacing = hit.normal;
			_Player.wallRight = Vector3.Cross( _Player.wallFacing, transform.up );
			_Player.wallLeft = Quaternion.LookRotation(_Player.wallRight, transform.up) * Vector3.back;
			wallInteract( );
		}*/
		else if (_Player.hangContact && Vector3.Angle(hit.normal, transform.up) > 100f) {// If player is within hang triggerBox;
				stateChange("hang_idle");
				_Player.hanging = true;
				_Player.inAirVelocity = Vector3.zero;
				_Player.jumping = false;
				_Player.doubleJumping = false;
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
		if (_Player.verticalSpeed < -0.1f )
		{
			if (_Player.climbContact) {// If player is within climb triggerBox
				transform.rotation = Quaternion.LookRotation(_Player.wallFacing);
				Vector3 rotation = transform.rotation.eulerAngles;
				rotation = new Vector3( rotation.x, rotation.y + 180, rotation.z );
				transform.rotation = Quaternion.Euler( rotation );
				
				stateChange("climb_wall_idle");
				_Player.climbing = true;
				_Player.inAirVelocity = Vector3.zero;
				_Player.jumping = false;
				_Player.doubleJumping = false;
			}
			else
			{
				Debug.Log("jump here");
				float rightDiff = Mathf.Abs(Vector3.Angle( _Player.moveDirection, _Player.wallRight));
				float leftDiff = Mathf.Abs(Vector3.Angle( _Player.moveDirection, _Player.wallLeft));
				if( leftDiff < rightDiff )
				{
					if( leftDiff < 91f )
					{
						_Player.moveDirection = _Player.wallLeft;
						_Player.wallSlideDirection = (int)WallDirections.left;
					}
				}
				else
				{
					if( rightDiff < 91f )
					{
						_Player.moveDirection = _Player.wallRight;
						_Player.wallSlideDirection = (int)WallDirections.right;
					}
				}
				
				_Player.moveSpeed *= (90f - Mathf.Abs(Vector3.Angle( _Player.moveDirection, transform.forward ))) / 90f;
				transform.rotation = Quaternion.LookRotation(_Player.wallFacing);
				
				_Player.inAirVelocity = Vector3.zero;
				stateChange("wall_slide");
				_Player.wallSliding = true;
				_Player.jumping = false;
				_Player.doubleJumping = false;
			}
		}
	}
}