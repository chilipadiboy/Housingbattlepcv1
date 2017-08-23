using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour {

	#region Public Variables

	public Button[] buttonList2;
    public GameObject house1;
    public GameObject house2;
    public GameObject hotel1;
    public GameObject hotel2;
    public GameObject specialHouse1;
    public GameObject specialHouse2;
	public GameObject powerup;
	public GameObject mine1;
	public GameObject mine2;
	public GameObject rock;
	public Text playerScore1;
    public Text playerScore2;
	public Text gameOver;
	public Text currentPlayer;
	public PowerUpButtonController currentPowerUp1;
	public PowerUpButtonController currentPowerUp2;
	public int spawnRate;
	public MatchController matchController;
	public PlayerController playerController;
	public CanvasController canvasController;
	public GameObject explosionMine;
	public GameObject explosionDynamite;

	#endregion

	#region Private variables

    private System.Random randomness = new System.Random(); //used for powerup spawn
    private int playerSide;
	private int power1;
	private int power2;
	private int gamePowerUp;
    private int score1;
    private int score2;
    private int moveCount;
	public int numberOfObstacles;
    private GridSpace[,] grid;
	private Button[] buttonList;

    private int newRow1;
    private int newCol1;
    private bool newBuilding;
	private int buildNumber; // for hammer power up

	#endregion

	#region Startup Functions

    private void Awake() {
		// change button array from public to private (there can be array out of bounds otherwise)
		buttonList = new Button[25];
		for (int i = 0; i < 25; i++) {
			buttonList [i] = buttonList2 [i];
		}
        SetGameControllerReferenceButtons();
		currentPowerUp1.SetGameControllerReference (this);
		currentPowerUp2.SetGameControllerReference (this);
		canvasController.SetGameControllerReference (this);

        moveCount = 0;
		power1 = -1;
		power2 = -1;
		gamePowerUp = -1;
        score1 = 0;
        score2 = 0;
		gameOver.text = "";
		currentPlayer.text = "Player 1";
		currentPlayer.color = new Color (0.67F, 0.02F, 0.02F, 1.0F);
		buildNumber = 0;

        playerSide = 1;
        grid = new GridSpace[5, 5];
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
                grid[i, j] = new GridSpace {
                    buildingType = 0,
                    player = 0,
					index = i*5 + j,
					hasMine = false,
					button = buttonList[(5 * i) + j],
					mine = null,
                    building = null,
                    connected = new List<GridSpace>(),
                    connected1 = new List<GridSpace>(),
                    connected2 = new List<GridSpace>(),
                    connected3 = new List<GridSpace>(),
                    connected4 = new List<GridSpace>()
                };
            }
        }

        newBuilding = false;
        playerScore1.text = score1.ToString();
        playerScore2.text = score2.ToString();
    }

	// start of new code (regarding networking)

	void OnEnable(){
		this.AddObserver (OnMatchReady, MatchController.MatchReady);
		// this.AddObserver (OnAddHouse, PlayerController.AddHouse);
	}

	void OnDisable(){
		this.RemoveObserver (OnMatchReady, MatchController.MatchReady);
		// this.RemoveObserver (OnAddHouse, PlayerController.AddHouse);
	}

	void OnMatchReady(object sender, object args){
		if (matchController.clientPlayer.isLocalPlayer) {
			matchController.clientPlayer.CmdCoinToss();
		}

		for (int i = 0; i < numberOfObstacles; i++) {
			CmdSpawnObstacles ();
		}
	}

	#endregion

	#region Set Button Functions

	void SetGameControllerReferenceButtons() {
		for (int i = 0; i < buttonList.Length; i++) {
			buttonList[i].GetComponentInParent<ButtonController>().SetGameControllerReference(this);
		}
	}

	public void ToggleButtonsInteractable(bool toggle){
		for (int i = 0; i < 5; i++){
			for (int j = 0; j < 5; j++) {
				grid [i, j].button.interactable = toggle;
			}
		}
		// Debug.Log ("Successfully toggled buttons");
	}
		
	public void SetPowerUp(int powerUp){
		if (powerUp == 1) {
			currentPowerUp1.SetInteractable (false);
			currentPowerUp2.SetInteractable (false);
		} else if (powerUp == 2) {
			SetBuildingsInteractable (true);
		}
		for (int i = 0; i < buttonList.Length; i++) {
			buttonList [i].GetComponentInParent<ButtonController> ().SetPowerUp (powerUp);
		}
		gamePowerUp = powerUp;
	}

	#endregion

	#region Network Functions

	public void SetBuildingsInteractable(bool interactable){
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 5; j++) {
				if (grid [i, j].buildingType > 0) { // buildingType is {1,3,4,5,9,10}
					grid [i, j].button.interactable = interactable;
				} else {
					grid [i, j].button.interactable = !interactable;
				}
			}
		}
	}

	[Command]
	public void CmdSetBuildingsInteractable(bool interactable){
		RpcSetBuildingsInteractable (interactable);
	}

	[ClientRpc]
	void RpcSetBuildingsInteractable(bool interactable) {
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 5; j++) {
				if (grid [i, j].buildingType > 0) { // buildingType is {1,3,4,5,9,10}
					grid [i, j].button.interactable = interactable;
				} else {
					grid [i, j].button.interactable = !interactable;
				}
			}
		}
	}

	[Command]
	public void CmdAddPowerUp(int powerUpNumber){
		RpcAddPowerUp (powerUpNumber);
	}

	[ClientRpc]
	void RpcAddPowerUp(int powerUpNumber){
		if (playerSide == 1) {
			power1 = powerUpNumber;
			currentPowerUp1.AddPowerUp (powerUpNumber);
		} else {
			power2 = powerUpNumber;
			currentPowerUp2.AddPowerUp (powerUpNumber);
		}
	}

	[Command]
	public void CmdSpawnObstacles(){
		int obstacleIndex = GenerateObstaclePosition ();
		int row = obstacleIndex / 5;
		int col = obstacleIndex % 5;
		RpcSpawnObstacles(row, col);
	}

	[ClientRpc]
	void RpcSpawnObstacles(int row, int col){
		grid [row, col].buildingType = 10;
		GameObject newObstacle = grid [row, col].building = Instantiate (rock, grid [row, col].button.transform.position - new Vector3 (0.42f, 0, 0.5f), Quaternion.Euler (new Vector3 (-90, 0, 0)));
		NetworkServer.Spawn(newObstacle);
		grid [row, col].button.interactable = false;
	}

	[Command]
	public void CmdAddHouse(int row, int col){
		RpcAddHouse (row, col);
	}

	[ClientRpc]
	void RpcAddHouse(int row, int col){
		AddHouse (row, col);
		grid [row, col].button.interactable = false;
		EndTurn ();
	}

	[Command]
	public void CmdSpawnPowerUp(){
		int powerUpIndex = GetPowerUpIndex ();
		RpcSpawnPowerUp (powerUpIndex);
	}

	[ClientRpc]
	void RpcSpawnPowerUp(int powerUpIndex){
		SpawnPowerUp(powerUpIndex);
	}

	[ClientRpc]
	public void RpcDestroyObject(int row, int col){
		Destroy (grid[row, col].building);
	}
		
	[Command]
	public void CmdToggleButtonInteractable(int row, int col, bool toggle){
		RpcToggleButtonInteractable (row, col, toggle);
	}

	[ClientRpc]
	public void RpcToggleButtonInteractable(int row, int col, bool toggle){
		grid [row, col].button.interactable = toggle;
	}

	#endregion

	#region Add Objects

	int GenerateObstaclePosition(){
		bool obstacleBuilt = false;
		int row = randomness.Next (5);
		int col = randomness.Next (5);
		while (!obstacleBuilt) {
			if (grid [row, col].buildingType == 0) {
				obstacleBuilt = true;
			}
		}
		return row * 5 + col;
	}

	public void SpawnPowerUp(int index){
		if (index == -1) {
			return;
		}
		int row = index / 5;
		int col = index % 5;
		grid[row, col].buildingType = -1;
		grid[row, col].building = Instantiate(powerup, grid[row, col].button.transform.position, Quaternion.Euler(new Vector3(0,226,0)));
		grid[row, col].player = 0;
	}

	public int GetPowerUpIndex() { //power up spawn checks for emptyspaces then adds power up randomly every turn
		int spawndecision = randomness.Next(spawnRate); //1 in 4 chance of spawning a powerup
		if (spawndecision == spawnRate - 1) {

			List<GridSpace> emptyslots = new List<GridSpace>();
			for (int i = 0; i < 5; i++) {
				for (int j = 0; j < 5; j++) {
					if (grid[i, j].buildingType == 0) {
						emptyslots.Add(grid[i, j]); //get the list of emptyslots
						// Debug.Log("Add the grid location " + i + "," + j);
					}
				}
			}

			if (emptyslots.Count >= 1) {
				GridSpace spawnLocation = emptyslots[randomness.Next(emptyslots.Count)];
				return spawnLocation.index;
			}
		}
		return -1;
	}

	public GameObject AddMine(int row, int col){
		// destroy mine, if there is one already
		Vector3 location = grid[row, col].button.transform.position;
		// possible bug here
		Destroy (grid [row, col].mine);
		if (playerSide == 1) {
			grid [row, col].mine = Instantiate (mine1, location, Quaternion.identity);
			power1 = -1;
		} else {
			grid [row, col].mine = Instantiate (mine2, location, Quaternion.identity);
			power2 = -1;
		}
		grid[row, col].hasMine = true; // mine
		AddPowerUp(-1);
		return grid [row, col].mine;
	}

	public GameObject AddHouse(int row, int col) {
		Vector3 location = grid [row, col].button.transform.position;
		// if there is mine, it explodes and the player's new house explodes
		if (grid [row, col].hasMine) {
			return null;
		} else {
			if (playerSide == 1) {
				if (grid [row, col].buildingType == -1) { // if power up
					Destroy(grid[row, col].building);
					//set power up
					// AddPowerUp(randomness.Next(3));
				}

				grid[row, col].building = Instantiate(house1, location, Quaternion.identity);
				grid[row, col].player = 1;
			}
			else {
				if (grid [row, col].buildingType == -1) { // if power up
					Destroy(grid[row, col].building); 
					//setpowerup
					// AddPowerUp(randomness.Next(3));
				}

				grid [row, col].building = Instantiate (house2, location, Quaternion.identity);
				grid[row, col].player = 2;
			}

			grid[row, col].buildingType = 1;
			newRow1 = row;
			newCol1 = col;
			newBuilding = true;
			return grid [row, col].building;
		}
	}

	public void AddPowerUp(){
		int powerUpNumber = randomness.Next (3);
		if (playerSide == 1) {
			power1 = powerUpNumber;
			currentPowerUp1.AddPowerUp (powerUpNumber);
		} else {
			power2 = powerUpNumber;
			currentPowerUp2.AddPowerUp (powerUpNumber);
		}
	}

	public void AddPowerUp(int powerUpNumber){
		if (playerSide == 1) {
			power1 = powerUpNumber;
			currentPowerUp1.AddPowerUp (powerUpNumber);
		} else {
			power2 = powerUpNumber;
			currentPowerUp2.AddPowerUp (powerUpNumber);
		}
	}

	public void SetReference(GameObject newHouse, int row, int col){
		grid [row, col].building = newHouse;
	}

	public void SetReferenceMine(GameObject newMine, int row, int col){
		if (newMine == null) {
			grid [row, col].hasMine = true;
			return;
		}
		grid [row, col].mine = newMine;
		grid [row, col].hasMine = true;
	}

	public void AddHouseHammer(int row, int col){
		if (buildNumber == 1) {
			AddHouse (row, col);
			EndTurn ();
		}
		else {
			AddHouse(row, col);
			buildNumber++;
			if (isGameOver ()) {
				EndTurn ();
				return;
			}
		}
	}


	#endregion

	#region Game Logic

	public int GetPlayer(){
		return playerSide;
	}
		
	public bool hasPowerUp(int row, int col){
		if (grid [row, col].buildingType == -1) {
			return true;
		}
		return false;
	}

	public void UsePowerUp(int player){
		if (player != playerSide) {
			return;
		}
		if (player == 1) {
			canvasController.powerUp = power1;
			SetPowerUp (power1);
			AddPowerUp (-1);
		} else {
			canvasController.powerUp = power2;
			SetPowerUp(power2);
			AddPowerUp (-1);
		}
	}

	public void CheckBuildings(){
		if (newBuilding) {
			// cannot build hotel and row at the same time
			if (!CheckHotel (playerSide)) {
				int[] multiples = CheckMultiple (playerSide, newRow1, newCol1);
				if (playerSide == 1) {
					score1 += multiples [0] * 10 + multiples [1] * 20 + multiples [2] * 50;
				} else {
					score2 += multiples [0] * 10 + multiples [1] * 20 + multiples [2] * 50;
				}
			}
		}
		playerScore1.text = score1.ToString ();
		playerScore2.text = score2.ToString ();
		newBuilding = false;
	}

	public void HostRageQuit(){
		matchController.GameOver (2);
	}

	public void ClientRageQuit(){
		matchController.GameOver (1);
	}

	public void EndTurn() {
		moveCount++;

		CheckBuildings();

		if (playerSide == 1) {
			playerSide = 2;
			currentPlayer.color = new Color (0.1F, 0.13F, 0.8F, 1.0F); // blue
			currentPlayer.text = "Player 2";
		} else {
			playerSide = 1;
			currentPlayer.color = new Color (0.67F, 0.02F, 0.02F, 1.0F); // red
			currentPlayer.text = "Player 1";
		}

		// no more possible moves
		if (isGameOver()) {
			if (score1 > score2) { // player 1 win
				gameOver.color = new Color (0.67F, 0.02F, 0.02F, 1.0F);
				gameOver.text = "Player 1 Wins!";
				matchController.GameOver (1);
			} else if (score2 > score1) { // player 2 win
				gameOver.color = new Color (0.1F, 0.13F, 0.8F, 1.0F);
				gameOver.text = "Player 2 Wins!";
				matchController.GameOver (2);
			} else { // draw
				gameOver.color = Color.grey;
				gameOver.text = "Draw!";
				matchController.GameOver ();
			}

		}

		CmdSpawnPowerUp();
		newBuilding = false;
		SetPowerUp (-1);
		currentPowerUp1.SetInteractable (true);
		currentPowerUp2.SetInteractable (true);
		buildNumber = 0;
		canvasController.powerUp = -1;
		SetBuildingsInteractable (false);
	}

	public bool HasMine(int row, int col){
		return grid [row, col].hasMine;
	}

	public void StepMine(int row, int col){
		Instantiate(explosionMine, grid[row, col].button.transform.position, Quaternion.identity);
		NetworkServer.UnSpawn (grid [row, col].mine);
		Destroy (grid [row, col].mine);
		grid [row, col].hasMine = false;
		grid [row, col].button.interactable = true;
		if (playerSide == 1) {
			score1 = Math.Max (0, score1 - 10);
		} else {
			score2 = Math.Max (0, score2 - 10);
		}
		if (gamePowerUp == 1) {
			buildNumber = 1;
		}
		playerScore1.text = score1.ToString ();
		playerScore2.text = score2.ToString ();
	}


	public void Bomb(int row, int col) {
		Instantiate(explosionDynamite, grid[row, col].button.transform.position, Quaternion.identity);
		if (grid [row, col].buildingType <= 0) {
			// do nothing
		} else {
			IDictionary<int, int> pointsDict = new Dictionary<int, int>() {
				{0,0},
				{1,0},
				{3,5},
				{4,10},
				{5,25},
				{9,15}
			};
			if (grid [row, col].player == 1) {
				if (grid [row, col].buildingType == 9) {
					score1 = Math.Max (0, score1 - 15);
				} else {
					score1 = Math.Max (0, score1 - pointsDict [grid [row, col].buildingType1] - pointsDict [grid [row, col].buildingType2] - pointsDict [grid [row, col].buildingType3] - pointsDict [grid [row, col].buildingType4]);
				}
			} else if (grid [row, col].player == 2) {
				if (grid [row, col].buildingType == 9) {
					score2 = Math.Max (0, score2 - 15);
				} else {
					score2 = Math.Max (0, score2 - pointsDict [grid [row, col].buildingType1] - pointsDict [grid [row, col].buildingType2] - pointsDict [grid [row, col].buildingType3] - pointsDict [grid [row, col].buildingType4]);
				}
			} else if (grid[row, col].buildingType == 10) { // boulder
				if (playerSide == 1) {
					score1 += 5;
				} else {
					score2 += 5;
				}
			}
			foreach (GridSpace connectedGrid in grid[row, col].connected) {
				connectedGrid.player = 0;
				connectedGrid.buildingType = 0;
				connectedGrid.connected = new List<GridSpace>();
				Destroy (connectedGrid.building);
			}
			foreach (GridSpace connectedGrid in grid[row, col].connected1) {
				connectedGrid.player = 0;
				connectedGrid.buildingType = 0;
				connectedGrid.buildingType1 = 0;
				connectedGrid.connected1 = new List<GridSpace>();
				Destroy (connectedGrid.building);
			}
			foreach (GridSpace connectedGrid in grid[row, col].connected2) {
				connectedGrid.player = 0;
				connectedGrid.buildingType = 0;
				connectedGrid.buildingType2 = 0;
				connectedGrid.connected2 = new List<GridSpace>();
				Destroy (connectedGrid.building);
			}
			foreach (GridSpace connectedGrid in grid[row, col].connected3) {
				connectedGrid.player = 0;
				connectedGrid.buildingType = 0;
				connectedGrid.buildingType3 = 0;
				connectedGrid.connected3 = new List<GridSpace>();
				Destroy (connectedGrid.building);
			}
			foreach (GridSpace connectedGrid in grid[row, col].connected4) {
				connectedGrid.player = 0;
				connectedGrid.buildingType = 0;
				connectedGrid.buildingType4 = 0;
				connectedGrid.connected4 = new List<GridSpace>();
				Destroy (connectedGrid.building);
			}
			grid[row, col].player = 0;
			grid[row, col].buildingType = 0;
			grid[row, col].buildingType1 = 0;
			grid[row, col].buildingType2 = 0;
			grid[row, col].buildingType3 = 0;
			grid[row, col].buildingType4 = 0;
			grid[row, col].connected = new List<GridSpace>();
			grid[row, col].connected1 = new List<GridSpace>();
			grid[row, col].connected2 = new List<GridSpace>();
			grid[row, col].connected3 = new List<GridSpace>();
			grid[row, col].connected4 = new List<GridSpace>();
			Destroy (grid[row, col].building);
		}
		playerScore1.text = score1.ToString ();
		playerScore2.text = score2.ToString ();
	}

	public bool isGameOver(){
		// if any square is empty game is still not over
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 5; j++) {
				if (grid [i, j].buildingType <= 0) {
					return false;
				}
			}
		}

		// corner cases where game is still not over
		if (playerSide == 1 && currentPowerUp1.GetPowerUp () == 2) {
			return false;
		} else if (playerSide == 2 && currentPowerUp2.GetPowerUp () == 2) {
			return false;
		}

		return true;
	}

    bool CheckHotel(int playerSide) {
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 4; j++) {
                if (grid[i,j].player == playerSide) {
					if ((grid[i+1,j].player == playerSide) && (grid[i+1,j].buildingType == 1) && (grid[i,j+1].player == playerSide) && (grid[i,j+1].buildingType == 1) && (grid[i+1,j+1].player == playerSide) && grid[i+1,j+1].buildingType == 1) {
                        Destroy(grid[i, j].building);
                        Destroy(grid[i+1, j].building);
                        Destroy(grid[i, j+1].building);
                        Destroy(grid[i+1, j+1].building);
                        Vector3 location = (grid[i, j].button.transform.position + grid[i+1, j].button.transform.position + grid[i, j+1].button.transform.position + grid[i+1, j+1].button.transform.position) / 4;
                        if (playerSide == 1) {
                            grid[i, j].building = Instantiate(hotel1, location, Quaternion.identity);
							score1 += 30;
                        }
                        else {
                            grid[i, j].building = Instantiate(hotel2, location, Quaternion.identity);
							score2 += 30;
                        }
                        grid[i+1, j].building = grid[i, j + 1].building = grid[i + 1, j + 1].building = grid[i, j].building;
                        grid[i + 1, j].buildingType = grid[i, j + 1].buildingType = grid[i + 1, j + 1].buildingType = grid[i, j].buildingType = 9;
						grid[i, j].connected = new List<GridSpace>(new GridSpace[3] { grid[i+1, j], grid[i, j+1], grid[i+1, j+1] });
						grid[i+1, j].connected = new List<GridSpace>(new GridSpace[3] { grid[i, j], grid[i, j+1], grid[i+1, j+1] });
						grid[i, j+1].connected = new List<GridSpace>(new GridSpace[3] { grid[i+1, j], grid[i, j], grid[i+1, j+1] });
						grid[i+1, j+1].connected = new List<GridSpace>(new GridSpace[3] { grid[i+1, j], grid[i, j+1], grid[i, j] });
						NetworkServer.SpawnWithClientAuthority (grid [i, j].building, playerController.gameObject);
						return true;
                    }
                }
            }
        }
        return false;
    }
		
	int[] CheckMultiple(int playerSide, int newRow, int newCol) {

		IDictionary<int, int> dict = new Dictionary<int, int>() {
			{3,0},
			{4,0},
			{5,0}
		};

		// horizontal
		int colLeft = newCol;
		int colRight = newCol;
		int rowLeft = newRow;
		int rowRight = newRow;

		for (int i = 1; i < 5; i++) {
			if (colLeft-1 >= 0) {
				if ((grid [newRow, colLeft - 1].player == playerSide) && (grid [newRow, colLeft - 1].buildingType == 1 || grid [newRow, colLeft - 1].buildingType == 6)) {
					colLeft = colLeft - 1;
					continue;
				}
			}
			break;
		}

		for (int i = 1; i < 5; i++) {
			if (colRight+1 < 5) {
				if ((grid [newRow, colRight + 1].player == playerSide) && (grid [newRow, colRight + 1].buildingType == 1 || grid [newRow, colRight + 1].buildingType == 6)) {
					colRight = colRight + 1;
					continue;
				}
			}
			break;
		}

		if (colRight - colLeft > 1) {
			for (int i = colLeft; i <= colRight; i++) {
				Destroy (grid [newRow, i].building);
				if (playerSide == 1) {
					grid[newRow, i].building = Instantiate(specialHouse1, grid[newRow, i].button.transform.position, Quaternion.identity);
					NetworkServer.SpawnWithClientAuthority (grid [newRow, i].building, playerController.gameObject);
				} 
				else {
					grid[newRow, i].building = Instantiate(specialHouse2, grid[newRow, i].button.transform.position, Quaternion.identity);
					NetworkServer.SpawnWithClientAuthority (grid [newRow, i].building, playerController.gameObject);
				}
					
				grid [newRow, i].buildingType = 6;
				grid [newRow, i].buildingType1 = colRight - colLeft + 1;
				for (int j = colLeft; j <= colRight; j++) {
					if (i == j) {
						continue;
					}

					grid [newRow, j].connected1.Add (grid [newRow, i]);
				}
			}
			dict[colRight - colLeft + 1] = dict [colRight - colLeft + 1] + 1;
		}

		// vertical
		colLeft = newCol;
		colRight = newCol;
		rowLeft = newRow;
		rowRight = newRow;

		for (int i = 1; i < 5; i++) {
			if (rowLeft-1 >= 0) {
				if ((grid [rowLeft-1, newCol].player == playerSide) && (grid [rowLeft-1, newCol].buildingType == 1 || grid [rowLeft-1, newCol].buildingType == 6)) {
					rowLeft = rowLeft - 1;
					continue;
				}
			}
			break;
		}

		for (int i = 1; i < 5; i++) {
			if (rowRight+1 < 5) {
				if ((grid [rowRight+1, newCol].player == playerSide) && (grid [rowRight+1, newCol].buildingType == 1 || grid [rowRight+1, newCol].buildingType == 6)) {
					rowRight = rowRight + 1;
					continue;
				}
			}
			break;
		}

		if (rowRight - rowLeft > 1) {
			// Debug.Log ("Success");
			for (int i = rowLeft; i <= rowRight; i++) {
				Destroy (grid [i, newCol].building);
				if (playerSide == 1) {
					grid[i, newCol].building = Instantiate(specialHouse1, grid[i, newCol].button.transform.position, Quaternion.identity);
					NetworkServer.SpawnWithClientAuthority (grid [i, newCol].building, playerController.gameObject);
				} 
				else {
					grid[i, newCol].building = Instantiate(specialHouse2, grid[i, newCol].button.transform.position, Quaternion.identity);
					NetworkServer.SpawnWithClientAuthority (grid [i, newCol].building, playerController.gameObject);
				}

				grid [i, newCol].buildingType = 6;
				grid [i, newCol].buildingType2 = rowRight - rowLeft + 1;
				for (int j = rowLeft; j <= rowRight; j++) {
					if (i == j) {
						continue;
					}

					grid [j, newCol].connected2.Add (grid [i, newCol]);
				}
			}
			dict[rowRight - rowLeft + 1] = dict [rowRight - rowLeft + 1] + 1;
		}

		//diagonal (top left to bottom right)
		colLeft = newCol;
		colRight = newCol;
		rowLeft = newRow;
		rowRight = newRow;

		for (int i = 1; i < 5; i++) {
			if (rowLeft-1 >= 0 && colLeft-1 >= 0) {
				if ((grid [rowLeft-1, colLeft-1].player == playerSide) && (grid [rowLeft-1, colLeft-1].buildingType == 1 || grid [rowLeft-1, colLeft-1].buildingType == 6)) {
					rowLeft = rowLeft - 1;
					colLeft = colLeft - 1;
					continue;
				}
			}
			break;
		}

		for (int i = 1; i < 5; i++) {
			if (rowRight+1 < 5 && colRight+1 < 5) {
				if ((grid [rowRight+1, colRight+1].player == playerSide) && (grid [rowRight+1, colRight+1].buildingType == 1 || grid [rowRight+1, colRight+1].buildingType == 6)) {
					rowRight = rowRight + 1;
					colRight = colRight + 1;
					continue;
				}
			}
			break;
		}

		if (rowRight - rowLeft > 1) {
			int j = colLeft;
			for (int i = rowLeft; i <= rowRight; i++) {
				Destroy (grid [i, j].building);
				if (playerSide == 1) {
					grid[i, j].building = Instantiate(specialHouse1, grid[i, j].button.transform.position, Quaternion.identity);
					NetworkServer.SpawnWithClientAuthority (grid [i, j].building, playerController.gameObject);
				} 
				else {
					grid[i, j].building = Instantiate(specialHouse2, grid[i, j].button.transform.position, Quaternion.identity);
					NetworkServer.SpawnWithClientAuthority (grid [i, j].building, playerController.gameObject);
				}

				grid[i, j].buildingType = 6;
				grid[i, j].buildingType3 = rowRight - rowLeft + 1;

				int b = colLeft;
				for (int a = rowLeft; a <= rowRight; a++) {
					if (i == a) {
						b++;
						continue;
					}

					grid[a, b].connected3.Add (grid[i,j]);
					b++;
				}
				j++;
			}
			dict[rowRight - rowLeft + 1] = dict[rowRight - rowLeft + 1] + 1;
		}
			
		// diagonal (bottom left to top right)
		colLeft = newCol;
		colRight = newCol;
		rowLeft = newRow;
		rowRight = newRow;

		for (int i = 1; i < 5; i++) {
			if (rowLeft+1 < 5 && colLeft-1 >= 0) {
				if ((grid [rowLeft+1, colLeft-1].player == playerSide) && (grid [rowLeft+1, colLeft-1].buildingType == 1 || grid [rowLeft+1, colLeft-1].buildingType == 6)) {
					rowLeft = rowLeft + 1;
					colLeft = colLeft - 1;
					continue;
				}
			}
			break;
		}

		for (int i = 1; i < 5; i++) {
			if (rowRight-1 >= 0 && colRight+1 < 5) {
				if ((grid [rowRight-1, colRight+1].player == playerSide) && (grid [rowRight-1, colRight+1].buildingType == 1 || grid [rowRight-1, colRight+1].buildingType == 6)) {
					rowRight = rowRight - 1;
					colRight = colRight + 1;
					continue;
				}
			}
			break;
		}

		if (colRight - colLeft > 1) {
			int i = rowLeft;
			for (int j = colLeft; j <= colRight; j++) {
				Destroy (grid [i, j].building);
				if (playerSide == 1) {
					grid[i, j].building = Instantiate(specialHouse1, grid[i, j].button.transform.position, Quaternion.identity);
					NetworkServer.SpawnWithClientAuthority (grid [i, j].building, playerController.gameObject);
				} 
				else {
					grid[i, j].building = Instantiate(specialHouse2, grid[i, j].button.transform.position, Quaternion.identity);
					NetworkServer.SpawnWithClientAuthority (grid [i, j].building, playerController.gameObject);
				}

				grid[i, j].buildingType = 6;
				grid[i, j].buildingType4 = colRight - colLeft + 1;

				int a = rowLeft;
				for (int b = colLeft; b <= colRight; b++) {
					if (j == b) {
						a--;
						continue;
					}

					grid[a, b].connected4.Add (grid[i,j]);
					a--;
				}
				i--;
			}
			dict[colRight - colLeft + 1] = dict[colRight - colLeft + 1] + 1;
		}

		// Debug.Log (dict [3] + ", " + dict [4] + ", " + dict [5]);
		return new int[3] {dict[3],dict[4],dict[5]};
    }

	#endregion
}

class GridSpace {
    /*
    Building type list:
    -1: Power-up
    0: Empty
    1: Player 1 House
    3: 3-Row
    4: 4-Row
    5: 5-Row
    6: 3-row, 4-row and/or 5-row
    9: Hotel
    10: Obstacle
    */
    public int buildingType;
	public int player;
	public List<GridSpace> connected;
    public Button button;
    public GameObject building;
	public GameObject mine;
	public bool hasMine;
	public int index;
	/* used when rows are formed
	   1: horizontal
	   2: vertical
	   3: diagonal (top left to bottom right)
	   4: diagonal (bottom left to top right)
	 */
	public int buildingType1;
	public int buildingType2;
	public int buildingType3;
	public int buildingType4;
	public List<GridSpace> connected1;
	public List<GridSpace> connected2;
	public List<GridSpace> connected3;
	public List<GridSpace> connected4;
}