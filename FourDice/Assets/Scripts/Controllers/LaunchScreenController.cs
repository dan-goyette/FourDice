using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Constants;
using UnityEngine;
using UnityEngine.UI;

public class LaunchScreenController : MonoBehaviour
{
	public Image FadeOverlayImage;
	public Text FourDiceLogo;
	public Image UIContainer;
	public Button NewGameButton;
	public Button GameRulesButton;
	private Text _newGameButtonText;
	private Text _gameRulesButtonText;

	public Text VersionText;


	public Button OptionsButton;
	private Text _optionsButtonText;


	float _fadeOverlayAlpha = 1;
	float _targetLogoAlpha = 0;
	float _targetVersionAlpha = 0;
	float _targetUIContainerAlpha = 0;

	float _targetButtonAlpha = 0;


	// Use this for initialization
	void Start()
	{

		var animationSpeed = PlayerPrefs.GetInt( "AnimationSpeed" );
		if ( animationSpeed >= 1 ) {
			Time.timeScale = animationSpeed;
		}

		VersionText.text = string.Format( "v{0}", ApplicationConstants.ApplicationVersion );

		StartCoroutine( StartIntro() );
	}

	private IEnumerator StartIntro()
	{

		var newLogoColor = new Color( FourDiceLogo.color.r, FourDiceLogo.color.g, FourDiceLogo.color.b, 0 );
		FourDiceLogo.color = newLogoColor;

		var newUIContainerColor = new Color( UIContainer.color.r, UIContainer.color.g, UIContainer.color.b, 0 );
		UIContainer.color = newUIContainerColor;


		var newVersionColor = new Color( VersionText.color.r, VersionText.color.g, VersionText.color.b, 0 );
		VersionText.color = newVersionColor;

		var newNewGameButtonImageColor = new Color( NewGameButton.image.color.r, NewGameButton.image.color.g, NewGameButton.image.color.b, 0 );
		NewGameButton.image.color = newNewGameButtonImageColor;

		_newGameButtonText = NewGameButton.GetComponentInChildren<Text>();
		var newNewGameButtonTextColor = new Color( _newGameButtonText.color.r, _newGameButtonText.color.g, _newGameButtonText.color.b, 0 );
		_newGameButtonText.color = newNewGameButtonTextColor;


		var newOptionsButtonImageColor = new Color( OptionsButton.image.color.r, OptionsButton.image.color.g, OptionsButton.image.color.b, 0 );
		OptionsButton.image.color = newOptionsButtonImageColor;

		_optionsButtonText = OptionsButton.GetComponentInChildren<Text>();
		var newOptionsButtonTextColor = new Color( _optionsButtonText.color.r, _optionsButtonText.color.g, _optionsButtonText.color.b, 0 );
		_optionsButtonText.color = newOptionsButtonTextColor;





		var newGameRulesButtonImageColor = new Color( GameRulesButton.image.color.r, GameRulesButton.image.color.g, GameRulesButton.image.color.b, 0 );
		GameRulesButton.image.color = newGameRulesButtonImageColor;

		_gameRulesButtonText = GameRulesButton.GetComponentInChildren<Text>();
		var newGameRulesButtonTextColor = new Color( _gameRulesButtonText.color.r, _gameRulesButtonText.color.g, _gameRulesButtonText.color.b, 0 );
		_gameRulesButtonText.color = newGameRulesButtonTextColor;



		yield return new WaitUntil( () => _fadeOverlayAlpha <= 0 );

		// Create Dice
		StartCoroutine( CreateDice() );

		yield return new WaitForSeconds( 2 );
		_targetLogoAlpha = 1;
		_targetVersionAlpha = 1;
		_targetUIContainerAlpha = .3f;
		_targetButtonAlpha = 1;
	}

