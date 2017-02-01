using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempButtonController : MonoBehaviour
{

	MainBoardSceneController mainBoard;

	// Use this for initialization
	void Start()
	{
		mainBoard = GameObject.FindObjectOfType<MainBoardSceneController>();
	}


	void OnMouseOver()
	{
		if ( Input.GetMouseButtonDown( 0 ) ) {
			mainBoard.RollSelectedDice( isInitialDiceRoll: false );
		}
	}




	// Update is called once per frame
	void Update()
	{

	}
}
