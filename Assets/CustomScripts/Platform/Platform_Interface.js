#pragma strict

function Start () {

}

function Update () {

}

public interface PlatformInterface{
	//Returns the position of the transform of the platform
	function getPosition():Vector3;
	
	//Sets the position of the transform of the platform
	function setPosition(x:float,y:float,z:float):void;
	
	//Returns true if the sides of the platform is climbable, false otherwise
	function isClimbable():boolean;
	
	//Sets the sides of the platform to be climbable
	function setClimbable():void;
	
	//Sets the sides of the platform to be not climbable
	function setNonClimbable():void;
}