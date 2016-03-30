using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MoveButton : MonoBehaviour 
{
	GameObject Holder;
	
	// Use this for initialization
	void Start () 
	{
		Holder = GameObject.Find("Holder");
		if (Holder != null || Holder.GetComponent<InstantiateThings>().Things.Length == 1)
			GetComponent<RectTransform>().anchoredPosition = new Vector3(-100f,50f,0f);
	}
}
