using UnityEngine;
using System.Collections;

public class bullet : MonoBehaviour
{
	public float speed;
	public Vector3 destination;

	private Vector3 position
	{
		get
		{
			return gameObject.transform.position;
		}
		set
		{
			gameObject.transform.position = value;
			if(gameObject.transform.position == destination)
				Destroy(gameObject);
		}
	}
	void Update ()
	{
		Debug.Log(position);
		position = Vector3.MoveTowards( position, destination, speed * Time.deltaTime );
	}
}

