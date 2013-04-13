
function Start () {

}

function Update () {

}

public interface ROBInterface{

	function UpdateSmoothedMovementDirection ();
	function ApplyJumping ();
	function ApplyGravity ();
	// Determine the direction of the player against the surface (for climbing side to side and wallsliding)
	function DirectionOnWall(dir);
	function CalculateJumpVerticalSpeed (targetJumpHeight : float);

	// Handle the jump/double-jump actions
	function DidJump ();

	// Player has come in contact with a surface
	function OnControllerColliderHit (hit : ControllerColliderHit );
	
	// These functions just return the truth value of their implied variables
	function GetSpeed (); 
	
	function IsJumping (); 
	
	function IsDoubleJumping (); 

	function IsGrounded ():boolean;

	function GetDirection (); 

	
	function IsMovingBackwards (); 

	
	function GetLockCameraTimer (); 
	function IsMoving (): boolean;
	
	function HasJumpReachedApex ();
	
	function Reset ();
	

}