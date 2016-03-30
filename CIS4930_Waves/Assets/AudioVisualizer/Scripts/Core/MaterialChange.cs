using UnityEngine;
using System.Collections;
using System.Collections.Generic;

 
namespace AudioVisualizer
{

    /// <summary>
    /// Material change.
    /// Lerps between 2 materials. Note that if you want to blend textures, you must use the BlendTex or BlendTexLit shaders provided
    /// </summary>
    public class MaterialChange : MonoBehaviour 
	{
		public int audioSource = 0; // index into audioSampler audioSource array. Determines which audio source we want to sample
		public FrequencyRange frequencyRange = FrequencyRange.Decibal; // what frequency will we listen to? 
		public float sensitivity = 2; // how sensitive is this script to the audio. This value is multiplied by the audio sample data.
		public Material lowMat; // when music decibal level is low, use this material.
		public Material highMat; // when music decibal level is high, use this material.
		public float lerpSpeed = 10; // lerp between current material, and desired material
		private List<Material> materials; // renderer material
		private Renderer renderer; // mesh renderer
		private float lastAvg = 0f; // last decibal avg
        

		// Use this for initialization
		void Start () 
		{
			renderer = this.GetComponent<Renderer> ();
            materials = new List<Material>();

            foreach (Material mat in this.GetComponent<MeshRenderer>().materials)
            {
                materials.Add(mat);
            }
		}
		
		// Update is called once per frame
		void Update () 
		{
			LerpMaterial ();
		}

		//blend between materials
		void LerpMaterial()
		{
			float value;
			if(frequencyRange == FrequencyRange.Decibal)
			{
				value = AudioSampler.instance.GetRMS (audioSource)*sensitivity;//get the noise level 0-1 of the audio right now
			}
			else
			{
				value = AudioSampler.instance.GetFrequencyVol(audioSource,frequencyRange)*sensitivity;
			}
			
			value = Mathf.Clamp (value, 0, 1);

			float lerp = Mathf.Lerp(lastAvg,value,lerpSpeed*Time.deltaTime); // lerp between lastAvg and currentAvg

            foreach (Material mat in materials)
            {
                if (mat.GetFloat("_Blend") != null) // set the blend property in the shader
                {
                    mat.SetFloat("_Blend", lerp);
                }
                else
                {
                    mat.Lerp(lowMat, highMat, lerp); // if material doesn't have the _Blend property, just use a normal material lerp
                }
            }

			lastAvg = value;
		}
	}
}
