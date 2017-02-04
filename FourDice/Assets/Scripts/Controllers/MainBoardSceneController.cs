using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.DomainModel;
using Assets.Scripts.DomainModel.AI;
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
	public Button UndoTurnButton;
	public Button WriteDebugButton;
	public Text Player1TurnLabel;
	public Text Player2TurnLabel;
	public Text LogText;

	Vector3 _mainCameraStandardPosition;

	DieController[] _dice;
	Vector3[] _dicePositions;
	Vector3[] _dicePreRollPositions;

	private GameState _turnStartGameState;

	AttackerController[] _player1Attackers;
	AttackerController[] _player2Attackers;
	DefenderController[] _player1Defenders;
	DefenderController[] _player2Defenders;

	LanePositionController[] _lanePositions;

	private FourDice _fourDice;

	private PlayerType _activePlayerType;
	private GameLoopPhase _gameLoopPhase;

	private List<GameObjectTransformAnimation> _gameObjectAnimations;

	private TurnAction _lastTurnAction;

	private AIBase _player1AI;
	private AIBase _player2AI;

	// Use this for initialization
	void Start()
	{
		_dice = new DieController[4];
		_dicePositions = new Vector3[4];
		_dicePreRollPositions = new Vector3[4];

		_gameObjectAnimations = new List<GameObjectTransformAnimation>();

		_mainCameraStandardPosition = Camera.main.transform.position;


		DiceKeeperCollider.enabled = false;

		CreateDice();
		CreateGamePieces();

		_lanePositions = GameObject.FindObjectsOfType<LanePositionController>().OrderBy( lp => lp.LanePosition ).ToArray();
		foreach ( var lp in _lanePositions ) {
			lp.OnSelectionChanged += LanePositionController_OnSelectionChanged;
		}

		Player1TurnLabel.gameObject.SetActive( false );
		Player2TurnLabel.gameObject.SetActive( false );

		_gameLoopPhase = GameLoopPhase.Waiting;

		StartGameButton.gameObject.SetActive( true );
		EndTurnButton.gameObject.SetActive( false );
		RollDiceButton.gameObject.SetActive( false );
		UndoTurnButton.gameObject.SetActive( false );
		WriteDebugButton.gameObject.SetActive( false );

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
			attackerController.LanePosition = null;
			attackerController.BoardPositionType = BoardPositionType.OwnGoal;
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
			defenderController.LanePosition = null;
			defenderController.BoardPositionType = BoardPositionType.DefenderCircle;
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
			gamePiece.Deselect();
		}
	}



	private void CreateDice()
	{
		for ( var i = 0; i < 4; i++ ) {
			GameObject die = (GameObject)Instantiate( Resources.Load( "Die" ) );
			var dieController = die.GetComponent<DieController>();
			_dice[i] = dieController;
			die.transform.position = _dicePositions[i] = new Vector3( -3.75f + 2.5f * i, .5f, -2 );
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


	private void LanePositionController_OnSelectionChanged( object sender, SelectableObjectSelectionChangedEvent arg )
	{
		if ( arg.IsSelected ) {
			_lastSelectedLanePosition = (LanePositionController)sender;
		}
		else {
			_lastSelectedLanePosition = null;
		}
	}

	private void DieController_OnSelectionChanged( object sender, SelectableObjectSelectionChangedEvent arg )
	{

	}

	// Update is called once per frame
	void Update()
	{
		if ( _gameObjectAnimations.Count > 0 ) {
			foreach ( var movementInfo in _gameObjectAnimations.ToList() ) {
				if ( movementInfo.GameObject.transform.position == movementInfo.TargetPosition ) {
					_gameObjectAnimations.Remove( movementInfo );
					if ( movementInfo.OnComplete != null ) {
						movementInfo.OnComplete();
					}
				}
				else {
					movementInfo.ElapsedAnimationTime += Time.deltaTime * movementInfo.AnimationSpeed;
					movementInfo.GameObject.transform.position = Vector3.Slerp( movementInfo.GameObject.transform.position, movementInfo.TargetPosition, movementInfo.ElapsedAnimationTime );
					if ( movementInfo.TargetRotation.HasValue ) {
						movementInfo.GameObject.transform.rotation = Quaternion.Slerp( movementInfo.GameObject.transform.rotation, movementInfo.TargetRotation.Value, movementInfo.ElapsedAnimationTime );

					}
				}
			}
		}
	}


	public void StartGameButtonPressed()
	{
		for ( var i = 0; i < _player1Attackers.Length; i++ ) {
			_player1Attackers[i].TurnStartPosition = InitialPlayer1AttackerPlaceHolders[i].transform.position;
			_player1Attackers[i].TurnStartRotation = Quaternion.Euler( 0, 0, 0 );
			_player1Attackers[i].TurnStartInUpperSlot = null;
		}

		for ( var i = 0; i < _player1Defenders.Length; i++ ) {
			_player1Defenders[i].TurnStartPosition = InitialPlayer1DefenderPlaceHolders[i].transform.position;
			_player1Defenders[i].TurnStartRotation = Quaternion.Euler( 0, 0, 0 );
			_player1Defenders[i].TurnStartInUpperSlot = null;
		}


		for ( var i = 0; i < _player2Attackers.Length; i++ ) {
			_player2Attackers[i].TurnStartPosition = InitialPlayer2AttackerPlaceHolders[i].transform.position;
			_player2Attackers[i].TurnStartRotation = Quaternion.Euler( 0, 0, 0 );
			_player2Attackers[i].TurnStartInUpperSlot = null;
		}

		for ( var i = 0; i < _player2Defenders.Length; i++ ) {
			_player2Defenders[i].TurnStartPosition = InitialPlayer2DefenderPlaceHolders[i].transform.position;
			_player2Defenders[i].TurnStartRotation = Quaternion.Euler( 0, 0, 0 );
			_player2Defenders[i].TurnStartInUpperSlot = null;
		}

		foreach ( var die in _dice ) {
			die.Deselect( force: true );
			die.SetSelectable( false );
			die.SetDeselectable( false );
		}

		DeselectAllGamePieces();
		DeselectAllLanePositions();


		_fourDice = new FourDice( null, null );// "DefenderAI" );
		_turnStartGameState = new GameState( null, null );// "DefenderAI" );


		var assembly = Assembly.GetExecutingAssembly();



		if ( string.IsNullOrEmpty( _fourDice.GameState.Player1.AIName ) ) {
			_player1AI = null;
		}
		else {
			var type = assembly.GetTypes().First( t => t.Name == _fourDice.GameState.Player1.AIName );
			_player1AI = (AIBase)Activator.CreateInstance( type, PlayerType.Player1, true );
		}
		if ( string.IsNullOrEmpty( _fourDice.GameState.Player2.AIName ) ) {
			_player2AI = null;
		}
		else {
			var type = assembly.GetTypes().First( t => t.Name == _fourDice.GameState.Player2.AIName );
			_player2AI = (AIBase)Activator.CreateInstance( type, PlayerType.Player2, true );
		}

		StartGameButton.gameObject.SetActive( false );
		EndTurnButton.gameObject.SetActive( false );
		RollDiceButton.gameObject.SetActive( false );
		UndoTurnButton.gameObject.SetActive( false );
		WriteDebugButton.gameObject.SetActive( false );

		SynchronizeBoardWithGameState();

		_gameLoopPhase = GameLoopPhase.Waiting;

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



	public void UndoTurnButtonPressed()
	{
		_turnStartGameState.CopyTo( _fourDice.GameState );
		_fourDice.ClearLastTurnAction();
		SynchronizeBoardWithGameState();
		AppendToLogText( _fourDice.GameState.GetAsciiState() );
		_previousGameLoopPhase = GameLoopPhase.Waiting;
		_gameLoopPhase = GameLoopPhase.DieSelection;
	}



	public void WriteDebugButtonPressed()
	{
		AppendToLogText( _fourDice.GameState.GetAsciiState() );
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

		using ( new DisabledButtonInteractabilityScope() ) {

			while ( player1Score == player2Score ) {

				// Roll player 1's dice
				_dice[0].Select( force: true );
				_dice[1].Select( force: true );
				_player1InitialDiceRolling = true;

				RollSelectedDice( isInitialDiceRoll: true, callback: () => _player1InitialDiceRolling = false );

				_dice[0].Deselect();
				_dice[1].Deselect();

				_dice[0].SetSelectable( false );
				_dice[1].SetSelectable( false );

				yield return new WaitUntil( () => !_player1InitialDiceRolling );

				_dice[2].Select( force: true );
				_dice[3].Select( force: true );
				_player2InitialDiceRolling = true;

				RollSelectedDice( isInitialDiceRoll: true, callback: () => _player2InitialDiceRolling = false );
				_dice[2].SetSelectable( false );
				_dice[2].Deselect();
				_dice[3].SetSelectable( false );
				_dice[3].Deselect();

				yield return new WaitUntil( () => !_player2InitialDiceRolling );

				player1Score = _dice[0].GetDieValue().Value + _dice[1].GetDieValue().Value;
				player2Score = _dice[2].GetDieValue().Value + _dice[3].GetDieValue().Value;

				if ( player1Score == player2Score ) {
					AppendToLogText( "Tie score. Rerolling." );
				}
			}
		}

		SetActivePlayer( player1Score > player2Score ? PlayerType.Player1 : PlayerType.Player2 );

		SynchronizeGameStateWithBoard();

		AppendToLogText( string.Format( "{0} plays first", _activePlayerType ) );

		// The dice have already been rolled. Skip right to die selection.
		_previousGameLoopPhase = _gameLoopPhase;
		_gameLoopPhase = GameLoopPhase.DieSelection;

		StartCoroutine( RunGameLoop() );
	}


	private TurnAction[] _bestPlayer1AITurnAction = null;
	private TurnAction[] _bestPlayer2AITurnAction = null;
	private IEnumerator RunGameLoop()
	{
		while ( true ) {
			var attackers = _activePlayerType == PlayerType.Player1 ? _player1Attackers : _player2Attackers;
			var defenders = _activePlayerType == PlayerType.Player1 ? _player1Defenders : _player2Defenders;


			var currentPlayerAI = _activePlayerType == PlayerType.Player1 ? _player1AI : _player2AI;
			var currentPlayerAIBestTurnAction = _activePlayerType == PlayerType.Player1 ? _bestPlayer2AITurnAction : _bestPlayer2AITurnAction;


			// Main Loop - Observe changes to the state, potentially fall into an animation.
			if ( _gameLoopPhase == GameLoopPhase.Waiting ) {
				// TODO - Not sure we'll use this.

				DoGameLoopPhaseInitialization( () => { } );
			}
			else if ( _gameLoopPhase == GameLoopPhase.WaitingForDiceRoll ) {
				DoGameLoopPhaseInitialization( () => {
					UndoTurnButton.gameObject.SetActive( false );

					SynchronizeGameStateWithBoard();
					DeselectAllGamePieces();
					DeselectAllLanePositions();
					RollDiceButton.gameObject.SetActive( true );

					if ( currentPlayerAI != null ) {
						RollDieButtonPressed();
					}
				} );


			}
			else if ( _gameLoopPhase == GameLoopPhase.WaitingForFinalization ) {
				// Nothing to do here. We're just waiting for the player to click a UI button.

				DoGameLoopPhaseInitialization( () => {

				} );

				if ( currentPlayerAI != null ) {
					_bestPlayer1AITurnAction = null;
					_bestPlayer2AITurnAction = null;
					EndTurnButtonPressed();
				}

			}
			else if ( _gameLoopPhase == GameLoopPhase.DieSelection ) {


				DoGameLoopPhaseInitialization( () => {
					_fourDice.GameState.CopyTo( _turnStartGameState );
					_lastTurnAction = null;
					UndoTurnButton.gameObject.SetActive( true );

					_lastSelectedPiece = null;
					_lastSelectedLanePosition = null;

					DeselectAllGamePieces();
					DeselectAllLanePositions();


					foreach ( var die in _dice ) {
						die.Deselect();
						die.SetSelectable( true );
						die.SetDeselectable( true );
					}

					if ( currentPlayerAI != null ) {
						currentPlayerAIBestTurnAction = currentPlayerAI.GetNextMoves( this._fourDice.GameState );
						if ( _activePlayerType == PlayerType.Player1 ) {
							_bestPlayer1AITurnAction = currentPlayerAIBestTurnAction;
						}
						else {
							_bestPlayer2AITurnAction = currentPlayerAIBestTurnAction;
						}

						for ( var i = 0; i < 4; i++ ) {
							if ( currentPlayerAIBestTurnAction[0].DieIndex == i || currentPlayerAIBestTurnAction[1].DieIndex == i ) {
								_dice[i].Select( force: true );
							}
						}
					}
				} );




				// Determine if all dice are selected.
				if ( _dice.Count( d => d.IsSelected ) == 2 ) {

					_gameLoopPhase = GameLoopPhase.FirstPieceSelection;
					SynchronizeGameStateWithBoard();
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
						lanePosition.Deselect();
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

					_lastSelectedPiece = null;

					if ( currentPlayerAI != null ) {
						// PIck the first piece.
						if ( currentPlayerAIBestTurnAction[0].PieceIndex.HasValue ) {
							GamePieceController pieceToMove = null;
							if ( _activePlayerType == PlayerType.Player1 ) {
								if ( currentPlayerAIBestTurnAction[0].PieceType == PieceType.Attacker ) {
									pieceToMove = _player1Attackers[currentPlayerAIBestTurnAction[0].PieceIndex.Value];
									pieceToMove.Select( force: true );
								}
								else {
									pieceToMove = _player1Defenders[currentPlayerAIBestTurnAction[0].PieceIndex.Value];
									pieceToMove.Select( force: true );
								}
							}
							else {
								if ( currentPlayerAIBestTurnAction[0].PieceType == PieceType.Attacker ) {
									pieceToMove = _player2Attackers[currentPlayerAIBestTurnAction[0].PieceIndex.Value];
									pieceToMove.Select( force: true );
								}
								else {
									pieceToMove = _player2Defenders[currentPlayerAIBestTurnAction[0].PieceIndex.Value];
									pieceToMove.Select( force: true );
								}
							}
						}
						else {
							_gameLoopPhase = GameLoopPhase.SecondPieceTargetSelection;
						}
					}
				} );


				// Wait for the player to select a piece.
				if ( _lastSelectedPiece != null ) {
					_gameLoopPhase = GameLoopPhase.FirstPieceTargetSelection;
				}
			}
			else if ( _gameLoopPhase == GameLoopPhase.FirstPieceTargetSelection
				|| _gameLoopPhase == GameLoopPhase.SecondPieceTargetSelection ) {

				DoGameLoopPhaseInitialization( () => {
					foreach ( var piece in attackers.Cast<GamePieceController>().Union( defenders ) ) {
						piece.SetSelectable( false );
						piece.SetDeselectable( true );
					}

					DeselectAllLanePositions();

					HashSet<int> validPositions = new HashSet<int>();

					for ( var i = 0; i < _dice.Length; i++ ) {
						var die = _dice[i];
						if ( die.IsSelected && !die.IsChosen ) {
							foreach ( var direction in Enum.GetValues( typeof( PieceMovementDirection ) ).Cast<PieceMovementDirection>() ) {
								var turnAction = new TurnAction( i, direction, _lastSelectedPiece.PieceType, _lastSelectedPiece.PieceIndex );
								var validationResult = FourDice.ValidateTurnAction( _fourDice.GameState, turnAction, null );
								if ( validationResult.IsValidAction ) {
									validPositions.Add( validationResult.NewLanePosition.Value );
								}
							}
						}
					}

					foreach ( var lanePosition in _lanePositions ) {
						if ( validPositions.Contains( lanePosition.LanePosition ) ) {
							lanePosition.Deselect();
							lanePosition.SetSelectable( true );
							lanePosition.SetDeselectable( true );
						}
					}


					if ( currentPlayerAI != null ) {

						var turnIndex = _gameLoopPhase == GameLoopPhase.FirstPieceTargetSelection ? 0 : 1;
						GamePieceController pieceToMove = null;
						if ( _activePlayerType == PlayerType.Player1 ) {
							if ( currentPlayerAIBestTurnAction[turnIndex].PieceType == PieceType.Attacker ) {
								pieceToMove = _player1Attackers[currentPlayerAIBestTurnAction[turnIndex].PieceIndex.Value];
								pieceToMove.Select( force: true );
							}
							else {
								pieceToMove = _player1Defenders[currentPlayerAIBestTurnAction[turnIndex].PieceIndex.Value];
								pieceToMove.Select( force: true );
							}
						}
						else {
							if ( currentPlayerAIBestTurnAction[turnIndex].PieceType == PieceType.Attacker ) {
								pieceToMove = _player2Attackers[currentPlayerAIBestTurnAction[turnIndex].PieceIndex.Value];
								pieceToMove.Select( force: true );
							}
							else {
								pieceToMove = _player2Defenders[currentPlayerAIBestTurnAction[turnIndex].PieceIndex.Value];
								pieceToMove.Select( force: true );
							}
						}


						var newLanePositionIndex = 0;
						if ( pieceToMove.BoardPositionType == BoardPositionType.OwnGoal ) {
							newLanePositionIndex = _activePlayerType == PlayerType.Player1 ? FourDice.Player1GoalLanePosition : FourDice.Player2GoalLanePosition;
						}
						else if ( pieceToMove.BoardPositionType == BoardPositionType.DefenderCircle ) {
							newLanePositionIndex = _activePlayerType == PlayerType.Player1 ? FourDice.Player1DefenderCircleLanePosition : FourDice.Player2DefenderCircleLanePosition;

							if ( _activePlayerType == PlayerType.Player1 ) {
								if ( currentPlayerAIBestTurnAction[turnIndex].Direction == PieceMovementDirection.Forward ) {
									newLanePositionIndex -= 1;
								}
								else {
									newLanePositionIndex += 1;
								}
							}
							else {
								if ( currentPlayerAIBestTurnAction[turnIndex].Direction == PieceMovementDirection.Forward ) {
									newLanePositionIndex += 1;
								}
								else {
									newLanePositionIndex -= 1;
								}
							}
						}
						else {
							newLanePositionIndex = pieceToMove.LanePosition.Value;
						}

						int directionMultiplier = 1;
						if ( currentPlayerAIBestTurnAction[turnIndex].Direction == PieceMovementDirection.Backward ) {
							directionMultiplier *= -1;
						}
						if ( _activePlayerType == PlayerType.Player2 ) {
							directionMultiplier *= -1;
						}
						newLanePositionIndex += directionMultiplier * _dice[currentPlayerAIBestTurnAction[turnIndex].DieIndex].GetDieValue().Value;

						var lanePosition = _lanePositions[newLanePositionIndex];
						lanePosition.Select( force: true );
					}

				} );

				if ( _lastSelectedPiece == null ) {
					// They've deselected the piece. Revert to First Piece Selection.
					_gameLoopPhase = _gameLoopPhase == GameLoopPhase.FirstPieceTargetSelection
						? GameLoopPhase.FirstPieceSelection
						: GameLoopPhase.SecondPieceSelection;
				}
				else {

					// Determine which positions are valid.


					if ( _lastSelectedLanePosition != null ) {

						var pieceToMove = _lastSelectedPiece;
						var targetLanePosition = _lastSelectedLanePosition;


						// Apply the action

						GameLogEntry appliedGameLogEntry = null;
						for ( var i = 0; i < _dice.Length; i++ ) {
							if ( appliedGameLogEntry == null ) {
								var die = _dice[i];
								if ( die.IsSelected && !die.IsChosen ) {

									foreach ( var direction in Enum.GetValues( typeof( PieceMovementDirection ) ).Cast<PieceMovementDirection>() ) {
										var turnAction = new TurnAction( i, direction, pieceToMove.PieceType, pieceToMove.PieceIndex );
										var validationResult = FourDice.ValidateTurnAction( _fourDice.GameState, turnAction, null );
										if ( validationResult.NewLanePosition == targetLanePosition.LanePosition ) {
											appliedGameLogEntry = _fourDice.ApplyTurnAction( turnAction );
											_lastTurnAction = turnAction;

											if ( validationResult.NewBoardPositionType.HasValue ) {
												pieceToMove.BoardPositionType = validationResult.NewBoardPositionType.Value;
											}
											if ( validationResult.NewLanePosition.HasValue ) {
												pieceToMove.LanePosition = validationResult.NewLanePosition.Value;
											}


											AppendToLogText( _fourDice.GameState.GetAsciiState() );

											// Mark the die as chosen, and slide it away.
											die.ChooseDie();

											_gameObjectAnimations.Add( new GameObjectTransformAnimation() {
												GameObject = die.gameObject,
												TargetPosition = die.gameObject.transform.position - new Vector3( 0, 0, 1.5f )
											} );


											break;
										}
									}
								}
							}
						}

						// Remove selections
						DeselectAllGamePieces();
						DeselectAllLanePositions();


						using ( new DisabledButtonInteractabilityScope() ) {

							var finalAttackerRotation = Quaternion.Euler( 0, 0, 180 );
							var attackerHasReachedGoal = (pieceToMove.LanePosition == FourDice.Player1GoalLanePosition || pieceToMove.LanePosition == FourDice.Player2GoalLanePosition);




							// Determine if we should land in the upper or lower slot. 

							Vector3 targetLandingPosition = new Vector3();

							if ( targetLanePosition.LanePosition == FourDice.Player1GoalLanePosition ) {
								targetLandingPosition = InitialPlayer1AttackerPlaceHolders[pieceToMove.PieceIndex].transform.position - new Vector3( 0.5f, 0, 0.5f );
							}
							else if ( targetLanePosition.LanePosition == FourDice.Player2GoalLanePosition ) {
								targetLandingPosition = InitialPlayer2AttackerPlaceHolders[pieceToMove.PieceIndex].transform.position + new Vector3( 0.5f, 0, -0.5f );
							}
							else {
								// Start with the center of the lane position.
								targetLandingPosition = targetLanePosition.gameObject.transform.position;

								var otherPiecesAtSlot = AllGamePieces().Where( g => g != pieceToMove
									&& g.LanePosition == targetLanePosition.LanePosition ).ToList();
								if ( otherPiecesAtSlot.Count == 0 ) {
									// Nothing is here. Upper slot.
									pieceToMove.InUpperSlot = true;
								}
								else if ( otherPiecesAtSlot.Count == 1 ) {
									// One other piece is here. Go to the other slot.
									pieceToMove.InUpperSlot = !otherPiecesAtSlot[0].InUpperSlot.Value;
								}
								else {
									// Capture the piece according to the turn action result.
									var capturedPiece = _activePlayerType == PlayerType.Player1
										? _player2Attackers[appliedGameLogEntry.CapturedAttackerIndex.Value]
										: _player1Attackers[appliedGameLogEntry.CapturedAttackerIndex.Value];
									pieceToMove.InUpperSlot = capturedPiece.InUpperSlot;
								}

								targetLandingPosition += pieceToMove.InUpperSlot == true
									? new Vector3( 0, 0, 1f )
									: new Vector3( 0, 0, -1f );
							}



							bool animationSegmentComplete = false;
							// Move the piece to the desired location.
							var movement1 = new GameObjectTransformAnimation() {
								GameObject = pieceToMove.gameObject,
								TargetPosition = pieceToMove.gameObject.transform.position + new Vector3( 0, 3, 0 ),
								TargetRotation = GameObjectTransformAnimation.DefaultRotation,
								OnComplete = () => { animationSegmentComplete = true; }
							};
							_gameObjectAnimations.Add( movement1 );

							yield return new WaitUntil( () => animationSegmentComplete );

							animationSegmentComplete = false;
							var movement2 = new GameObjectTransformAnimation() {
								GameObject = pieceToMove.gameObject,
								TargetPosition = targetLandingPosition + (attackerHasReachedGoal ? new Vector3( 0, 6, 0 ) : new Vector3( 0, 3, 0 )),
								TargetRotation = attackerHasReachedGoal ? finalAttackerRotation : GameObjectTransformAnimation.DefaultRotation,
								OnComplete = () => { animationSegmentComplete = true; }
							};
							_gameObjectAnimations.Add( movement2 );
							yield return new WaitUntil( () => animationSegmentComplete );

							animationSegmentComplete = false;
							var movement3 = new GameObjectTransformAnimation() {
								GameObject = pieceToMove.gameObject,
								TargetPosition = targetLandingPosition + (attackerHasReachedGoal ? new Vector3( 0, 1.5f, 0 ) : new Vector3( 0, 0, 0 )),
								TargetRotation = attackerHasReachedGoal ? finalAttackerRotation : GameObjectTransformAnimation.DefaultRotation,
								OnComplete = () => { animationSegmentComplete = true; }
							};
							_gameObjectAnimations.Add( movement3 );
							yield return new WaitUntil( () => animationSegmentComplete );



							if ( appliedGameLogEntry.CapturedAttackerIndex.HasValue ) {
								// Capture the piece and send it home.
								var attacker = _activePlayerType == PlayerType.Player1
									? _player2Attackers[appliedGameLogEntry.CapturedAttackerIndex.Value]
									: _player1Attackers[appliedGameLogEntry.CapturedAttackerIndex.Value];
								var targetPosition = _activePlayerType == PlayerType.Player1
									? InitialPlayer2AttackerPlaceHolders[appliedGameLogEntry.CapturedAttackerIndex.Value].transform.position
									: InitialPlayer1AttackerPlaceHolders[appliedGameLogEntry.CapturedAttackerIndex.Value].transform.position;


								animationSegmentComplete = false;
								// Move the piece to the desired location.
								var movement = new GameObjectTransformAnimation() {
									GameObject = attacker.gameObject,
									TargetPosition = targetPosition,
									TargetRotation = GameObjectTransformAnimation.DefaultRotation,
									OnComplete = () => { animationSegmentComplete = true; }
								};
								_gameObjectAnimations.Add( movement );

								attacker.LanePosition = null;
								attacker.InUpperSlot = null;
								attacker.BoardPositionType = BoardPositionType.OwnGoal;

								yield return new WaitUntil( () => animationSegmentComplete );

							}


							if ( attackerHasReachedGoal ) {
								pieceToMove.BoardPositionType = BoardPositionType.OpponentGoal;
								pieceToMove.LanePosition = null;
							}





						}

						_lastSelectedLanePosition = null;
						_lastSelectedPiece = null;
						_gameLoopPhase = _gameLoopPhase == GameLoopPhase.FirstPieceTargetSelection
							? GameLoopPhase.SecondPieceSelection
							: GameLoopPhase.WaitingForFinalization;

						var gameEnd = FourDice.GetGameEndResult( _fourDice.GameState );

						if ( gameEnd.IsFinished ) {
							_gameLoopPhase = GameLoopPhase.GameOver;
							AppendToLogText( string.Format( "{0} is the winner!", gameEnd.WinningPlayer ) );

							StartGameButton.gameObject.SetActive( true );
							EndTurnButton.gameObject.SetActive( false );
							RollDiceButton.gameObject.SetActive( false );
							UndoTurnButton.gameObject.SetActive( false );
							WriteDebugButton.gameObject.SetActive( false );

							DeselectAllGamePieces();
							DeselectAllLanePositions();

						}
					}


				}


			}
			else if ( _gameLoopPhase == GameLoopPhase.SecondPieceSelection ) {

				DoGameLoopPhaseInitialization( () => {
					// Ensure all pieces are in a consistent state for this turn state.

					DeselectAllGamePieces();
					DeselectAllLanePositions();

					foreach ( var lanePosition in _lanePositions ) {
						lanePosition.Deselect();
						lanePosition.SetSelectable( false );
						lanePosition.SetDeselectable( false );
					}


					List<int> dieIndices = new List<int>();
					for ( var i = 0; i < _dice.Length; i++ ) {
						var die = _dice[i];
						die.SetSelectable( false );
						die.SetDeselectable( false );
						if ( die.IsSelected && !die.IsChosen ) {
							dieIndices.Add( i );
						}
					}

					MakePiecesSelectable( dieIndices );



					if ( currentPlayerAI != null ) {
						// PIck the first piece.
						if ( currentPlayerAIBestTurnAction[1].PieceIndex.HasValue ) {
							GamePieceController pieceToMove = null;
							if ( _activePlayerType == PlayerType.Player1 ) {
								if ( currentPlayerAIBestTurnAction[1].PieceType == PieceType.Attacker ) {
									pieceToMove = _player1Attackers[currentPlayerAIBestTurnAction[1].PieceIndex.Value];
									pieceToMove.Select( force: true );
								}
								else {
									pieceToMove = _player1Defenders[currentPlayerAIBestTurnAction[1].PieceIndex.Value];
									pieceToMove.Select( force: true );
								}
							}
							else {
								if ( currentPlayerAIBestTurnAction[1].PieceType == PieceType.Attacker ) {
									pieceToMove = _player2Attackers[currentPlayerAIBestTurnAction[1].PieceIndex.Value];
									pieceToMove.Select( force: true );
								}
								else {
									pieceToMove = _player2Defenders[currentPlayerAIBestTurnAction[1].PieceIndex.Value];
									pieceToMove.Select( force: true );
								}
							}
						}
						else {
							// End Turn
							EndTurnButtonPressed();
						}
					}
				} );


				// Wait for the player to select a piece.
				if ( _lastSelectedPiece != null ) {
					_gameLoopPhase = GameLoopPhase.SecondPieceTargetSelection;
				}
			}
			else if ( _gameLoopPhase == GameLoopPhase.GameOver ) {
				UndoTurnButton.gameObject.SetActive( false );
			}




			yield return new WaitForEndOfFrame();
		}

	}

	private IEnumerable<GamePieceController> AllGamePieces()
	{
		foreach ( var gamePiece in _player1Attackers ) {
			yield return gamePiece;
		}
		foreach ( var gamePiece in _player1Defenders ) {
			yield return gamePiece;
		}
		foreach ( var gamePiece in _player2Attackers ) {
			yield return gamePiece;
		}
		foreach ( var gamePiece in _player2Defenders ) {
			yield return gamePiece;
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
			gamePiece.Deselect( force: true );
			gamePiece.SetSelectable( false );
			gamePiece.SetDeselectable( false );
		}
	}

	private void DeselectAllLanePositions()
	{
		foreach ( var lanePosition in _lanePositions ) {
			lanePosition.Deselect( force: true );
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

		SynchronizeGameStateWithBoard();

		foreach ( var piece in attackers.Cast<GamePieceController>().Union( defenders ) ) {
			piece.SetSelectable( false );
			piece.SetSelectable( false );
			piece.Deselect();

			foreach ( var dieIndex in dieIndices ) {
				var die = _dice[dieIndex];

				foreach ( var direction in Enum.GetValues( typeof( PieceMovementDirection ) ).Cast<PieceMovementDirection>() ) {
					var turnAction = new TurnAction( dieIndex, direction, piece.PieceType, piece.PieceIndex );

					var validationResult = FourDice.ValidateTurnAction( _fourDice.GameState, turnAction, _lastTurnAction );

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


	private void SynchronizeGameStateWithBoard()
	{
		AppendToLogText( "Synchonizing" );
		_fourDice.GameState.CurrentPlayerType = _activePlayerType;

		for ( var i = 0; i < 4; i++ ) {
			_fourDice.GameState.Dice[i].Value = _dice[i].GetDieValue().Value;
			_fourDice.GameState.Dice[i].IsChosen = _dice[i].IsChosen;
		}

		// Player 1
		for ( var i = 0; i < _player1Attackers.Length; i++ ) {
			_fourDice.GameState.Player1.Attackers[i].BoardPositionType = _player1Attackers[i].BoardPositionType;
			_fourDice.GameState.Player1.Attackers[i].LanePosition = _player1Attackers[i].LanePosition;
		}
		for ( var i = 0; i < _player1Defenders.Length; i++ ) {
			_fourDice.GameState.Player1.Defenders[i].BoardPositionType = _player1Defenders[i].BoardPositionType;
			_fourDice.GameState.Player1.Defenders[i].LanePosition = _player1Defenders[i].LanePosition;
		}


		// Player 2
		for ( var i = 0; i < _player2Attackers.Length; i++ ) {
			_fourDice.GameState.Player2.Attackers[i].BoardPositionType = _player2Attackers[i].BoardPositionType;
			_fourDice.GameState.Player2.Attackers[i].LanePosition = _player2Attackers[i].LanePosition;
		}
		for ( var i = 0; i < _player2Defenders.Length; i++ ) {
			_fourDice.GameState.Player2.Defenders[i].BoardPositionType = _player2Defenders[i].BoardPositionType;
			_fourDice.GameState.Player2.Defenders[i].LanePosition = _player2Defenders[i].LanePosition;
		}

	}

	private void SynchronizeBoardWithGameState()
	{
		AppendToLogText( "Reverse Synchonizing" );
		StartCoroutine( BeginSynchronizeBoardWithGameState() );
	}

	private IEnumerator BeginSynchronizeBoardWithGameState()
	{
		// Move any dice.


		for ( int i = 0; i < 4; i++ ) {
			_dice[i].SetSelectable( true );
			_dice[i].SetDeselectable( true );
			_dice[i].UnchooseDie();
			_dice[i].Deselect();
			_dice[i].SetSelectable( false );
			_dice[i].SetDeselectable( false );

			_gameObjectAnimations.Add( new GameObjectTransformAnimation() {
				GameObject = _dice[i].gameObject,
				TargetPosition = _dicePositions[i]
			} );
		}

		for ( var i = 0; i < _player1Attackers.Length; i++ ) {
			_gameObjectAnimations.Add( new GameObjectTransformAnimation() {
				GameObject = _player1Attackers[i].gameObject,
				TargetPosition = _player1Attackers[i].TurnStartPosition,
				TargetRotation = _player1Attackers[i].TurnStartRotation
			} );

			_player1Attackers[i].BoardPositionType = _fourDice.GameState.Player1.Attackers[i].BoardPositionType;
			_player1Attackers[i].LanePosition = _fourDice.GameState.Player1.Attackers[i].LanePosition;
		}
		for ( var i = 0; i < _player1Defenders.Length; i++ ) {
			_gameObjectAnimations.Add( new GameObjectTransformAnimation() {
				GameObject = _player1Defenders[i].gameObject,
				TargetPosition = _player1Defenders[i].TurnStartPosition,
				TargetRotation = _player1Defenders[i].TurnStartRotation
			} );

			_player1Defenders[i].BoardPositionType = _fourDice.GameState.Player1.Defenders[i].BoardPositionType;
			_player1Defenders[i].LanePosition = _fourDice.GameState.Player1.Defenders[i].LanePosition;
		}


		for ( var i = 0; i < _player2Attackers.Length; i++ ) {
			_gameObjectAnimations.Add( new GameObjectTransformAnimation() {
				GameObject = _player2Attackers[i].gameObject,
				TargetPosition = _player2Attackers[i].TurnStartPosition,
				TargetRotation = _player2Attackers[i].TurnStartRotation
			} );

			_player2Attackers[i].BoardPositionType = _fourDice.GameState.Player2.Attackers[i].BoardPositionType;
			_player2Attackers[i].LanePosition = _fourDice.GameState.Player2.Attackers[i].LanePosition;
		}
		for ( var i = 0; i < _player2Defenders.Length; i++ ) {
			_gameObjectAnimations.Add( new GameObjectTransformAnimation() {
				GameObject = _player2Defenders[i].gameObject,
				TargetPosition = _player2Defenders[i].TurnStartPosition,
				TargetRotation = _player2Defenders[i].TurnStartRotation
			} );

			_player2Defenders[i].BoardPositionType = _fourDice.GameState.Player2.Defenders[i].BoardPositionType;
			_player2Defenders[i].LanePosition = _fourDice.GameState.Player2.Defenders[i].LanePosition;
		}



		yield return new WaitForSeconds( 1 );



	}

	private void SetActivePlayer( PlayerType playerType )
	{
		_activePlayerType = playerType;
		Player1TurnLabel.gameObject.SetActive( _activePlayerType == PlayerType.Player1 );
		Player2TurnLabel.gameObject.SetActive( _activePlayerType == PlayerType.Player2 );
		_fourDice.EndTurn();
		SynchronizeGameStateWithBoard();

		foreach ( var gamePiece in _player1Attackers.Cast<GamePieceController>().Union( _player1Defenders ).Union( _player2Attackers ).Union( _player2Defenders ) ) {
			gamePiece.TurnStartPosition = gamePiece.gameObject.transform.position;
			gamePiece.TurnStartRotation = gamePiece.gameObject.transform.rotation;
			gamePiece.TurnStartInUpperSlot = gamePiece.InUpperSlot;
		}
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
		_gameObjectAnimations.Add( new GameObjectTransformAnimation() {
			GameObject = Camera.main.transform.gameObject,
			TargetPosition = _mainCameraStandardPosition - new Vector3( 0, 0, 10 )
		} );



		int diceMovingCount = 0;
		for ( var i = 0; i < 4; i++ ) {
			var die = _dice[i];
			if ( !die.IsSelected && !die.IsRolling ) {
				continue;
			}
			diceMovingCount++;
			die.SetDeselectable( true );
			die.Deselect();
			die.SetDeselectable( false );
			die.IsRolling = true;
			die.UnchooseDie();

			_gameObjectAnimations.Add( new GameObjectTransformAnimation() {
				GameObject = die.gameObject,
				TargetPosition = _dicePreRollPositions[i],
				TargetRotation = UnityEngine.Random.rotation,
				OnComplete = () => { diceMovingCount--; }
			} );
		}

		// Let the dice get into position
		yield return new WaitUntil( () => diceMovingCount == 0 );


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
				}
				break;
			}
		}
		foreach ( var die in _dice ) {
			die.GetComponent<Rigidbody>().isKinematic = true;
		}

		if ( shouldReroll ) {
			RollSelectedDice( isInitialDiceRoll: isInitialDiceRoll, callback: callback );
		}
		else {
			DiceKeeperCollider.enabled = false;
			AppendToLogText( string.Format( "Dice values are: {0}", string.Join( ", ", _dice.Select( d => d.GetDieValue() ).Select( dv => dv == null ? "?" : dv.Value.ToString() ).ToArray() ) ) );


			_gameObjectAnimations.Add( new GameObjectTransformAnimation() {
				GameObject = Camera.main.transform.gameObject,
				TargetPosition = _mainCameraStandardPosition
			} );


			int diceMovingCount = 0;
			for ( var i = 0; i < 4; i++ ) {
				var die = _dice[i];
				diceMovingCount++;

				var euler = FourDiceUtils.SnapTo( die.transform.localEulerAngles, 90 );
				_gameObjectAnimations.Add( new GameObjectTransformAnimation() {
					GameObject = die.gameObject,
					TargetPosition = _dicePositions[i],
					TargetRotation = Quaternion.Euler( euler.x, euler.y, euler.z ),
					OnComplete = () => { diceMovingCount--; }
				} );





			}

			yield return new WaitUntil( () => diceMovingCount == 0 );

			for ( var i = 0; i < 4; i++ ) {
				_dice[i].IsRolling = false;
			}

			if ( !isInitialDiceRoll && _dice.Select( d => d.GetDieValue() ).Distinct().Count() == 1 ) {
				// All the same number have been rolled. Reroll all die.

				for ( var i = 0; i < 4; i++ ) {
					_dice[i].Select( force: true );
				}

				RollSelectedDice( isInitialDiceRoll: isInitialDiceRoll, callback: callback );
			}

			// Update the Game State with the new Dice values.


			if ( callback != null ) {
				callback();
			}

		}
	}

	private void AppendToLogText( string text )
	{
		var newText = string.Format( "{0}{1}{2}", text, Environment.NewLine, LogText.text );
		if ( newText.Length > 15000 ) {
			newText = newText.Substring( 0, 15000 );
			newText = string.Format( "{0}{1}<<Truncated>>", newText, Environment.NewLine );
		}
		LogText.text = newText;
	}
}

class GameObjectTransformAnimation
{
	public GameObjectTransformAnimation()
	{
		ElapsedAnimationTime = 0;
		AnimationSpeed = 1;
	}
	public GameObject GameObject;
	public float ElapsedAnimationTime;
	public float AnimationSpeed;
	public Vector3 TargetPosition;
	public Quaternion? TargetRotation;


	public Action OnComplete;

	public static Quaternion DefaultRotation = Quaternion.Euler( 0, 0, 0 );
}

class DisabledButtonInteractabilityScope : IDisposable
{
	Dictionary<Button, bool> _buttons;

	public DisabledButtonInteractabilityScope()
	{
		_buttons = GameObject.FindObjectsOfType<Button>().ToDictionary( b => b, b => b.enabled );
		foreach ( var button in _buttons.Keys ) {
			button.interactable = false;
		}
	}
	public void Dispose()
	{
		foreach ( var kvp in _buttons ) {
			kvp.Key.interactable = kvp.Value;
		}
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
	WaitingForFinalization,
	GameOver,

}