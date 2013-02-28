#pragma strict
import System.Collections.Generic;

function Start () {

}

function Update () {

}

public class MovingPlatform extends Platform implements MovingObjectInterface{
	private var speed:float;
	private var wayPoints:List.<GameObject>;
	private var wayPointIndex:int = 0;
	private var prevPos : Vector3;
	private var prevForward : Vector3;
	private var prevTime : float;
	private var rotationSpeed : float;
	public var playerContact : System.Boolean = false;
	
	function MovingPlatform(transform:Transform, speed:float, rotationSpeed:float, wayPoints){
		super(transform);
		this.speed = speed;
		this.rotationSpeed = rotationSpeed;
		this.wayPoints = wayPoints;
	}
	function getSpeed():float{
		return this.speed;
	}
	
	function setSpeed(speed:float):void{
		this.speed = speed;
	}

	function getCurrentWayPoint():Vector3{
		if (wayPoints.Count > 0)
			return wayPoints[this.wayPointIndex].transform.position;
		return this.transform.position;
	}
	
	function numWayPoints():int{
		return this.wayPoints.Count;
	}
	
	function move():void{	
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
	
	function rotate():void{
		this.prevForward = this.transform.forward;
		this.transform.Rotate(Vector3.up * Time.deltaTime * this.rotationSpeed);
	}
	
	function rotateOnPlatform(player : Transform, controller : CharacterController) {
		player.RotateAround(this.transform.position, this.transform.up, Time.deltaTime * this.rotationSpeed);
		if (controller.velocity.sqrMagnitude < 0.1)
			player.forward = this.transform.forward;
		
	}
	
	function getDeltaPos() : Vector3 {
		var returnVector : Vector3 = Vector3.zero;
		returnVector = this.transform.position - this.prevPos;
		return returnVector;
	}
}
