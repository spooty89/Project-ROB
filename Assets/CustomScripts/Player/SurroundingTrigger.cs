using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void wallNormalChangeEvent( Vector3 wallNormal );

public class SurroundingTrigger : MonoBehaviour
{
	public bool horizontalUp, horizontalDown, vertical;
	public wallNormalChangeEvent wallNormal;

	void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			if(contact.otherCollider.gameObject != transform.parent.gameObject)
			{
				if( Mathf.Abs( contact.normal.y ) <= 0.1f)
				{
					wallNormal( contact.normal );
					vertical = true;
				}
				else if( Mathf.Abs( contact.normal.y ) >= 0.9f)
				{
					if( contact.normal.y > 0f)
						horizontalUp = true;
					else
						horizontalDown = true;
				}
				//Debug.Log( "object: " + contact.otherCollider.gameObject.name + ", vertical: " + vertical + ", horizontalUp: " + horizontalUp + ", horizontalDown: " + horizontalDown);
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
					wallNormal( contact.normal );
				}
			}
		}
	}

	void OnCollisionExit(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			if( Mathf.Abs( contact.normal.y ) <= 0.1f)
			{
				wallNormal( contact.normal );
				vertical = false;
			}
			else if( Mathf.Abs( contact.normal.y ) >= 0.9f)
			{
				if( contact.normal.y > 0f)
					horizontalUp = false;
				else
					horizontalDown = false;
			}
			//Debug.Log( "object: " + contact.otherCollider.gameObject.name + ", vertical: " + vertical + ", horizontalUp: " + horizontalUp + ", horizontalDown: " + horizontalDown);
		}
	}
}

