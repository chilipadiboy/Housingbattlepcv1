using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour {

	const string privateCode = "vj4FUe6bEEKrwnG6cHUkcAPdkDXYKeWE6wHB0vVQHmYg";
	const string publicCode = "599276e3ef12d81294044226";
	const string webURL = "http://dreamlo.com/lb/";

	public InputField username;
	public InputField password;
	public Text status;
	public Username globalUser;

	private bool isLoggedIn;

	// Use this for initialization
	void Start () {
		isLoggedIn = false;
	}

    void Awake()
    {
        // DontDestroyOnLoad(transform.gameObject);
    }

	public void AddNewPlayer(){
		if (username.text == "" || password.text == "") {
			status.text = "Please fill in all fields";
			return;
		}
		StartCoroutine (AddNew (username.text, password.text));
	}

	public void Login(){
		if (username.text == "" || password.text == "") {
			status.text = "Please fill in all fields";
			return;
		}
		StartCoroutine(LoginCoroutine ());
	}

	IEnumerator LoginCoroutine(){
		yield return CheckUser (username.text, password.text);
	}

	IEnumerator CheckUser(string user, string password){
        Debug.Log("Checking user: " + user + " password:" + password);
		WWW downloadURL = new WWW(webURL + publicCode + "/pipe-get/"+ user);
		Debug.Log(webURL + publicCode + "/pipe-get/" + user);
		yield return downloadURL;

		if (string.IsNullOrEmpty (downloadURL.error)) {
			if (formatString (downloadURL.text) == "\n" || formatString (downloadURL.text) == "") {
				// do nothing
				status.text = "User doesn't exist";
			} else {
				string[] entry = downloadURL.text.Split(new char[] { '|' });
				if (entry [3] == password) {
					status.text = "Successfully logged in";
                    Debug.Log("Checking user: " + user + " password:" + password);
					PlayerPrefs.SetString ("Name", username.text);
					PlayerPrefs.SetInt ("Login", 1);
                    isLoggedIn = true;
					globalUser.username = user;
					globalUser.isLoggedIn = true;
				} else {
					status.text = "Wrong password!";
				}
			}
		}
		
	}

	IEnumerator AddNew(string user, string password){
		WWW downloadURL = new WWW(webURL + publicCode + "/pipe-get/"+ user);
		Debug.Log(webURL + publicCode + "/pipe-get/" + user);
		yield return downloadURL;

		if (string.IsNullOrEmpty (downloadURL.error)) {
			if (formatString (downloadURL.text) == "\n" || formatString (downloadURL.text) == "") {
				yield return addHighscore (user, password, 0, 0);
				Debug.Log ("Adding new player " + user);
				status.text = "New account successfully created";
			} else {
				// do nothing, name already exists
				status.text = "Username already exists";
			}
		}

		/*
		// if name already exists
		if (string.IsNullOrEmpty (downloadURL.error) || formatString(downloadURL.text) == "\n" || formatString(downloadURL.text) == "") {
			// do nothing, name already exists
			status.text = "Username already exists";
		} else {
			yield return addHighscore (user, password, 0, 0);
			Debug.Log ("Adding new player " + user);
			status.text = "New account successfully created";
		}*/
	}
		
	private string formatString(string str){
        Debug.Log("Formatting String "+str);
        string result = "";
		for (int i = 0; i < str.Length; i++) {
			if (str [i] != ' ') {
				result += str [i];
			} else {
				Debug.Log ("fail");
			}
		}
		return result;
	}

	IEnumerator addHighscore(string username, string password, int score, int games)
	{
		WWW uploadURL = new WWW(webURL + privateCode + "/add/" + WWW.EscapeURL(username) + "/" + score + "/" + games + "/" + WWW.EscapeURL(password));
		yield return uploadURL;

		if (string.IsNullOrEmpty(uploadURL.error))
		{
			Debug.Log("Upload Successful");
		}
		else
		{
			Debug.Log("Error uploading score " + uploadURL.error);
		}
	}

}
