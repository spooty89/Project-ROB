using UnityEngine;

public abstract class StateClass : MonoBehaviour
{
	protected CharacterClass _Player;
	public stateChangeEvent stateChange;
	public abstract void Run();
	public abstract void CollisionHandler(ControllerColliderHit hit);
	
	private void Start()
	{
		_Player = GetComponent<CharacterClass>();
	}
}

