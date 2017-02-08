using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUIController : MonoBehaviour
{

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void NewGameButtonPressed()
	{
		var newGamePanel = (GameObject)Instantiate( Resources.Load( "NewGamePanel" ) );
		CancelButtonPressed();
	}

	public void OptionsButtonPressed()
	{
		var optionsPanel = (GameObject)Instantiate( Resources.Load( "GameOptionsPanel" ) );
		CancelButtonPressed();
	}

	public void GameRulesButtonPressed()
	{
		var optionsPanel = (GameObject)Instantiate( Resources.Load( "GameRulesPanel" ) );
		CancelButtonPressed();
	}

	public void ToggleDebugButtonPressed()
	{
		var mainBoard = GameObject.FindObjectOfType<MainBoardSceneController>();
		mainBoard.ToggleDebugUI();
	}

	public void ExitButtonPressed()
	{
		Application.Quit();
	}


	public void CancelButtonPressed()
	{
		Destroy( this.gameObject );
	}
}
