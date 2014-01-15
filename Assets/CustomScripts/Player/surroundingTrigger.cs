using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void wallNormalChangeEvent( Vector3 wallNormal );

public class surroundingTrigger : MonoBehaviour
{
	public bool horizontalUp = false, horizontalDown = false, vertical = false;
	public wallNormalChangeEvent wallNormal;
	public float testLineDuration = 0f;

	void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			//if( contact.point.y > transform.position
			if(contact.otherCollider.gameObject != transform.parent.gameObject)
			{
				if( Mathf.Abs( contact.normal.y ) <= 0.1f)
				{
					Debug.Log( contact.normal );
					wallNormal( contact.normal );
					vertical = true;
				}
				/*else if( Mathf.Abs( contact.normal.y ) >= 0.9f)
				{
					if( contact.normal.y > 0f)
					{
						horizontalUp = true;
					}
					else
						horizontalDown = true;
				}*/
				//Debug.Log( "object: " + contact.otherCollider.gameObject.name + ", vertical: " + vertical + ", horizontalUp: " + horizontalUp + ", horizontalDown: " + horizontalDown);
			}
		}
	}

	void OnCollisionStay(Collision collision)
	{
		int verts = 0;
		foreach (ContactPoint contact in collision.contacts) {
			if(contact.otherCollider.gameObject != transform.parent.gameObject)
			{
				if( Mathf.Abs( contact.normal.y ) <= 0.1f)
				{
					wallNormal( contact.normal );
					vertical = true;
					verts += 1;
					Debug.DrawRay( contact.point, contact.normal, Color.white, testLineDuration );
				}
				/*else if( Mathf.Abs( contact.normal.y ) >= 0.9f)
				{
					if( contact.normal.y > 0f)
					{
						horizontalUp = true;
					}
					else
						horizontalDown = true;
				}*/
			}
		}
		/*if( verts == 0 )
		{
			vertical = false;
		}*/
	}

	void OnCollisionExit(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			if(contact.otherCollider.gameObject != transform.parent.gameObject)
			{
			if( Mathf.Abs( contact.normal.y ) <= 0.1f)
			{
				wallNormal( contact.normal );
				vertical = false;
			}
			/*else if( Mathf.Abs( contact.normal.y ) >= 0.9f)
			{
				if( contact.normal.y > 0f)
					horizontalUp = false;
				else
					horizontalDown = false;
			}*/
			}
			//Debug.Log( "object: " + contact.otherCollider.gameObject.name + ", vertical: " + vertical + ", horizontalUp: " + horizontalUp + ", horizontalDown: " + horizontalDown);
		}
	}
}

