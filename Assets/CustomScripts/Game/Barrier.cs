using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp
{
	public class Barrier : MonoBehaviour
	{
		private ROBgui gui;
		//public List <GameObject> tokens;
		public int tokens;
		
		/*private void Awake () {
			gui = GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>();
			gui.totalTokens = tokens;
		}
		
		private void Update () {
			if (gui.tokens == gui.totalTokens) {
				gui.tokenRetrieve = false;
				Destroy(this.gameObject);
			}
		}*/
	}
}

