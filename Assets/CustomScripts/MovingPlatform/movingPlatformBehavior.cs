using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class movingPlatformBehavior : MonoBehaviour {
	public MovingPlatform movingPlatform;
	public List<GameObject> wayPoints;
	public float speed = 3;
	public float rotationSpeed = 45;
	public bool rotateAround = false;
	public bool maintainUp = false;
	public bool enable = false;

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
	}
}