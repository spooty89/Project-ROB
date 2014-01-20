using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void wallNormalChangeEvent( Vector3 wallNormal );

public class surroundingTrigger : MonoBehaviour
{
	public bool vertical = false;
	public wallNormalChangeEvent wallNormal;
	public float angleThreshold = 90f, testLineDuration = 0f;
	public LayerMask layerMask;

	int numContacts = 0, printNum = 0;
	bool stay = false, free = true;
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
				if( Mathf.Abs( hit.normal.y ) < 0.2f)
				{
					vertical = true;
					wallNormal( hit.normal );
					normal = hit.normal;
					stay = true;
					free = false;
					numContacts++;
					Debug.DrawRay( contact.point, hit.normal, Color.white, testLineDuration );
					break;
				}
			}
		}
		yield return new WaitForFixedUpdate();
	}

	IEnumerator collisionManager()
	{
		while( true )
		{
			if( !stay )
			{
				if( numContacts == 0 && !free )
				{
					vertical = false;
					wallNormal( transform.forward );
				}
				free = true;
			}
			stay = false;
			printNum = numContacts;
			numContacts = 0;
			yield return new WaitForFixedUpdate();
		}
	}

	void OnGUI()
	{
		GUI.Box(new Rect(0,0,100,50), normal.ToString() + "\nNumContacts: " + printNum + "\nSleeping?: " + GetComponent<Rigidbody>().IsSleeping() );
	}
}

