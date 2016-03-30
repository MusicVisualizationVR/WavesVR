using UnityEngine;
using System.Collections;

public class DisableButton : MonoBehaviour {

	GameObject Holder;

	// Use this for initialization
	void Awake () 
	{
		Holder = GameObject.Find("Holder");
		if (Holder.GetComponent<InstantiateThings>().Things.Length == 1)
			gameObject.SetActive(false);
	}
}
