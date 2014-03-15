using UnityEngine;
using System.Collections.Generic;

public class Domain : MonoBehaviour
{
	public List<GameObject> douchbags = new List<GameObject>();

	void Awake()
	{
		foreach( GameObject douchbag in douchbags )
		{
			douchbag.GetComponent<EnemyAI_Base>().dieFunction += RemoveFromListOfDouchbags;
		}
	}

	public void triggerEnter( GameObject objectInTrigger )
	{
		if( objectInTrigger.CompareTag( "Player" ) )
		{
			foreach( GameObject douchbag in douchbags )
				douchbag.SendMessage( "DomainEntered", objectInTrigger, SendMessageOptions.DontRequireReceiver );
		}
	}
	
	public void triggerExit( GameObject objectInTrigger )
	{
		if( objectInTrigger.CompareTag( "Player" ) )
		{
			foreach( GameObject douchbag in douchbags )
				douchbag.SendMessage( "DomainExited", objectInTrigger, SendMessageOptions.DontRequireReceiver );
		}
	}

	void RemoveFromListOfDouchbags( GameObject douchbag )
	{
		douchbags.Remove( douchbag );
	}
}

