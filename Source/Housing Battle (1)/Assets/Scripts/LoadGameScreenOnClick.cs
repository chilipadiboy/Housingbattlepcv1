using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGameScreenOnClick : MonoBehaviour {

	public void LoadGameScreen(int GameScreenInd)
    {
        SceneManager.LoadScene(GameScreenInd);
    }
}
