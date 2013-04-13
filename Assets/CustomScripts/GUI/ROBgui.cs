using System;
using UnityEngine;

/* Orang Juice text by Brittney Murphy */

namespace AssemblyCSharp
{
	public class ROBgui : MonoBehaviour
	{
		public GUIStyle tokenStyle;
		public GUIStyle messageStyle;
		public GUIStyle congradulationsStyle;
		public int tokens = 0;
		public int totalTokens;
		public String message;
		public Boolean gameFinished = false;
		
		public void Awake () {
			totalTokens = GameObject.FindGameObjectsWithTag("Token").Length;
			message = "W/A/S/D = fwrd/left/bkwrd/aright\nMove the mouse to rotate the camera\nCollect all tokens to unlock the prize...";
		}
		
		public void OnGUI () {
			if (!gameFinished) {
				GUI.Label (new Rect ((Screen.width / 60), (Screen.height - (Screen.height / 10)), (Screen.width / 20), (Screen.height / 10)), 
							"1-Ups: " + tokens + "/" + totalTokens, tokenStyle);
				GUI.Label (new Rect (Screen.width - (2 * Screen.width / 5), (Screen.height - (Screen.height / 6)), (2 * Screen.width / 5), (Screen.height / 6)), message, messageStyle);
			}
			else {
				GUI.Label (new Rect ((Screen.width / 2), (Screen.height / 2), (Screen.width / 20), (Screen.height / 10)), 
							"CONGRADULATIONS!\nYOU GOT THE THREE-FOURTHS\n... OF A TRIANGLE", congradulationsStyle);
			}
		}
	}
}

