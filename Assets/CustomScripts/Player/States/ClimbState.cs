using UnityEngine;

public class ClimbState : StateClass
{
	public override void Run()
	{
		InputHandler();
		MovementHandler();
	}
	
	private void InputHandler()
	{	
		float vertical = Input.GetAxisRaw("Vertical");
		float horizontal = Input.GetAxisRaw("Horizontal");
		if (Input.anyKey || vertical != 0f || horizontal != 0f)
		{
			// If one of the up/down buttons is pressed
			if (Input.GetButton ("Vertical") || vertical != 0f) {
				if(Input.GetAxis("Vertical") > 0)
				{
					_Player.SetCurrentState("climb_wall_up");
				}
				else
				{
					_Player.SetCurrentState("climb_wall_down");
				}
				_Player.verticalSpeed = 2.0f * Input.GetAxis("Vertical");
				if (Input.GetButton("Shift"))		// Climb faster
				{
					_Player.verticalSpeed *= 1.5f;
				}
			}
			else
			{
				_Player.verticalSpeed = 0.0f;
			}
			// If one of the left/right buttons is pressed
			if (Input.GetButton ("Horizontal") || horizontal != 0f)
			{
				if( Input.GetAxis("Horizontal") > 0 )
				{
					_Player.SetCurrentState("climb_wall_right");
				}
				else
				{
					_Player.SetCurrentState("climb_wall_left");
				}
				_Player.moveDirection += new Vector3(_Player.wallRight.x * Input.GetAxis("Horizontal"), _Player.wallRight.y * 
								 Input.GetAxis("Horizontal"), _Player.wallRight.z * Input.GetAxis("Horizontal"));
				_Player.moveDirection = _Player.moveDirection.normalized;
				_Player.moveSpeed = 2.0f;
				if (Input.GetButton("Shift"))
					_Player.moveSpeed *= 1.5f;
			}
			else
			{
				_Player.moveSpeed = 0.0f;
				_Player.moveDirection = transform.forward;
			}
			
			if (Input.GetButtonDown("Jump"))
			{
				_Player.moveDirection = _Player.wallFacing;
				_Player.moveDirection = _Player.moveDirection.normalized;
				_Player.moveSpeed = 8.0f;
				_Player.verticalSpeed = _Player.CalculateJumpVerticalSpeed( _Player.jumpHeight );
				_Player.doubleJumping = true;
				_Player.climbing = false;
				_Player.climbContact = false;
				stateChange("double_jump");
			}

			if( Input.GetButton( "Interact" ) )
			{
				_Player.verticalSpeed = -0.1f;
				_Player.transform.forward = _Player.wallFacing;
				_Player.climbing = false;
				_Player.transform.position += new Vector3(_Player.wallFacing.x * 0.25f, _Player.wallFacing.y * 0.25f, _Player.wallFacing.z * 0.25f);
				_Player.moveDirection = _Player.wallFacing;
				stateChange("jump_after_apex");
				_Player.jumpingReachedApex = true;
			}
		}
		else
		{
			_Player.moveSpeed = 0.0f;
			_Player.moveDirection = transform.forward;
			_Player.SetCurrentState("climb_wall_idle");
			_Player.inAirVelocity = Vector3.zero;
		}
	}
	
	private void MovementHandler()
	{
		if(_Player.transitioning){
			stateChange("transition");
			return;
		}		
		if (Mathf.Abs(_Player.verticalSpeed) < 0.5f)
		{
			_Player.verticalSpeed = 0.0f;
		}
		else
		{
			if (_Player.verticalSpeed < 0.0f)
				_Player.verticalSpeed += _Player.gravity * Time.deltaTime;
			else
				_Player.verticalSpeed -= _Player.gravity * Time.deltaTime;
		}
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_Player.IsGrounded() && _Player.verticalSpeed < 0.0f)
		{
			_Player.moveDirection = _Player.wallFacing;
			stateChange("idle");
		}
	}
}

