#pragma strict

function Start () {

}

function Update () {

}

public interface WeaponInterface{
	//returns the cooldown of the weapon
	function getCooldown() : float;
	
	//returns the range of the weapon
	function getRange() : float;
	
	//returns true if the weapon has bullets left, false otherwise
	function hasBulletsLeft() : boolean;
	
	//returns the amount of the current ammo type
	function getBulletCount() : int;
	
	//returns the type of the current ammo in the weapon
	function getAmmoType() : String;
	
	//switches the current ammo type to the next ammo type
	//in line. 
	function switchAmmo() : boolean;
	
	//adds ammo to the storage. returns true if add was successful, false otherwise
	function storeAmmo(ammo : Ammo) : boolean;
	
	//reloads the weapon. returns true if reload is successful
	//and false if it fails
	function reload() : boolean;
	
	//shoots the weapon. returns the projectile
	function shoot() : Projectile;
}