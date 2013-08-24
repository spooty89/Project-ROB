using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public interface ROBInterface
	{
		void UpdateSmoothedMovementDirection ();
		void ApplyJumping ();
		void ApplyGravity ();
		// Determine the direction of the player against the surface (for climbing side to side and wallsliding)
		Vector3 DirectionOnWall();
		float CalculateJumpVerticalSpeed (float targetJumpHeight);
	
		// Handle the jump/double-jump actions
		void DidJump ();
	
		// Player has come in contact with a surface
		void OnControllerColliderHit (ControllerColliderHit hit);
		
		// These functions just return the truth value of their implied variables
		//function GetSpeed (); 
		
		bool IsJumping (); 
		
		bool IsDoubleJumping (); 
	
		bool IsGrounded ();
	
		//function GetDirection (); 
	
		
		//function IsMovingBackwards (); 
	
		
		//function GetLockCameraTimer (); 
		//function IsMoving (): boolean;
		
		//function HasJumpReachedApex ();
		
		//function Reset ();
	}
}

