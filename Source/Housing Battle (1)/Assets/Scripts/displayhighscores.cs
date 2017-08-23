using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class displayhighscores : MonoBehaviour {

    public Username Pusername;
    public Text[] leaderBoardText;
    Highscore leaderBoardFunctions;
	// Use this for initialization
	void Start () {
		for (int i=0; i < 5; i++)
        {
            leaderBoardText[i].text = i + 1 + ". Loading score...";
        }

		Pusername = GameObject.Find ("Username").GetComponent<Username> ();
        leaderBoardFunctions = GetComponent<Highscore>();
        StartCoroutine("RefreshScores");
        StartCoroutine("GetOwnScore");
    }
	
    public void OnScoresDownloaded(leaderScore[] highscoreList)
    {
        for (int i=0;i<5; i++)
        {
            leaderBoardText[i].text = i + 1 + ". ";
            leaderBoardText[i].text += highscoreList[i].username + " - " + highscoreList[i].score;
        }
    }

	public void OnOwnScoresDownloaded(int score)
    {
        leaderBoardText[5].text ="Your Current Score: " + score;
    }
    IEnumerator RefreshScores()
    {
        while (true)
        {
            leaderBoardFunctions.downloadHighscores();
            yield return new WaitForSeconds(20);
        }
    }

    IEnumerator GetOwnScore()
    {
        while (true)
        {
			Debug.Log("User is " + Pusername.username);
			leaderBoardFunctions.getSingleHighscore(Pusername.username);
            yield return new WaitForSeconds(20);
        }
    }
}
