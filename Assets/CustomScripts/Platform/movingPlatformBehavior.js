#pragma strict

var movingPlatform : MovingPlatform;
public var wayPoints:List.<GameObject>;

function Start () {
	movingPlatform = MovingPlatform(transform, 3, wayPoints);
	/*movingPlatform.setPosition(0,-13,0);
	movingPlatform.addWayPoint(Vector3(0,-13,0));
	movingPlatform.addWayPoint(Vector3(-5,-13,0));
	movingPlatform.addWayPoint(Vector3(-5,-13,-5));
	movingPlatform.addWayPoint(Vector3(0,-13,-5));*/
}

function Update () {
	movingPlatform.move();
}