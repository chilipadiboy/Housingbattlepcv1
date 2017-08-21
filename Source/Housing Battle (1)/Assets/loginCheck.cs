using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class loginCheck : MonoBehaviour
{

    public GameObject menuScreen;
    public GameObject loginScreen;
    public Text continueText;

    public Username userNameFunctions;
    // Use this for initialization
    void Start()
    {
		userNameFunctions = GameObject.Find ("Username").GetComponent<Username>();
    }
		
    public void Update()
    {
		if (loginScreen.activeSelf) {
			if (userNameFunctions.getisLoggedIn ()) {
				Debug.Log (userNameFunctions.getisLoggedIn ().ToString ());
				menuScreen.SetActive (true);
				loginScreen.SetActive (false);

			}
		}
    }

    public void onClick()
    {
        if (userNameFunctions.getisLoggedIn())
        {
            Debug.Log(userNameFunctions.getisLoggedIn().ToString());
            menuScreen.SetActive(true);
            loginScreen.SetActive(false);

        }else
        {
            continueText.text = "Login failed. Please try again";
        }
    }

	public void OnClickLogout(){
		userNameFunctions.username = "";
		userNameFunctions.isLoggedIn = false;
		menuScreen.SetActive(false);
		loginScreen.SetActive(true);
	}

}
