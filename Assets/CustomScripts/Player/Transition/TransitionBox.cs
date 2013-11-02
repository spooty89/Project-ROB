using UnityEngine;
using System.Collections;

// this script should be appied to a transition trigger box
public class TransitionBox : MonoBehaviour {
	// use these arrays to define player's behavior in different transitional stages
	public TransitionEnterCondition[] enterConditions;
	public TransitionEnterCondition curCond;
	
	public TransitionBox(){
		enterConditions = new TransitionEnterCondition[2];
		enterConditions[0] = new TransitionEnterCondition(new Vector3(0, 0, 1), 100, new Vector3(0, 3, 3), "free_fall");
		enterConditions[1] = new TransitionEnterCondition(new Vector3(1, 0, 0), 100, new Vector3(1, 3, 0), "free_fall");
	}
	
	public TransitionEnterCondition matchEnterCondition(CharacterClass _Player){
		foreach(TransitionEnterCondition ec in enterConditions){
			if(ec.matchCondition(_Player)){
				return ec;
			}
		}
		return null;
	}
}
