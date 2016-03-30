using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace AudioVisualizer
{
    /// <summary>
    /// Object scale waveform.
    /// Scale objects along a given axis, to create an audio waveform. Objects are typically arranged in a line or close together.
    /// </summary>
    public class ObjectScaleWaveform : MonoBehaviour
    {

		public int audioSource = 0; // index into audioSampler audioSource array. Determines which audio source we want to sample
		public FrequencyRange frequencyRange = FrequencyRange.Decibal; // what frequency will we listen to? 
		public float sensitivity = 2; // how sensitive is this script to the audio. This value is multiplied by the audio sample data.
		public List<GameObject> objects; // objects to scale with the music
		public Vector3 scaleAxis = Vector3.up; // the direction we scale each object.
		public float maxHeight = 10; // the max scale along the scaleAxis. i.e. if scaleAxis is(0,1,0), this is our maxHeight
		public float lerpSpeed = 1; // speed at which we scale, from currentScale to nextScale.
		public bool absoluteVal = true; // /use absolute value of audio decibal levels
		private List<Vector3> startingScales; // the scale of each object on game start
		private List<Vector3> startingPositions; // the position of each object on game start



		// Use this for initialization
		void Start () {

			//initialize starting scales and positions
			startingScales = new List<Vector3> ();
			startingPositions = new List<Vector3> ();
			foreach(GameObject obj in objects)
			{
				startingScales.Add(obj.transform.localScale);
				startingPositions.Add(obj.transform.position);
			}


		}
		
		// Update is called once per frame
		void Update () {
			ScaleObjects();
		}
		
		void ScaleObjects()
		{
			float[] audioSamples;
			if (frequencyRange == FrequencyRange.Decibal) {
				audioSamples = AudioSampler.instance.GetAudioSamples (audioSource, objects.Count, absoluteVal);
			}
			else
			{
				audioSamples = AudioSampler.instance.GetFrequencyData (audioSource, frequencyRange, objects.Count, absoluteVal);
			}
			//for each object
			for(int i = 0; i < objects.Count; i++)
			{

				float sampleScale = Mathf.Min(audioSamples[i]*sensitivity,1); // % of maxHiehgt, via teh audio sample
				float currentHeight = sampleScale*maxHeight; 

				Vector3 desiredScale = startingScales[i] + currentHeight*transform.InverseTransformDirection(scaleAxis); // get desired scale, in correct direction, using audio
				objects[i].transform.localScale = Vector3.Lerp(objects[i].transform.localScale,desiredScale ,Time.smoothDeltaTime*lerpSpeed); // lerp from current scale to desired scale

				//reposition the object after scaling, so that it's seemingly in the same place.
				float distanceScaled = (objects[i].transform.localScale - startingScales[i]).y; // get change in scale
				Vector3 direction = objects[i].transform.TransformDirection(scaleAxis); // get movement direction, relative to object
				objects[i].transform.position = startingPositions[i] + distanceScaled*direction*.5f; // move the object
			
			}
		}
	}
}
