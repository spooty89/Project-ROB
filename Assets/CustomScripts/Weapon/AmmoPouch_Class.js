#pragma strict

function Start () {

}

function Update () {

}

public class AmmoPouch extends AmmoStorage{
	private var storageLimit : int;
	
	public function AmmoPouch(){
		this.storageLimit = 10;
	}
	
	//Override
	protected function storageHasSpace() : boolean{
		return this.ammoList.Count < this.storageLimit;
	}
	
	
	
}