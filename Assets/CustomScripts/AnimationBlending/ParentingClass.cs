using UnityEngine;
using System.Collections.Generic;

public class ParentingClass : MonoBehaviour
{
	private List<GameObject> children = new List<GameObject>();
	
	/* Parenting functions */
	public void Parent(GameObject objectToParent)
	{
		Parent(new List<GameObject>{objectToParent}, 0 );
	}
	
	public void Parent(List<GameObject> objectsToParent)
	{
		Parent( objectsToParent, 0 );
	}
	
	public void Parent(GameObject objectToParent, float whenToParent)
	{
		Parent( new List<GameObject>{objectToParent}, whenToParent );
	}
	
	public void Parent(List<GameObject> objectsToParent, float whenToParent)
	{
		CoRoutine.AfterWait( whenToParent, () =>
		{
			foreach( GameObject child in objectsToParent )
			{
				children.Add( child );
				child.transform.parent = gameObject.transform;
				child.transform.position = child.transform.parent.position;
			}
		});
	}
	
	
	/* Unparenting functions */
	// Unparent all children immediately
	public void UnparentAll()
	{
		Unparent( children, 0 );
	}
	
	// Unparent specific child immediately
	public void Unparent( GameObject objectToUnparent )
	{
		Unparent( new List<GameObject>{objectToUnparent}, 0 );
	}
	
	// Unparent specific children immediately
	public void Unparent( List<GameObject> objectsToUnparent )
	{
		Unparent( objectsToUnparent, 0 );
	}
	
	// Unparent all children at specified time
	public void UnparentAll(float whenToUnparent)
	{
		Unparent( children, whenToUnparent );
	}
	
	// Unparent specific child at specified time
	public void Unparent(GameObject objectToUnparent, float whenToUnparent )
	{
		Unparent( new List<GameObject>{objectToUnparent}, whenToUnparent );
	}
	
	// Unparent specific children at specified time
	public void Unparent( List<GameObject> objectsToUnparent, float whenToUnparent )
	{
		if( children.Count != 0 )
		{
			CoRoutine.AfterWait( whenToUnparent, () =>
			{
				foreach( GameObject child in objectsToUnparent )
				{
					if( child.transform.IsChildOf( gameObject.transform ) && children.Contains( child ) )
					{
						child.transform.parent = null;
						children.Remove( child );
					}
				}
			});
		}
	}
	
	// Unparent specific children at specified time (customize unparenting rotation for desired outcome)
	public void Unparent( List<GameObject> objectsToUnparent, float whenToUnparent, Quaternion unparentRotation )
	{
		if( children.Count != 0 )
		{
			CoRoutine.AfterWait( whenToUnparent, () =>
			{
				foreach( GameObject child in objectsToUnparent )
				{
					if( child.transform.IsChildOf( gameObject.transform ) && children.Contains( child ) )
					{
						child.transform.parent = null;
						children.Remove( child );
						
						child.transform.rotation = unparentRotation;
					}
				}
			});
		}
	}
	
	// Unparent specific children at specified time (customize unparenting position for desired outcome)
	public void Unparent( List<GameObject> objectsToUnparent, float whenToUnparent, Vector3 unparentPosition )
	{
		if( children.Count != 0 )
		{
			CoRoutine.AfterWait( whenToUnparent, () =>
			{
				foreach( GameObject child in objectsToUnparent )
				{
					if( child.transform.IsChildOf( gameObject.transform ) && children.Contains( child ) )
					{
						child.transform.parent = null;
						children.Remove( child );
						
						child.transform.position = unparentPosition;
					}
				}
			});
		}
	}
	
	// Unparent specific children at specified time (customize unparenting position and rotation for desired outcome)
	public void Unparent( List<GameObject> objectsToUnparent, float whenToUnparent, Vector3 unparentPosition, Quaternion unparentRotation )
	{
		if( children.Count != 0 )
		{
			CoRoutine.AfterWait( whenToUnparent, () =>
			{
				foreach( GameObject child in objectsToUnparent )
				{
					if( child.transform.IsChildOf( gameObject.transform ) && children.Contains( child ) )
					{
						child.transform.parent = null;
						children.Remove( child );
						
						if(unparentPosition != null)
							child.transform.position = unparentPosition;
						if(unparentRotation != null)
							child.transform.rotation = unparentRotation;
					}
				}
			});
		}
	}
}