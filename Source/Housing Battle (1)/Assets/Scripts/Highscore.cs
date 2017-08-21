using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highscore : MonoBehaviour {

    //adapted dreamlo tutorial by sebastial lague
    const string privateCode = "vj4FUe6bEEKrwnG6cHUkcAPdkDXYKeWE6wHB0vVQHmYg";
    const string publicCode = "599276e3ef12d81294044226";
    const string webURL = "http://dreamlo.com/lb/";

    public leaderScore[] leaderScoreList;
    public leaderScore[] leaderScoreList2;
    displayhighscores highscoresDisplay;

    void Awake()
    {
        Debug.Log("getting highscore functions");
        highscoresDisplay = GetComponent<displayhighscores>();
    }

    public void getSingleHighscore(string username) ///get a single persons highscore
    {
        StartCoroutine(getSingleScore(username));
    }

    IEnumerator getSingleScore(string username)
    {
        WWW downloadURL = new WWW(webURL + publicCode + "/pipe-get/"+ username);
        Debug.Log(webURL + publicCode + "/pipe-get/" + username);
        yield return downloadURL;

        if (string.IsNullOrEmpty(downloadURL.error))
        {
            Debug.Log(downloadURL.text);
            formatScores2(downloadURL.text);
            highscoresDisplay.OnOwnScoresDownloaded(leaderScoreList2);
        }
        else
        {
            Debug.Log("Error downloading score " + downloadURL.error);
            getSingleScore(username);
        }
    }

    public void deleteHighscoreByName(string username) //delete highscore by name
    {
        StartCoroutine(delHighscore(username));
    }

    IEnumerator delHighscore(string username)
    {
        WWW uploadURL = new WWW(webURL + privateCode + "/delete/" + WWW.EscapeURL(username));
        yield return uploadURL;

        if (string.IsNullOrEmpty(uploadURL.error))
        {
            Debug.Log("Delete Successful");
        }
        else
        {
            Debug.Log("Delete failed " + uploadURL.error);
        }
    }

    public void AddNewHighscore(string username, int score) //add high score
    {
        StartCoroutine(addHighscore(username, score));
    }

    IEnumerator addHighscore(string username, int score)
    {
        WWW uploadURL = new WWW(webURL + privateCode + "/add/" + WWW.EscapeURL(username) + "/" + score);
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


    public void downloadHighscores()  //get all the highscores
    {
        StartCoroutine("getHighscores");
    }
    IEnumerator getHighscores()
    {
        WWW downloadURL = new WWW(webURL + publicCode + "/pipe/");
        Debug.Log(webURL + publicCode + "/pipe/");
        yield return downloadURL;

        if (string.IsNullOrEmpty(downloadURL.error))
        {
            Debug.Log(downloadURL.text);
            formatScores(downloadURL.text);
            highscoresDisplay.OnScoresDownloaded(leaderScoreList);
        }
        else
        {
            Debug.Log("Error downloading score " + downloadURL.error);
            downloadHighscores();
        }
    }


    void formatScores(string scores)
    {
        Debug.Log(scores);
        string[] scorebyLine = scores.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        leaderScoreList = new leaderScore[scorebyLine.Length];
        for(int i = 0; i < scorebyLine.Length; i++)
        {
            Debug.Log(scorebyLine.Length + " Currently at " + i);
            Debug.Log(scorebyLine[i]);
            string[] entry = scorebyLine[i].Split(new char[] { '|' });
            Debug.Log("Parsing username");
            string username = entry[0];
            Debug.Log("Parsing score");
            int score = int.Parse(entry[1]);
            Debug.Log("Parsing games");
            int games = int.Parse(entry[2]);
            leaderScoreList[i] = new leaderScore(username, score, games);
            Debug.Log(leaderScoreList[i].username + "===" + leaderScoreList[i].score+ "========="+ leaderScoreList[i].games);
        }
    }

    void formatScores2(string scores)
    {
        Debug.Log(scores);
        string[] scorebyLine = scores.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        leaderScoreList2 = new leaderScore[scorebyLine.Length];
        for (int i = 0; i < scorebyLine.Length; i++)
        {
            Debug.Log(scorebyLine.Length + " Currently at " + i);
            Debug.Log(scorebyLine[i]);
            string[] entry = scorebyLine[i].Split(new char[] { '|' });
            Debug.Log("Parsing username");
            string username = entry[0];
            Debug.Log("Parsing score");
            int score = int.Parse(entry[1]);
            Debug.Log("Parsing games");
            int games = int.Parse(entry[2]);
            leaderScoreList2[i] = new leaderScore(username, score, games);
            Debug.Log(leaderScoreList2[i].username + "===" + leaderScoreList2[i].score + "=========" + leaderScoreList2[i].games);
        }
    }
}

public struct leaderScore
{
    public string username;
    public int score;
    public int games;

    public leaderScore(string _username, int _score, int _games)
    {
        username = _username;
        score = _score;
        games = _games;
    }
}