#pragma strict
var prefabBullet : GameObject;
var ammoPouch : AmmoPouch = new AmmoPouch();
var handShot : HandShot = new HandShot(ammoPouch);
var energyBolt : EnergyBolt = new EnergyBolt(50);
var projectileList : List.<Projectile> = new List.<Projectile>();
var toDestroy : List.<Projectile> = new List.<Projectile>();
var move : boolean;
var aim : boolean;
var hand : Transform;
var collisionLayers : LayerMask = -1;     // What the camera will collide with

function Awake () {
	ammoPouch.storeAmmo(energyBolt);
	handShot.reload();
	aim = false;
}

function Update () {
	if (Input.GetButtonDown("Fire2")){
		aim = !aim;
	}
	
	if (Input.GetButtonDown("Fire1") && aim){
		if (handShot.getBulletCount() <= 0)
			handShot.storeAmmo(energyBolt);
		var instanceBullet : GameObject = Instantiate(prefabBullet, hand.position, Quaternion.identity);
		var bullet : Projectile = handShot.shoot(instanceBullet, getTarget());//new Vector3(0,-10,0));
		if (bullet != null){
			projectileList.Add(bullet);
		}
	}
	for (projectile in projectileList){
		projectile.move();
		if (projectile.reachedTarget())
			toDestroy.Add(projectile);
	}
	for (projectile in toDestroy){
		Destroy(projectile.getPref());
		projectileList.Remove(projectile);
	}
	

}

function getTarget() : Vector3 {
    var hit: RaycastHit;
    var ray : Ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
    Physics.Raycast(ray, hit, handShot.getRange(), collisionLayers);
    return hit.point;
}

function OnCollisionEnter(hit : Collision) {
	Debug.Log("flip switch");
	hit.gameObject.SendMessage("flipSwitch", SendMessageOptions.DontRequireReceiver);
}