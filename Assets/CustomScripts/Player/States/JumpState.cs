using UnityEngine;

public class JumpState : StateClass
{
	private bool isMoving;
	private float v, h;
	
	public override void Run()
	{
		InputHandler();
		MovementHandler();
		if(_Player.IsGrounded())
			_Player.currentState = "idle";
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
		if (_Player.verticalSpeed > -15.0)
			_Player.verticalSpeed -= _Player.gravity * Time.deltaTime;
		// When we reach the apex
		if (_Player.verticalSpeed < -1.0)
		{
			if (!_Player.jumpingReachedApex)
			{
				_Player.jumpingReachedApex = true;
				_Player.currentState = "jump_after_apex";
			}
			else if (_Player.verticalSpeed <= -15.0){
				_Player.verticalSpeed = (float)-15.0;
				_Player.currentState = "free_fall";
			}
		}
		
		
		Transform cameraTransform = Camera.main.transform;
		
		// Forward vector relative to the camera along the x-z plane	
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = (float)0.0;
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
			_Player.currentState = "double_jump";
			_Player.doubleJumping = true;
		}
	}
}