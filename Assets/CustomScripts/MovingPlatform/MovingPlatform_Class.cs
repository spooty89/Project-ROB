using System;
using UnityEngine;
using System.Collections.Generic;

public class MovingPlatform : Platform, MovingObjectInterface{
	private float speed;
	private List<GameObject> wayPoints;
	private int wayPointIndex = 0;
	private Vector3 prevPos;
	private Quaternion rotationDeg;
	private float rotationSpeed;
	public bool playerContact = false;
	public bool maintainUp = false;

	public MovingPlatform(Transform transform, float speed, float rotationSpeed, List<GameObject> wayPoints) : base(transform) {
		this.speed = speed;
		this.rotationSpeed = rotationSpeed;
		this.wayPoints = wayPoints;
		this.prevPos = this.transform.position;
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
		this.prevPos = this.transform.position;
		this.transform.position = Vector3.MoveTowards(this.transform.position, this.wayPoints[wayPointIndex].transform.position,  Time.deltaTime * this.speed);
	}
	
	public void rotate(){
		Vector3 oldEuler = this.transform.eulerAngles;
		this.transform.Rotate(Vector3.up * Time.deltaTime * this.rotationSpeed);
		this.rotationDeg = Quaternion.Inverse(Quaternion.FromToRotation( oldEuler, this.transform.eulerAngles ));
	}
	
	public void rotateAround(){
		Vector3 oldEuler = this.transform.eulerAngles;
		this.prevPos = this.transform.position;
		this.transform.RotateAround(this.wayPoints[this.wayPointIndex].transform.position, this.wayPoints[wayPointIndex].transform.up, Time.deltaTime * this.rotationSpeed);
		if (maintainUp)
			this.transform.up = Vector3.up;
		this.rotationDeg = Quaternion.Inverse(Quaternion.FromToRotation( oldEuler, this.transform.eulerAngles ));
	}
	
	public void rotateOnPlatform(Transform player) {
		player.RotateAround(this.transform.position, this.transform.up, Time.deltaTime * this.rotationSpeed);
		//if (controller.velocity.sqrMagnitude < 0.1)
		//{
		player.Rotate( this.rotationDeg.eulerAngles );
		//}*/
	}
	
	public void rotateAroundOnPlatform(Transform player) {
		player.RotateAround(this.wayPoints[this.wayPointIndex].transform.position, this.wayPoints[wayPointIndex].transform.up, Time.deltaTime * this.rotationSpeed);
		//if (controller.velocity.sqrMagnitude < 0.1)
		//{
		player.Rotate( this.rotationDeg.eulerAngles );
		//}*/
	}
	
	public Vector3 getDeltaPos() {
		Vector3 returnVector = Vector3.zero;
		returnVector = this.transform.position - this.prevPos;
		return returnVector;
	}
}