#pragma strict

var movingPlatform : MovingPlatform;
public var wayPoints:List.<GameObject>;
private var player : Transform;
public var speed : int = 3;
public var rotationSpeed : int = 45;
private var controller : CharacterController;

function Start () {
	this.movingPlatform = MovingPlatform(transform, speed, rotationSpeed, wayPoints);
}

function Update () {
	this.movingPlatform.move();
	this.movingPlatform.rotate();
	if (movingPlatform.playerContact){
		this.player.position += this.movingPlatform.getDeltaPos();
		this.movingPlatform.rotateOnPlatform(player, controller);
	}
}

function transferSpeed (playerTransform : Transform) : void {
	this.player = playerTransform;
	this.movingPlatform.playerContact = true;
	this.controller = this.player.GetComponent(CharacterController);
}
	
function noContact() : void {
	this.movingPlatform.playerContact = false;
}