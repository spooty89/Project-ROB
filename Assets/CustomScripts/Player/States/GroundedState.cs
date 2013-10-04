using UnityEngine;

public class GroundedState : StateClass
{
	private bool isMoving, sliding = false;
	private float v, h;
	private float walkSpeed = 4.0f;	// The speed when walking
	private float runSpeed = 8.0f;	// When pressing Shift button we start running
	private Vector3 surfaceUp = Vector3.up;
	
	
	public override void Run()
	{
		InputHandler();
		if(!(_Player.jumping || sliding))
		{
			MovementHandler();
		}
		else if(sliding)
		{
			Slide();
		}
	}
	
	
	private void InputHandler()
	{
		if( Input.anyKey )
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
		
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);		// Forward vector relative to the camera along the x-z plane	
		forward.y = surfaceUp.z;
		forward = forward.normalized;
	

		Vector3 right = new Vector3(forward.z, 0, -forward.x);		// Right vector relative to the camera
		Vector3 targetDirection = transform.forward;				// Always orthogonal to the forward vector
		targetDirection = h * right + v * forward;					// Target direction relative to the camera
		
	
		// We store speed and direction seperately,
		// so that when the character stands still we still have a valid forward direction
		// moveDirection is always normalized, and we only update it if there is user input.
		if (targetDirection != Vector3.zero)
		{
			_Player.moveDirection = Vector3.RotateTowards(_Player.moveDirection, targetDirection, 			// Smoothly turn towards the target direction
															_Player.rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
			_Player.moveDirection = _Player.moveDirection.normalized;
		}
		
		float curSmooth = _Player.speedSmoothing * Time.deltaTime;			// Smooth the speed based on the current target direction
		float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);	//* Support analog input but insure you cant walk faster diagonally than just f/b/l/r
	
		if (!isMoving)
		{
			_Player.SetCurrentState("idle");
		}
		else{
			// Pick speed modifier
			if (Input.GetKey (KeyCode.LeftShift))
			{
				targetSpeed *= runSpeed;
				_Player.SetCurrentState("run");
			}
			else
			{
				targetSpeed *= walkSpeed;
				_Player.SetCurrentState("walk");
			}
		}
		
		_Player.moveSpeed = Mathf.Lerp(_Player.moveSpeed, targetSpeed, curSmooth);
		
		transform.rotation = Quaternion.LookRotation(new Vector3(_Player.moveDirection.x, 0.0f, _Player.moveDirection.z));
		
		if (!_Player.IsGrounded())
			stateChange("jump_after_apex");
	}
	
	
	private void Slide()
	{
		_Player.moveDirection = Vector3.RotateTowards(surfaceUp, Vector3.down, 1.5f, 0f);
		transform.rotation = Quaternion.LookRotation(_Player.moveDirection);
		_Player.moveSpeed = 10.0f;
	}
	
	
	private void ApplyJump ()
	{
		_Player.verticalSpeed = _Player.CalculateJumpVerticalSpeed (_Player.jumpHeight);
		_Player.jumping = true;
		stateChange("jump");
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_Player.IsGrounded())
		{
			surfaceUp = hit.normal;
			
			float angle = Vector3.Angle(surfaceUp, Vector3.up);
			if( angle > 55f && angle < 88f)
			{
				Debug.Log(angle);
				sliding = true;
			}
			else
			{
				sliding = false;
			}
		}
	}
}



