using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class bulletCollision : MonoBehaviour
	{
		void OnTriggerEnter(Collider hit) {
			hit.gameObject.SendMessage("activate", SendMessageOptions.DontRequireReceiver);
		}
	}
}

