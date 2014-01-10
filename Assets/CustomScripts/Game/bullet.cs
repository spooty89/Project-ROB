using UnityEngine;
using System.Collections;

public class bullet : MonoBehaviour
{
	public float speed;
	public Vector3 direction;
	public float maxTime;
	[HideInInspector]
	public CoRoutine destroyOnTime;

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
}

