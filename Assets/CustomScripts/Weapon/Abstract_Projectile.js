#pragma strict
import UnityEngine;

function Start () {

}

function Update () {

}

/*
 * class is abstract. user isn't expected to instantiated the class as an object.
 * If the user does so, behaviour is undefined.
 */
public class Projectile implements ProjectileInterface{

	private var bulletPref : GameObject;
	private var target : Vector3;
	protected var speed : float;
	protected var damage : float;
	
	protected function Projectile(transform : GameObject, target : Vector3){
		this.bulletPref = transform;
		this.target = target;
	} 
	
	function getSpeed() : float{
		return this.speed;
	}
	
	function getDamage() : float{
		return this.damage;
	}
	
	function reachedTarget() : boolean{
		if (this.bulletPref.transform.position == this.target)
			return true;
		return false;
	}
	
	function getPref() : GameObject {
		return this.bulletPref;
	}
	
	function move(){
		this.bulletPref.transform.position = Vector3.MoveTowards(this.bulletPref.transform.position, this.target,  Time.deltaTime * this.speed);
	}
	
}