using UnityEngine;
using System.Collections;

public class surroundingTrigger : MonoBehaviour
{

		// Use this for initialization
		void Start ()
		{
	
		}
	
		// Update is called once per frame
		void Update ()
		{
	
		}

	void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			Debug.DrawRay(contact.point, contact.normal, Color.white);
		}
	}

	void OnCollisionStay(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			if(contact.otherCollider.gameObject != transform.parent.gameObject)
				Debug.DrawRay(contact.point, contact.normal, Color.white);
		}
	}

	void OnCollisionExit(Collision collision)
	{
		Debug.Log("exited");
	}
}

