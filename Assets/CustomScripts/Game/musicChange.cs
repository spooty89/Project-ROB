using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp
{
	public class musicChange : MonoBehaviour
	{
		public AudioClip song;
		public bool change;
		//private bool trigger;
		public bool chosen = false;
		private AudioSource source;
		public float audioVol;
		public float maxVol;
		public GameObject otherChange;
		private musicChange other;
		//private float audioVol2;
		
		void Awake () {
			//chosen = false;
			source = Camera.main.GetComponent<AudioSource>();
			//audioVol = maxVol;
			other = otherChange.GetComponent<musicChange>();
		}
		
		void OnTriggerEnter(Collider hit) {
			if (hit.CompareTag("Player")) {
				//if (source.clip.name != song.name) {
					chosen = true;
					other.chosen = false;
					other.change = false;
				/*	source.volume = other.audioVol;
				}*/
			}
		}
		
		void Update () {
			if (chosen) {
				if (other.audioVol == 0.0) {
					if (change) {
						source.clip = song;
						source.volume = audioVol;
						if(source.enabled)
							source.Play();
						//other.audioVol = other.maxVol;
					}
					else {
						fadeIn();
					}
					//chosen = false;
					change = false;
					//fadeIn();
				}
				else {
					fadeOut();
				}
			}
		}
		
		void fadeOut() {
		    if(other.audioVol > 0.04f)
		    {
		        other.audioVol -= (float)(0.2 * Time.deltaTime);
				source.volume = other.audioVol;
		    }
			else {
				change = true;
				other.audioVol = 0.0f;
			}
		}
		
		void fadeIn() {
		    if(audioVol < maxVol)
		    {
		        audioVol += (float)(0.2 * Time.deltaTime);
				source.volume = audioVol;
		    }
			else {
				chosen = false;
				audioVol = maxVol;
			}
		}
	}
}



