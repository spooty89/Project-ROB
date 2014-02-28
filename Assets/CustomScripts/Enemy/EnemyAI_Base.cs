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
					 rotateSpeed = 300f;
		[HideInInspector]
		public Vector3 destination;
		public Range2D idleTime = new Range2D( 1.5f, 2.5f );
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
		public Transform target;
		public Vector3 attackPoint;
		public Range2D attackDistanceRange = new Range2D( 1f, 2f );
		public float attackDamage = 1f;
		public Range2D cooldownTime = new Range2D( 2f, 4f );
		[HideInInspector]
		public float attackDistance;
	}
	
	public int	health,
				damageAmount;
	public Transform domain;
	public State state;
	public Movement movement;
	public Attack attack;
	
	protected CharacterController controller;
	private delegate void updateFunction();
	private updateFunction moveFunction, attackFunction;
	
	
	
	void Awake()
	{
		controller = gameObject.GetComponent<CharacterController>();
		if(controller == null)
			controller = gameObject.AddComponent<CharacterController>();
		if(domain == null)
			domain = new GameObject("domain").transform;
		
		attack.attackDistance = attack.attackDistanceRange.low + Random.value * (attack.attackDistanceRange.high - attack.attackDistanceRange.low);

		if( movement.canMove )
		{
			if( movement.canFly )
			{
				movement.maxSpeed *= 2f/3f;
				moveFunction += () => airMovement();
			}
			else
			{
				moveFunction += () => groundedMovement();
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
			
		if( attack.rangedAttack )
		{
			attackFunction += () => rangedAttack();
		}
		else
		{
			attackFunction += () => hitAttack();
		}
	}
	
	void Update()
	{
		if( attack.enabled )
			attackFunction();
		else if( movement.canMove )
			moveFunction();
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
			currentMovementOffset = transform.forward;
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
				Vector3 nextPosition = getRandomDestination();
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
				Vector3 nextPosition = getRandomDestination();
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

	}
	
	
	void hitAttack()
	{
		if( Vector3.Distance( gameObject.transform.position, attack.target.transform.position ) > attack.attackDistance )
		{
			movement.destination = attack.target.transform.position;
			movement.move = true;
			movement.idle = false;
		}
		
	}
	
	
	public void detectZoneEntered( GameObject target )
	{
		if( target.CompareTag( "Player" ) )
		{
			attack.target = target.transform;
			attack.enabled = true;
		}
	}
	
	
	public void detectZoneExited( GameObject target )
	{
		if( target.CompareTag( "Player" ) )
		{
			attack.enabled = false;
		}
	}
	
	
	public void playerBulletHit( GameObject shooter )
	{
		if(!attack.enabled)
		{
			attack.enabled = true;
			attack.target = shooter.transform;
			health -= damageAmount;
		}
		else
		{
			if(!attack.defending)
				health -= damageAmount;
		}
	}
	
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if(hit.normal.y > 0.1f && movement.canFly)
			movement.grounded = true;
	}


	public Vector3 getRandomDestination()
	{
		Vector3 randomDestination = Random.insideUnitSphere;
		randomDestination.Scale( domain.localScale/2 );
		randomDestination = domain.rotation * randomDestination;
		randomDestination += domain.position;
		return randomDestination;
	}
}

