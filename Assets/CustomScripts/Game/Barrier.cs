using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class Barrier : MonoBehaviour
	{
		private ROBgui gui;
		
		private void Awake () {
			gui = GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>();
		}
		
		private void Update () {
			if (gui.tokens == gui.totalTokens) {
				gui.message = "The seal is broken! Collect your reward!";
				Destroy(this.gameObject);
			}
		}
	}
}

