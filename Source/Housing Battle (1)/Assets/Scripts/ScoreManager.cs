using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {
	
	const string privateCode = "vj4FUe6bEEKrwnG6cHUkcAPdkDXYKeWE6wHB0vVQHmYg";
	const string publicCode = "599276e3ef12d81294044226";
	const string webURL = "http://dreamlo.com/lb/";

	[SerializeField]
	private int currScore1;

	[SerializeField]
	private int currScore2;

	[SerializeField]
	private int currGames1;

	[SerializeField]
	private int currGames2;

	void Start () {
		DontDestroyOnLoad (transform.gameObject);
		if (FindObjectsOfType<ScoreManager> ().Length > 1) {
			Destroy (gameObject);
		}
	}

	public void GameOver(string winner, string loser, bool isDraw){
		StartCoroutine(GameOverCoroutine(winner, loser, isDraw));
	}

	public void AddNewPlayer(string user){
		StartCoroutine (AddNew (user));
	}

	IEnumerator AddNew(string user){
		WWW downloadURL = new WWW(webURL + publicCode + "/pipe-get/"+ user);
		Debug.Log(webURL + publicCode + "/pipe-get/" + user);
		yield return downloadURL;

		if (string.IsNullOrEmpty (downloadURL.error)) {
			// do nothing, name already exists
		} else {
			yield return addHighscore (user, 0, 0);
			Debug.Log ("Adding new player " + user);
		}
	}

	IEnumerator GameOverCoroutine(string winner, string loser, bool isDraw){
		// Get winner score, games and delete their score
		yield return getDoubleScore (winner, loser);

		int winnerScore = currScore1;
		int winnerGames = currGames1;
		// Debug.Log ("Winner score is " + winnerScore + " and winner games is " + winnerGames);

		// Get loser score, games and delete their score
		int loserScore = currScore2;
		int loserGames = currGames2;
		// Debug.Log ("Loser score is " + loserScore + " and loser games is " + loserGames);

		if (isDraw) {
			// calculate new elo
			int newWinnerScore = (winnerScore * winnerGames + loserScore)/ (winnerGames + 1);
			int newLoserScore = (loserScore * loserGames + winnerScore) / (loserGames + 1);

			// update new elo
			yield return addHighscore (winner, newWinnerScore, winnerGames + 1);
			yield return addHighscore (loser, newLoserScore, loserGames + 1);
		} else {
			int newWinnerScore;
			int newLoserScore;
			// calculate new elo
			if (winnerScore - loserScore >= 400) {
				// so that winner doesn't lose points when winning someone with at least 400 points less.
				newWinnerScore = winnerScore + 1;
				newLoserScore = Mathf.Max (0, loserScore - 1);
			} else {
				newWinnerScore = (winnerScore * winnerGames + loserScore + 400) / (winnerGames + 1);
				newLoserScore = Mathf.Max (0, (loserScore * loserGames + winnerScore - 400) / (loserGames + 1));
			}

			// update new elo
			yield return addHighscore (winner, newWinnerScore, winnerGames + 1);
			yield return addHighscore (loser, newLoserScore, loserGames + 1);
		}

	}

	public void getTwoPlayersScore(string user1, string user2){
		StartCoroutine (getDoubleScore (user1, user2));
	}


	public int getSingleHighScore(string username) ///get a single persons highscore
	{
		StartCoroutine(getSingleScore(username, 1));
		return currScore1;
	}

	public int getSingleGames(string username) ///get a single persons games
	{
		StartCoroutine(getSingleScore(username, 2));
		return currGames1;
	}


	IEnumerator getSingleScore(string username, int field)
	{
		WWW downloadURL = new WWW(webURL + publicCode + "/pipe-get/"+ username);
		Debug.Log(webURL + publicCode + "/pipe-get/" + username);
		yield return downloadURL;

		if (string.IsNullOrEmpty(downloadURL.error))
		{
			Debug.Log(downloadURL.text);
			string[] entry = downloadURL.text.Split(new char[] { '|' });
			if (field == 1) {
				currScore1 = int.Parse(entry [field]);
			} else if (field == 2) {
				currGames1 = int.Parse(entry [field]);
			}
		}
		else
		{
			Debug.Log("Error downloading score " + downloadURL.error);
			getSingleScore (username, field);
		}
	}


	IEnumerator getDoubleScore(string user1, string user2)
	{
		WWW downloadURL1 = new WWW(webURL + publicCode + "/pipe-get/"+ user1);
		WWW downloadURL2 = new WWW(webURL + publicCode + "/pipe-get/"+ user2);
		Debug.Log(webURL + publicCode + "/pipe-get/" + user1);
		Debug.Log(webURL + publicCode + "/pipe-get/" + user2);
		yield return downloadURL1;
		yield return downloadURL2;

		if (string.IsNullOrEmpty(downloadURL1.error))
		{
			Debug.Log(downloadURL1.text);
			string[] entry1 = downloadURL1.text.Split(new char[] { '|' });
			currScore1 = int.Parse(entry1 [1]);
			currGames1 = int.Parse(entry1 [2]);
		}
		else
		{
			Debug.Log("Error downloading score " + downloadURL1.error);
			getDoubleScore (user1, user2);
		}

		if (string.IsNullOrEmpty(downloadURL2.error))
		{
			Debug.Log(downloadURL2.text);
			string[] entry2 = downloadURL2.text.Split(new char[] { '|' });
			currScore2 = int.Parse(entry2 [1]);
			currGames2 = int.Parse(entry2 [2]);
		}
		else
		{
			Debug.Log("Error downloading score " + downloadURL2.error);
			getDoubleScore (user1, user2);
		}
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
			delHighscore (username);
		}
	}

	public void AddNewHighscore(string username, int score, int games) //add high score
	{
		StartCoroutine(addHighscore(username, score, games));
	}

	IEnumerator addHighscore(string username, int score, int games)
	{
		string password = "";
		WWW downloadURL = new WWW(webURL + publicCode + "/pipe-get/"+ WWW.EscapeURL(username));
		yield return downloadURL;

		if (string.IsNullOrEmpty (downloadURL.error)) {
			string[] entry = downloadURL.text.Split(new char[] { '|' });
			password = entry [3];
		}
			
		yield return delHighscore (username);

		WWW uploadURL = new WWW(webURL + privateCode + "/add/" + WWW.EscapeURL(username) + "/" + score + "/" + games + "/" + password);
		yield return uploadURL;

		if (string.IsNullOrEmpty(uploadURL.error))
		{
			Debug.Log("Upload for " + username + " Successful. Score is now " + score + " and games is " + games);
		}
		else
		{
			Debug.Log("Error uploading score " + uploadURL.error);
		}
	}


}