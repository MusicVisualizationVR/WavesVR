using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {
	

	// Update is called once per frame
	void Update () 
	{
		transform.Rotate(0f, 30f * Time.deltaTime, 0f);
	}
}
