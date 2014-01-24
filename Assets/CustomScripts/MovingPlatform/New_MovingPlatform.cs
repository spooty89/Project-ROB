using UnityEngine;
using System.Collections;

public class New_MovingPlatform : MonoBehaviour
{
	new bool enabled = true;
	
	[System.NonSerialized]
	public Transform hitPlatform;
	
	[System.NonSerialized]
	public Transform activePlatform;
	
	[System.NonSerialized]
	Vector3 activeLocalPoint;
	
	[System.NonSerialized]
	Vector3 activeGlobalPoint;
	
	[System.NonSerialized]
	Quaternion activeLocalRotation;
	
	[System.NonSerialized]
	Quaternion activeGlobalRotation;
	
	[System.NonSerialized]
	Matrix4x4 lastMatrix;
	
	[System.NonSerialized]
	Vector3 platformVelocity;
	
	[System.NonSerialized]
	bool newPlatform;
}

