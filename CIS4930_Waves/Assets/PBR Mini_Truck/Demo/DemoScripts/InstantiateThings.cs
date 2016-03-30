using UnityEngine;
using System.Collections;

public class InstantiateThings : MonoBehaviour 
{
	public GameObject[] Things;
	GameObject newgo;
	bool functionCalled = false;
	int counter = 0;
	
	private void Awake()
	{
		counter = 0;
		InstantiateNewGo();
	}

	public void Change()
	{
		functionCalled = true;
		Destroy(newgo);

		if (counter != Things.Length - 1 && functionCalled)
		{
			counter++;
			InstantiateNewGo();
		}
		else 
		{
			counter = 0;
			InstantiateNewGo();
		}
		functionCalled = false;
	}

	private void InstantiateNewGo()
	{
		newgo = Instantiate(Things[counter],transform.position, transform.rotation) as GameObject;
		newgo.transform.parent = transform;	
		newgo.name = newgo.name.Remove (newgo.name.Length - 7);
	}
}
