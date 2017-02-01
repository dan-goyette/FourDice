using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DomainModel;
using UnityEngine;
using UnityEngine.UI;

public class MainBoardSceneController : MonoBehaviour
{
	public Collider DiceKeeperCollider;

	public GameObject[] InitialPlayer1AttackerPlaceHolders;
	public GameObject[] InitialPlayer2AttackerPlaceHolders;
	public GameObject[] InitialPlayer1DefenderPlaceHolders;
	public GameObject[] InitialPlayer2DefenderPlaceHolders;

	public Button StartGameButton;
	public Text Player1TurnLabel;
	public Text Player2TurnLabel;

	Vector3 _mainCameraStandardPosition;
	Vector3 _mainCameraTargetPosition;
	float _mainCameraSlerpTime;

	DieController[] _dice;
	Vector3[] _dicePositions;
	Vector3[] _dicePreRollPositions;
	Vector3[] _diceTargetPositions;
	List<Vector3> _diceTargetEulerPositions;
	float _diceAnimationSlerpTime;
	private bool _animateDice;

	AttackerController[] _player1Attackers;
	AttackerController[] _player2Attackers;
	DefenderController[] _player1Defenders;
	DefenderController[] _player2Defenders;

	LanePositionController[] _lanePositions;

	private FourDice _fourDice;

	private PlayerType _activePlayerType;
	private GameTurnState _gameTurnState;

	// Use this for initialization
	void Start()
	{
		_dice = new DieController[4];
		_dicePositions = new Vector3[4];
		_dicePreRollPositions = new Vector3[4];
		_diceTargetPositions = new Vector3[4];
		_diceTargetEulerPositions = new List<Vector3>();

		_mainCameraStandardPosition = Camera.main.transform.localPosition;
		_mainCameraTargetPosition = _mainCameraStandardPosition;

		DiceKeeperCollider.enabled = false;

		CreateDice();
		CreateGamePieces();

		_lanePositions = GameObject.FindObjectsOfType<LanePositionController>().OrderBy( lp => lp.LanePosition ).ToArray();

		_fourDice = new FourDice( "DefenderBot" );

		Player1TurnLabel.gameObject.SetActive( false );
		Player2TurnLabel.gameObject.SetActive( false );

		_gameTurnState = GameTurnState.Waiting;

	}

	private void CreateGamePieces()
	{
		_player1Attackers = new AttackerController[5];
		for ( var i = 0; i < InitialPlayer1AttackerPlaceHolders.Length; i++ ) {
			var placeHolder = InitialPlayer1AttackerPlaceHolders[i];
			GameObject attacker = (GameObject)Instantiate( Resources.Load( "Attacker" ) );
			var attackerController = attacker.GetComponent<AttackerController>();
			attackerController.PlayerType = PlayerType.Player1;
			attackerController.PieceIndex = i;
			attacker.transform.position = placeHolder.transform.position;
			attackerController.OnSelectionChanged += AttackerController_OnSelectionChanged;
			_player1Attackers[i] = attackerController;
			placeHolder.SetActive( false );
		}

		_player1Defenders = new DefenderController[2];
		for ( var i = 0; i < InitialPlayer1DefenderPlaceHolders.Length; i++ ) {
			var placeHolder = InitialPlayer1DefenderPlaceHolders[i];
			GameObject defender = (GameObject)Instantiate( Resources.Load( "Defender" ) );
			var defenderController = defender.GetComponent<DefenderController>();
			defenderController.PlayerType = PlayerType.Player1;
			defenderController.PieceIndex = i;
			defender.transform.position = placeHolder.transform.position;
			defenderController.OnSelectionChanged += DefenderController_OnSelectionChanged;
			_player1Defenders[i] = defenderController;
			placeHolder.SetActive( false );
		}

		_player2Attackers = new AttackerController[5];
		for ( var i = 0; i < InitialPlayer2AttackerPlaceHolders.Length; i++ ) {
			var placeHolder = InitialPlayer2AttackerPlaceHolders[i];
			GameObject attacker = (GameObject)Instantiate( Resources.Load( "Attacker" ) );
			var attackerController = attacker.GetComponent<AttackerController>();
			attackerController.PlayerType = PlayerType.Player2;
			attackerController.PieceIndex = i;
			attacker.transform.position = placeHolder.transform.position;
			attackerController.OnSelectionChanged += AttackerController_OnSelectionChanged;
			_player2Attackers[i] = attackerController;
			placeHolder.SetActive( false );
		}


		_player2Defenders = new DefenderController[2];
		for ( var i = 0; i < InitialPlayer2DefenderPlaceHolders.Length; i++ ) {
			var placeHolder = InitialPlayer2DefenderPlaceHolders[i];
			GameObject defender = (GameObject)Instantiate( Resources.Load( "Defender" ) );
			var defenderController = defender.GetComponent<DefenderController>();
			defenderController.PlayerType = PlayerType.Player2;
			defenderController.PieceIndex = i;
			defender.transform.position = placeHolder.transform.position;
			_player2Defenders[i] = defenderController;
			placeHolder.SetActive( false );
		}

		foreach ( var gamePiece in _player1Attackers.Cast<SelectableObjectController>()
			.Union( _player2Attackers ).Union( _player1Defenders ).Union( _player2Defenders ) ) {
			gamePiece.SetSelectable( false );
			gamePiece.SetDeselectable( true );
			gamePiece.IsSelected = false;
		}
	}



