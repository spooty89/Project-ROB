#pragma strict

function Start () {

}

function Update () {

}

public class HandShot extends Weapon{
	public function HandShot(ammoStorage : AmmoStorage){
		super(ammoStorage);
		this.cooldown = 5;
		this.range = 500;
	}
}