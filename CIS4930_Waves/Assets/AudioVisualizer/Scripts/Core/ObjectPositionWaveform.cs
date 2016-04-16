using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


namespace AudioVisualizer
{
    /// <summary>
    /// Object position waveform.
    /// Positions game objects up and down along a given axis, to create an audio waveform.
    /// For example if positionAxis is Vector3.up, the script will move all the objects up and down using the audio.
    /// Objects are typically arranged in a line or close together.
    /// </summary>
    public class ObjectPositionWaveform : MonoBehaviour
    {

		public int audioSource = 0; // index into audioSampler audioSource array. Determines which audio source we want to sample
		public FrequencyRange frequencyRange = FrequencyRange.Decibal; // what frequency will we listen to? 
		public float sensitivity = 2; // how sensitive is this script to the audio. This value is multiplied by the audio sample data.
		public List<GameObject> objects; // list of objects to be moved up and down
		public Vector3 positionAxis = Vector3.up;// move the objects along this axis
		public float maxHeight = 10; // the max distance along the positionAxis. i.e. if positionAxis is(0,1,0), this is our maxHeight
		public float lerpSpeed = 1; // speed at which we move each object from it's current position, to it's next position.
		public bool absoluteVal = true; // take the abs value

		private List<Vector3> startingPositions; // the position of each object on game start


		// Use this for initialization
		void Start () {

			//initialize startingPositions list
			startingPositions = new List<Vector3> ();
			foreach(GameObject obj in objects)
			{
				Vector3 relativePos = obj.transform.position - this.transform.position;
				startingPositions.Add(relativePos);
			}

		}
		
		// Update is called once per frame
		void Update () {
			PositionObjects(); // move objects along positionAxis, accourding to the music
			for(int i = 0; i < objects.Count; i++)
			{
				Debug.DrawLine(startingPositions[i], startingPositions[i] + objects[i].transform.up*maxHeight);
			}
		}

		// move objects along positionAxis, accourding to the music
		void PositionObjects()
		{
			float[] audioSamples;
			if (frequencyRange == FrequencyRange.Decibal) {
				audioSamples = AudioSampler.instance.GetAudioSamples (audioSource, objects.Count, absoluteVal);
			}
			else
			{
				audioSamples = AudioSampler.instance.GetFrequencyData (audioSource, frequencyRange, objects.Count, absoluteVal);
			}

			for(int i = 0; i < objects.Count; i++)//for each object
			{
				//position the object accourding to the average
				float samplePosition = Mathf.Min(audioSamples[i]*sensitivity,1); // % of maxHiehgt, via the audio sample
				float currentHeight = samplePosition*maxHeight; 

				Vector3 direction = objects[i].transform.TransformDirection(positionAxis); // get positionAxis relative to each object
				Vector3 desiredPos = this.transform.position + startingPositions[i] + currentHeight*direction;
				
				//reposition the objects, lerp between current position and desired position.
				objects[i].transform.position = Vector3.Lerp(objects[i].transform.position,desiredPos ,Time.smoothDeltaTime*lerpSpeed);
			}
			
		}

	}
}