	private void CreateDice()
	{
		for ( var i = 0; i < 4; i++ ) {
			GameObject die = (GameObject)Instantiate( Resources.Load( "Die" ) );
			var dieController = die.GetComponent<DieController>();
			_dice[i] = dieController;
			die.transform.position = _dicePositions[i] = _diceTargetPositions[i] = new Vector3( -3.75f + 2.5f * i, .75f, 5 );
			_dicePreRollPositions[i] = new Vector3( -4 + 2.5f * i, 8f, -4 );
			die.GetComponent<Rigidbody>().isKinematic = true;
			dieController.OnSelectionChanged += DieController_OnSelectionChanged;

			// Start a game with all 4s.
			die.transform.localEulerAngles = new Vector3( 0, 0, 90 );
		}
	}

	private void DefenderController_OnSelectionChanged( object sender, SelectableObjectSelectionChangedEvent arg )
	{
		if ( arg.IsSelected ) {
			_lastSelectedPiece = (GamePieceController)sender;
		}
	}

	private void AttackerController_OnSelectionChanged( object sender, SelectableObjectSelectionChangedEvent arg )
	{
		if ( arg.IsSelected ) {
			_lastSelectedPiece = (GamePieceController)sender;
		}
	}

	private void DieController_OnSelectionChanged( object sender, SelectableObjectSelectionChangedEvent arg )
	{

	}

	// Update is called once per frame
	void Update()
	{
		if ( _animateDice ) {
			_diceAnimationSlerpTime += Time.deltaTime / 2f;
			for ( var i = 0; i < 4; i++ ) {
				if ( _dice[i].IsRolling ) {
					_dice[i].transform.localPosition = Vector3.Slerp( _dice[i].transform.localPosition, _diceTargetPositions[i], _diceAnimationSlerpTime );
				}
			}

			if ( _diceTargetEulerPositions.Any() ) {
				for ( var i = 0; i < 4; i++ ) {
					if ( _dice[i].IsRolling ) {
						_dice[i].transform.localRotation = Quaternion.Slerp( _dice[i].transform.localRotation, Quaternion.Euler( _diceTargetEulerPositions[i].x, _diceTargetEulerPositions[i].y, _diceTargetEulerPositions[i].z ), _diceAnimationSlerpTime );
					}
				}
			}
		}
		if ( _mainCameraTargetPosition != Camera.main.transform.localPosition ) {
			_mainCameraSlerpTime += Time.deltaTime / 2f;
			Camera.main.transform.localPosition = Vector3.Slerp( Camera.main.transform.localPosition, _mainCameraTargetPosition, _mainCameraSlerpTime );
		}
	}


