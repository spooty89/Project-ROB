using UnityEngine;

public abstract class StateClass : MonoBehaviour
{
	protected CharacterClass _Player;
	
	public abstract void InputHandler();
	public abstract void MovementHandler();
	
	private void Start()
	{
		_Player = GetComponent<CharacterClass>();
	}
}

