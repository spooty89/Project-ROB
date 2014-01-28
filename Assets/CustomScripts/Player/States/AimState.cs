using UnityEngine;

public class AimState : StateClass
{
	public AnimationClip aimUpDown;
	public string leftAnim, rightAnim, forwardAnim, backwardAnim;
	public GameObject bullet, bulletOrigin;
	public float maxDistance, bulletSpeed;
	public LayerMask layerMask;
	public float targetHeight = 1.0f;
	private float normalHeight, normalDistance;
	public GUIStyle aimStyle;

	private bool isMoving;
	private float v, h;
	public float walkSpeed = 3.0f;	// The speed when walking
	public float runSpeed = 6.0f;	// When pressing Shift button we start running
	private Vector3 surfaceUp = Vector3.up;
	private CustomCameraController camController;
	
	protected override void Awake()
	{
		base.Awake();
		camController = Camera.main.GetComponent<CustomCameraController>();
		normalHeight = camController.targetHeight;
		normalDistance = camController.normalDistance;
	}

	private void OnEnable()
	{
		animation[aimUpDown.name].layer = 2;
		camController.targetHeight = Mathf.Max( targetHeight * (camController.desiredDistance/normalDistance), normalHeight );
	}

	private void OnDisable()
	{
		animation[aimUpDown.name].layer = 0;
		camController.targetHeight = normalHeight;
	}

	public void OnGUI () 
	{
		GUI.Label (new Rect (Screen.width/2, Screen.height/2, 3, 3), "+", aimStyle);
	}

	public override void Run()
	{
		InputHandler();
		AimHandler();
		MovementHandler();
	}
	
	
	private void InputHandler()
	{
		v = Input.GetAxisRaw("Vertical");
		h = Input.GetAxisRaw("Horizontal");
			
		isMoving = Mathf.Abs (h) > 0.05f || Mathf.Abs (v) > 0.05f;

		if( Input.GetButtonUp( "Aim" ) )
		{
			_cc.moveDirection = Camera.main.transform.TransformDirection(Vector3.forward).normalized;
			_cc.stateChange( "idle" );
		}

		if( Input.GetButtonUp( "fire" ) )
		{
			GameObject bulletInstance = (GameObject)Resources.Load( "Prefab/" + bullet.name );
			bulletInstance.GetComponent<bullet>().speed = bulletSpeed;
			bulletInstance.GetComponent<bullet>().maxTime = maxDistance/bulletSpeed;

			RaycastHit hit;
			Ray r = Camera.main.ViewportPointToRay (new Vector3(0.5f,0.5f, 0f));
			Physics.Raycast( r.origin, r.direction, out hit, maxDistance, layerMask );
			if( hit.point != Vector3.zero)
			{
				r = new Ray( bulletOrigin.transform.position, hit.point - bulletOrigin.transform.position );
			}
			else
			{
				r = new Ray( bulletOrigin.transform.position, r.GetPoint(maxDistance) - bulletOrigin.transform.position );
			}
			
			bulletInstance.GetComponent<bullet>().direction = r.direction;
			Instantiate( bulletInstance, bulletOrigin.transform.position, Quaternion.LookRotation(r.direction) );
		}
		if( Input.GetButton( "Mouse ScrollWheel" ) )
		{
			camController.targetHeight = Mathf.Max( targetHeight * (camController.desiredDistance/normalDistance), normalHeight );
		}
	}
	
	
	private void AimHandler()
	{
		animation[aimUpDown.name].time = aimUpDown.length * (-camController.yDeg/(camController.yMaxLimit-camController.yMinLimit) + 0.5f);
		animation.Blend(aimUpDown.name);
	}
	
	
	private void MovementHandler()
	{
		if(_cc.transitioning){
			stateChange("transition");
			return;
		}		
		_cc.inAirVelocity = Vector3.zero;
		Transform cameraTransform = Camera.main.transform;
		
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);		// Forward vector relative to the camera along the x-z plane	
		forward.y = surfaceUp.z;
		forward = forward.normalized;
		
		
		Vector3 right = new Vector3(forward.z, 0, -forward.x);		// Right vector relative to the camera
		transform.forward = forward;
		Vector3 targetDirection = h * right + v * forward;					// Target direction relative to the camera
		
		
		// We store speed and direction seperately,
		// so that when the character stands still we still have a valid forward direction
		// moveDirection is always normalized, and we only update it if there is user input.
		if (targetDirection != Vector3.zero)
		{
			_cc.moveDirection = targetDirection; 			// Smoothly turn towards the target direction
			_cc.moveDirection = _cc.moveDirection.normalized;
		}
		
		float curSmooth = _cc.speedSmoothing * Time.deltaTime;			// Smooth the speed based on the current target direction
		float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);		// Support analog input but insure you cant walk faster diagonally than just f/b/l/r
		
		if (!isMoving)
		{
			_cc.SetCurrentState("aim_idle");
		}
		else{
			transform.rotation = Quaternion.LookRotation(new Vector3(forward.x, 0.0f, forward.z));
			if( Mathf.Abs( h ) > Mathf.Abs( v ) )
			{
				if( h > 0 ) { _cc.SetCurrentState( rightAnim ); }
				else { _cc.SetCurrentState( leftAnim ); }
			}
			else
			{
				if( v > 0 ) { _cc.SetCurrentState( forwardAnim ); }
				else { _cc.SetCurrentState( backwardAnim ); }
			}
			// Pick speed modifier
			if (Input.GetButton("Shift"))
			{
				targetSpeed *= runSpeed;
			}
			else
			{
				targetSpeed *= walkSpeed;
			}
		}
		
		_cc.moveSpeed = Mathf.Lerp(_cc.moveSpeed, targetSpeed, curSmooth);
		//_cc.inputMoveDirection = _cc.moveDirection * _cc.moveSpeed;


		_cc.inputMoveDirection = _cc.moveDirection * _cc.moveSpeed;
		_cc.movement.updateVelocity = _cc.movement.velocity;
		_cc.movement.updateVelocity = _cc.ApplyInputVelocityChange( _cc.movement.updateVelocity );
		
		if (!_cc.IsGrounded())
		{
			stateChange("jump_after_apex");
		}
	}


	public override void surroundingCollisionHandler()
	{

	}
	
	
	public override void topCollisionHandler()
    {
        
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_cc.IsGrounded())
		{
			surfaceUp = hit.normal;
		}
	}


	public override void TriggerEnterHandler(Collider other)
	{

	}


	public override void TriggerExitHandler(Collider other)
	{

	}
}



