using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RotateCar : MonoBehaviour 
{
	Slider slider;
	float angle;
	
	void Start()
	{
		slider = GameObject.Find("Canvas/CarRotation").GetComponent<Slider>();
		
	}
	
	public void RotateAround()
	{
		angle = slider.value;
		transform.localEulerAngles = new Vector3 (0f, angle, 0f);
	}

}
