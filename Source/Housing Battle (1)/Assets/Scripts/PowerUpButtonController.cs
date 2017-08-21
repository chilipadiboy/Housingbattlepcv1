using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PowerUpButtonController : MonoBehaviour {

	public int player;
	public Sprite powerUpHammer;
	public Sprite powerUpDynamite;
	public Sprite powerUpMine1;
	public Sprite powerUpMine2;
	public Sprite empty;
	public Image powerImage;

	private int powerUp; // 0=mine 1=hammer 2=dynamite -1=no Power up
	private Button powerButton;
	private GameController gameController;

	private void Awake(){
		powerUp = -1;
		powerImage.sprite = empty;
		powerButton = GetComponentInParent<Button> ();
	}

	public void SetGameControllerReference(GameController controller){
		gameController = controller;
	}

	public void AddPowerUp(int powerUp){
		if (powerUp == -1) {
			powerImage.sprite = empty;
		} else if (powerUp == 0) {
			if (player == 1) {
				powerImage.sprite = powerUpMine1;
			} else {
				powerImage.sprite = powerUpMine2;
			}
		} else if (powerUp == 1) {
			powerImage.sprite = powerUpHammer;
		} else {
			powerImage.sprite = powerUpDynamite;
		}
	}

	public void UsePowerUp(){
		if (powerUp == -1 || gameController.GetPlayer() != player) {
			// do nothing
		} else {
			gameController.SetPowerUp (powerUp);
			powerUp = -1;
			powerImage.sprite = empty;
		}
	}

	public int GetPowerUp(){
		return powerUp;
	}

	public void SetInteractable(bool interactable){
		powerButton.interactable = interactable;
	}
}
