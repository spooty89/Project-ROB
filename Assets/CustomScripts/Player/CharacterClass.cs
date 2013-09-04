using UnityEngine;

public class CharacterClass : MonoBehaviour
{
	[HideInInspector]
	public float gravity = 17.0f;
	[HideInInspector]
	public float verticalSpeed = (float)0.0;				// The current vertical speed
	[HideInInspector]
	public Vector3 moveDirection = Vector3.zero;	// The current move direction in x-z
	[HideInInspector]
	public float moveSpeed = (float)0.0;					// The current x-z move speed
	[HideInInspector]
	public Vector3 inAirVelocity = Vector3.zero;
	[HideInInspector]
	public Vector3 wallFacing = Vector3.zero;
	[HideInInspector]
	public Vector3 wallRight = Vector3.zero;
	[HideInInspector]
	public string currentState;
}

