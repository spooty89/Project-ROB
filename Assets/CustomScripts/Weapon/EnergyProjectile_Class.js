#pragma strict

function Start () {

}

function Update () {

}

public class EnergyProjectile extends Projectile{

	public function EnergyProjectile(bulletPref : GameObject, target : Vector3){
		super(bulletPref, target);
		this.speed = 30;
		this.damage = 50;
	}
}