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
		protected int originalValue = 0;

		public AIBase( PlayerType playerType, bool simulateOpponent = true )
		{
			this._playerType = playerType;
            if (simulateOpponent)
            {
                this._opponentAI = new OpponentAI( _playerType == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1, false);
            }
        }

		public TurnAction[] GetNextMoves( GameState originalGameState )
		{
			var gameState = new GameState( null, null );
			originalGameState.CopyTo( gameState );
			originalValue = GameStateValue( originalGameState );

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
				checkValueofAction( gameState, prevAction, copiedGameState, dieIndex );

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
					value -= originalValue;
					if (_opponentAI != null)
                    {
                        value -= (int) (_opponentAI.GetNextMoveValue( copiedGameState ) * 0.3);
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
            var myPlayer = getMyPlayer(gameState);
            var opponentPlayer = getOpponentPlayer(gameState);
            var value = 0;

            var allPiecesAtPositions = FourDice.GetGamePiecesAtAllLanePosition(gameState);

            // Offense
            foreach (var piece in myPlayer.Attackers)
            {
                if (piece.BoardPositionType == BoardPositionType.OpponentGoal)
                {
                    value += 200;
                }
                else if (piece.BoardPositionType == BoardPositionType.Lane && piece.LanePosition != null)
                {
                    value += PositionValue(piece.LanePosition.Value);
                }
            }

            // Keep defenders apart
            if (myPlayer.Defenders[0].LanePosition != myPlayer.Defenders[1].LanePosition)
            {
                value += 5;
            }

            // Defense
            foreach (var piece in opponentPlayer.Attackers)
            {
                if (piece.BoardPositionType == BoardPositionType.OwnGoal)
                {
                    value += 300;
                }
			}

			for ( var position = 1; position < FourDice.Player2GoalLanePosition; position++ ) {
				var myPosition = PositionValue( position );
				List<GamePiece> piecesAtLocation;
				if ( !allPiecesAtPositions.TryGetValue( myPosition, out piecesAtLocation ) ) {
					piecesAtLocation = new List<GamePiece>();
				}
				if ( piecesAtLocation.Count() == 2 ) {
					if ( position <= FourDice.Player1ThresholdLanePosition && (piecesAtLocation.ElementAt( 0 ).PlayerType != _playerType || piecesAtLocation.ElementAt( 1 ).PlayerType != _playerType) ) {
						value += 100;
					}
					else if ( position > FourDice.Player1ThresholdLanePosition ) {
						value -= 500;
					}
					else {
						value -= 30;
					}
				}
			}
			//Debug.WriteLine(string.Format("{0} ",value));
			return value;
        }
	}
}
