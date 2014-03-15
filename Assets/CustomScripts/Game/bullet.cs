using UnityEngine;
using System.Collections;

public class bullet : MonoBehaviour
{
	public Vector3 direction;
	public float speed;
	public float maxTime;
	public GameObject shooter;
	[HideInInspector]
	public CoRoutine destroyOnTime;

	public bullet( Vector3 Direction, GameObject Shooter )
	{
		new bullet( Direction, speed, maxTime, Shooter );
	}

	public bullet( Vector3 Direction, float Speed, GameObject Shooter )
	{
		new bullet( Direction, Speed, maxTime, Shooter );
	}

	public bullet( Vector3 Direction, float Speed, float MaxTime, GameObject Shooter )
	{
		direction = Direction;
		speed = Speed;
		maxTime = MaxTime;
		shooter = Shooter;
	}

	private Vector3 position
	{
		get
		{
			return gameObject.transform.position;
		}
		set
		{
			gameObject.transform.position = value;
		}
	}

	void Awake()
	{
		destroyOnTime = CoRoutine.AfterWait( maxTime, () => Destroy( this.gameObject ) );
	}
	void Update ()
	{
		position = Vector3.MoveTowards( position, position + direction * speed, speed * Time.deltaTime );
	}

	void OnTriggerEnter(Collider hit) {
		if(hit.gameObject.CompareTag( "bulletTrigger" ))
		{
			Hit( hit.gameObject );
		}
	}
	
	void OnCollisionEnter(Collision hit)
	{
		if( !hit.gameObject.CompareTag( "bulletIgnore" ) )
			Hit( hit.gameObject );
	}
	
	void Hit(GameObject hit)
	{
		Destroy(gameObject.GetComponent<Rigidbody>());
		destroyOnTime.Stop();
		hit.SendMessage("playerBulletHit", gameObject.GetComponent<bullet>().shooter, SendMessageOptions.DontRequireReceiver);
		Destroy(gameObject);
	}
}

