using System;
using UnityEngine;

public class Platform : PlatformInterface{
	
	protected Transform transform;
	
	public Platform(Transform transform){
		this.transform = transform;
	}
	
	public Vector3 getPosition(){
		return transform.position;
	}
	
	public void setPosition(float x, float y, float z){
		transform.position = new Vector3(x,y,z);
	}
}