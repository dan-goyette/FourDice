using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourDiceGame
{
	public class FourDice
	{
		public GameState GameState;



		public PlayerType CurrentPlayer;

		private TurnAction _lastTurnAction;


		public FourDice( string player2AiName, string player1AiName = null )
		{
			this.GameState = new GameState( player2AiName, player1AiName: player1AiName );
		}



		public void ApplyTurnAction( TurnAction action )
		{
			ValidateTurnAction( action );
		}


		private void ValidateTurnAction( TurnAction action )
		{
			// Ensures that the chosen actions are reasonable given the state of the board. 

			if ( _lastTurnAction != null ) {
				// Validate that if this is the second action, it is consistent with the previous action.
				if ( _lastTurnAction.DieIndex == action.DieIndex ) {
					throw new InvalidOperationException( string.Format( "The same DieIndex may not be chosen in both actions." ) );
				}

				if ( _lastTurnAction.PieceIndex.HasValue && action.PieceIndex.HasValue && _lastTurnAction.PieceIndex.Value == action.PieceIndex.Value ) {
					throw new InvalidOperationException( string.Format( "The same PieceIndex may not be chosen in both actions." ) );

				}
			}

			// Ensure that the action is internally consistent
			if ( action.PieceType == null || action.PieceIndex == null || action.Direction == null ) {
				if ( action.PieceType != null || action.PieceIndex != null || action.Direction != null ) {
					throw new InvalidOperationException( string.Format( "If any of PieceType, PieceIndex, or Direction is null, all must be null." ) );
				}
			}


			// Ensure that the action can be taken.

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

		public Die[] Dice;


		public GameState( string player2AiName, string player1AiName = null )
		{
			Player1 = new Player( player1AiName );
			Player2 = new Player( player2AiName );

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



	}


	[Serializable]
	public class Player
	{
		public Player( string aiName )
		{
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
		/// The index of the Lane position on which the player is currently placed. 0 presents the first
		/// position out of Player 1's goal, while 11 represents the first postion out of Player 2's goal.
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
