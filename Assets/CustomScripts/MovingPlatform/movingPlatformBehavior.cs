using System;
using UnityEngine;
using System.Collections;
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
	
	void FixedUpdate () {
			this.movingPlatform.move();
			if (rotateAround)
				this.movingPlatform.rotateAround();
			else
				this.movingPlatform.rotate();
			if (movingPlatform.playerContact){
				if (rotateAround && !maintainUp)
					this.movingPlatform.rotateAroundOnPlatform(player);
			else
				this.player.position += this.movingPlatform.getDeltaPos();
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
	
	/*IEnumerator OnCollisionStay(Collision collision)
	{
		if(collision.collider.gameObject.transform.parent.gameObject.CompareTag( "Player" ))
		{
			movingPlatform.playerContact = true;
			this.player = collision.collider.gameObject.transform.parent;
			//Debug.Log(this.movingPlatform.getDeltaPos());
			//transferSpeed(collision.collider.gameObject.transform.parent);
		}

		yield return new WaitForFixedUpdate();
	}
	
	IEnumerator OnCollisionExit(Collision collision)
	{
		if(collision.collider.gameObject.transform.parent.gameObject.CompareTag( "Player" ))
		{
			Debug.Log("right here");
			movingPlatform.playerContact = false;
			this.player = null;
			//Debug.Log(this.movingPlatform.getDeltaPos());
			//transferSpeed(collision.collider.gameObject.transform.parent);
		}
		
		yield return new WaitForFixedUpdate();
	}*/
}