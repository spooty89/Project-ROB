using System;
using UnityEngine;
using System.Collections.Generic;

public class MovingPlatform : Platform, MovingObjectInterface{
	private float speed;
	private List<GameObject> wayPoints;
	private int wayPointIndex = 0;
	private float rotationSpeed;
	public bool playerContact = false;
	public bool maintainUp = false;

	public MovingPlatform(Transform transform, float speed, float rotationSpeed, List<GameObject> wayPoints) : base(transform) {
		this.speed = speed;
		this.rotationSpeed = rotationSpeed;
		this.wayPoints = wayPoints;
	}
	
	public float getSpeed(){
		return this.speed;
	}
	
	public void setSpeed(float speed){
		this.speed = speed;
	}

	public Vector3 getCurrentWayPoint(){
		if (wayPoints.Count > 0)
			return wayPoints[this.wayPointIndex].transform.position;
		return this.transform.position;
	}
	
	public int numWayPoints(){
		return this.wayPoints.Count;
	}
	
	public void move(){	
		if (this.numWayPoints() == 0)
			return;
		if (this.wayPoints[this.wayPointIndex].transform.position == this.transform.position){
			if(this.wayPointIndex == this.numWayPoints()-1)
				this.wayPointIndex = 0; //start from first way point if last at last waypoint
			else
				this.wayPointIndex++; 
		}
		this.transform.position = Vector3.MoveTowards(this.transform.position, this.wayPoints[wayPointIndex].transform.position,  Time.deltaTime * this.speed);
	}
	
	public void rotate(){
		Vector3 oldEuler = this.transform.eulerAngles;
		this.transform.Rotate(Vector3.up * Time.deltaTime * this.rotationSpeed);
	}
	
	public void rotateAround(){
		Vector3 oldEuler = this.transform.eulerAngles;
		this.transform.RotateAround(this.wayPoints[this.wayPointIndex].transform.position, this.wayPoints[wayPointIndex].transform.up, Time.deltaTime * this.rotationSpeed);
		if (maintainUp)
			this.transform.up = Vector3.up;
	}
}