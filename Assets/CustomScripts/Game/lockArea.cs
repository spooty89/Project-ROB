using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp
{
	public class lockArea : MonoBehaviour
	{
		public List <GameObject> barriers;
		public GameObject player;
		
		void OnTriggerEnter (Collider hit) {
			if (hit.CompareTag("Player")) {
				for(int i = 0; i < barriers.Count; i++){
					barriers[i].SetActive(true);
				}	
				player.GetComponent<ROBgui>().tokenRetrieve = true;
				Destroy(this.gameObject);
			}
		}
	}
}

