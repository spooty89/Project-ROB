#pragma strict

function Start () {

}

function Update () {

}

/*
 * class is abstract. user isn't expected to instantiated the class as an object.
 * If the user does so, behaviour is undefined.
 */
public class Weapon implements WeaponInterface{
	
	protected var ammo : Ammo;
	protected var ammoStorage : AmmoStorage;
	protected var cooldown : float;
	protected var range : float;
	
	protected function Weapon(ammoStorage : AmmoStorage){
		this.ammoStorage = ammoStorage;
	} 
	
	public function getCooldown() : float{
		return this.cooldown;
	}
	
	public function getRange() : float{
		return this.range;
	}
	
	public function hasBulletsLeft() : boolean{
		if (this.ammo == null){
			return false;
		}
		else{
			return true;
		}
	}
	
	public function getBulletCount() : int{
		if (this.ammo == null){
			return 0;
		}
		else{
			return this.ammo.getBulletCount();
		}
	}
	
	public function getAmmoType() : String{
		if (this.ammo == null){
			return null;
		}
		else{
			return this.ammo.getType();
		}
	}
	
	public function switchAmmo() : boolean{
		this.ammo = this.ammoStorage.swapAmmo(this.ammo);
		if (this.ammo == null)
			return false;
		else
			return true;
	}
	
	public function storeAmmo(ammo : Ammo) : boolean{
		return this.ammoStorage.storeAmmo(ammo);
	}
	
	public function reload() : boolean{
		if (this.ammoStorage.reloadAmmo(this.ammo) == false){
			this.ammo = this.ammoStorage.swapAmmo(this.ammo);
		}
		if (this.ammo == null)
			return false;
		else
			return true;
	}
	
	
	public function shoot(prefabBullet : GameObject, targetPos : Vector3) : Projectile{
		if (this.ammo == null){
			//this.switchAmmo();
			return null;
		}
		else if (this.ammo.getBulletCount() == 0){
			reload();
		}
		else if (this.ammo.getBulletCount() < 0){
			Debug.Log("Error in Weapon, bullet count < 0\n");
		}
		if (this.ammo == null){
			return null;
		}
		else if(this.ammo.getBulletCount() < 0){
			//ammo should either be null or have ammo after a reload
			Debug.Log("Error in Weapon, bullet count < 0 after reload\n");
			return null;
		}
		else{
			this.ammo.decrementBulletCount();
			return this.ammo.createProjectile(prefabBullet, targetPos);
		}
	}
}