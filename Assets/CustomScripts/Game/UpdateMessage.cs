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
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "Press SPACEBAR to jump\nWhile jumping, press SPACEBAR again to doublejump";
					Destroy(this.gameObject);
				break;
				case 2:
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "Touch a climbable surface to begin climbing.\n" +
																								"Press W/A/S/D to climb up/left/down/right. Press E to let go";
					Destroy(this.gameObject);
				break;
				case 3:
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "Press SPACEBAR to jump off a climbable surface";
					Destroy(this.gameObject);
				break;
				case 4:
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "Touch a regular wall while falling/jumping to begin wallsliding.\n" +
																								"Press SPACEBAR while wallsliding to walljump. TIP: You can't doublejump after walljumping, but you can perform another walljump...";
					Destroy(this.gameObject);
				break;
				case 5:
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "Collect all of the 1-Ups to get your reward";
				break;
				case 6:
					GameObject.FindGameObjectWithTag("Player").GetComponent<ROBgui>().message = "You can hang from vines that cling to the ceiling.\n" +
																								"Press W/A/S/D to move while hanging. Press E to let go";
					Destroy(this.gameObject);
				break;
			}	
		}
	}
}

