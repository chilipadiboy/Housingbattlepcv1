using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour {

	void Awake()
	{
		Application.logMessageReceived += Application_logMessageReceived;
	}

	private void Application_logMessageReceived(string condition, string stackTrace, LogType type){
		GameObject.Find ("debugtext").GetComponent<Text> ().text = string.Format ("{0}, {1}, {2}", condition, stackTrace, type);
	}
}
