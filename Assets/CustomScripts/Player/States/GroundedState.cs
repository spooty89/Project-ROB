using UnityEngine;

public class GroundedState : StateClass
{
	private bool isMoving;
	private float v, h;
	private float walkSpeed = (float)4.0;	// The speed when walking
	private float runSpeed = (float)8.0;	// When pressing Shift button we start running
	
	public override void Run()
	{
		InputHandler();
		if(_Player.IsGrounded())
			MovementHandler();
		else if (!_Player.jumping)
			_Player.currentState = "jump_after_apex";
	}
	
	
	private void InputHandler()
	{
		if( Input.anyKey)
		{
			v = Input.GetAxisRaw("Vertical");
			h = Input.GetAxisRaw("Horizontal");
			
			isMoving = Mathf.Abs (h) > 0.05f || Mathf.Abs (v) > 0.05f;
			
			if( Input.GetButton( "Jump" ) )
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
		_Player.inAirVelocity = Vector3.zero;
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
			_Player.moveDirection = Vector3.RotateTowards(_Player.moveDirection, targetDirection, _Player.rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
			_Player.moveDirection = _Player.moveDirection.normalized;
			
		}
		
		// Smooth the speed based on the current target direction
		float curSmooth = _Player.speedSmoothing * Time.deltaTime;
		
		// Choose target speed
		float targetSpeed = Mathf.Min(targetDirection.magnitude, (float)1.0);	//* Support analog input but insure you cant walk faster diagonally than just f/b/l/r
	
		if (!isMoving)
		{
			_Player.currentState = "idle";
		}
		else{
			// Pick speed modifier
			if (Input.GetKey (KeyCode.LeftShift))
			{
				targetSpeed *= runSpeed;
				_Player.currentState = "run";
			}
			else
			{
				targetSpeed *= walkSpeed;
				_Player.currentState = "walk";
			}
		}
		
		_Player.moveSpeed = Mathf.Lerp(_Player.moveSpeed, targetSpeed, curSmooth);
		
		if (_Player.moveDirection == Vector3.zero) 
			_Player.moveDirection = _Player.wallFacing;
		
		transform.rotation = Quaternion.LookRotation(_Player.moveDirection);
	}
	
	
	public void ApplyJump ()
	{
		_Player.verticalSpeed = _Player.CalculateJumpVerticalSpeed (_Player.jumpHeight);
		_Player.currentState = "jump";
		_Player.jumping = true;
	}
}



