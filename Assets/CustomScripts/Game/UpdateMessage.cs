using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class UpdateMessage : MonoBehaviour
	{
		public void message(int i) {
			switch (i)
			{
				case 1:
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "Press SPACEBAR to jump\nWhile jumping, press SPACEBAR to doublejump";
					Destroy(this.gameObject);
				break;
				case 2:
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "W/A/S/D = climb up/left/down/right\nE to let go";
					Destroy(this.gameObject);
				break;
				case 3:
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "Press SPACEBAR to jump off climbing surface";
					Destroy(this.gameObject);
				break;
				case 4:
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "Press SPACEBAR while sliding down a wall to walljump";
					Destroy(this.gameObject);
				break;
				case 5:
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "Collect all of the 1-Ups to get your reward";
				break;
				case 6:
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "W/A/S/D = move while hanging\nE to let go";
					Destroy(this.gameObject);
				break;
			}	
		}
	}
}

