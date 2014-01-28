using UnityEngine;

public abstract class StateClass : MonoBehaviour
{
	protected CharacterClass _cc;
	new protected bool enabled;
	public stateChangeEvent stateChange;
	public abstract void Run();
	public abstract void CollisionHandler(ControllerColliderHit hit);
	public abstract void surroundingCollisionHandler();
	public abstract void topCollisionHandler();
	public abstract void TriggerEnterHandler(Collider other);
	public abstract void TriggerExitHandler(Collider other);
	
	protected virtual void Awake()
	{
		_cc = GetComponent<CharacterClass>();
	}
}

