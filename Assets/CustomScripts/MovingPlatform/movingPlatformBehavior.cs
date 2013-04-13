using System;
using UnityEngine;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class movingPlatformBehavior : MonoBehaviour {
		public MovingPlatform movingPlatform;
		public List<GameObject> wayPoints;
		private Transform player;
		public float speed = 3;
		public float rotationSpeed = 45;
		private CharacterController controller;
		
		public void Start () {
			this.movingPlatform = new MovingPlatform(this.transform, speed, rotationSpeed, wayPoints);
		}
		
		public void Update () {
			this.movingPlatform.move();
			this.movingPlatform.rotate();
			if (movingPlatform.playerContact){
				this.player.position += this.movingPlatform.getDeltaPos();
				this.movingPlatform.rotateOnPlatform(player, controller);
			}
		}
		
		public void transferSpeed (Transform playerTransform) {
			this.player = playerTransform;
			this.movingPlatform.playerContact = true;
			this.controller = this.player.GetComponent<CharacterController>();
		}
			
		public void noContact() {
			this.movingPlatform.playerContact = false;
		}
	}
}