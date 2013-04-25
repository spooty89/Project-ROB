using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class teleport : MonoBehaviour
	{
		public GameObject player;
		public GameObject tp_position;
		
		public void activate() {
			player.transform.position = tp_position.transform.position;
		}
	}
}

