#pragma strict
import System.Collections.Generic;

function Start () {

}

function Update () {

}

/*
 * class is abstract. user isn't expected to instantiated the class as an object.
 * If the user does so, behaviour is undefined.
 */
public class AmmoStorage implements AmmoStorageInterface{

	//ammoDict and ammoList must contain same number of elements
	protected var ammoDict : Dictionary.<String,int>;
	protected var ammoList : List.<String>;

	protected function AmmoStorage(){
		this.ammoDict = new Dictionary.<String,int>();
		this.ammoList = new List.<String>();
	}

	//checks if the ammo storage has space
	/***************************************
	********    must be OVERRIDEN   ********
	***************************************/
	protected function storageHasSpace() : boolean{
		return false;
	}
	

	public function getNumAmmoTypes() : int{
		return this.ammoDict.Count;
	}
	
	private function createAmmo(ammoType : String, bulletCount : int) : Ammo{
		if (ammoType.Equals("EnergyBolt")){
			return new EnergyBolt(bulletCount);
		}
		else if (ammoType.Equals("PlasmaBolt")){
			return new PlasmaBolt(bulletCount);
		}
		else
			return null;
	}
	
	//any oldAmmo should already have a record in ammoDict
	public function swapAmmo(oldAmmo : Ammo) : Ammo{
		if (this.ammoList.Count == 0)
			return null;
		else{
			if(oldAmmo != null){
				var oldAmmoType = oldAmmo.getType();
				var oldBulletCount : int = oldAmmo.getBulletCount();
				ammoDict[oldAmmoType] = ammoDict[oldAmmoType] + oldBulletCount;
			}			
			var newAmmoType : String = this.ammoList[0];
			//move the new ammo to the back of the list. acts like a queue
			this.ammoList.RemoveAt(0);
			this.ammoList.Add(newAmmoType);
			var newBulletCount : int = this.ammoDict[newAmmoType];
			var newAmmo : Ammo = this.createAmmo(newAmmoType, 0);
			var newMagLimit : int = newAmmo.getMagLimit();
			/*
			 * Check if the amount of bullets in the storage is less than the
			 * magLimit of the ammo type. If it is less than the limit,
			 * simply use all the bullets. If it is more, deduct the magLimit
			 * from the count and store the remaining bullets
			 */
			if (newBulletCount < newMagLimit){
				this.ammoDict[newAmmoType] = 0;
				newAmmo.addBullets(newBulletCount);
				return newAmmo;
			}
			else{
				this.ammoDict[newAmmoType] = newBulletCount - newMagLimit;
				newAmmo.addBullets(newMagLimit);
				return newAmmo;
			}
		}
	}
	
	//any argument ammo should already have a record in ammoDict
	public function reloadAmmo(ammo : Ammo) : boolean{
		if (ammo == null)
			return false;
		else{
			var magLimit : int = ammo.getMagLimit();
			var ammoType : String = ammo.getType();
			var currBullets : int = ammo.getBulletCount();
			var storedBullets : int = this.ammoDict[ammoType];
			var totalBullets : int = currBullets + storedBullets;
		}	
		if (totalBullets == 0){
			//remove the ammo record if there are no more bullets
			this.ammoDict.Remove(ammoType);
			this.ammoList.Remove(ammoType);
			return false;
		}
		else if (totalBullets < magLimit){
			//if there are less bullets than the mag limit,
			//retrieve all the bullets
			ammo.addBullets(storedBullets);
			this.ammoDict[ammoType] = 0;
		}
		else{
			//if there are more bullets than the mag limit,
			//add the mag limit to ammo then put the remaining
			//in the storage
			ammo.addBullets(magLimit - currBullets);
			this.ammoDict[ammoType] = storedBullets - (magLimit - currBullets);
		}
		return true;
	}

	public function storeAmmo(ammo : Ammo) : boolean{
		var ammoType : String = ammo.getType();
		var bulletCount : int = ammo.getBulletCount();
		if (!storageHasSpace())
			return false;
		//checks if dictionary already contains this ammo type
		//if it does, increment the amt of ammo, if not insert
		//it into the dictionary with bulletCount amt of ammo
		if (this.ammoDict.ContainsKey(ammoType)){
			this.ammoDict[ammoType] = this.ammoDict[ammoType] + bulletCount;
		}
		else{
			this.ammoDict[ammoType] = bulletCount;
			this.ammoList.Add(ammoType);
		}
		return true;
	}
	
	
}