#pragma strict

function Start () {

}

function Update () {

}

public interface MovingObjectInterface{
	//gets the speed of the moving object
	function getSpeed():float;
	
	//sets the speed of the moving object
	function setSpeed(speed:float):void;
	
	/*adds a way point for the moving object to move to. A moving object will move to
	 *way points in the sequence that they are added
	 */
	//function addWayPoint(wayPoint:Vector3):void;
	
	//removes the last added waypoint, returns null if no waypoints were added
	//function popWayPoint():Vector3;
	
	/*returns the current way point that the platform will move to.
	 *will return the current position if no waypoint is added
	 */
	function getCurrentWayPoint():Vector3;
	
	//returns the number of waypoints of the moving object
	function numWayPoints():int;
	
	//move the object to a step towards its waypoint,
	//if no waypoint is defined the object will not move
	function move():void;
	
}