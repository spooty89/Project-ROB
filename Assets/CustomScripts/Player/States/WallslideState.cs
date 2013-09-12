using UnityEngine;

public class WallslideState : StateClass
{
	private bool isMoving;
	private float v, h, wallslideGravity;
		
	
	protected override void Start()
	{
		_Player = GetComponent<CharacterClass>();
		wallslideGravity = _Player.gravity / 3f;
	}
	
	
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
		if (_Player.verticalSpeed > -15.0f)
		{
			_Player.verticalSpeed -= wallslideGravity * Time.deltaTime;
		}
	}
	
	
	private void ApplyJump ()
	{
		_Player.verticalSpeed = _Player.CalculateJumpVerticalSpeed (_Player.doubleJumpHeight);
		_Player.jumping = true;
		_Player.doubleJumping = true;
		stateChange("double_jump");
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_Player.IsGrounded())
		{
			_Player.moveDirection = _Player.wallFacing;
			stateChange("idle");
		}
	}
}

