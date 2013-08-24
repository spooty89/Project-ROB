using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp
{
	public class musicSelector : MonoBehaviour
	{
		public AudioClip song;
		private bool change;
		private bool trigger;
		private AudioSource source;
		private float audioVol1;
		private float audioVol2;
		
		void Awake () {
			change = false;
			trigger = false;
			source = Camera.main.GetComponent<AudioSource>();
			audioVol1 = source.volume;
			audioVol2 = 0.0f;
		}
		
		void OnTriggerEnter(Collider hit) {
			if (hit.CompareTag("Player")) {
				if (source.clip.name != song.name) {
					trigger = true;
				}
			}
		}
		
		void Update () {
			if (trigger) {
				if (change) {
					source.clip = song;
					source.volume = audioVol2;
					source.Play();
					change = false;
				}
				if (audioVol1 == 0.0f)
					fadeIn();
				else
					fadeOut();
			}
		}
		
		void fadeOut() {
		    if(audioVol1 > 0.04f)
		    {
		        audioVol1 -= (float)(0.1 * Time.deltaTime);
				source.volume = audioVol1;
		    }
			else {
				change = true;
				audioVol1 = 0.0f;
				audioVol2 = 0.0f;
			}
		}
		
		void fadeIn() {
		    if(audioVol2 < 1.0)
		    {
		        audioVol2 += (float)(0.1 * Time.deltaTime);
				source.volume = audioVol2;
		    }
			else {
				trigger = false;
				audioVol1 = 1.0f;
				audioVol2 = 1.0f;
			}
		}
	}
}

