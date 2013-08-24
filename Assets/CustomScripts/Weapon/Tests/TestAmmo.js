#pragma strict

function Start () {
	testAmmo();
}

function Update () {

}

final var energyMagLimit : int = 50;
final var plasmaMagLimit : int = 20;
var s : String = "TestAmmo";

function testAmmo(){
	//testing constructor
	var overLimitEnergy : EnergyBolt = new EnergyBolt(energyMagLimit+20);
	var atLimitEnergy : EnergyBolt = new EnergyBolt(energyMagLimit);
	var underLimitEnergy : EnergyBolt = new EnergyBolt(energyMagLimit-20);
	Assert.assertTrue(overLimitEnergy.getBulletCount() == energyMagLimit, s);
	Assert.assertTrue(atLimitEnergy.getBulletCount() == energyMagLimit,s);
	Assert.assertTrue(underLimitEnergy.getBulletCount() == energyMagLimit-20,s);
	
	//testing getters
	var plasma : PlasmaBolt = new PlasmaBolt(3);
	var energy : EnergyBolt = new EnergyBolt(3);
	Assert.assertTrue(plasma.getType() == "PlasmaBolt",s);
	Assert.assertTrue(energy.getType() == "EnergyBolt",s);
	Assert.assertTrue(plasma.getMagLimit() == plasmaMagLimit,s);
	Assert.assertTrue(plasma.getBulletCount() == 3,s);
	
	//test decrementBulletCount
	Assert.assertTrue(plasma.decrementBulletCount(),s);
	Assert.assertTrue(plasma.decrementBulletCount(),s);
	Assert.assertTrue(plasma.decrementBulletCount(),s);
	Assert.assertFalse(plasma.decrementBulletCount(),s);
	Assert.assertFalse(plasma.decrementBulletCount(),s);
	Assert.assertTrue(plasma.getBulletCount() == 0,s);
	
	//test addBullets
	plasma.addBullets(10);
	Assert.assertTrue(plasma.getBulletCount() == 10,s);
	plasma.addBullets(10);
	Assert.assertTrue(plasma.getBulletCount() == 20,s);
	plasma.addBullets(10);
	Assert.assertTrue(plasma.getBulletCount() == 20,s);
	Assert.assertTrue(plasma.decrementBulletCount(),s);
	Assert.assertTrue(plasma.getBulletCount() == 19,s);
}
