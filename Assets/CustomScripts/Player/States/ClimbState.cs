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
				_Player.currentState = "climb_vertical";
				_Player.verticalSpeed = 2.0f * Input.GetAxis("Vertical");
				if (Input.GetKey (KeyCode.LeftShift))		// Climb faster
					_Player.verticalSpeed *= 2.0f;
			}
			// If one of the left/right buttons is pressed
			if (Input.GetButton ("Horizontal"))
			{
				_Player.currentState = "climb_horizontal";
				_Player.moveSpeed = 2.0f;
				_Player.moveDirection += new Vector3(_Player.wallRight.x * Input.GetAxis("Horizontal"), _Player.wallRight.y * 
								 Input.GetAxis("Horizontal"), _Player.wallRight.z * Input.GetAxis("Horizontal"));
				_Player.moveDirection = _Player.moveDirection.normalized;
				if (Input.GetKey (KeyCode.LeftShift))
					_Player.moveSpeed *= 2.0f;
			}
		}
		else
		{
			_Player.currentState = "climb_idle";
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
}

