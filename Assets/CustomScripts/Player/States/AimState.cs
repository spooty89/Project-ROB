using UnityEngine;

public class AimState : StateClass
{
	public AnimationClip aimUpDown;
	public string leftAnim, rightAnim, forwardAnim, backwardAnim;
	public GameObject bullet, bulletOrigin;
	public float maxDistance, bulletSpeed;
	public LayerMask layerMask;
	public float targetHeight = 1.0f;
	private float normalHeight;
	public GUIStyle aimStyle;

	private bool isMoving;
	private float v, h;
	private float walkSpeed = 3.0f;	// The speed when walking
	private float runSpeed = 6.0f;	// When pressing Shift button we start running
	private Vector3 surfaceUp = Vector3.up;
	private CustomCameraController camController;
	
	private void Awake()
	{
		camController = Camera.main.GetComponent<CustomCameraController>();
		normalHeight = camController.targetHeight;
	}

	private void OnEnable()
	{
		animation[aimUpDown.name].layer = 2;
		camController.targetHeight = targetHeight;
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

		if( Input.GetButtonDown( "Aim" ) )
		{
			_Player.moveDirection = Camera.main.transform.TransformDirection(Vector3.forward).normalized;
			_Player.stateChange( "idle" );
		}

		if( Input.GetButtonUp( "fire" ) )
		{
			RaycastHit hit;
			GameObject bulletInstance = (GameObject)Resources.Load( "Prefab/" + bullet.name );
			bulletInstance.GetComponent<bullet>().speed = bulletSpeed;

			if(Physics.Raycast( Camera.main.transform.position, Camera.main.transform.forward,
			                out hit, maxDistance, layerMask ))
			{
				bulletInstance.GetComponent<bullet>().destination = hit.point;
			}
			else
			{
				Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
				bulletInstance.GetComponent<bullet>().destination = ray.GetPoint( maxDistance );
			}
			Instantiate( bulletInstance, bulletOrigin.transform.position, bulletOrigin.transform.rotation );
		}
	}
	
	
	private void AimHandler()
	{
		animation[aimUpDown.name].time = aimUpDown.length * (-camController.yDeg/(camController.yMaxLimit-camController.yMinLimit) + 0.5f);
		animation.Blend(aimUpDown.name);
	}
	
	
	private void MovementHandler()
	{
		if(_Player.transitioning){
			stateChange("transition");
			return;
		}		
		_Player.inAirVelocity = Vector3.zero;
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
			_Player.moveDirection = targetDirection;/* Vector3.RotateTowards(_Player.moveDirection, targetDirection, 			// Smoothly turn towards the target direction
			                                              _Player.rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);*/
			_Player.moveDirection = _Player.moveDirection.normalized;
		}
		
		float curSmooth = _Player.speedSmoothing * Time.deltaTime;			// Smooth the speed based on the current target direction
		float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);	//* Support analog input but insure you cant walk faster diagonally than just f/b/l/r
		
		if (!isMoving)
		{
			_Player.SetCurrentState("aim_idle");
		}
		else{
			if( Mathf.Abs( h ) > Mathf.Abs( v ) )
			{
				if( h > 0 ) { _Player.SetCurrentState( rightAnim ); }
				else { _Player.SetCurrentState( leftAnim ); }
			}
			else
			{
				if( v > 0 ) { _Player.SetCurrentState( forwardAnim ); }
				else { _Player.SetCurrentState( backwardAnim ); }
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
		
		_Player.moveSpeed = Mathf.Lerp(_Player.moveSpeed, targetSpeed, curSmooth);
		
		transform.rotation = Quaternion.LookRotation(new Vector3(forward.x, 0.0f, forward.z));
		
		if (!_Player.IsGrounded())
		{
			stateChange("jump_after_apex");
		}
	}
	
	
	public override void CollisionHandler(ControllerColliderHit hit)
	{
		if(_Player.IsGrounded())
		{
			surfaceUp = hit.normal;
		}
	}
}



