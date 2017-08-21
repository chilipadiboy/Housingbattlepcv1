using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour {

    public Button button;
    public int buttonNumber;
    public Button powerUpButton;

    private GameController gameController;
    private int row;
    private int col;
	private int powerUp;

    private void Awake(){
        row = buttonNumber / 5;
        col = buttonNumber % 5;
		powerUp = -1;
    }

    public void SetGameControllerReference(GameController controller){
        gameController = controller;
    }

    public void ButtonPress(){
		// 0=mine 1=hammer 2=dynamite -1=no Power up
		if (powerUp == -1) {
			gameController.CmdAddHouse (row, col);
			// gameController.EndTurn ();
		} else if (powerUp == 0) {
			gameController.AddMine (row, col);
			powerUp = -1;
		} else if (powerUp == 1) {
			button.interactable = false;
			gameController.AddHouseHammer (row, col);
		} else {
			gameController.Bomb (row, col);
			gameController.CmdSetBuildingsInteractable (false);
			gameController.EndTurn ();
		}
    }

	// called when a power up is used, and when turn ends
	public void SetPowerUp(int currPowerUp){
		powerUp = currPowerUp;
	}
}
