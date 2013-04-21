using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class bulletCollision : MonoBehaviour
	{
		void OnCollisionEnter(Collision hit) {
			Debug.Log("I'm here");
			hit.gameObject.SendMessage("activate", SendMessageOptions.DontRequireReceiver);
		}
	}
}

