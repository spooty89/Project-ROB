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
	
	function MovingPlatform(transform:Transform, speed:float, wayPoints){
		super(transform);
		this.speed = speed;
		this.wayPoints = wayPoints;
	}
	function getSpeed():float{
		return this.speed;
	}
	
	function setSpeed(speed:float):void{
		this.speed = speed;
	}
	
	/*function addWayPoint(wayPoint:Vector3):void{
		this.wayPoints.Add(wayPoint);
	}*/
	
	/*function popWayPoint():Vector3{
		var lastIndex:int = wayPoints.Count - 1;
		if (lastIndex < 0){
			throw System.Exception("No way points to pop");
		}
		var wayPoint :Vector3 = this.wayPoints[lastIndex];
		this.wayPoints.RemoveAt(lastIndex);
		return wayPoint;
	}*/

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
		this.transform.position = Vector3.MoveTowards(this.transform.position, this.wayPoints[wayPointIndex].transform.position,  Time.deltaTime * this.speed);
	}
}
