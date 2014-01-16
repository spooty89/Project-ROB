using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void wallNormalChangeEvent( Vector3 wallNormal );

public class surroundingTrigger : MonoBehaviour
{
	public bool vertical = false;
	public wallNormalChangeEvent wallNormal;
	public float angleThreshold = 90f, testLineDuration = 0f;

	int numContacts = 0;
	bool changed = false;

	IEnumerator OnCollisionEnter(Collision collision)
	{
		changed = true;
		numContacts++;
		yield return new WaitForFixedUpdate();
	}

	void OnCollisionStay(Collision collision)
	{
		if( vertical )
		{
			foreach (ContactPoint contact in collision.contacts) {
				if(contact.otherCollider.gameObject != transform.parent.gameObject)
				{
					if( Mathf.Abs( contact.normal.y ) < 0.2f)
					{
						wallNormal( contact.normal );
						Debug.DrawRay( contact.point, contact.normal, Color.white, testLineDuration );
					}
				}
			}
		}
	}

	IEnumerator OnCollisionExit(Collision collision)
	{
		changed = true;
		numContacts--;
		yield return new WaitForFixedUpdate();
	}

	void Update()
	{
		Debug.Log( numContacts );
		if( changed )
		{
			if( numContacts == 0 )
			{
				vertical = false;
				wallNormal( transform.forward );
			}
			else
				vertical = true;
		}
		changed = false;
	}
}

