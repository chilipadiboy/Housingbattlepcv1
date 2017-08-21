using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facebook.Unity;

public class FBScript : MonoBehaviour {

    // Use this for initialization
    // Awake function from Unity's MonoBehavior 
    //From FB example

    public Text txtStatus;
    public string dispName;
    List<string> perms = new List<string>() { "public_profile", "email", "user_friends" };
    void Awake()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
           

        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
            txtStatus.text = "Failed to Initialize the Facebook SDK";
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    public void FBLogin()  //this is what gets called for login
    {
        Debug.Log("Trying to login");
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);

            FB.API("/me?fields=name", HttpMethod.GET, displayName);
            
            //txtStatus.text = "Logged in as" + aToken.UserId;
            
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }
        }
        else
        {
            Debug.Log("User cancelled login");
            txtStatus.text = "User cancelled login";
        }
    }

    void displayName(IResult result) //get displayname
    {
        if (result.Error != null)
        {
            dispName= result.Error;
        }
        else
        {
            dispName= (string)result.ResultDictionary["name"];
            txtStatus.text = "Logged in as " + dispName;
        }
    }

    public string getDisplayName() //for other scripts to call
    {
        return dispName;
    }
}
