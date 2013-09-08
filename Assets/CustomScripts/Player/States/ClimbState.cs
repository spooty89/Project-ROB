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
		if (Input.anyKey)
		{
			// If one of the up/down buttons is pressed
			if (Input.GetButton ("Vertical")) {
				_Player.SetCurrentState("climb_vertical");
				_Player.verticalSpeed = 2.0f * Input.GetAxis("Vertical");
				if (Input.GetKey (KeyCode.LeftShift))		// Climb faster
				{
					_Player.verticalSpeed *= 1.5f;
				}
			}
			else if (Input.GetButtonUp("Vertical"))
			{
				_Player.verticalSpeed = 0.0f;
				_Player.SetCurrentState("climb_idle");
			}
			// If one of the left/right buttons is pressed
			if (Input.GetButton ("Horizontal"))
			{
				_Player.SetCurrentState("climb_horizontal");
				_Player.moveDirection += new Vector3(_Player.wallRight.x * Input.GetAxis("Horizontal"), _Player.wallRight.y * 
								 Input.GetAxis("Horizontal"), _Player.wallRight.z * Input.GetAxis("Horizontal"));
				_Player.moveDirection = _Player.moveDirection.normalized;
				_Player.moveSpeed = 2.0f;
				if (Input.GetKey (KeyCode.LeftShift))
					_Player.moveSpeed *= 1.5f;
			}
			else if (Input.GetButtonUp("Horizontal"))
			{
				_Player.moveSpeed = 0.0f;
				_Player.moveDirection = transform.forward;
				_Player.SetCurrentState("climb_idle");
			}
			
			if (Input.GetButtonDown("Jump"))
			{
				_Player.moveDirection = _Player.wallFacing;
				_Player.moveDirection = _Player.moveDirection.normalized;
				_Player.moveSpeed = 8.0f;
				_Player.verticalSpeed = _Player.CalculateJumpVerticalSpeed( _Player.jumpHeight );
				stateChange("jump");
			}
		}
		else
		{
			_Player.moveSpeed = 0.0f;
			_Player.moveDirection = transform.forward;
			_Player.SetCurrentState("climb_idle");
			_Player.inAirVelocity = Vector3.zero;
		}
	}
	
	private void MovementHandler()
	{
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