	private IEnumerator CreateDice()
	{
		yield return new WaitForSeconds( 0.1f );
		for ( var i = 0; i < 4; i++ ) {
			GameObject die = (GameObject)Instantiate( Resources.Load( "Die" ) );
			var dieController = die.GetComponent<DieController>();

			die.transform.position = new Vector3( 5 + 3 * i, 6f, -1 * (5 + 3 * i) );

			die.GetComponent<Rigidbody>().isKinematic = false;
			die.GetComponent<Rigidbody>().AddForceAtPosition( new Vector3( UnityEngine.Random.Range( 8, 12 ), UnityEngine.Random.Range( -2, 0 ), UnityEngine.Random.Range( 8, 12 ) ) * 25, new Vector3( 2, 2, 2 ), ForceMode.Impulse );
		}
	}

	// Update is called once per frame
	void Update()
	{
		if ( _fadeOverlayAlpha >= 0 ) {
			SetImageAlpha( FadeOverlayImage, _fadeOverlayAlpha );
			_fadeOverlayAlpha -= Time.deltaTime / 1.5f;
		}

		if ( FourDiceLogo.color.a != _targetLogoAlpha ) {
			var newAlpha = FourDiceLogo.color.a + Time.deltaTime * 2f;
			var newColor = new Color( FourDiceLogo.color.r, FourDiceLogo.color.g, FourDiceLogo.color.b, newAlpha );
			FourDiceLogo.color = newColor;
		}

		if ( VersionText.color.a != _targetVersionAlpha ) {
			var newAlpha = VersionText.color.a + Time.deltaTime * 2f;
			var newColor = new Color( VersionText.color.r, VersionText.color.g, VersionText.color.b, newAlpha );
			VersionText.color = newColor;
		}

		if ( UIContainer.color.a < _targetUIContainerAlpha ) {
			var newAlpha = UIContainer.color.a + Time.deltaTime;
			var newColor = new Color( UIContainer.color.r, UIContainer.color.g, UIContainer.color.b, newAlpha );
			UIContainer.color = newColor;
		}


		if ( NewGameButton.image.color.a < _targetButtonAlpha ) {
			var newAlpha = NewGameButton.image.color.a + Time.deltaTime;
			var newButtonColor = new Color( NewGameButton.image.color.r, NewGameButton.image.color.g, NewGameButton.image.color.b, newAlpha );
			NewGameButton.image.color = newButtonColor;

			var newTextColor = new Color( _newGameButtonText.color.r, _newGameButtonText.color.g, _newGameButtonText.color.b, newAlpha );
			_newGameButtonText.color = newTextColor;
		}


		if ( OptionsButton.image.color.a < _targetButtonAlpha ) {
			var newAlpha = OptionsButton.image.color.a + Time.deltaTime;
			var newButtonColor = new Color( OptionsButton.image.color.r, OptionsButton.image.color.g, OptionsButton.image.color.b, newAlpha );
			OptionsButton.image.color = newButtonColor;

			var newTextColor = new Color( _optionsButtonText.color.r, _optionsButtonText.color.g, _optionsButtonText.color.b, newAlpha );
			_optionsButtonText.color = newTextColor;
		}


		if ( GameRulesButton.image.color.a < _targetButtonAlpha ) {
			var newAlpha = GameRulesButton.image.color.a + Time.deltaTime;
			var newButtonColor = new Color( GameRulesButton.image.color.r, GameRulesButton.image.color.g, GameRulesButton.image.color.b, newAlpha );
			GameRulesButton.image.color = newButtonColor;

			var newTextColor = new Color( _gameRulesButtonText.color.r, _gameRulesButtonText.color.g, _gameRulesButtonText.color.b, newAlpha );
			_gameRulesButtonText.color = newTextColor;
		}
	}

	private void SetImageAlpha( Image image, float newAlpha )
	{
		var newColor = new Color( image.color.r, image.color.g, image.color.b, newAlpha );
		image.color = newColor;
	}


	public void NewGameButtonPressed()
	{
		Instantiate( Resources.Load( "NewGamePanel" ) );
	}


	public void GameRulesButtonPressed()
	{
		Instantiate( Resources.Load( "GameRulesPanel" ) );
	}


	public void OptionsButtonPressed()
	{
		Instantiate( Resources.Load( "GameOptionsPanel" ) );
	}

}
