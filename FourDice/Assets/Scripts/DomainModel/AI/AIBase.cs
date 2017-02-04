using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Assets.Scripts.DomainModel.AI
{
	public class AIBase : IFourDiceAI
	{
		protected PlayerType _playerType;
		protected AIBase _opponentAI;
		protected TurnAction[] bestActions = new TurnAction[2];
		protected int bestValue = -10000;

		public AIBase( PlayerType playerType, bool simulateOpponent = true )
		{
			this._playerType = playerType;
			if ( simulateOpponent ) {
				this._opponentAI = new DefenderAI( _playerType == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1, false );
			}
		}

		public TurnAction[] GetNextMoves( GameState originalGameState )
		{
			var gameState = new GameState( null, null );
			originalGameState.CopyTo( gameState );

			bestActions = new TurnAction[2];
			bestValue = -10000;

			return GetNextMove( gameState, null );
		}

		public int GetNextMoveValue( GameState originalGameState )
		{
			GetNextMoves( originalGameState );
			return bestValue;
		}

		protected TurnAction[] GetNextMove( GameState gameState, TurnAction prevAction )
		{
			var copiedGameState = new GameState( null, null );
			gameState.CopyTo( copiedGameState );

			var player = getMyPlayer( gameState );
			var checkedDieValues = new bool[6];
			for ( var dieIndex = 0; dieIndex < 4; dieIndex++ ) {
				if ( gameState.Dice[dieIndex].IsChosen ) continue;
				var dieValue = gameState.Dice[dieIndex].Value;
				if ( checkedDieValues[dieValue - 1] ) continue;
				checkedDieValues[dieValue - 1] = true;

				var checkedAttackerPositions = new bool[13];
				for ( var pieceIndex = 0; pieceIndex < player.Attackers.Length; pieceIndex++ ) {
					var attacker = player.Attackers[pieceIndex];
					if ( attacker.BoardPositionType == BoardPositionType.OpponentGoal ) continue;
					var attackerPosition = attacker.LanePosition ?? 0;
					if ( checkedAttackerPositions[attackerPosition] ) continue;
					checkedAttackerPositions[attackerPosition] = true;

					checkValueofAction( gameState, prevAction, copiedGameState, dieIndex, pieceIndex, PieceType.Attacker );
				}
				for ( var pieceIndex = 0; pieceIndex < player.Defenders.Length; pieceIndex++ ) {
					checkValueofAction( gameState, prevAction, copiedGameState, dieIndex, pieceIndex, PieceType.Defender );
				}
				checkValueofAction( gameState, prevAction, copiedGameState, dieIndex );
			}
			if ( bestActions[0] == null ) {
				Debug.WriteLine( "no good actions :-(" );
			}
			//FourDice.ApplyTurnActionToGameState(gameState, bestAction, prevAction);
			return bestActions;

		}

		protected void checkValueofAction( GameState gameState,
			TurnAction prevAction,
			GameState copiedGameState,
			int dieIndex,
			int? pieceIndex = null,
			PieceType? pieceType = null )
		{
			foreach ( PieceMovementDirection direction in Enum.GetValues( typeof( PieceMovementDirection ) ) ) {
				if ( pieceType == PieceType.Attacker && direction == PieceMovementDirection.Backward ) continue;

				gameState.CopyTo( copiedGameState );
				TurnAction turnAction;
				if ( pieceIndex.HasValue && pieceType.HasValue ) {
					turnAction = new TurnAction( dieIndex, direction, pieceType.Value, pieceIndex.Value );
				}
				else {
					turnAction = new TurnAction( dieIndex );
				}
				var validationResult = FourDice.ValidateTurnAction( copiedGameState, turnAction, prevAction );

				if ( !validationResult.IsValidAction ) continue;

				FourDice.ApplyTurnActionToGameState( copiedGameState, turnAction, prevAction );


				if ( prevAction == null ) {
					GetNextMove( copiedGameState, turnAction );
				}
				else {
					var value = GameStateValue( copiedGameState );
					if ( _opponentAI != null ) {
						value -= _opponentAI.GetNextMoveValue( copiedGameState );
					}
					if ( value > bestValue ) {
						bestActions[0] = prevAction;
						bestActions[1] = turnAction;
						bestValue = value;
					}
				}
			}
		}

		protected Player getMyPlayer( GameState gameState )
		{
			return _playerType == PlayerType.Player1 ? gameState.Player1 : gameState.Player2;
		}

		protected Player getOpponentPlayer( GameState gameState )
		{
			return _playerType == PlayerType.Player1 ? gameState.Player2 : gameState.Player1;
		}

		protected int PositionValue( int position )
		{
			return _playerType == PlayerType.Player1 ? position : FourDice.Player2GoalLanePosition - position;
		}

		protected virtual int GameStateValue( GameState gameState )
		{
			var player = getMyPlayer( gameState );
			var value = 0;
			foreach ( var piece in player.Attackers ) {
				if ( piece.BoardPositionType == BoardPositionType.OpponentGoal ) {
					value += 20;
				}
				else if ( piece.BoardPositionType == BoardPositionType.Lane && piece.LanePosition != null ) {
					value += PositionValue( piece.LanePosition.Value );
				}
			}
			//Debug.WriteLine(string.Format("{0} {1}",_playerType,  value));
			return value;
		}
	}
}
