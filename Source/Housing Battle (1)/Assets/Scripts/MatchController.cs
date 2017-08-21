using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MatchController : NetworkBehaviour {

	// private int turn = 1;

	public const string MatchReady = "MatchController.Ready";

	public bool IsReady { get { return localPlayer != null && remotePlayer != null; }}
	public PlayerController localPlayer;
	public PlayerController remotePlayer;
	public PlayerController hostPlayer;
	public PlayerController clientPlayer;
	public CanvasController canvasController;
	public ScoreManager scoreManager;

	[SyncVar]
	public string hostName;
	public string clientName;

	public UnityEngine.Networking.NetworkInstanceId hostPlayerID;
	public UnityEngine.Networking.NetworkInstanceId clientPlayerID;
	public List<PlayerController> players = new List<PlayerController>();

	void Start(){
		scoreManager = GameObject.Find ("Score Manager").GetComponent<ScoreManager> ();
	}

	void OnEnable(){
		this.AddObserver (OnPlayerStarted, PlayerController.Started);
		this.AddObserver (OnPlayerStartedLocal, PlayerController.StartedLocal);
		this.AddObserver (OnPlayerDestroyed, PlayerController.Destroyed);
	}

	void OnDisable(){
		this.RemoveObserver (OnPlayerStarted, PlayerController.Started);
		this.RemoveObserver (OnPlayerStartedLocal, PlayerController.StartedLocal);
		this.RemoveObserver (OnPlayerDestroyed, PlayerController.Destroyed);
	}

	void OnPlayerStarted(object sender, object args){
		players.Add((PlayerController)sender);
		Configure();
	}

	void OnPlayerStartedLocal(object sender, object args){
		localPlayer = ((PlayerController)sender);
		Configure ();
	}

	void OnPlayerDestroyed(object sender, object args){
		PlayerController pc = ((PlayerController)sender);
		if (localPlayer == pc)
			localPlayer = null;
		if (remotePlayer == pc)
			remotePlayer = null;
		if (hostPlayer == pc)
			hostPlayer = null;
		if (clientPlayer == pc)
			clientPlayer = null;
		if (players.Contains(pc))
			players.Remove(pc);
	}
		
	void Configure(){
		if (localPlayer == null || players.Count < 2)
			return;

		for (int i = 0; i < players.Count; i++) {
			if (players [i] != localPlayer) {
				remotePlayer = players [i];
				break;
			}
		}

		hostPlayer = (localPlayer.isServer) ? localPlayer : remotePlayer;
		clientPlayer = (localPlayer.isServer) ? remotePlayer : localPlayer;

		// set player number and matchcontroller references
		hostPlayer.player = 1;
		clientPlayer.player = 2;
		hostPlayer.matchController = this;
		clientPlayer.matchController = this;

		hostPlayerID = hostPlayer.netId;
		clientPlayerID = clientPlayer.netId;
		canvasController.ChangePlayer (hostPlayer);
		canvasController.SetPlayer1 (hostPlayer);
		canvasController.SetPlayer2 (clientPlayer);

		// for debugging
		hostName = hostPlayer.playername;
		clientName = clientPlayer.playername;

		this.PostNotification (MatchReady);
	}

	// when there is a winner
	public void GameOver(int winner){

		if (winner == 1) {
			// host player wins
			scoreManager.GameOver(hostName, clientName, false);
		} else if (winner == 2){
			// client player wins
			scoreManager.GameOver(clientName, hostName, false);
		}
	}

	// when game is a draw
	public void GameOver(){
		scoreManager.GameOver (hostName, clientName, true);
	}
}
