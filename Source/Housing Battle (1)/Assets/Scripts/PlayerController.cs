using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

	public int buildNumber = 0;
	public int player;
    public string playername; //local playername
	public GameController gameController;
	public CanvasController canvasController;
	public MatchController matchController;
	public Username globalUser;
	public ScoreManager manager;

	[HideInInspector]
	public PlayerController otherPlayer;

	public const string Started = "PlayerController.Start";
	public const string StartedLocal = "PlayerController.StartedLocal";
	public const string Destroyed = "PlayerController.Destroyed";
	public const string CoinToss = "PlayerController.CoinToss";
	public const string AddHouse = "PlayerController.AddHouse";

    // FBScript facebookFunctions; //facebook for login

	void Awake(){
		gameController = GameObject.Find ("GameController").GetComponent<GameController> ();
		// globalUser = GameObject.Find ("Username").GetComponent<Username> ();
		// playername = globalUser.username;
        //facebookFunctions = GetComponent<FBScript>(); //get the script
	}

	void Start(){
		gameController.playerController = this;
		playername = globalUser.username;
        // facebookFunctions = GetComponent<FBScript>(); //get the script
        // facebookFunctions.FBLogin(); //forcelogin once this starts
        // playername = facebookFunctions.getDisplayName();
	}

	void Update(){
		// playername = facebookFunctions.getDisplayName();
	}

	public override void OnStartClient(){
		base.OnStartClient();
		this.PostNotification (Started);
	}

	public override void OnStartLocalPlayer(){
		base.OnStartLocalPlayer ();
		// CmdSetHostName ();
		SetPlayerName();
		this.PostNotification (StartedLocal);
	}

	[Command]
	public void CmdSetHostName(){
		RpcSetHostName();
	}

	[ClientRpc]
	void RpcSetHostName(){
		GameObject.Find ("MatchController").GetComponent<MatchController> ().hostName = globalUser.username;
	}

	[Client]
	void SetPlayerName(){
		playername = PlayerPrefs.GetString ("Name");
		CmdSendNameToServer (playername);
	}

	[Command]
	void CmdSendNameToServer(string nameToSend){
		GameObject.Find ("MatchController").GetComponent<MatchController> ().clientName = nameToSend;
	}

	void OnDestroy(){
		this.PostNotification (Destroyed);
	}


	[Command]
	public void CmdCoinToss (){
		bool coinToss = Random.value < 0.5;
		RpcCoinToss(coinToss);
	}

	[ClientRpc]
	void RpcCoinToss (bool coinToss){
		this.PostNotification(CoinToss, coinToss);
	}

	[Command]
	public void CmdSpawnHouse(int idx){
		RpcSpawnHouse (idx);
	}

	[ClientRpc]
	void RpcSpawnHouse(int idx){
		int row = idx / 5;
		int col = idx % 5;
		if (gameController.HasMine(row, col)){
			gameController.StepMine(row, col);
			RpcEndTurn();
			return;
		}
		if (gameController.hasPowerUp(row, col)){
			if (isLocalPlayer) {
				gameController.AddPowerUp ();
			}
		}
		GameObject newHouse = gameController.AddHouse(row, col);
		NetworkServer.SpawnWithClientAuthority (newHouse, this.gameObject);
		gameController.SetReference (newHouse, row, col);
		gameController.CmdToggleButtonInteractable (row, col, false);
		RpcEndTurn ();
	}


	[Command]
	public void CmdSpawnHouseHammer(int idx){
		RpcSpawnHouseHammer (idx);
	}

	[ClientRpc]
	void RpcSpawnHouseHammer(int idx){
		int row = idx / 5;
		int col = idx % 5;
		if (buildNumber == 0) {
			if (gameController.HasMine (row, col)) {
				gameController.StepMine (row, col);
				buildNumber++;
				return;
			}
			if (gameController.hasPowerUp(row, col)){
				if (isLocalPlayer) {
					gameController.AddPowerUp ();
				}
			}
			GameObject newHouse = gameController.AddHouse(row, col);
			NetworkServer.SpawnWithClientAuthority (newHouse, this.gameObject);
			gameController.SetReference (newHouse, row, col);
			gameController.CmdToggleButtonInteractable (row, col, false);
			gameController.CheckBuildings ();
			if (gameController.isGameOver ()) {
				gameController.EndTurn ();
				return;
			}
			buildNumber++;
		} else {
			if (gameController.HasMine (row, col)) {
				gameController.StepMine (row, col);
				buildNumber = 0;
				RpcEndTurn();
				return;
			}
			if (gameController.hasPowerUp(row, col)){
				if (isLocalPlayer) {
					gameController.AddPowerUp ();
				}
			}
			GameObject newHouse = gameController.AddHouse(row, col);
			NetworkServer.SpawnWithClientAuthority (newHouse, this.gameObject);
			gameController.SetReference (newHouse, row, col);
			gameController.CmdToggleButtonInteractable (row, col, false);
			RpcEndTurn ();
			buildNumber = 0;
		}
	}

	[Command]
	public void CmdUsePowerUp(int player) {
		RpcUsePowerUp (player);
	}

	[ClientRpc]
	void RpcUsePowerUp(int player){
		if (player == 1) {
			// Debug.Log ("success 1");
			gameController.UsePowerUp (1);
		} else {
			// Debug.Log ("success 2");
			gameController.UsePowerUp (2);
		}
	}

	[Command]
	public void CmdSpawnMine(int idx){
		RpcSpawnMine (idx);
	}

	[ClientRpc]
	public void RpcSpawnMine(int idx){
		GameObject newMine = null;
		int row = idx / 5;
		int col = idx % 5;
		if (isLocalPlayer) {
			newMine = gameController.AddMine (row, col);
		}
		gameController.SetReferenceMine (newMine, row, col);
		gameController.canvasController.powerUp = -1;
	}

	[Command]
	public void CmdBomb(int idx){
		RpcBomb (idx);
	}

	[ClientRpc]
	void RpcBomb(int idx){
		int row = idx / 5;
		int col = idx % 5;
		// gameController.SetBuildingsInteractable (true);
		gameController.Bomb (row, col);
		RpcEndTurn ();
	}

	void EndTurn(){
		RpcEndTurn();
		// canvasController.ChangePlayer(otherPlayer);
	}

	[ClientRpc]
	void RpcEndTurn(){
		gameController.EndTurn ();
		// matchController.CmdEndTurn ();
		// gameController.playerController = otherPlayer;
	}
}
