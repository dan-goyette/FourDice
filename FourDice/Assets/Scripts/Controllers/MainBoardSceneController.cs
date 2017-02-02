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
	public Button EndTurnButton;
	public Button RollDiceButton;
	public Text Player1TurnLabel;
	public Text Player2TurnLabel;
	public Text LogText;

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
	private GameLoopPhase _gameLoopPhase;

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

		_gameLoopPhase = GameLoopPhase.Waiting;

		StartGameButton.gameObject.SetActive( true );
		EndTurnButton.gameObject.SetActive( false );
		RollDiceButton.gameObject.SetActive( false );

	}

	private void CreateGamePieces()
	{


		Action<int, PlayerType, GameObject[], AttackerController[]> createAttacker = ( pieceIndex, playerType, placeHolders, attackers ) => {
			var placeHolder = placeHolders[pieceIndex];
			GameObject attacker = (GameObject)Instantiate( Resources.Load( "Attacker" ) );
			attacker.transform.position = placeHolder.transform.position;

			var attackerController = attacker.GetComponent<AttackerController>();
			attackerController.PlayerType = playerType;
			attackerController.PieceIndex = pieceIndex;
			attackerController.OnSelectionChanged += AttackerController_OnSelectionChanged;

			attackers[pieceIndex] = attackerController;
			placeHolder.SetActive( false );
		};


		Action<int, PlayerType, GameObject[], DefenderController[]> createDefender = ( pieceIndex, playerType, placeHolders, defenders ) => {
			var placeHolder = placeHolders[pieceIndex];
			GameObject defender = (GameObject)Instantiate( Resources.Load( "Defender" ) );
			defender.transform.position = placeHolder.transform.position;

			var defenderController = defender.GetComponent<DefenderController>();
			defenderController.PlayerType = playerType;
			defenderController.PieceIndex = pieceIndex;
			defenderController.OnSelectionChanged += DefenderController_OnSelectionChanged;

			defenders[pieceIndex] = defenderController;
			placeHolder.SetActive( false );
		};

		_player1Attackers = new AttackerController[5];
		for ( var i = 0; i < InitialPlayer1AttackerPlaceHolders.Length; i++ ) {
			createAttacker( i, PlayerType.Player1, InitialPlayer1AttackerPlaceHolders, _player1Attackers );
		}

		_player1Defenders = new DefenderController[2];
		for ( var i = 0; i < InitialPlayer1DefenderPlaceHolders.Length; i++ ) {
			createDefender( i, PlayerType.Player1, InitialPlayer1DefenderPlaceHolders, _player1Defenders );
		}

		_player2Attackers = new AttackerController[5];
		for ( var i = 0; i < InitialPlayer2AttackerPlaceHolders.Length; i++ ) {
			createAttacker( i, PlayerType.Player2, InitialPlayer2AttackerPlaceHolders, _player2Attackers );
		}


		_player2Defenders = new DefenderController[2];
		for ( var i = 0; i < InitialPlayer2DefenderPlaceHolders.Length; i++ ) {
			createDefender( i, PlayerType.Player2, InitialPlayer2DefenderPlaceHolders, _player2Defenders );
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
			die.transform.position = _dicePositions[i] = _diceTargetPositions[i] = new Vector3( -3.75f + 2.5f * i, .25f, -2 );
			_dicePreRollPositions[i] = new Vector3( -4 + 2.5f * i, 8f, -11 );
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
		else {
			_lastSelectedPiece = null;
		}
	}

	private void AttackerController_OnSelectionChanged( object sender, SelectableObjectSelectionChangedEvent arg )
	{
		if ( arg.IsSelected ) {
			_lastSelectedPiece = (GamePieceController)sender;
		}
		else {
			_lastSelectedPiece = null;
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



	public void EndTurnButtonPressed()
	{
		EndTurnButton.gameObject.SetActive( false );
		SetActivePlayer( _activePlayerType == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1 );
		_gameLoopPhase = GameLoopPhase.WaitingForDiceRoll;
	}



	public void RollDieButtonPressed()
	{
		RollDiceButton.gameObject.SetActive( false );
		RollSelectedDice( false, () => {
			_gameLoopPhase = GameLoopPhase.DieSelection;
		} );
	}



	bool _player1InitialDiceRolling;
	bool _player2InitialDiceRolling;
	GamePieceController _lastSelectedPiece;
	LanePositionController _lastSelectedLanePosition;
	private GameLoopPhase _previousGameLoopPhase;

	private IEnumerator InitiateStartGame()
	{
		AppendToLogText( "Rolling to see who plays first." );

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

			if ( player1Score == player2Score ) {
				AppendToLogText( "Tie score. Rerolling." );
			}
		}

		for ( var i = 0; i < _dice.Length; i++ ) {
			_fourDice.GameState.Dice[i].Value = _dice[i].GetDieValue().Value;
		}


		SetActivePlayer( player1Score > player2Score ? PlayerType.Player1 : PlayerType.Player2 );

		AppendToLogText( string.Format( "{0} plays first", _activePlayerType ) );

		// The dice have already been rolled. Skip right to die selection.
		_previousGameLoopPhase = _gameLoopPhase;
		_gameLoopPhase = GameLoopPhase.DieSelection;

		StartCoroutine( RunGameLoop() );
	}

	private IEnumerator RunGameLoop()
	{
		while ( true ) {
			var attackers = _activePlayerType == PlayerType.Player1 ? _player1Attackers : _player2Attackers;
			var defenders = _activePlayerType == PlayerType.Player1 ? _player1Defenders : _player2Defenders;



			// Main Loop - Observe changes to the state, potentially fall into an animation.
			if ( _gameLoopPhase == GameLoopPhase.Waiting ) {
				// TODO - Not sure we'll use this.

				DoGameLoopPhaseInitialization( () => { } );
			}
			else if ( _gameLoopPhase == GameLoopPhase.WaitingForDiceRoll ) {
				DoGameLoopPhaseInitialization( () => {
					DeselectAllGamePieces();
					DeselectAllLanePositions();
					RollDiceButton.gameObject.SetActive( true );
				} );
			}
			else if ( _gameLoopPhase == GameLoopPhase.WaitingForFinalization ) {
				// Nothing to do here. We're just waiting for the player to click a UI button.

				DoGameLoopPhaseInitialization( () => { } );
			}
			else if ( _gameLoopPhase == GameLoopPhase.DieSelection ) {


				DoGameLoopPhaseInitialization( () => {
					_lastSelectedPiece = null;
					_lastSelectedLanePosition = null;

					DeselectAllGamePieces();
					DeselectAllLanePositions();


					foreach ( var die in _dice ) {
						die.IsSelected = false;
						die.SetSelectable( true );
						die.SetDeselectable( true );
					}
				} );

				// Determine if all dice are selected.
				if ( _dice.Count( d => d.IsSelected ) == 2 ) {

					_gameLoopPhase = GameLoopPhase.FirstPieceSelection;

				}


			}
			else if ( _gameLoopPhase == GameLoopPhase.FirstPieceSelection ) {

				DoGameLoopPhaseInitialization( () => {
					// After dice are chosen, the player may end their turn.
					EndTurnButton.gameObject.SetActive( true );

					// Ensure all pieces are in a consistent state for this turn state.

					DeselectAllGamePieces();
					DeselectAllLanePositions();

					foreach ( var lanePosition in _lanePositions ) {
						lanePosition.IsSelected = false;
						lanePosition.SetSelectable( false );
						lanePosition.SetDeselectable( false );
					}


					List<int> dieIndices = new List<int>();
					for ( var i = 0; i < _dice.Length; i++ ) {
						var die = _dice[i];
						die.SetSelectable( false );
						die.SetDeselectable( false );
						if ( die.IsSelected ) {
							dieIndices.Add( i );
						}
					}

					MakePiecesSelectable( dieIndices );
				} );


				// Wait for the player to select a piece.
				if ( _lastSelectedPiece != null ) {
					_gameLoopPhase = GameLoopPhase.FirstPieceTargetSelection;
				}
			}
			else if ( _gameLoopPhase == GameLoopPhase.FirstPieceTargetSelection ) {

				DoGameLoopPhaseInitialization( () => {
					foreach ( var piece in attackers.Cast<GamePieceController>().Union( defenders ) ) {
						piece.SetSelectable( false );
						piece.SetDeselectable( true );
					}

					DeselectAllLanePositions();

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


				} );

				if ( _lastSelectedPiece == null ) {
					// They've deselected the piece. Revert to First Piece Selection.
					_gameLoopPhase = GameLoopPhase.FirstPieceSelection;
				}
				else {

					// Determine which positions are valid.


					if ( _lastSelectedLanePosition != null ) {
						// Move the piece to the desired location.

						yield return new WaitForSeconds( 1 );

						_gameLoopPhase = GameLoopPhase.SecondPieceSelection;
					}


				}


			}




			yield return new WaitForEndOfFrame();
		}

	}


	private void DoGameLoopPhaseInitialization( Action onInitializing )
	{
		if ( _gameLoopPhase != _previousGameLoopPhase ) {

			if ( onInitializing != null ) {
				onInitializing();
			}

			// We keep track of the phase from the previous iteration so we can easuily tell whether
			// we're hitting a step for the first time.

			_previousGameLoopPhase = _gameLoopPhase;
		}
	}

	private void DeselectAllGamePieces()
	{
		foreach ( var gamePiece in _player1Attackers.Cast<GamePieceController>().Union( _player2Attackers )
			.Union( _player1Defenders ).Union( _player2Defenders ) ) {
			gamePiece.IsSelected = false;
			gamePiece.SetSelectable( false );
			gamePiece.SetDeselectable( false );
		}
	}

	private void DeselectAllLanePositions()
	{
		foreach ( var lanePosition in _lanePositions ) {
			lanePosition.IsSelected = false;
			lanePosition.SetSelectable( false );
			lanePosition.SetDeselectable( false );
		}
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
			die.SetDeselectable( true );
			die.Deselect();
			die.SetDeselectable( false );
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
				AppendToLogText( "Took too long. Rerolling." );
				shouldReroll = true;
				break;
			}
			if ( _dice.Any( d => d.IsMoving() ) ) {
				secondsPassed += interval;
				yield return new WaitForSeconds( interval );
			}
			else {
				// The dice have stopped. 
				if ( _dice.Select( d => d.GetDieValue() ).Any( dv => dv == null ) ) {
					AppendToLogText( "Dice landed weird. Rerolling." );
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
			AppendToLogText( string.Format( "Dice values are: {0}", string.Join( ", ", _dice.Select( d => d.GetDieValue() ).Select( dv => dv == null ? "?" : dv.Value.ToString() ).ToArray() ) ) );

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

	private void AppendToLogText( string text )
	{
		LogText.text = string.Format( "{0}{1}{2}", text, Environment.NewLine, LogText.text );
	}
}


enum GameLoopPhase
{
	Waiting,
	WaitingForDiceRoll,
	DieSelection,
	FirstPieceSelection,
	SecondPieceSelection,
	FirstPieceTargetSelection,
	SecondPieceTargetSelection,
	WaitingForFinalization
}