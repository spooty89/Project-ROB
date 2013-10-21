using UnityEngine;
using System.Collections;

// this script should be appied to a transition trigger box
public class TransitionAnimSequence : MonoBehaviour {
	// use these arrays to define player's behavior in different transitional stages
	public Vector3[] directions = {new Vector3(0, 0, -1), new Vector3(0, 0, -1)};	// move directions
	public int[] durations = {50, 500};	// length of stages (in frames)
	public float[] velocities = {10.0f, 10.0f};	// move speed
}
