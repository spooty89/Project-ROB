#pragma strict

function Start () {

}

function Update () {

}

public class Platform implements PlatformInterface{

	protected var transform:Transform;
	
	function Platform(transform:Transform){
		this.transform = transform;
	}
	
	function getPosition():Vector3{
		return transform.position;
	}
	function setPosition(x:float, y:float, z:float):void{
		transform.position = new Vector3(x,y,z);
	}
	
	function isClimbable():boolean{
		return false;
	}
	
	function setClimbable():void{
		return;
	}
	
	function setNonClimbable():void{
		return;
	}
}