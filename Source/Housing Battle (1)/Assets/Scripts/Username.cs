using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Username : MonoBehaviour {

	public string username;
	public bool isLoggedIn;

	void Awake(){
		DontDestroyOnLoad (transform.gameObject);
		if (FindObjectsOfType<Username> ().Length > 1) {
			Destroy (gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public string getUsername()
    {
        return username;
    }

    public bool getisLoggedIn()
    {
        return isLoggedIn;
    }
}
