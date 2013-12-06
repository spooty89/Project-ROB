using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyCSharp
{
	public class bulletCollision : MonoBehaviour
	{
		void OnTriggerEnter(Collider hit) {
			if(hit.gameObject.CompareTag( "bulletTrigger" ))
				Hit( hit.gameObject );
		}

		void OnCollisionEnter(Collision hit)
		{
			Hit( hit.gameObject );
		}

		void Hit(GameObject hit)
		{
			hit.SendMessage("activate", SendMessageOptions.DontRequireReceiver);
			Destroy(gameObject);
		}
	}
}

