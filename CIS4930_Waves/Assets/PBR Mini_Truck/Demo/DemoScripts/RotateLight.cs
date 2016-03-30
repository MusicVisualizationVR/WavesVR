using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RotateLight : MonoBehaviour 
{
	Slider slider;
	float angle;

	void Start()
	{
		slider = GameObject.Find("Canvas/Light").GetComponent<Slider>();

	}

	public void RotateAround()
	{
		angle = slider.value;
		transform.localEulerAngles = new Vector3 (angle, 0f, 0f);
	}
}
