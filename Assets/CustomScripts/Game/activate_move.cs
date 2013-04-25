using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp
{
	public class activate_move : MonoBehaviour
	{
		public List <GameObject> targets;
		public void activate ()
		{
			for(int i = 0; i < targets.Count; i++){
				targets[i].GetComponent<movingPlatformBehavior>().enabled = !targets[i].GetComponent<movingPlatformBehavior>().enabled;
			}
		}
	}
}

