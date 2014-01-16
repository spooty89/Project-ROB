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
	Vector3 normal = Vector3.zero;
	
	void Awake()
	{
		new CoRoutine( collisionManager() );
		GetComponent<Rigidbody>().sleepVelocity = 0f;
		GetComponent<Rigidbody>().sleepAngularVelocity = 0f;
	}
	
	IEnumerator OnCollisionEnter(Collision collision)
	{
		changed = true;
		numContacts++;
		yield return new WaitForFixedUpdate();
		//yield return new WaitForFixedUpdate();
	}

	IEnumerator OnCollisionStay(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			if( Mathf.Abs( contact.normal.y ) < 0.1f)
			{
				vertical = true;
				wallNormal( contact.normal );
				normal = contact.normal;
				break;
				//Debug.DrawRay( contact.point, contact.normal, Color.white, testLineDuration );
			}
		}
		yield return new WaitForFixedUpdate();
	}

	IEnumerator OnCollisionExit(Collision collision)
	{
		changed = true;
		numContacts--;
		yield return new WaitForFixedUpdate();
		//yield return new WaitForFixedUpdate();
	}

	IEnumerator collisionManager()
	{
		if( numContacts == 0 )
		{
			vertical = false;
			wallNormal( transform.forward );
		}
		else
		{
			vertical = true;
			wallNormal( normal );
		}
		yield return new WaitForFixedUpdate();
	}

	/*void Update()
	{
//		Debug.Log( numContacts );
		if( changed )
		{
			if( numContacts == 0 )
			{
				vertical = false;
				wallNormal( transform.forward );
			}
		}
		changed = false;
	}*/

	void OnGUI()
	{
		GUI.Box(new Rect(0,0,100,50), normal.ToString() + "\nNumContacts: " + numContacts);
	}
}

