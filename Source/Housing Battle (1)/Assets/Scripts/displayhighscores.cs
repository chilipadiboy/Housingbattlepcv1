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

        leaderBoardFunctions = GetComponent<Highscore>();
        StartCoroutine("RefreshScores");
        StartCoroutine("GetOwnScore");
    }
	
    public void OnScoresDownloaded(leaderScore[] highscoreList)
    {
        for (int i=0;i<leaderBoardText.Length; i++)
        {
            leaderBoardText[i].text = i + 1 + ". ";
            if (highscoreList.Length > i)
            {
                leaderBoardText[i].text += highscoreList[i].username + " - " + highscoreList[i].score;
            }
        }
    }

	public void OnOwnScoresDownloaded(int score)
    {
        leaderBoardText[5].text ="Your Current Score" + ": ";
		leaderBoardText[5].text += score;
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
			leaderBoardFunctions.getSingleHighscore(Pusername.username);
            yield return new WaitForSeconds(20);
        }
    }
}
