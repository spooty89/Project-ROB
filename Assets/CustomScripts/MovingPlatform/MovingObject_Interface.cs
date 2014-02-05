using System;
using UnityEngine;

public interface MovingObjectInterface{
	/* Gets the speed of the moving object */
	float getSpeed();
	
	/* Sets the speed of the moving object */
	void setSpeed(float speed);
	
	/* Returns the current way point that the platform will move to.
	 * Will return the current position if no waypoint is added */
	Vector3 getCurrentWayPoint();
	
	/* Returns the number of waypoints of the moving object */
	int numWayPoints();
	
	/* Move the object to a step towards its waypoint.
	 * If no waypoint is defined the object will not move */
	void move();
}