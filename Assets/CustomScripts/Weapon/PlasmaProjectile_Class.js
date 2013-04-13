#pragma strict

function Start () {

}

function Update () {

}

public class PlasmaProjectile extends Projectile{

	public function PlasmaProjectile(bulletPref : GameObject, target : Vector3){
		super(bulletPref, target);
		this.speed = 60;
		this.damage = 20;
	}
}