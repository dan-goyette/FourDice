using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.DomainModel
{
	public class FourDice
	{
		public const int Player1GoalLanePosition = 0;
		public const int Player1DefenderCircleLanePosition = 3;
		public const int Player1ThresholdLanePosition = 6;
		public const int Player2GoalLanePosition = 13;
		public const int Player2DefenderCircleLanePosition = 10;
		public const int Player2ThresholdLanePosition = 7;


		public GameState GameState;

		public List<GameLogEntry> GameLog;

		private TurnAction _lastTurnAction;

		public FourDice( string player1AiName, string player2AiName )
		{
			this.GameState = new GameState( player1AiName: player1AiName, player2AiName: player2AiName );
			this.GameLog = new List<GameLogEntry>();
		}

		public GameLogEntry ApplyTurnAction( TurnAction turnAction )
		{
			var gameLogEntry = new GameLogEntry();
			var player = this.GameState.GetCurrentPlayer();
			gameLogEntry.DieIndex = turnAction.DieIndex;
			gameLogEntry.DieValue = this.GameState.Dice[turnAction.DieIndex].Value;
			gameLogEntry.PieceIndex = turnAction.PieceIndex;
			gameLogEntry.PieceType = turnAction.PieceType;
			gameLogEntry.PlayerType = this.GameState.CurrentPlayerType;
			gameLogEntry.DiceValues[0] = this.GameState.Dice[0].Value;
			gameLogEntry.DiceValues[1] = this.GameState.Dice[1].Value;
			gameLogEntry.DiceValues[2] = this.GameState.Dice[2].Value;
			gameLogEntry.DiceValues[3] = this.GameState.Dice[3].Value;

			if ( turnAction.PieceType.HasValue ) {
				if ( turnAction.PieceType == PieceType.Attacker ) {
					gameLogEntry.InitialBoardPositionType = player.Attackers[turnAction.PieceIndex.Value].BoardPositionType;
					gameLogEntry.InitialLanePosition = player.Attackers[turnAction.PieceIndex.Value].LanePosition;
				}
				else if ( turnAction.PieceType == PieceType.Defender ) {
					gameLogEntry.InitialBoardPositionType = player.Defenders[turnAction.PieceIndex.Value].BoardPositionType;
					gameLogEntry.InitialLanePosition = player.Defenders[turnAction.PieceIndex.Value].LanePosition;
				}
			}

			var applyResult = ApplyTurnActionToGameState( this.GameState, turnAction, _lastTurnAction );

			if ( turnAction.PieceType.HasValue ) {
				if ( turnAction.PieceType == PieceType.Attacker ) {
					gameLogEntry.FinalBoardPositionType = player.Attackers[turnAction.PieceIndex.Value].BoardPositionType;
					gameLogEntry.FinalLanePosition = player.Attackers[turnAction.PieceIndex.Value].LanePosition;
				}
				else if ( turnAction.PieceType == PieceType.Defender ) {
					gameLogEntry.FinalBoardPositionType = player.Defenders[turnAction.PieceIndex.Value].BoardPositionType;
					gameLogEntry.FinalLanePosition = player.Defenders[turnAction.PieceIndex.Value].LanePosition;
				}
			}
			gameLogEntry.CapturedAttackerIndex = applyResult.CapturedAttackerIndex;


			this.GameLog.Add( gameLogEntry );


			// Set this to the last turn if this was the first turn, otherwise switch to the other player.
			if ( _lastTurnAction == null ) {
				_lastTurnAction = turnAction;
			}
			else {
				_lastTurnAction = null;
			}

			return gameLogEntry;
		}

		public void EndTurn()
		{
			this._lastTurnAction = null;
			this.GameState.CurrentPlayerType = this.GameState.CurrentPlayerType == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;
		}


		public static TurnActionAppliedResult ApplyTurnActionToGameState( GameState gameState, TurnAction turnAction, TurnAction lastTurnAction )
		{
			var retval = new TurnActionAppliedResult();

			var validationResult = ValidateTurnAction( gameState, turnAction, lastTurnAction );
			if ( !validationResult.IsValidAction ) {
				throw new InvalidOperationException( "Validation failed: " + validationResult.ValidationFailureReason );
			}


			// Apply the change
			var die = gameState.Dice[turnAction.DieIndex];

			// Indicate that this die was chosen. This will cause this to be rerolled.
			die.IsChosen = true;

			// Move the piece, if applicable.
			var currentPlayer = gameState.GetCurrentPlayer();

			if ( validationResult.PieceToMove != null ) {
				validationResult.PieceToMove.BoardPositionType = validationResult.NewBoardPositionType.Value;
				validationResult.PieceToMove.LanePosition = validationResult.NewLanePosition.Value;

				// Check whether the player has scored.
				if ( gameState.CurrentPlayerType == PlayerType.Player1 && validationResult.PieceToMove.LanePosition == Player2GoalLanePosition
				|| gameState.CurrentPlayerType == PlayerType.Player2 && validationResult.PieceToMove.LanePosition == Player1GoalLanePosition ) {
					// The player has scored.					
					validationResult.PieceToMove.BoardPositionType = BoardPositionType.OpponentGoal;
					validationResult.PieceToMove.LanePosition = null;
				}




				// Determine whether any opponent pieces must be sent back.
				if ( validationResult.PieceToMove.LanePosition.HasValue ) {
					int pieceCount = GetGamePiecesAtLanePosition( gameState, validationResult.PieceToMove.LanePosition.Value ).Count();

					if ( pieceCount == 3 ) {
						Player playerToAffect = gameState.GetOtherPlayer();
						for ( var attackerIndex = 0; attackerIndex < playerToAffect.Attackers.Length; attackerIndex++ ) {
							var attacker = playerToAffect.Attackers[attackerIndex];
							if ( attacker.LanePosition == validationResult.PieceToMove.LanePosition.Value ) {
								// Send this attack back. If there were two attackers here, we just send back 
								// the first one, since it doesn't matter which moves back.
								attacker.LanePosition = null;
								attacker.BoardPositionType = BoardPositionType.OwnGoal;
								retval.CapturedAttackerIndex = attackerIndex;
								break;
							}
						}
					}
				}
			}


			if ( lastTurnAction != null ) {
				gameState.CurrentPlayerType = gameState.CurrentPlayerType == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;
			}

			return retval;
		}

		public static IEnumerable<GamePiece> GetGamePiecesAtLanePosition( GameState gameState, int lanePosition )
		{
			for ( int i = 0; i < gameState.Player1.Attackers.Length; i++ ) {
				if ( gameState.Player1.Attackers[i].LanePosition == lanePosition ) {
					yield return gameState.Player1.Attackers[i];
				}
			}

			for ( int i = 0; i < gameState.Player1.Defenders.Length; i++ ) {
				if ( gameState.Player1.Defenders[i].LanePosition == lanePosition ) {
					yield return gameState.Player1.Defenders[i];
				}
			}

			for ( int i = 0; i < gameState.Player2.Attackers.Length; i++ ) {
				if ( gameState.Player2.Attackers[i].LanePosition == lanePosition ) {
					yield return gameState.Player2.Attackers[i];
				}
			}

			for ( int i = 0; i < gameState.Player2.Defenders.Length; i++ ) {
				if ( gameState.Player2.Defenders[i].LanePosition == lanePosition ) {
					yield return gameState.Player2.Defenders[i];
				}
			}
		}

		public static Dictionary<int, List<GamePiece>> GetGamePiecesAtAllLanePosition( GameState gameState )
		{
			Dictionary<int, List<GamePiece>> retval = new Dictionary<int, List<GamePiece>>();
			for ( int i = Player1GoalLanePosition + 1; i < Player1GoalLanePosition; i++ ) {
				retval[i] = GetGamePiecesAtLanePosition( gameState, i ).ToList();
			}
			return retval;
		}


		public static TurnActionValidationResult ValidateTurnAction( GameState gameState, TurnAction turnAction, TurnAction lastTurnAction )
		{
			// Ensures that the chosen actions are reasonable given the state of the board. 

			if ( lastTurnAction != null ) {
				// Validate that if this is the second action, it is consistent with the previous action.
				if ( lastTurnAction.DieIndex == turnAction.DieIndex ) {
					return TurnActionValidationResult.Fail( string.Format( "The same DieIndex may not be chosen in both actions." ) );
				}

				if ( lastTurnAction.PieceIndex.HasValue && turnAction.PieceIndex.HasValue
					&& lastTurnAction.PieceType.HasValue && turnAction.PieceType.HasValue
					&& lastTurnAction.PieceIndex.Value == turnAction.PieceIndex.Value
					&& lastTurnAction.PieceType.Value == turnAction.PieceType.Value ) {
					return TurnActionValidationResult.Fail( string.Format( "The same PieceIndex may not be chosen for the same PieceType in both actions." ) );
				}
			}

			// Ensure that the action is internally consistent
			if ( turnAction.PieceType == null || turnAction.PieceIndex == null || turnAction.Direction == null ) {
				if ( turnAction.PieceType != null || turnAction.PieceIndex != null || turnAction.Direction != null ) {
					return TurnActionValidationResult.Fail( string.Format( "If any of PieceType, PieceIndex, or Direction is null, all must be null." ) );
				}
			}


			// Ensure that the action can be taken based on the current game state.
			var currentPlayer = gameState.GetCurrentPlayer();

			var die = gameState.Dice[turnAction.DieIndex];

			GamePiece pieceToMove = null;
			int? newLanePosition = null;
			BoardPositionType? newBoardPositionType = null;


			// Defender motion.
			if ( turnAction.PieceType == PieceType.Defender ) {
				// Determine the movement delta 
				int movementDelta = die.Value;

				if ( gameState.CurrentPlayerType == PlayerType.Player2 ) {
					movementDelta *= -1;
				}
				if ( turnAction.Direction == PieceMovementDirection.Backward ) {
					movementDelta *= -1;
				}

				pieceToMove = currentPlayer.Defenders[turnAction.PieceIndex.Value];

				if ( pieceToMove.BoardPositionType == BoardPositionType.DefenderCircle ) {
					newBoardPositionType = BoardPositionType.Lane;
					newLanePosition = gameState.CurrentPlayerType == PlayerType.Player1 ? Player1DefenderCircleLanePosition : Player2DefenderCircleLanePosition;

					// Make the movement delta one closer to 0, as one movement was consumed.
					if ( movementDelta > 0 ) {
						movementDelta -= 1;
					}
					else {
						movementDelta += 1;
					}
				}
				else {
					newLanePosition = pieceToMove.LanePosition;
				}

				newLanePosition += movementDelta;
				newBoardPositionType = BoardPositionType.Lane;

				if ( newLanePosition <= Player1GoalLanePosition || newLanePosition >= Player2GoalLanePosition ) {
					return TurnActionValidationResult.Fail( "Defenders may not move into, or past, the goal." );
				}


				if ( newBoardPositionType == BoardPositionType.Lane ) {
					if ( (currentPlayer.PlayerType == PlayerType.Player1 && newLanePosition > Player1ThresholdLanePosition)
						|| (currentPlayer.PlayerType == PlayerType.Player2 && newLanePosition < Player2ThresholdLanePosition) ) {
						return TurnActionValidationResult.Fail( "Defenders may not be moved into the opponent's half of the board." );
					}

					// Ensure that the spot isn't already full.
					var otherPiecesAtNewLanePostion = GetGamePiecesAtLanePosition( gameState, newLanePosition.Value );
					if ( otherPiecesAtNewLanePostion.Count( p => p.PlayerType == currentPlayer.PlayerType ) == 2 ) {
						return TurnActionValidationResult.Fail( "The selected location is already occupied by two of the player's pieces." );
					}


				}
			}
			else if ( turnAction.PieceType == PieceType.Attacker ) {

				if ( turnAction.Direction == PieceMovementDirection.Backward ) {
					return TurnActionValidationResult.Fail( "Attackers may not move backwards." );
				}

				int movementDelta = die.Value;

				if ( gameState.CurrentPlayerType == PlayerType.Player2 ) {
					movementDelta *= -1;
				}

				pieceToMove = currentPlayer.Attackers[turnAction.PieceIndex.Value];

				if ( pieceToMove.BoardPositionType == BoardPositionType.OpponentGoal ) {
					return TurnActionValidationResult.Fail( "The selected attacker is already in the opponent's goal." );
				}

				if ( pieceToMove.BoardPositionType == BoardPositionType.OwnGoal ) {
					var attackersInPlay = currentPlayer.Attackers.Where( a => a.BoardPositionType == BoardPositionType.Lane );
					if ( attackersInPlay.Count() == 2 ) {
						return TurnActionValidationResult.Fail( "The attacker may not enter the Lane, because two other attackers for this player are already in the Lane." );
					}
				}


				if ( pieceToMove.LanePosition == null ) {
					// This piece hasn't moved. Set its initial lane position to the current player's goal.
					newBoardPositionType = BoardPositionType.Lane;
					newLanePosition = gameState.CurrentPlayerType == PlayerType.Player1 ? Player1GoalLanePosition : Player2GoalLanePosition;
				}
				else {
					newLanePosition = pieceToMove.LanePosition;
				}

				// Now move it the apropriate number of places.
				newLanePosition += movementDelta;
				newBoardPositionType = BoardPositionType.Lane;

				if ( newLanePosition < Player1GoalLanePosition || newLanePosition > Player2GoalLanePosition ) {
					return TurnActionValidationResult.Fail( "Attackers may not move or past the goal." );
				}


				if ( newBoardPositionType == BoardPositionType.Lane ) {
					// Ensure that the spot isn't already full.
					var otherPiecesAtNewLanePostion = GetGamePiecesAtLanePosition( gameState, newLanePosition.Value );
					if ( otherPiecesAtNewLanePostion.Count( p => p.PlayerType == currentPlayer.PlayerType ) == 2 ) {
						return TurnActionValidationResult.Fail( "The selected location is already occupied by two of the player's pieces." );
					}
					if ( otherPiecesAtNewLanePostion.Count( p => p.PlayerType != currentPlayer.PlayerType && p.PieceType == PieceType.Defender ) == 2 ) {
						return TurnActionValidationResult.Fail( "The selected location is already occupied by two of the opponent's defenders." );
					}
					if ( otherPiecesAtNewLanePostion.Count( p => p.PlayerType == currentPlayer.PlayerType && p.PieceType == PieceType.Attacker ) == 1
						&& otherPiecesAtNewLanePostion.Count( p => p.PlayerType != currentPlayer.PlayerType && p.PieceType == PieceType.Defender ) == 1 ) {
						return TurnActionValidationResult.Fail( "The selected location is already occupied by a defender and one of your attackers." );
					}

				}
			}



			// Catch some cases that should probably never happen.
			if ( newBoardPositionType.HasValue && newBoardPositionType == BoardPositionType.OwnGoal ) {
				return TurnActionValidationResult.Fail( "A piece may not be moved into its own goal." );
			}
			if ( newBoardPositionType.HasValue && newBoardPositionType == BoardPositionType.DefenderCircle ) {
				return TurnActionValidationResult.Fail( "A piece may not be moved into a Defender Circle." );
			}

			return TurnActionValidationResult.Succeed( pieceToMove: pieceToMove, newLanePosition: newLanePosition, newBoardPositionType: newBoardPositionType );

		}

		public void ClearLastTurnAction()
		{
			_lastTurnAction = null;
		}

		public PlayerType RollToSeeWhoGoesFirst()
		{
			int player1Total = 0;
			int player2Total = 0;

			while ( player1Total == player2Total ) {
				RerollDice( onlyRerollChosenDie: false );
				player1Total = GameState.Dice[0].Value + GameState.Dice[1].Value;
				player2Total = GameState.Dice[2].Value + GameState.Dice[3].Value;

			}

			return player1Total > player2Total ? PlayerType.Player1 : PlayerType.Player2;
		}

		public void RerollDice( bool onlyRerollChosenDie = true )
		{
			foreach ( var die in GameState.Dice ) {
				if ( die.IsChosen || !onlyRerollChosenDie ) {
					die.IsChosen = false;
					die.Roll();
				}
			}
		}


		/// <summary>
		/// Evaluates whether the game is over, and that a given player has won.
		/// </summary>
		/// <returns></returns>
		public static GameEndResult GetGameEndResult( GameState gameState )
		{
			// See if player 1 wins
			var player1Wins = true;
			for ( var i = 0; i < 5; i++ ) {
				if ( gameState.Player1.Attackers[i].BoardPositionType != BoardPositionType.OpponentGoal ) {
					player1Wins = false;
					break;
				}
			}
			if ( player1Wins ) {
				return new GameEndResult() {
					IsFinished = true,
					WinningPlayer = PlayerType.Player1
				};
			}

			// See if player 2 wins
			var player2Wins = true;
			for ( var i = 0; i < 5; i++ ) {
				if ( gameState.Player2.Attackers[i].BoardPositionType != BoardPositionType.OpponentGoal ) {
					player2Wins = false;
					break;
				}
			}
			if ( player2Wins ) {
				return new GameEndResult() {
					IsFinished = true,
					WinningPlayer = PlayerType.Player2
				};
			}


			// No one has won.
			return new GameEndResult() {
				IsFinished = false
			};
		}
	}


	public class TurnActionAppliedResult
	{
		public int? CapturedAttackerIndex;
	}

	[Serializable]
	public class GameState
	{
		public Player Player1;
		public Player Player2;
		public PlayerType CurrentPlayerType;

		public Die[] Dice;


		public GameState( string player1AiName, string player2AiName )
		{
			Player1 = new Player( PlayerType.Player1, player1AiName );
			Player2 = new Player( PlayerType.Player2, player2AiName );

			Dice = new Die[4];

			for ( int i = 0; i < 4; i++ ) {
				Dice[i] = new Die();
			}
		}

		public void RollDice()
		{
			for ( int i = 0; i < 4; i++ ) {
				Dice[i].Roll();
			}
		}

		public void CopyTo( GameState other )
		{
			if ( other == null ) {
				other = new GameState( null, null );
			}

			other.CurrentPlayerType = this.CurrentPlayerType;

			for ( var i = 0; i < this.Dice.Length; i++ ) {
				other.Dice[i].Value = this.Dice[i].Value;
				other.Dice[i].IsChosen = this.Dice[i].IsChosen;
			}

			for ( var i = 0; i < this.Player1.Attackers.Length; i++ ) {
				other.Player1.Attackers[i].BoardPositionType = this.Player1.Attackers[i].BoardPositionType;
				other.Player1.Attackers[i].LanePosition = this.Player1.Attackers[i].LanePosition;
				other.Player1.Attackers[i].PieceType = this.Player1.Attackers[i].PieceType;
				other.Player1.Attackers[i].PlayerType = this.Player1.Attackers[i].PlayerType;
			}

			for ( var i = 0; i < this.Player1.Defenders.Length; i++ ) {
				other.Player1.Defenders[i].BoardPositionType = this.Player1.Defenders[i].BoardPositionType;
				other.Player1.Defenders[i].LanePosition = this.Player1.Defenders[i].LanePosition;
				other.Player1.Defenders[i].PieceType = this.Player1.Defenders[i].PieceType;
				other.Player1.Defenders[i].PlayerType = this.Player1.Defenders[i].PlayerType;
			}

			other.Player1.PlayerType = this.Player1.PlayerType;
			other.Player1.AIName = this.Player1.AIName;
			other.Player1.PlayerControlType = this.Player1.PlayerControlType;

			for ( var i = 0; i < this.Player2.Attackers.Length; i++ ) {
				other.Player2.Attackers[i].BoardPositionType = this.Player2.Attackers[i].BoardPositionType;
				other.Player2.Attackers[i].LanePosition = this.Player2.Attackers[i].LanePosition;
				other.Player2.Attackers[i].PieceType = this.Player2.Attackers[i].PieceType;
				other.Player2.Attackers[i].PlayerType = this.Player2.Attackers[i].PlayerType;
			}

			for ( var i = 0; i < this.Player2.Defenders.Length; i++ ) {
				other.Player2.Defenders[i].BoardPositionType = this.Player2.Defenders[i].BoardPositionType;
				other.Player2.Defenders[i].LanePosition = this.Player2.Defenders[i].LanePosition;
				other.Player2.Defenders[i].PieceType = this.Player2.Defenders[i].PieceType;
				other.Player2.Defenders[i].PlayerType = this.Player2.Defenders[i].PlayerType;
			}

			other.Player2.PlayerType = this.Player2.PlayerType;
			other.Player2.AIName = this.Player2.AIName;
			other.Player2.PlayerControlType = this.Player2.PlayerControlType;
		}


		public Player GetCurrentPlayer()
		{
			return this.CurrentPlayerType == PlayerType.Player1 ? Player1 : Player2;
		}

		public Player GetOtherPlayer()
		{
			return this.CurrentPlayerType == PlayerType.Player1 ? Player2 : Player1;
		}



		public string GetPrettyState()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( string.Format( "Current Player: {0}", this.CurrentPlayerType ) );
			sb.Append( "Dice: " );
			List<string> diceValues = new List<string>();
			for ( int i = 0; i < Dice.Length; i++ ) {
				diceValues.Add( string.Format( "{0}:{1}{2}", i, Dice[i].Value, Dice[i].IsChosen ? "*" : "" ) );
			}
			sb.AppendLine( string.Join( ", ", diceValues.ToArray() ) );


			Action<Player> printPlayerStats = ( player ) => {
				sb.AppendLine( string.Format( "{0}: ", player.PlayerType ) );
				sb.AppendLine( string.Format( "  Defenders: " ) );
				for ( var i = 0; i < player.Defenders.Length; i++ ) {
					sb.AppendLine( string.Format( "    {0}: BPT: {1}; Position: {2}",
						i,
						player.Defenders[i].BoardPositionType,
						player.Defenders[i].LanePosition ) );
				}
				sb.AppendLine( string.Format( "  Attackers: " ) );
				for ( var i = 0; i < player.Attackers.Length; i++ ) {
					sb.AppendLine( string.Format( "    {0}: BPT: {1}; Position: {2}",
						i,
						player.Attackers[i].BoardPositionType,
						player.Attackers[i].LanePosition ) );
				}
			};

			printPlayerStats( this.Player1 );
			printPlayerStats( this.Player2 );

			return sb.ToString();
		}


		public string GetAsciiState()
		{
			string blank = "   ";
			Func<GamePiece, string> getPieceLabel = ( piece ) => {
				return string.Format( "{0}{1}{2}",
					piece.PlayerType == PlayerType.Player1 ? "1" : "2",
					piece.PieceType == PieceType.Attacker ? "A" : "D",
					piece.PieceIndex );
			};
			string format = @"
 |---|                                                |---|
 |{0}|                 {48} {49} {50} {51}                |{10}|
 |{1}|                                                |{11}|
 |{2}|       ({20})                        ({22})       |{12}|
 |{3}|------------------------------------------------|{13}|
 |{4}|{24}|{26}|{28}|{30}|{32}|{34}||{36}|{38}|{40}|{42}|{44}|{46}|{14}|
 |{5}|{25}|{27}|{29}|{31}|{33}|{35}||{37}|{39}|{41}|{43}|{45}|{47}|{15}|
 |{6}|------------------------------------------------|{16}|
 |{7}|       ({21})                        ({23})       |{17}|
 |{8}|                                                |{18}|
 |{9}|                                                |{19}|
 |---|                                                |---|";

			var inputs = new List<string>() { 
				// 0-4: Player1 OwnGoal pieces.
				this.Player1.Attackers[0].BoardPositionType == BoardPositionType.OwnGoal ? getPieceLabel( this.Player1.Attackers[0] ) : blank,
				this.Player1.Attackers[1].BoardPositionType == BoardPositionType.OwnGoal ? getPieceLabel( this.Player1.Attackers[1] ) : blank,
				this.Player1.Attackers[2].BoardPositionType == BoardPositionType.OwnGoal ? getPieceLabel( this.Player1.Attackers[2] ) : blank,
				this.Player1.Attackers[3].BoardPositionType == BoardPositionType.OwnGoal ? getPieceLabel( this.Player1.Attackers[3] ) : blank,
				this.Player1.Attackers[4].BoardPositionType == BoardPositionType.OwnGoal ? getPieceLabel( this.Player1.Attackers[4] ) : blank,

				// 5-9: Player2 OpponentGoal pieces.
				this.Player2.Attackers[0].BoardPositionType == BoardPositionType.OpponentGoal ? getPieceLabel( this.Player2.Attackers[0]) : blank,
				this.Player2.Attackers[1].BoardPositionType == BoardPositionType.OpponentGoal ? getPieceLabel( this.Player2.Attackers[1]) : blank,
				this.Player2.Attackers[2].BoardPositionType == BoardPositionType.OpponentGoal ? getPieceLabel( this.Player2.Attackers[2]) : blank,
				this.Player2.Attackers[3].BoardPositionType == BoardPositionType.OpponentGoal ? getPieceLabel( this.Player2.Attackers[3]) : blank,
				this.Player2.Attackers[4].BoardPositionType == BoardPositionType.OpponentGoal ? getPieceLabel( this.Player2.Attackers[4]) : blank,

				// 10-14: Player2 OwnGoal pieces.
				this.Player2.Attackers[0].BoardPositionType == BoardPositionType.OwnGoal ? getPieceLabel( this.Player2.Attackers[0] ) : blank,
				this.Player2.Attackers[1].BoardPositionType == BoardPositionType.OwnGoal ? getPieceLabel( this.Player2.Attackers[1] ) : blank,
				this.Player2.Attackers[2].BoardPositionType == BoardPositionType.OwnGoal ? getPieceLabel( this.Player2.Attackers[2] ) : blank,
				this.Player2.Attackers[3].BoardPositionType == BoardPositionType.OwnGoal ? getPieceLabel( this.Player2.Attackers[3] ) : blank,
				this.Player2.Attackers[4].BoardPositionType == BoardPositionType.OwnGoal ? getPieceLabel( this.Player2.Attackers[4] ) : blank,

				// 15-19: Player1 OpponentGoal pieces.
				this.Player1.Attackers[0].BoardPositionType == BoardPositionType.OpponentGoal ? getPieceLabel( this.Player1.Attackers[0]) : blank,
				this.Player1.Attackers[1].BoardPositionType == BoardPositionType.OpponentGoal ? getPieceLabel( this.Player1.Attackers[1]) : blank,
				this.Player1.Attackers[2].BoardPositionType == BoardPositionType.OpponentGoal ? getPieceLabel( this.Player1.Attackers[2]) : blank,
				this.Player1.Attackers[3].BoardPositionType == BoardPositionType.OpponentGoal ? getPieceLabel( this.Player1.Attackers[3]) : blank,
				this.Player1.Attackers[4].BoardPositionType == BoardPositionType.OpponentGoal ? getPieceLabel( this.Player1.Attackers[4]) : blank,
				
				// 20-23: Defender Circles
				this.Player1.Defenders[0].BoardPositionType == BoardPositionType.DefenderCircle ? getPieceLabel( this.Player1.Defenders[0]) : blank,
				this.Player1.Defenders[1].BoardPositionType == BoardPositionType.DefenderCircle ? getPieceLabel( this.Player1.Defenders[1]) : blank,
				this.Player2.Defenders[0].BoardPositionType == BoardPositionType.DefenderCircle ? getPieceLabel( this.Player2.Defenders[0]) : blank,
				this.Player2.Defenders[1].BoardPositionType == BoardPositionType.DefenderCircle ? getPieceLabel( this.Player2.Defenders[1]) : blank

			};

			// 24-47: Pieces at positions.
			for ( int index = 1; index < FourDice.Player2GoalLanePosition; index++ ) {
				var piecesAtPosition = FourDice.GetGamePiecesAtLanePosition( this, index ).ToList();

				if ( piecesAtPosition.Count() == 0 ) {
					inputs.Add( blank );
					inputs.Add( blank );
				}
				else if ( piecesAtPosition.Count() == 1 ) {
					inputs.Add( getPieceLabel( piecesAtPosition[0] ) );
					inputs.Add( blank );
				}
				else {
					inputs.Add( getPieceLabel( piecesAtPosition[0] ) );
					inputs.Add( getPieceLabel( piecesAtPosition[1] ) );
				}
			}

			// 48-51: Dice.
			for ( int i = 0; i < this.Dice.Length; i++ ) {
				inputs.Add( string.Format( "{0}{1}",
					this.Dice[i].Value,
					this.Dice[i].IsChosen ? "* " : "  " ) );
			}

			return string.Format( format, inputs.ToArray() );
		}
	}


	[Serializable]
	public class Player
	{
		public Player( PlayerType playerType, string aiName )
		{
			this.PlayerType = playerType;
			this.PlayerControlType = string.IsNullOrEmpty( aiName ) ? PlayerControlType.Human : PlayerControlType.Computer;
			this.AIName = aiName;
			Attackers = new GamePiece[5];
			Defenders = new GamePiece[2];
			Initialize();
		}

		private void Initialize()
		{
			for ( int i = 0; i < 5; i++ ) {
				var attacker = new GamePiece( BoardPositionType.OwnGoal, PieceType.Attacker, this.PlayerType, i );
				Attackers[i] = attacker;
			}

			for ( int i = 0; i < 2; i++ ) {
				var defender = new GamePiece( BoardPositionType.DefenderCircle, PieceType.Defender, this.PlayerType, i );
				Defenders[i] = defender;
			}
		}

		public PlayerControlType PlayerControlType;
		public PlayerType PlayerType;
		public string AIName;
		public GamePiece[] Attackers;
		public GamePiece[] Defenders;
	}

	[Serializable]
	public class GamePiece
	{
		public GamePiece( BoardPositionType boardPositionType,
			PieceType pieceType,
			PlayerType playerType,
			int pieceIndex )
		{
			this.BoardPositionType = boardPositionType;
			this.PieceType = pieceType;
			this.PlayerType = playerType;
			this.PieceIndex = pieceIndex;
		}

		/// <summary>
		/// Which distinct area of the board on which the piece is currently placed.
		/// </summary>
		public BoardPositionType BoardPositionType;

		/// <summary>
		/// The index of the Lane position on which the player is currently placed. 0 represents player 1's
		/// goal, while 13 is player 2's goal.
		/// </summary>
		public int? LanePosition;

		public PieceType PieceType;

		public PlayerType PlayerType;

		public int PieceIndex;
	}

	[Serializable]
	public class Die
	{
		public Die()
		{
		}
		public int Value;

		public void Roll()
		{
			Value = FourDiceUtils.Random.Next( 1, 7 );
		}
		public bool IsChosen;
	}


	public class GameEndResult
	{
		public bool IsFinished;
		public PlayerType? WinningPlayer;
	}

	[Serializable]
	public class TurnAction
	{
		public TurnAction( int dieIndex )
		{
			this.DieIndex = dieIndex;
		}

		public TurnAction( int dieIndex, PieceMovementDirection direction, PieceType pieceType, int pieceIndex )
		{
			this.DieIndex = dieIndex;
			Direction = direction;
			PieceType = pieceType;
			PieceIndex = pieceIndex;
		}

		public int DieIndex;
		public PieceMovementDirection? Direction;
		public PieceType? PieceType;
		public int? PieceIndex;

	}

	public class TurnActionValidationResult
	{
		public bool IsValidAction;
		public string ValidationFailureReason;


		public GamePiece PieceToMove;
		public int? NewLanePosition;
		public BoardPositionType? NewBoardPositionType;

		public static TurnActionValidationResult Fail( string reason )
		{
			return new TurnActionValidationResult() {
				IsValidAction = false,
				ValidationFailureReason = reason
			};
		}

		public static TurnActionValidationResult Succeed( GamePiece pieceToMove = null,
			int? newLanePosition = null,
			BoardPositionType? newBoardPositionType = null )
		{
			return new TurnActionValidationResult() {
				IsValidAction = true,
				PieceToMove = pieceToMove,
				NewLanePosition = newLanePosition,
				NewBoardPositionType = newBoardPositionType
			};
		}
	}


	public class GameLogEntry
	{
		public GameLogEntry()
		{
			DiceValues = new Dictionary<int, int>();
		}
		public PlayerType PlayerType;
		public int DieIndex;
		public int DieValue;
		public Dictionary<int, int> DiceValues;
		public PieceType? PieceType;
		public int? PieceIndex;
		public int? CapturedAttackerIndex;
		public BoardPositionType? InitialBoardPositionType;
		public BoardPositionType? FinalBoardPositionType;
		public int? InitialLanePosition;
		public int? FinalLanePosition;

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();


			sb.Append( string.Format( "{0} - ", PlayerType ) );

			for ( int dieIndex = 0; dieIndex < 4; dieIndex++ ) {
				if ( dieIndex == DieIndex ) {
					sb.AppendFormat( "({0})", DiceValues[dieIndex] );
				}
				else {
					sb.Append( DiceValues[dieIndex] );
				}
			}

			if ( PieceType.HasValue ) {
				sb.Append( string.Format( " {0}[{1}] moves from {2} to {3}", PieceType,
					PieceIndex,
					InitialBoardPositionType == BoardPositionType.Lane ? InitialLanePosition.ToString() : InitialBoardPositionType.ToString(),
					FinalBoardPositionType == BoardPositionType.Lane ? FinalLanePosition.ToString() : FinalBoardPositionType.ToString() ) );

				if ( CapturedAttackerIndex.HasValue ) {
					sb.Append( string.Format( " (Captured Attacker[{0}])", CapturedAttackerIndex ) );
				}
			}

			return sb.ToString();
		}
	}
}
