using System;
using UnityEngine;

/* Orang Juice text by Brittney Murphy */

public class ROBgui : MonoBehaviour
{
	public GUIStyle tokenStyle;
	public GUIStyle messageStyle;
	public GUIStyle aimStyle;
	public GUIStyle congradulationsStyle;
	public int tokens = 0;
	public int totalTokens;
	private String message;
	public bool gameFinished = false;
	public bool tokenRetrieve = false;
	public bool messagePresent = true;
	private bool aiming = false;
	
	public void Awake () {
		totalTokens = GameObject.FindGameObjectsWithTag("Token").Length;
		message = "Press W/A/S/D to walk fwrd/left/bkwrd/right. Hold shift to run.\nMove the mouse to rotate the camera.";
	}
	
	public void OnGUI () {
		if (!gameFinished) {
			if (tokenRetrieve) {
				GUI.Label (new Rect ((Screen.width / 60), (Screen.height / 40), (Screen.width / 20), (Screen.height / 10)), 
						"1-Ups: " + tokens + "/" + totalTokens, tokenStyle);
			}
			//if (messagePresent) {
				GUI.Label (new Rect (0, (Screen.height - (Screen.height / 10)), Screen.width, (Screen.height / 10)), message, messageStyle); //(Screen.width/4, (Screen.height - (Screen.height / 10)), Screen.width / 2, (Screen.height / 10)), message, messageStyle);
			//}
			if (aiming) {
				GUI.Label (new Rect (Screen.width/2, Screen.height/2, 3, 3), "+", aimStyle);
			}
		}
		else {
			GUI.Label (new Rect ((Screen.width / 2), (Screen.height / 2), (Screen.width / 20), (Screen.height / 10)), 
						"CONGRADULATIONS!\nYOU GOT THE THREE-FOURTHS\n... OF A TRIANGLE", congradulationsStyle);
		}
	}
	
	public void aimSet (bool aim) {
		aiming = aim;	
	}
	
	public void messageSet (String words) {
		message = words;	
	}
}

