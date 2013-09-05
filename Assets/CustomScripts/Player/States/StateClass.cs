using UnityEngine;

public abstract class StateClass : MonoBehaviour
{
	protected CharacterClass _Player;
	public abstract void Run();
	
	private void Start()
	{
		_Player = GetComponent<CharacterClass>();
	}
}

