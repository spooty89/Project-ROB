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
	[System.Serializable]
	public class Movement {
		public bool canMove,
					canFly,
					alert,
					move,
					idle,
					bumped;
		public float moveSpeed = 0f,
					 maxSpeed = 1f,
					 speedSmoothing = 10f,
					 rotateSpeed = 300f,
					 gravity = 10f;
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
					cooling,
					defending,
					stunned,
					rangedAttack;
		public Transform target;
		public Vector3 attackPoint;
		public Range2D attackDistanceRange = new Range2D( 1f, 2f );
		public float attackDamage = 1f, attackDelay = .25f;
		public Range2D cooldownTime = new Range2D( 2f, 4f );
		[HideInInspector]
		public float attackDistance,
					 coolDown;
	}
	
	public int	health,
				damageAmount;
	public Transform domain;
	public Movement movement;
	public Attack attack;
	
	protected CharacterController controller;
	public delegate void passGameObjectDelegate( GameObject gameobject );
	public passGameObjectDelegate dieFunction;
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
		attack.coolDown = attack.cooldownTime.low + Random.value * (attack.cooldownTime.high - attack.cooldownTime.low);

		if( movement.canMove )
		{
			if( movement.canFly )
			{
				movement.maxSpeed *= 1/2f;
				moveFunction += () => airMovement();
			}
			else
			{
				moveFunction += () => groundedMovement();
			}
		
			if( !movement.idle )
			{
				Vector3 nextPosition = Random.insideUnitSphere;
				nextPosition.Scale( domain.localScale/4 );
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
			baseAttack();
		if( movement.canMove )
			moveFunction();
	}
	
	
	void groundedMovement()
	{
		movement.destination.y = transform.position.y;

		Vector3 currentMovementOffset = movement.destination - transform.position;
		currentMovementOffset.Normalize();
		if( movement.move )
		{
			float curSmooth = movement.speedSmoothing * Time.deltaTime;
			
			movement.moveSpeed = Mathf.Lerp(movement.moveSpeed, movement.maxSpeed, curSmooth);
			currentMovementOffset *= movement.moveSpeed;
			currentMovementOffset += Vector3.down * movement.gravity * Time.deltaTime;
			
			movement.collisionFlags = controller.Move(currentMovementOffset);
			if( (transform.position - movement.destination).magnitude < movement.moveSpeed || movement.bumped )
			{
				movement.move = false;
			}
		}
		else if( !attack.enabled )
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
		currentMovementOffset.Normalize();
		transform.rotation = Quaternion.LookRotation( Vector3.RotateTowards( transform.forward, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z), 			// Smoothly turn towards the target direction
		                                                                    movement.rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000 ) );
	}
	
	
	void airMovement()
	{
		Vector3 currentMovementOffset = movement.destination - transform.position;
		currentMovementOffset.Normalize();
		transform.rotation = Quaternion.LookRotation( Vector3.RotateTowards( transform.forward, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z), 			// Smoothly turn towards the target direction
		                                                                    movement.rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000 ) );
		if( movement.move )
		{
			float curSmooth = movement.speedSmoothing * Time.deltaTime;
			
			movement.moveSpeed = Mathf.Lerp(movement.moveSpeed, movement.maxSpeed, curSmooth);
			currentMovementOffset *= movement.moveSpeed;
			/*if(attack.enabled)
				Debug.Log("x: " + currentMovementOffset.x + ", y: " + currentMovementOffset.y + ", z: " + currentMovementOffset.z);*/
			movement.collisionFlags = controller.Move(currentMovementOffset);
			if( (transform.position - movement.destination).magnitude < movement.moveSpeed || movement.bumped )
			{
				movement.move = false;
			}
		}
		else if( !attack.enabled )
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
					movement.bumped = false;
				});
			}
		}
	}
	
	
	void baseAttack()
	{
		movement.destination = attack.target.transform.position + attack.target.GetComponent<CharacterController>().center;
		if( !attack.attacking )
		{
			if( !attack.cooling && Vector3.Distance( gameObject.transform.position, movement.destination ) > attack.attackDistance )
			{
				movement.move = true;
				movement.idle = false;
			}
			else
			{
				movement.move = false;
				if( !attack.cooling )
				{
					attack.attackPoint = movement.destination;
					attack.attacking = true;
					CoRoutine.AfterWait( attack.attackDelay, () =>
						{
							attackFunction();
							attack.attacking = false;
							attack.cooling = true;
							Debug.Log("i attacked you!");
							CoRoutine.AfterWait( attack.coolDown, () => attack.cooling = false );
						} );
				}
			}
		}
	}

	void rangedAttack()
	{

	}

	void hitAttack()
	{

	}
	
	
	public void DomainEntered( GameObject target )
	{
		attack.target = target.transform;
		attack.enabled = true;
	}
	
	
	public void DomainExited( GameObject target )
	{
		attack.enabled = false;
		movement.idle = false;
		movement.move = false;
	}
	
	
	public void playerBulletHit( GameObject shooter )
	{
		if(!attack.defending)
		{
			health -= damageAmount;
			if( health <= 0 )
			{
				dieFunction(gameObject);
				Destroy(gameObject);
			}
		}

		if(!attack.enabled)
		{
			attack.enabled = true;
			attack.target = shooter.transform;
		}
	}
	
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( !hit.gameObject.CompareTag( "Enemy" ) && (movement.canFly || CollisionFlags.CollidedSides == 0 ) )
			movement.bumped = true;
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

