using UnityEngine;
using System.Collections;

public class SwapCarTexture : MonoBehaviour
{
	public Texture2D[] textures;
	GameObject materialLocation;
	Material mat;

	int counter = 0;


	// Use this for initialization
	void Start () 
	{
		materialLocation = GameObject.Find("MainMesh");
		mat = materialLocation.GetComponent<Renderer>().material;
		mat.mainTexture = textures[counter];
	}

	public void SwapTexture()
	{
		if (counter < textures.Length - 1)
		{
			counter++;
		}
		else 
			counter = 0;

		mat.mainTexture = textures[counter];
	}

}