	public void StartGameButtonPressed()
	{
		StartGameButton.gameObject.SetActive( false );

		StartCoroutine( InitiateStartGame() );
	}



	bool _player1InitialDiceRolling;
	bool _player2InitialDiceRolling;
	GamePieceController _lastSelectedPiece;

	private IEnumerator InitiateStartGame()
	{

		int player1Score = 0;
		int player2Score = 0;

		while ( player1Score == player2Score ) {

			// Roll player 1's dice
			_dice[0].IsSelected = true;
			_dice[1].IsSelected = true;
			_player1InitialDiceRolling = true;

			RollSelectedDice( isInitialDiceRoll: true, callback: () => _player1InitialDiceRolling = false );

			yield return new WaitUntil( () => !_player1InitialDiceRolling );

			_dice[2].IsSelected = true;
			_dice[3].IsSelected = true;
			_player2InitialDiceRolling = true;

			RollSelectedDice( isInitialDiceRoll: true, callback: () => _player2InitialDiceRolling = false );

			yield return new WaitUntil( () => !_player2InitialDiceRolling );

			player1Score = _dice[0].GetDieValue().Value + _dice[1].GetDieValue().Value;
			player2Score = _dice[2].GetDieValue().Value + _dice[3].GetDieValue().Value;
		}

		for ( var i = 0; i < _dice.Length; i++ ) {
			_fourDice.GameState.Dice[i].Value = _dice[i].GetDieValue().Value;
		}


		SetActivePlayer( player1Score > player2Score ? PlayerType.Player1 : PlayerType.Player2 );


		while ( true ) {
			// Main Loop - Observe changes to the state, potentially fall into an animation.
			if ( _gameTurnState == GameTurnState.Waiting ) {
				// TODO - Not sure we'll use this.
			}
			else if ( _gameTurnState == GameTurnState.DieSelection ) {
				_lastSelectedPiece = null;

				// Determine if all dice are selected.
				if ( _dice.Count( d => d.IsSelected ) == 2 ) {

					List<int> dieIndices = new List<int>();
					for ( var i = 0; i < _dice.Length; i++ ) {
						var die = _dice[i];
						die.SetSelectable( false );
						die.SetDeselectable( false );
						if ( die.IsSelected ) {
							dieIndices.Add( i );
						}
					}

					var movesExist = MakePiecesSelectable( dieIndices );

					if ( movesExist ) {
						_gameTurnState = GameTurnState.FirstPieceSelection;
					}
					else {
						AwaitTurnFinalization();
					}
				}
			}
			else if ( _gameTurnState == GameTurnState.FirstPieceSelection ) {
				if ( _lastSelectedPiece != null ) {
					_gameTurnState = GameTurnState.FirstPieceTargetSelection;
				}
			}
			else if ( _gameTurnState == GameTurnState.FirstPieceTargetSelection ) {
				var attackers = _activePlayerType == PlayerType.Player1 ? _player1Attackers : _player2Attackers;
				var defenders = _activePlayerType == PlayerType.Player1 ? _player1Defenders : _player2Defenders;

				foreach ( var piece in attackers.Cast<GamePieceController>().Union( defenders ) ) {
					piece.SetSelectable( false );
					piece.SetDeselectable( false );
				}

				foreach ( var lanePosition in _lanePositions ) {
					lanePosition.IsSelected = false;
					lanePosition.SetSelectable( false );
					lanePosition.SetDeselectable( false );
				}

				// Determine which positions are valid.


				HashSet<int> validPositions = new HashSet<int>();

				for ( var i = 0; i < _dice.Length; i++ ) {
					var die = _dice[i];
					if ( die.IsSelected ) {
						foreach ( var direction in Enum.GetValues( typeof( PieceMovementDirection ) ).Cast<PieceMovementDirection>() ) {
							var turnAction = new TurnAction( i, direction, _lastSelectedPiece.PieceType, _lastSelectedPiece.PieceIndex );
							_fourDice.GameState.CurrentPlayerType = _activePlayerType;
							var validationResult = FourDice.ValidateTurnAction( _fourDice.GameState, turnAction, null );
							if ( validationResult.IsValidAction ) {
								validPositions.Add( validationResult.NewLanePosition.Value );
							}
						}
					}
				}

				foreach ( var lanePosition in _lanePositions ) {
					if ( validPositions.Contains( lanePosition.LanePosition ) ) {
						lanePosition.IsSelected = false;
						lanePosition.SetSelectable( true );
						lanePosition.SetDeselectable( true );
					}
				}


			}


			yield return new WaitForEndOfFrame();
		}
	}

