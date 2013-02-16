#pragma strict

var movingPlatform : MovingPlatform;

function Start () {
	movingPlatform = new MovingPlatform(transform, 3);
	movingPlatform.setPosition(0,-13,0);
	movingPlatform.addWayPoint(Vector3(0,-13,0));
	movingPlatform.addWayPoint(Vector3(-5,-13,0));
	movingPlatform.addWayPoint(Vector3(-5,-13,-5));
	movingPlatform.addWayPoint(Vector3(0,-13,-5));
}

function Update () {
	movingPlatform.move();
}