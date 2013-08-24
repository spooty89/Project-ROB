#pragma strict

function Start () {

}

function Update () {

}

public class EnergyBolt extends Ammo{

	public function EnergyBolt(bulletCount : int){
		this.bulletType = "EnergyBolt";
		this.magLimit = 50;
		if (bulletCount > this.magLimit)
			this.bulletCount = this.magLimit;
		else
			this.bulletCount = bulletCount;
	}
	
	//Override
	override public function createProjectile(bulletPref : GameObject, target : Vector3) : Projectile{
		return new EnergyProjectile(bulletPref, target);
	}

}