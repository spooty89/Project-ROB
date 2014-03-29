using System;
using System.Collections;
using UnityEngine;

/* Orang Juice text by Brittney Murphy */

public class ROBgui : MonoBehaviour
{
	public GUISkin gui;
	public int tokens = 0,
				totalTokens;
	public bool tokenRetrieve = false,
				messagePresent = true;

	private CharacterClass Player;
	
	private float fractionOfScreenWidth = 4f,// 0.15f; // 50% of screen width by default
					barHeight = 20f, // 50 px height
					maxHP,
					currentHP,
					hpFraction;
	private String message,
					textField = "Enter text here...";
	Vector2 screenSize;
	GUIStyle healthBar, tokenStyle, messageStyle;
	CoRoutine screenSizeChecker;

	public void Awake () {
		checkScreenSize();

		totalTokens = GameObject.FindGameObjectsWithTag("Token").Length;
		message = "Press W/A/S/D to walk fwrd/left/bkwrd/right. Hold shift to run.\nMove the mouse to rotate the camera!";
		Player = GameObject.FindObjectOfType<CharacterClass>().GetComponent<CharacterClass>();
		message += Player.name;
		
		tokenStyle = gui.GetStyle("Token");
		messageStyle = gui.GetStyle("Message");

		//Health Bar
		healthBar = gui.GetStyle("HealthBar");
		healthBar.normal.background = new Texture2D(1, 1);
		healthBar.stretchHeight = true; healthBar.stretchWidth = true;
		Player.healthChange += ()=> setHealth();
		setHealth();
	}
	
	public void OnGUI () {

		if (tokenRetrieve)
			GUI.Label (new Rect ((screenSize.x / 60), (screenSize.y / 40), (screenSize.x / 20), (screenSize.y / 10)),  "1-Ups: " + tokens + "/" + totalTokens, tokenStyle);
		GUI.Label (new Rect (0, (screenSize.y - (screenSize.y / 10)), screenSize.x, (screenSize.y / 10)), message, messageStyle);

		adjustHealthBar();

		//textfield that apparently lets the user type in stuff as the game is played, not sure where this will be used in the future
		textField = GUI.TextField (new Rect (screenSize.x*3/10,screenSize.y-screenSize.y/5,screenSize.x*7/10-screenSize.x*3/10,screenSize.y/5), textField);
	}

	//called by healthChange() of CharacterClass whenever pav's health changes
	public void setHealth(){
		currentHP = Player.getHealth(); 
		maxHP = Player.getMaxHealth();
		hpFraction = currentHP/maxHP;
		healthBar.normal.background.SetPixel(0, 0, Color.Lerp( Color.red, Color.green, hpFraction));
		healthBar.normal.background.Apply();
	}

	//code to draw the health bar
	private void adjustHealthBar(){
		GUI.Box(new Rect(100, 30, screenSize.x/fractionOfScreenWidth, barHeight), "Health"); // bottom texture
		GUI.Box(new Rect(100, 30, screenSize.x/fractionOfScreenWidth * hpFraction, barHeight), "", healthBar); // top texture, additionally scaled with health
	}
	
	public void messageSet (String words) {
		message = words;	
	}

	void checkScreenSize()
	{
		if( screenSize.x != Screen.width || screenSize.y != Screen.height ) screenResize();
		screenSizeChecker = CoRoutine.AfterWait( 1f, checkScreenSize );
	}

	void screenResize()
	{
		screenSize.x = Screen.width;
		screenSize.y = Screen.height;
	}
}

