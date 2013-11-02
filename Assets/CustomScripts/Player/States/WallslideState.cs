using UnityEngine;

public class WallslideState : StateClass
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
		if (Input.anyKey)
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
		if(_Player.transitioning){
			stateChange("transition");
			return;
		}		
		if( isMoving )
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
		}
		
		
		
		if (_Player.verticalSpeed > -5.0f)
		{
			_Player.verticalSpeed -= (_Player.gravity/2f) * Time.deltaTime;
		}
		else
		{
			_Player.verticalSpeed = -5.0f;
		}
		
		
		if(_Player.collisionFlags == 0)
		{
			stateChange("jump_after_apex");
		}	
	}
	
	
	public void ApplyJump ()
	{
		_Player.moveDirection = _Player.wallFacing;
		_Player.moveSpeed = 8.0f;
		_Player.verticalSpeed = _Player.CalculateJumpVerticalSpeed (_Player.doubleJumpHeight);
		_Player.doubleJumping = true;
		stateChange("double_jump");
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_Player.IsGrounded() && _Player.verticalSpeed < 0.0f)
		{
			_Player.moveSpeed = Vector3.Magnitude( _Player.inAirVelocity );
			stateChange("idle");
		}
	}
	
	/*void OnCollisionExit(Collision hit)
	{
		if((_Player.collisionFlags & CollisionFlags.CollidedSides) == 0)
		{
				Debug.Log("or here");
			stateChange("jump_after_apex");
		}
	}*/
}