	private void AwaitTurnFinalization()
	{
		_gameTurnState = GameTurnState.WaitingForFinalization;
	}

	private bool MakePiecesSelectable( List<int> dieIndices )
	{
		// Allow selection of the pieces that can be selected based on the current dice.

		var attackers = _activePlayerType == PlayerType.Player1 ? _player1Attackers : _player2Attackers;
		var defenders = _activePlayerType == PlayerType.Player1 ? _player1Defenders : _player2Defenders;

		HashSet<GamePieceController> moveablePieces = new HashSet<GamePieceController>();

		foreach ( var piece in attackers.Cast<GamePieceController>().Union( defenders ) ) {
			piece.SetSelectable( false );
			piece.SetSelectable( false );
			piece.IsSelected = false;

			foreach ( var dieIndex in dieIndices ) {
				var die = _dice[dieIndex];

				foreach ( var direction in Enum.GetValues( typeof( PieceMovementDirection ) ).Cast<PieceMovementDirection>() ) {
					var turnAction = new TurnAction( dieIndex, direction, piece.PieceType, piece.PieceIndex );
					_fourDice.GameState.CurrentPlayerType = _activePlayerType;
					var validationResult = FourDice.ValidateTurnAction( _fourDice.GameState, turnAction, null );

					if ( validationResult.IsValidAction ) {
						moveablePieces.Add( piece );
					}
				}
			}
		}

		foreach ( var piece in moveablePieces ) {
			piece.SetSelectable( true );
			piece.SetDeselectable( true );
		}

		return moveablePieces.Any();

	}




	private void SetActivePlayer( PlayerType playerType )
	{
		_activePlayerType = playerType;
		Player1TurnLabel.gameObject.SetActive( _activePlayerType == PlayerType.Player1 );
		Player2TurnLabel.gameObject.SetActive( _activePlayerType == PlayerType.Player2 );

		foreach ( var die in _dice ) {
			die.SetSelectable( true );
			die.SetDeselectable( true );
		}

		_gameTurnState = GameTurnState.DieSelection;

	}

	Coroutine awaitFinalDiceRolls;
	public void RollSelectedDice( bool isInitialDiceRoll, Action callback = null )
	{
		if ( _dice.Any( d => d.IsSelected || d.IsRolling ) ) {
			StartCoroutine( RollDiceCoroutine( isInitialDiceRoll, callback ) );
		}
	}
	private IEnumerator RollDiceCoroutine( bool isInitialDiceRoll, Action callback = null )
	{
		_mainCameraTargetPosition = _mainCameraStandardPosition - new Vector3( 0, 0, 10 );
		_mainCameraSlerpTime = 0;
		_diceAnimationSlerpTime = 0;
		_animateDice = true;

		for ( var i = 0; i < 4; i++ ) {
			var die = _dice[i];
			if ( !die.IsSelected && !die.IsRolling ) {
				continue;
			}
			die.Deselect();
			die.IsRolling = true;

			_diceTargetPositions[i] = _dicePreRollPositions[i];
			die.transform.rotation = UnityEngine.Random.rotation;
		}

		// Let the dice get into position
		yield return new WaitForSeconds( 0.5f );

		_animateDice = false;

		if ( awaitFinalDiceRolls != null ) {
			StopCoroutine( awaitFinalDiceRolls );
		}

		DiceKeeperCollider.enabled = true;

		for ( var i = 0; i < 4; i++ ) {
			var die = _dice[i];
			if ( !die.IsRolling ) {
				continue;
			}

			die.GetComponent<Rigidbody>().isKinematic = false;
			die.GetComponent<Rigidbody>().AddForceAtPosition( new Vector3( UnityEngine.Random.Range( -20, 20 ), 0, UnityEngine.Random.Range( -5, 5 ) ) * 25, new Vector3( 2, 2, 2 ), ForceMode.Impulse );
		}

		awaitFinalDiceRolls = StartCoroutine( AwaitFinalDiceRolls( isInitialDiceRoll, callback ) );
	}

