using UnityEngine;

public abstract class StateClass : MonoBehaviour
{
	protected CharacterClass _Player;
	public stateChangeEvent stateChange;
	public abstract void Run();
	public abstract void CollisionHandler(ControllerColliderHit hit);
	public abstract void surroundingCollisionHandler();
	public abstract void TriggerEnterHandler(Collider other);
	public abstract void TriggerExitHandler(Collider other);
	
	protected virtual void Start()
	{
		_Player = GetComponent<CharacterClass>();
	}
}

