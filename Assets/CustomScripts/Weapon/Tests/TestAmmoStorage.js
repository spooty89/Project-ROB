#pragma strict

function Start () {
	testReloadAmmo();
	testSwapAmmo();
}

function Update () {

}

final var energyMagLimit : int = 50;
final var plasmaMagLimit : int = 20;
var s : String = "TestAmmoStorage";

function testReloadAmmo(){
	var ammoPouch : AmmoStorage = new AmmoPouch();
	
	//test storeAmmo
	var fullEnergyMag : EnergyBolt = new EnergyBolt(energyMagLimit);
	var halfEnergyMag : EnergyBolt = new EnergyBolt(energyMagLimit/2);
	var emptyEnergyMag : EnergyBolt = new EnergyBolt(0);
	var fullPlasmaMag : PlasmaBolt = new PlasmaBolt(plasmaMagLimit);
	var halfPlasmaMag : PlasmaBolt = new PlasmaBolt(plasmaMagLimit/2);
	
	
	//test reload
	/*
	 * test empty reloading, i.e. ammo storage has no bullets and
	 * the magazine to be reloaded has no ammo as well. the record 
	 * should be removed
	 */
	Assert.assertTrue(ammoPouch.getNumAmmoTypes() == 0, s);
	Assert.assertTrue(ammoPouch.storeAmmo(emptyEnergyMag), s);
	Assert.assertTrue(ammoPouch.getNumAmmoTypes() == 1, s);
	ammoPouch.reloadAmmo(emptyEnergyMag);
	Assert.assertTrue(ammoPouch.getNumAmmoTypes() == 0, s);
	
	/*
	 * halfEnergyMag is stored in the ammo pouch, and the test is to
	 * attempt to keep reloading the emptyMag to see how many bullets 
	 * we can get. if it works properly, we should only be able to get
	 * bullets == the amt of bullets in halfEnergyMag in the first place
	 */
	Assert.assertTrue(ammoPouch.storeAmmo(halfEnergyMag), s);
	Assert.assertTrue(emptyEnergyMag.getBulletCount() == 0, s);
	ammoPouch.reloadAmmo(emptyEnergyMag);
	Assert.assertTrue(emptyEnergyMag.getBulletCount() == halfEnergyMag.getBulletCount(), s);
	ammoPouch.reloadAmmo(emptyEnergyMag);
	Assert.assertTrue(emptyEnergyMag.getBulletCount() == halfEnergyMag.getBulletCount(), s);

	/* now add the full mag. the emptyEnergyMag should be able to be filled */
	Assert.assertTrue(ammoPouch.storeAmmo(fullEnergyMag),s);
	ammoPouch.reloadAmmo(emptyEnergyMag);
	Assert.assertTrue(emptyEnergyMag.getBulletCount() == fullEnergyMag.getBulletCount(), s);
}

function testSwapAmmo(){
	var ammoPouch : AmmoStorage = new AmmoPouch();
	Assert.assertTrue(ammoPouch.swapAmmo(null) == null, s);
	
	var energyMag : EnergyBolt = new EnergyBolt(5);
	var plasmaMag : PlasmaBolt = new PlasmaBolt(5);
	
	//storage is stored w 5 plasma bullets and 5 energy bullets
	Assert.assertTrue(ammoPouch.storeAmmo(energyMag),s);
	Assert.assertTrue(ammoPouch.storeAmmo(plasmaMag),s);
	
	//5 plasma bullets are swapped in for an energyMag
	//there should be 10 plasma bullets at this point
	var currentMag : Ammo = ammoPouch.swapAmmo(plasmaMag);
	Assert.assertTrue(currentMag instanceof EnergyBolt, s);
	
	//check if there are really 10 plasma bullets
	var emptyPlasmaMag : PlasmaBolt = new PlasmaBolt(0);
	ammoPouch.reloadAmmo(emptyPlasmaMag);
	Assert.assertTrue(emptyPlasmaMag.getBulletCount() == 10, s);
	
	currentMag = ammoPouch.swapAmmo(energyMag);
	Assert.assertTrue(currentMag instanceof PlasmaBolt, s);
	
}