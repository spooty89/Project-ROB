using UnityEngine;

public class JumpState : StateClass
{
	private bool isMoving;
	private float v, h;
	
	public override void Run()
	{
		InputHandler();
		MovementHandler();
	}
	
	
	private void InputHandler()
	{
		if( Input.anyKey)
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
		if(_Player.transitioning){
			stateChange("transition");
			return;
		}
		_Player.moveDirection.y = 0.0f;
		if (_Player.verticalSpeed > -15.0)
			_Player.verticalSpeed -= _Player.gravity * Time.deltaTime;
		// When we reach the apex
		if (_Player.verticalSpeed < -1.0)
		{
			if (!_Player.jumpingReachedApex)
			{
				_Player.jumpingReachedApex = true;
				_Player.SetCurrentState("jump_after_apex");
			}
			else if (_Player.verticalSpeed <= -15.0f){
				_Player.verticalSpeed = -15.0f;
				_Player.SetCurrentState("free_fall");
			}
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
			_Player.moveDirection = Vector3.RotateTowards(_Player.moveDirection, targetDirection, _Player.inAirRotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
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
			_Player.doubleJumping = true;
		}
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_Player.IsGrounded())
		{
			stateChange("idle");
		}
		
		else if (Vector3.Angle(hit.normal, transform.forward) > 100f)
		{
			if (_Player.verticalSpeed < 0.0f )
			{
				if (_Player.climbContact) {// If player is within climb triggerBox
					_Player.wallFacing = hit.normal;
					_Player.moveDirection = -_Player.wallFacing;
					transform.rotation = Quaternion.LookRotation(_Player.moveDirection);
					_Player.wallRight = transform.right;
					
						stateChange("climb_idle");
						_Player.climbing = true;
						_Player.moveSpeed = 1.0f;
						_Player.inAirVelocity = Vector3.zero;
						_Player.jumping = false;
						_Player.doubleJumping = false;
				}
				else
				{
					_Player.wallFacing = hit.normal;
					transform.rotation = Quaternion.LookRotation(_Player.wallFacing);
					_Player.wallRight = new Vector3(hit.normal.z, hit.normal.y, -hit.normal.x);
					//_Player.moveSpeed = 0.0f;
					if(Vector3.Angle(_Player.wallRight, _Player.moveDirection) < 30f)
					{
						_Player.moveDirection = _Player.wallRight;
					}
					else if(Vector3.Angle(_Player.wallRight, _Player.moveDirection) > 150f)
					{
						_Player.moveDirection = new Vector3(-_Player.wallFacing.z, _Player.wallFacing.y, _Player.wallFacing.x);
					}
					/*else
					{
						_Player.moveDirection = Vector3.zero;
					}*/
						_Player.inAirVelocity = Vector3.zero;
					stateChange("wall_slide");
					_Player.wallSliding = true;
					_Player.jumping = false;
					_Player.doubleJumping = false;
				}
			}
		}
		else if (_Player.hangContact && Vector3.Angle(hit.normal, transform.up) > 100f) {// If player is within climb triggerBox;
				stateChange("hang_idle");
				_Player.hanging = true;
				_Player.inAirVelocity = Vector3.zero;
				_Player.jumping = false;
				_Player.doubleJumping = false;
		}
	}
}