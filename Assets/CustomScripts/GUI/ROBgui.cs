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

	private CharacterClass Player;
	
	private float fractionOfScreenWidth = 0.15f; // 50% of screen width by default
	
	private float barHeight = 20f; // 50 px height

	private float maxHP;

	private float currentHP;
	
	private String textField = "Enter text here...";
	public void Awake () {
		totalTokens = GameObject.FindGameObjectsWithTag("Token").Length;
		message = "Press W/A/S/D to walk fwrd/left/bkwrd/right. Hold shift to run.\nMove the mouse to rotate the camera!";
		Player = GetComponent<CharacterClass>();
		message += Player.name;
		maxHP = GameObject.FindObjectOfType<CharacterClass> ().GetComponent<CharacterClass> ().getMaxHealth ();
		currentHP = GameObject.FindObjectOfType<CharacterClass> ().GetComponent<CharacterClass>().getHealth();
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
			adjustHealthBar();
			if (aiming) {
				GUI.Label (new Rect (Screen.width/2, Screen.height/2, 3, 3), "+", aimStyle);
			}

			//textfield that apparently lets the user type in stuff as the game is placed, not sure where this will be used in the future
			textField = GUI.TextField (new Rect (Screen.width*3/10,Screen.height-Screen.height/5,Screen.width*7/10-Screen.width*3/10,Screen.height/5), textField);
		}
		else {
			GUI.Label (new Rect ((Screen.width / 2), (Screen.height / 2), (Screen.width / 20), (Screen.height / 10)), 
						"CONGRADULATIONS!\nYOU GOT THE THREE-FOURTHS\n... OF A TRIANGLE", congradulationsStyle);
		}
	}

	//called by healthChange() of CharacterClass whenever pav's health changes
	public void setHealth(){
		currentHP = GameObject.FindObjectOfType<CharacterClass> ().GetComponent<CharacterClass>().getHealth(); 
	}

	//code to draw the health bar
	private void adjustHealthBar(){
		float max = Math.Max(maxHP,1);	//in case maxHP is set to 0 for whatever reason
		float health = Mathf.Clamp(currentHP/maxHP, 0f, 1f); //just in case
		float w = Screen.width * fractionOfScreenWidth; // width scaled with screen size
		GUIStyle style = new GUIStyle (GUI.skin.box);
		style.normal.background = MakeTex (100, 100, health*Color.green+(1-health)*Color.red); //color approaches red as pav's HP goes down
		GUI.Box(new Rect(100, 30, w, barHeight), "Health"); // bottom texture

		GUI.Box(new Rect(100, 30, w * health, barHeight), "", style); // top texture, additionally scaled with health

	}	

	//creates the texture for the health bar
	private Texture2D MakeTex( int width, int height, Color col ){
		Color[] pix = new Color[width * height];
		for( int i = 0; i < pix.Length; ++i ){
			pix[ i ] = col;
		}
		Texture2D result = new Texture2D( width, height );
		result.SetPixels( pix );
		result.Apply();
		return result;
	}

	public void aimSet (bool aim) {
		aiming = aim;	
	}
	
	public void messageSet (String words) {
		message = words;	
	}
}

