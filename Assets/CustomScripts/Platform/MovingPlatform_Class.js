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
	private var forwardDiff : float;
	private var lastForward : Vector3;
	private var prevTime : float;
	private var rotationSpeed : float;
	private var rotatePlayer :System.Boolean = false;
	public var playerContact : System.Boolean = false;
	
	function MovingPlatform(transform:Transform, speed:float, rotationSpeed:float, wayPoints:List.<GameObject>){
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
		this.transform.Rotate(Vector3.up * Time.deltaTime * this.rotationSpeed);
	}
	
	function rotateOnPlatform(player : Transform, controller : CharacterController) {
		var tempForward : Vector3;
		var tempTrans : Transform;
		var tempAngle : float;
		player.RotateAround(this.transform.position, this.transform.up, Time.deltaTime * this.rotationSpeed);
		if (controller.velocity.sqrMagnitude < 0.1)
		{
			if (!this.rotatePlayer){
				this.lastForward = this.transform.forward;
				this.rotatePlayer = true;
				tempAngle = Quaternion.Angle(this.transform.rotation, player.rotation);
				Debug.Log("Angle: " + tempAngle);
			}
			else {
				//this.forwardDiff = Vector3.Angle(this.transform.forward, this.lastForward);
				//if (this.forwardDiff > 1.0f){
					//this.lastForward = this.transform.forward;
					tempTrans = player;
					tempTrans.RotateAround(tempTrans.position, tempTrans.up, Time.deltaTime * this.rotationSpeed);
					//tempTrans = player;
					//Debug.Log("1: " + tempTrans.forward);
					//this.transform.forward = player.forward;
					//rotate();
					//tempTrans.Rotate(tempTrans.up * Time.deltaTime * this.rotationSpeed);
					//tempTrans.Rotate(Vector3.up, tempAngle * Mathf.Deg2Rad);
					//Debug.Log("2: " + tempTrans.forward);
					//Debug.Log("1: " + player.forward);
					player.forward = tempTrans.forward;
					//this.transform.forward = tempForward;
					//Debug.Log("3: " + this.transform.forward);
					//Debug.Log("2: " + player.forward);
					/*Debug.Log("Angle: " + Vector3.Angle(this.transform.eulerAngles, this.lastForward));
					Debug.Log("OForward: " + this.transform.eulerAngles);*/
					//Debug.Log("PForward: " + player.eulerAngles);
				//}
			}
		}
		else
			this.rotatePlayer = false;
	}
	
	function getDeltaPos() : Vector3 {
		var returnVector : Vector3 = Vector3.zero;
		returnVector = this.transform.position - this.prevPos;
		return returnVector;
	}
}
