using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class bulletCollision : MonoBehaviour
{
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
		gameObject.GetComponent<bullet>().destroyOnTime.Stop();
		hit.SendMessage("activate", SendMessageOptions.DontRequireReceiver);
		Destroy(gameObject);
	}
}