	private IEnumerator AwaitFinalDiceRolls( bool isInitialDiceRoll, Action callback = null )
	{
		// Give is a moment to ensure the die has started moving. 
		yield return new WaitForSeconds( 0.25f );

		float secondsPassed = 0;
		float interval = 0.1f;
		var shouldReroll = false;
		while ( true ) {
			if ( secondsPassed > 5.0f ) {
				//Debug.Log( "Took too long. Rerolling." );
				shouldReroll = true;
				break;
			}
			if ( _dice.Any( d => d.IsMoving() ) ) {
				secondsPassed += interval;
				//Debug.Log( string.Format( "Still moving after {0} seconds", secondsPassed ) );
				yield return new WaitForSeconds( interval );
			}
			else {
				// The dice have stopped. 
				if ( _dice.Select( d => d.GetDieValue() ).Any( dv => dv == null ) ) {
					//Debug.Log( "Dice landed weird. Rerolling." );
					shouldReroll = true;
					break;
				}
				else {

				}
				break;
			}
		}

		if ( shouldReroll ) {
			RollSelectedDice( isInitialDiceRoll: isInitialDiceRoll, callback: callback );
		}
		else {
			DiceKeeperCollider.enabled = false;
			Debug.Log( string.Format( "Dice values are: {0}", string.Join( ", ", _dice.Select( d => d.GetDieValue() ).Select( dv => dv == null ? "?" : dv.Value.ToString() ).ToArray() ) ) );

			_mainCameraTargetPosition = _mainCameraStandardPosition;
			_mainCameraSlerpTime = 0;
			_animateDice = true;

			_diceAnimationSlerpTime = 0;
			_diceTargetEulerPositions.Clear();
			for ( var i = 0; i < 4; i++ ) {
				var die = _dice[i];

				_diceTargetPositions[i] = _dicePositions[i];


				die.GetComponent<Rigidbody>().isKinematic = true;

				_diceTargetEulerPositions.Add( FourDiceUtils.SnapTo( die.transform.localEulerAngles, 90 ) );

			}

			yield return new WaitForSeconds( 1f );
			_animateDice = false;
			_diceTargetEulerPositions.Clear();
			for ( var i = 0; i < 4; i++ ) {
				_dice[i].IsRolling = false;
			}

			if ( !isInitialDiceRoll && _dice.Select( d => d.GetDieValue() ).Distinct().Count() == 1 ) {
				// All the same number have been rolled. Reroll all die.

				for ( var i = 0; i < 4; i++ ) {
					_dice[i].IsSelected = true;
				}

				RollSelectedDice( isInitialDiceRoll: isInitialDiceRoll, callback: callback );
			}

			if ( callback != null ) {
				callback();
			}

		}
	}
}


enum GameTurnState
{
	Waiting,
	DieSelection,
	FirstPieceSelection,
	SecondPieceSelection,
	FirstPieceTargetSelection,
	SecondPieceTargetSelection,
	WaitingForFinalization
}