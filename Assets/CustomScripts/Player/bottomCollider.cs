using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class bottomCollider: MonoBehaviour
{
	public bool movingPlatform = false;
	bool check = false;
	int buffer = 0;
	
	void Awake()
	{
		GetComponent<Rigidbody>().sleepVelocity = 0f;
		GetComponent<Rigidbody>().sleepAngularVelocity = 0f;
	}

	
	
	void OnCollisionEnter(Collision collision)
	{
		if( collision.collider.gameObject.GetComponent<movingPlatformBehavior>() != null )
		{
			buffer = 10;
			movingPlatform = true;
			check = false;
		}
	}

	void OnCollisionStay(Collision collision)
	{
		if( collision.collider.gameObject.GetComponent<movingPlatformBehavior>() != null )
		{
			buffer = 10;
			movingPlatform = true;
			check = false;
		}
	}
	
	IEnumerator OnCollisionExit(Collision collision)
	{
		if( collision.collider.gameObject.GetComponent<movingPlatformBehavior>() != null )
			check = true;
		yield return new WaitForFixedUpdate();
	}
	
	void LateUpdate()
	{
		if(check)
		{
			if( buffer > 0 )
				buffer--;
			else
			{
				movingPlatform = false;
				check = false;
			}
		}
	}
	
	/*void OnGUI()
	{
		GUI.Box(new Rect(0,50,100,50), normal.ToString() + "\nNumContacts: " + printNum + "\nSleeping?: " + GetComponent<Rigidbody>().IsSleeping() );
	}*/
}

