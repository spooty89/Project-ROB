#pragma strict

function Start () {

}

function Update () {

}

public interface ProjectileInterface{

	//returns the speed of the projectile
	function getSpeed() : float;
	
	//returns the damage of the projectile
	function getDamage() : float;
	
	//returns true if the target is reached; returns false otherwise
	function reachedTarget() : boolean;
	
	//returns prefab
	function getPref() : GameObject;
}