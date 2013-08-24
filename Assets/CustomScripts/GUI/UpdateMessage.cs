using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class UpdateMessage : MonoBehaviour
	{
		public String text;
		
		void OnTriggerEnter (Collider hit) {
			if(hit.GetComponent<ROBgui>()) {
				hit.GetComponent<ROBgui>().messageSet(text);
				Destroy(this.gameObject);
			}
		}
	}
}

