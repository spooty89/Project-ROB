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

	void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			if(contact.otherCollider.gameObject != transform.parent.gameObject)
			{
				if( Mathf.Abs( contact.normal.y ) <= 0.1f)
				{
					Debug.Log( contact.normal );
					wallNormal( contact.normal );
					vertical = true;
				}
			}
		}
	}

	void OnCollisionStay(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			if(contact.otherCollider.gameObject != transform.parent.gameObject)
			{
				if( Mathf.Abs( contact.normal.y ) <= 0.1f)
				{
					vertical = true;
					wallNormal( contact.normal );
					Debug.DrawRay( contact.point, contact.normal, Color.white, testLineDuration );
				}
			}
		}

		numContacts = collision.contacts.Length;
	}

	void OnCollisionExit(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			if(contact.otherCollider.gameObject != transform.parent.gameObject)
			{
				if( Mathf.Abs( contact.normal.y ) <= 0.1f)
				{
					vertical = false;
					wallNormal( contact.normal );
				}
			}
		}
		if(collision.contacts.Length >= numContacts)
		{
			vertical = false;
		}
	}
}

