using UnityEngine;
using System.Collections;

public class EnemyAI_Base : MonoBehaviour
{	
	[System.Serializable]
	public class Range2D
	{
		public float low, high;
		[HideInInspector]
		public float range;
		public Range2D( float low, float high )
		{
			this.low = low;
			this.high = high;
			this.range = this.high - this.low;
		}
	}
	
	public enum State
	{
		Idle, Moving, Attacking, Dying
	}
	
	[System.Serializable]
	public class Movement {
		public bool canMove,
					canFly,
					alert,
					move,
					idle,
					grounded;
		public float moveSpeed = 0f,
					 maxSpeed = 1f,
					 speedSmoothing = 10f,
					 rotateSpeed;
		[HideInInspector]
		public Vector3 destination;
		public Range2D idleTime = new Range2D( 0.5f, 2f );
		[HideInInspector]
		public CollisionFlags collisionFlags; 
	}
	
	[System.Serializable]
	public class Attack {
		public bool enabled,
					attacking,
					defending,
					stunned,
					rangedAttack;
		public Vector3 target;
		public Range2D attackDistance = new Range2D( 1f, 2f );
		public float attackDamage = 1f;
		public Range2D cooldownTime = new Range2D( 2f, 4f );
	}
	
	public int	health,
				damageAmount;
	public Transform domain;
	public State state;
	public Movement movement;
	public Attack attack;
	
	protected CharacterController controller;
	private delegate void updateFunction();
	private updateFunction uFunction;
	
	
	
	void Awake()
	{
		controller = gameObject.GetComponent<CharacterController>();
		if(controller == null)
			controller = gameObject.AddComponent<CharacterController>();
		if(domain == null)
			domain = new GameObject("domain").transform;
		
		if( movement.canMove )
		{
			if( movement.canFly )
			{
				uFunction += () => airMovement();
			}
			else
			{
				uFunction += () => groundedMovement();
			}
		}
		else
		{
			
		}
			
		if( attack.rangedAttack )
		{
			uFunction += () => rangedAttack();
		}
		else
		{
			
		}
		
		if( !movement.idle )
		{
			Vector3 nextPosition = Random.insideUnitSphere;
			nextPosition.Scale( domain.localScale/2 );
			nextPosition += domain.position;
			movement.destination = nextPosition;
			movement.move = true;
		}
	}
	
	void Update()
	{
		uFunction();
	}
	
	
	void groundedMovement()
	{
		movement.destination.y = transform.position.y;
		if( movement.move )
		{
			Vector3 currentMovementOffset = movement.destination - transform.position;
			currentMovementOffset.Normalize();
			transform.rotation = Quaternion.LookRotation( Vector3.RotateTowards( transform.forward, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z), 			// Smoothly turn towards the target direction
															movement.rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000 ) );
			float curSmooth = movement.speedSmoothing * Time.deltaTime;
			
			movement.moveSpeed = Mathf.Lerp(movement.moveSpeed, movement.maxSpeed, curSmooth);
			currentMovementOffset *= movement.moveSpeed;
			
			movement.collisionFlags = controller.Move(currentMovementOffset);
			if( (transform.position - movement.destination).magnitude < movement.moveSpeed )
			{
				movement.move = false;
			}
		}
		else
		{
			if( !movement.idle )
			{
				movement.moveSpeed = 0f;
				movement.idle = true;
				Vector3 nextPosition = Random.insideUnitSphere;
				nextPosition.Scale( domain.localScale/2 );
				nextPosition = domain.rotation * nextPosition;
				nextPosition += domain.position;
				nextPosition.y = transform.position.y;
				CoRoutine.AfterWait( (movement.idleTime.range * Random.value) + movement.idleTime.low, () => 
				{
					movement.destination = nextPosition;
					movement.move = true;
					movement.idle = false;
				});
			}
		}
	}
	
	
	void airMovement()
	{
		if( movement.move )
		{
			Vector3 currentMovementOffset = movement.destination - transform.position;
			currentMovementOffset.Normalize();
			transform.rotation = Quaternion.LookRotation( Vector3.RotateTowards( transform.forward, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z), 			// Smoothly turn towards the target direction
															movement.rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000 ) );
			float curSmooth = movement.speedSmoothing * Time.deltaTime;
			
			movement.moveSpeed = Mathf.Lerp(movement.moveSpeed, movement.maxSpeed, curSmooth);
			currentMovementOffset *= movement.moveSpeed;
			
			movement.collisionFlags = controller.Move(currentMovementOffset);
			if( (transform.position - movement.destination).magnitude < movement.moveSpeed || movement.grounded )
			{
				movement.move = false;
			}
		}
		else
		{
			if( !movement.idle )
			{
				movement.moveSpeed = 0f;
				movement.idle = true;
				Vector3 nextPosition = Random.insideUnitSphere;
				nextPosition.Scale( domain.localScale/2 );
				nextPosition = domain.rotation * nextPosition;
				nextPosition += domain.position;
				CoRoutine.AfterWait( (movement.idleTime.range * Random.value) + movement.idleTime.low, () => 
				{
					movement.destination = nextPosition;
					movement.move = true;
					movement.idle = false;
					movement.grounded = false;
				});
			}
		}
	}
	
	
	void rangedAttack()
	{
		if(attack.enabled)
		{
			
		}
	}
	
	
	public void playerBulletHit()
	{
		if(!attack.defending)
			health -= damageAmount;
	}
	
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if(hit.normal.y > 0.1f && movement.canFly)
			movement.grounded = true;
	}
}

