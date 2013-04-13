using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public interface PlatformInterface{
		//Returns the position of the transform of the platform
		Vector3 getPosition();
		
		//Sets the position of the transform of the platform
		void setPosition(float x, float y, float z);
	}
}

