#pragma strict

function Start () {

}

function Update () {

}

public interface AmmoStorageInterface{

	//returns the number of different types of ammo in the ammo storage
	function getNumAmmoTypes() : int;

	//swaps the input ammo type for the next ammo type
	//in line. 
	function swapAmmo(ammo : Ammo) : Ammo;
	
	//adds ammo to the storage. returns true if add was successful, false otherwise
	function storeAmmo(ammo : Ammo) : boolean;
	
	//retrieves bullets from the storage to reload the ammo magazine. return
	//true if reload was successful and false otherwise
	function reloadAmmo(ammo : Ammo) : boolean;
}