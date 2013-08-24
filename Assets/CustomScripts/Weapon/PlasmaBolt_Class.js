#pragma strict

function Start () {

}

function Update () {

}

public class PlasmaBolt extends Ammo{

	public function PlasmaBolt(bulletCount : int){
		this.bulletType = "PlasmaBolt";
		this.magLimit = 20;
		if (bulletCount > this.magLimit)
			this.bulletCount = this.magLimit;
		else
			this.bulletCount = bulletCount;
	}
	
	//Override
	override public function createProjectile(bulletPref : GameObject, target : Vector3) : Projectile{
		return new PlasmaProjectile(bulletPref, target);
	}

}