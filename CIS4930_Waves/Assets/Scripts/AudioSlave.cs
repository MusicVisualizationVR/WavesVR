using UnityEngine;
using System.Collections;
using AudioItems;

public class AudioSlave : MonoBehaviour {

	public bool LookingAt;
	public int InstrumentID;
	public GameObject AudioSystem;
	public GameObject ParticleSystem;
	private AudioSource Audio;
	private bool startParticles = false;


	// Use this for initialization
	void Start() {
		LookingAt = false;
		Audio = (AudioSource)AudioSystem.GetComponents<AudioSource> ().GetValue(InstrumentID);


	}
	
	// Update is called once per frame
	void Update() {
		if (LookingAt && Input.GetButtonDown ("Fire1")) {
			Audio.mute = !Audio.mute;
			ToggleParticles();
		}
	}

	public void ToggleLookingAt() {
		LookingAt = !LookingAt;
	}

	public void ToggleParticles() {
		startParticles = !startParticles;
		ParticleSystem.SetActive (startParticles);
	}

}
