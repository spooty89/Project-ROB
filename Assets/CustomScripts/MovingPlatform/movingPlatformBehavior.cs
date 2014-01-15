using System;
using UnityEngine;
using System.Collections.Generic;

public class movingPlatformBehavior : MonoBehaviour {
	public MovingPlatform movingPlatform;
	public List<GameObject> wayPoints;
	private Transform player;
	public float speed = 3;
	public float rotationSpeed = 45;
	public bool rotateAround = false;
	public bool maintainUp = false;
	public bool enable = false;
	private CharacterController controller;
	
	void Awake () {
		this.movingPlatform = new MovingPlatform(this.transform, speed, rotationSpeed, wayPoints);
		this.movingPlatform.maintainUp = maintainUp;
		enabled = enable;
	}
	
	void Update () {
			this.movingPlatform.move();
			if (rotateAround)
				this.movingPlatform.rotateAround();
			else
				this.movingPlatform.rotate();
			if (movingPlatform.playerContact){
				this.player.position += this.movingPlatform.getDeltaPos();
				this.movingPlatform.rotateOnPlatform(player, controller);
			}
	}
	
	public void transferSpeed (Transform playerTransform) {
		this.player = playerTransform;
		this.movingPlatform.playerContact = true;
		this.controller = this.player.GetComponent<CharacterController>();
	}
		
	public void noContact() {
		this.movingPlatform.playerContact = false;
	}
}