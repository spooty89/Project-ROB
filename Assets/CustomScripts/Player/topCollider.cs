using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void ceilingNormalChangeEvent( Vector3 ceilingNormal );

public class topCollider: MonoBehaviour
{
	public bool horizontal = false;
	public ceilingNormalChangeEvent ceilingNormal;
	public float angleThreshold = 90f, testLineDuration = 0f;
	public LayerMask layerMask;
	
	int numContacts = 0, printNum = 0;
	bool exit = false;
	Vector3 normal = Vector3.zero;

	void Awake()
	{
		new CoRoutine( collisionManager(), true);
		GetComponent<Rigidbody>().sleepVelocity = 0f;
		GetComponent<Rigidbody>().sleepAngularVelocity = 0f;
	}
	
	IEnumerator OnCollisionStay(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			RaycastHit hit;
			if( Physics.Raycast( transform.position, (contact.point - transform.position).normalized,
			                    out hit, Vector3.Distance(contact.point, transform.position) + .5f, layerMask ) )
			{
				if( Mathf.Abs( hit.normal.y ) > 0.8f)
				{
					horizontal = true;
					ceilingNormal( hit.normal );
					normal = hit.normal;
					numContacts++;
					Debug.DrawRay( contact.point, hit.normal, Color.white, testLineDuration );
					break;
				}
			}
		}
		yield return new WaitForFixedUpdate();
	}

	void OnCollisionExit(Collision collision)
	{
		exit = true;
	}
	
	IEnumerator collisionManager()
	{
		while( true )
		{
			if( exit )
			{
				if( numContacts == 0 )
				{
					horizontal = false;
					ceilingNormal( Vector3.zero );
				}
				exit = false;
			}
			printNum = numContacts;
			numContacts = 0;
			yield return new WaitForFixedUpdate();
		}
	}
	
	void OnGUI()
	{
		GUI.Box(new Rect(0,50,100,50), normal.ToString() + "\nNumContacts: " + printNum + "\nSleeping?: " + GetComponent<Rigidbody>().IsSleeping() );
	}
}

