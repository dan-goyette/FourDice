using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FourDiceGame
{
	public class FourDice
	{
		public const int Player1GoalLanePosition = 0;
		public const int Player1DefenderCircleLanePosition = 3;
		public const int Player2GoalLanePosition = 13;
		public const int Player2DefenderCircleLanePosition = 10;


		public GameState GameState;

		private TurnAction _lastTurnAction;

		public FourDice( string player2AiName, string player1AiName = null )
		{
			this.GameState = new GameState( player2AiName, player1AiName: player1AiName );
		}

		public void ApplyTurnAction( TurnAction turnAction )
		{
			ApplyTurnActionToGameState( this.GameState, turnAction, ref this._lastTurnAction );
		}


		public static void ApplyTurnActionToGameState( GameState gameState, TurnAction turnAction, ref TurnAction lastTurnAction )
		{
			ValidateTurnAction( turnAction, ref lastTurnAction );


			// Apply the change
			var die = gameState.Dice[turnAction.DieIndex];

			// Indicate that this die was chosen. This will cause this to be rerolled.
			die.IsChosen = true;

			// Move the piece, if applicable.

			int? newPlayerPosition = null;
			if ( turnAction.PieceType == PieceType.Defender ) {
				// Determine the movement delta 
				int movementDelta = die.Value;

				if ( gameState.CurrentPlayer.PlayerType == PlayerType.Player2 ) {
					movementDelta *= -1;
				}
				if ( turnAction.Direction == PieceMovementDirection.Backward ) {
					movementDelta *= -1;
				}

				var defender = gameState.CurrentPlayer.Defenders[turnAction.PieceIndex.Value];

				if ( defender.BoardPositionType == BoardPositionType.DefenderCircle ) {
					defender.BoardPositionType = BoardPositionType.Lane;
					defender.LanePosition = gameState.CurrentPlayer.PlayerType == PlayerType.Player1 ? Player1DefenderCircleLanePosition : Player2DefenderCircleLanePosition;
					// Make the movement delta one closer to 0, as one movement was consumed.
					if ( movementDelta > 0 ) {
						movementDelta -= 1;
					}
					else {
						movementDelta += 1;
					}
				}

				defender.LanePosition += movementDelta;
				newPlayerPosition = defender.LanePosition;
			}
			else if ( turnAction.PieceType == PieceType.Attacker ) {
				int movementDelta = die.Value;

				if ( gameState.CurrentPlayer.PlayerType == PlayerType.Player2 ) {
					movementDelta *= -1;
				}

				var attacker = gameState.CurrentPlayer.Attackers[turnAction.PieceIndex.Value];
				if ( attacker.LanePosition == null ) {
					// This piece hasn't moved. Set its initial lane position to the current player's goal.
					attacker.BoardPositionType = BoardPositionType.Lane;
					attacker.LanePosition = gameState.CurrentPlayer.PlayerType == PlayerType.Player1 ? Player1GoalLanePosition : Player2GoalLanePosition;
				}
				// Now move it the apropriate number of places.
				attacker.LanePosition += movementDelta;


				if ( gameState.CurrentPlayer.PlayerType == PlayerType.Player1 && attacker.LanePosition == Player2GoalLanePosition
					|| gameState.CurrentPlayer.PlayerType == PlayerType.Player2 && attacker.LanePosition == Player1GoalLanePosition ) {
					// The player has scored.
					attacker.LanePosition = null;
					attacker.BoardPositionType = BoardPositionType.OpponentGoal;
				}

				newPlayerPosition = attacker.LanePosition;
			}
			else {
				throw new InvalidOperationException();
			}

			// Determine whether any opponent pieces must be sent back.
			if ( newPlayerPosition.HasValue ) {
				int pieceCount = gameState.Player1.Attackers.Count( a => a.LanePosition == newPlayerPosition.Value )
					+ gameState.Player1.Defenders.Count( a => a.LanePosition == newPlayerPosition.Value )
					+ gameState.Player2.Attackers.Count( a => a.LanePosition == newPlayerPosition.Value )
					+ gameState.Player2.Defenders.Count( a => a.LanePosition == newPlayerPosition.Value );

				if ( pieceCount == 3 ) {
					Player playerToAffect = gameState.CurrentPlayer == gameState.Player1 ? gameState.Player2 : gameState.Player1;
					foreach ( var attacker in playerToAffect.Attackers.ToList() ) {
						if ( attacker.LanePosition == newPlayerPosition.Value ) {
							// Send this attack back. If there were two attackers here, we just send back 
							// the first one, since it doesn't matter which moves back.
							attacker.LanePosition = null;
							attacker.BoardPositionType = BoardPositionType.OwnGoal;
							break;
						}
					}
				}
			}


			// Set this to the last turn if this was the first turn, otherwise switch to the other player.
			if ( lastTurnAction == null ) {
				lastTurnAction = turnAction;
			}
			else {
				lastTurnAction = null;
				gameState.CurrentPlayer = gameState.CurrentPlayer == gameState.Player1 ? gameState.Player2 : gameState.Player1;
			}
		}


		public static void ValidateTurnAction( TurnAction turnAction, ref TurnAction lastTurnAction )
		{
			// Ensures that the chosen actions are reasonable given the state of the board. 

			if ( lastTurnAction != null ) {
				// Validate that if this is the second action, it is consistent with the previous action.
				if ( lastTurnAction.DieIndex == turnAction.DieIndex ) {
					throw new InvalidOperationException( string.Format( "The same DieIndex may not be chosen in both actions." ) );
				}

				if ( lastTurnAction.PieceIndex.HasValue && turnAction.PieceIndex.HasValue
					&& lastTurnAction.PieceType.HasValue && turnAction.PieceType.HasValue
					&& lastTurnAction.PieceIndex.Value == turnAction.PieceIndex.Value
					&& lastTurnAction.PieceType.Value == turnAction.PieceType.Value ) {
					throw new InvalidOperationException( string.Format( "The same PieceIndex may not be chosen for the same PieceType in both actions." ) );

				}
			}

			// Ensure that the action is internally consistent
			if ( turnAction.PieceType == null || turnAction.PieceIndex == null || turnAction.Direction == null ) {
				if ( turnAction.PieceType != null || turnAction.PieceIndex != null || turnAction.Direction != null ) {
					throw new InvalidOperationException( string.Format( "If any of PieceType, PieceIndex, or Direction is null, all must be null." ) );
				}
			}


			// Ensure that the action can be taken.

		}

		public PlayerType RollToSeeWhoGoesFirst()
		{
			int player1Total = 0;
			int player2Total = 0;

			while ( player1Total == player2Total ) {
				RerollDice( onlyRerollChosenDie: false );
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


	[Serializable]
	public class GameState
	{
		public Player Player1;
		public Player Player2;
		public Player CurrentPlayer;

		public Die[] Dice;


		public GameState( string player2AiName, string player1AiName = null )
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

		public GameState GetCopy()
		{
			return JsonConvert.DeserializeObject<GameState>( JsonConvert.SerializeObject( this ) );
		}



	}


	[Serializable]
	public class Player
	{
		public Player( PlayerType playerType, string aiName )
		{
			this.PlayerType = playerType;
			this.PlayerControlType = string.IsNullOrWhiteSpace( aiName ) ? PlayerControlType.Human : PlayerControlType.Computer;
			this.AIName = aiName;
			Attackers = new GamePiece[5];
			Defenders = new GamePiece[2];
			Initialize();
		}

		private void Initialize()
		{
			for ( int i = 0; i < 5; i++ ) {
				var attacker = new GamePiece() {
					BoardPositionType = BoardPositionType.OwnGoal,
					PieceType = PieceType.Attacker
				};
				Attackers[i] = attacker;
			}

			for ( int i = 0; i < 2; i++ ) {
				var defender = new GamePiece() {
					BoardPositionType = BoardPositionType.DefenderCircle,
					PieceType = PieceType.Defender
				};
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
	}

	[Serializable]
	public class Die
	{
		private Random _random;
		public Die()
		{
			_random = new Random();
		}
		public int Value;

		public void Roll()
		{
			Value = _random.Next( 1, 7 );
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
		public int DieIndex;
		public PieceMovementDirection? Direction;
		public PieceType? PieceType;
		public int? PieceIndex;

	}
}
