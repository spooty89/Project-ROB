#pragma strict

function Start () {

}

function Update () {

}

/*
 * class is abstract. user isn't expected to instantiated the class as an object.
 * If the user does so, behaviour is undefined.
 */
public class Ammo implements AmmoInterface{
	
	protected var bulletCount : int;
	protected var magLimit : int;
	protected var bulletType : String;
	
	public function getType() : String{
		return this.bulletType;
	}
	
	public function getMagLimit() : int{
		return this.magLimit;
	}

	
	public function getBulletCount() : int{
		return this.bulletCount;
	}
	
		
	public function decrementBulletCount() : boolean{
		if (this.bulletCount == 0)
			return false;
		else if (this.bulletCount < 0){
			Debug.Log("ERROR bullet count < 0 in Ammo\n");
			return false;
		}
		else{
			this.bulletCount--;
			return true;
		}
			
	}
	
	public function addBullets(numBullets : int) : void{
		var newBulletCount : int = this.bulletCount + numBullets;
		if (newBulletCount > this.magLimit)
			newBulletCount = this.magLimit;
		this.bulletCount = newBulletCount;
	}
	
	/***************************************
	********    must be OVERRIDEN   ********
	***************************************/
	public function createProjectile(bulletPref : GameObject, target : Vector3) : Projectile{
	}
	
	//Override
	override public function Equals(obj : Object) : boolean{
		if (!(obj instanceof Ammo)){
			return false;
		}
		else{
			var other : Ammo = obj as Ammo;
			return this.bulletType.Equals(other.bulletType);
		}
	}
	
	//Override
	override public function GetHashCode() : int{
		return this.bulletType.GetHashCode();
	}

}