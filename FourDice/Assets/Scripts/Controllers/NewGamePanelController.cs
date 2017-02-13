using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.DomainModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGamePanelController : MonoBehaviour
{
	public Toggle Player1HumanToggle;
	public Toggle Player1AIToggle;
	public Dropdown Player1AIDropdown;


	public Toggle Player2HumanToggle;
	public Toggle Player2AIToggle;
	public Dropdown Player2AIDropdown;

	public Button StartGameButton;

	private List<Dropdown.OptionData> _aiOptions;
	private List<AIDefinition> _aiDefinitions;


	// Use this for initialization
	void Start()
	{
		var ais = FourDice.GetAIDefinitions();

		_aiOptions = new List<Dropdown.OptionData>();
		_aiDefinitions = new List<AIDefinition>();


		foreach ( var ai in FourDice.GetAIDefinitions().OrderBy( ai => ai.FriendlyName ) ) {
			var option = new Dropdown.OptionData() {
				text = string.Format( "{0} - Difficulty: {1}", ai.FriendlyName, ai.DifficultyRating )
			};
			_aiOptions.Add( option );
			_aiDefinitions.Add( ai );
		}

		Player1AIDropdown.options = _aiOptions;
		Player2AIDropdown.options = _aiOptions;

		for ( var i = 0; i < _aiOptions.Count; i++ ) {
			if ( _aiOptions[i].text.StartsWith( "BestAI" ) ) {
				Player2AIDropdown.value = i;
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		Player1AIDropdown.interactable = Player1AIToggle.isOn;
		Player1AIDropdown.gameObject.SetActive( Player1AIToggle.isOn );
		Player2AIDropdown.interactable = Player2AIToggle.isOn;
		Player2AIDropdown.gameObject.SetActive( Player2AIToggle.isOn );
	}


	public void StartGameButtonPressed()
	{
		Player1AI = Player1AIToggle.isOn ? _aiDefinitions[Player1AIDropdown.value] : null;
		Player2AI = Player2AIToggle.isOn ? _aiDefinitions[Player2AIDropdown.value] : null;

		Utils.ShowAd( () => SceneManager.LoadScene( "MainBoard" ) );


	}


	public void CancelButtonPressed()
	{
		Destroy( this.gameObject );
	}

	public static AIDefinition Player1AI;
	public static AIDefinition Player2AI;
}
