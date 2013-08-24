#pragma strict

function Start () {

}

function Update () {

}

/*
 * Although ammo is abstract, terminology is used with a gun as an
 * analogy for better understanding. In this case, ammo refers to the
 * entire magazine not the individual bullets
 */
public interface AmmoInterface{

	//returns the type of the ammo
	function getType() : String;
	
	//returns the maximum amt of bullets the magazine
	//can hold
	function getMagLimit() : int;
	
	//returns the number of bullets 
	function getBulletCount() : int;
	
	//decrements the bullet count. return true if there
	//are still bullets remaining and false otherwise
	function decrementBulletCount() : boolean;
	
	//add bullets to the magazine
	function addBullets(numBullets : int) : void;
	
	//creates a projectile based on the target given
	function createProjectile(transform : Transform, target : Vector3) : Projectile;
	
}