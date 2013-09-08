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
			else if (_Player.verticalSpeed <= -15.0){
				_Player.verticalSpeed = (float)-15.0;
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
			if (_Player.doubleJumping)
				_Player.inAirVelocity += targetDirection.normalized * Time.deltaTime * _Player.jumpAcceleration;
			else
				_Player.inAirVelocity += targetDirection.normalized * Time.deltaTime * _Player.doubleJumpAcceleration;
		}
		
		transform.rotation = Quaternion.LookRotation(_Player.moveDirection);
	}
	
	
	public void ApplyJump ()
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
			stateChange("idle");
		
		else if (_Player.verticalSpeed < 0.0f && _Player.climbContact && Vector3.Angle(hit.normal, transform.forward) > 100f) {// If player is within climb triggerBox
			_Player.wallFacing = hit.normal;
			_Player.moveDirection = -_Player.wallFacing;
			transform.rotation = Quaternion.LookRotation(_Player.moveDirection);
			_Player.wallRight = transform.right;
			
				stateChange("climb_idle");
				_Player.climbing = true;
				_Player.moveSpeed = (float)1.0;
				_Player.inAirVelocity = Vector3.zero;
				_Player.jumping = false;
				_Player.doubleJumping = false;
		}
	}
}