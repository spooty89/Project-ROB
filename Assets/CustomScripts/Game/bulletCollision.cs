using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class bulletCollision : MonoBehaviour
{
	public List<int> layerMask;
	void OnTriggerEnter(Collider hit) {
		if(hit.gameObject.CompareTag( "bulletTrigger" ))
		{
			//Debug.Log("activate");
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