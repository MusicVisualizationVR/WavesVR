using UnityEngine;
using System.Collections;

public class ChangeScene : MonoBehaviour 
{
	public void Restart()
	{
		Application.LoadLevel(Application.loadedLevel);
	}

	public void LoadPresentation()
	{
		Application.LoadLevel("DemoScene1");
	}

	public void LoadDriving()
	{
		Application.LoadLevel("DemoScene2");
	}
}
