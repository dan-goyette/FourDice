using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourDiceGame.AI
{
	public class AIBase : IFourDiceAI
	{
		private PlayerType _playerType;
		public AIBase( PlayerType playerType )
		{
			this._playerType = playerType;
		}

        public TurnAction[] GetNextMoves( GameState originalGameState )
		{
            var gameState = new GameState("");
            originalGameState.CopyTo(gameState);
            
            return GetNextMove(gameState, null);
		}

        protected TurnAction[] GetNextMove(GameState gameState, TurnAction prevAction)
        {
            var player = getMyPlayer(gameState);
            var copiedGameState = new GameState("");

            var bestActions = new TurnAction[2];
            int bestValue = -10000;
            for (var dieIndex = 0; dieIndex < 4; dieIndex++)
            {
                if (gameState.Dice[dieIndex].IsChosen) continue;
                for (var pieceIndex = 0; pieceIndex < player.Attackers.Length; pieceIndex++)
                {
                    checkValueofAction(gameState, prevAction, copiedGameState, ref bestActions, ref bestValue, dieIndex, pieceIndex, PieceType.Attacker);
                }
                for (var pieceIndex = 0; pieceIndex < player.Defenders.Length; pieceIndex++)
                {
                    checkValueofAction(gameState, prevAction, copiedGameState, ref bestActions, ref bestValue, dieIndex, pieceIndex, PieceType.Defender);
                }
                checkValueofAction(gameState, prevAction, copiedGameState, ref bestActions, ref bestValue, dieIndex);
            }
            if (bestActions[0] == null)
            {
                Debug.WriteLine("no good actions :-(");
            }
            //FourDice.ApplyTurnActionToGameState(gameState, bestAction, prevAction);
            return bestActions;

        }

        protected void checkValueofAction(GameState gameState, 
            TurnAction prevAction, 
            GameState copiedGameState, 
            ref TurnAction[] bestActions, 
            ref int bestValue, 
            int dieIndex, 
            int? pieceIndex = null,
            PieceType? pieceType = null)
        {
            foreach (PieceMovementDirection direction in Enum.GetValues(typeof(PieceMovementDirection)))
            {
                if (pieceType == PieceType.Attacker && direction == PieceMovementDirection.Backward) continue;

                gameState.CopyTo(copiedGameState);
                TurnAction turnAction;
                if (pieceIndex.HasValue && pieceType.HasValue)
                {
                    turnAction = new TurnAction(dieIndex, direction, pieceType.Value, pieceIndex.Value);
                } else
                {
                    turnAction = new TurnAction(dieIndex);
                }
                var validationResult = FourDice.ValidateTurnAction(copiedGameState, turnAction, prevAction);

                if (!validationResult.IsValidAction) continue;

                FourDice.ApplyTurnActionToGameState(copiedGameState, turnAction, prevAction);


                if (prevAction == null)
                {
                    GetNextMove(copiedGameState, turnAction);
                } else
                {
                    var value = GameStateValue(copiedGameState);
                    if (value > bestValue)
                    {
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

        protected Player getOpponentPlayer(GameState gameState)
        {
            return _playerType == PlayerType.Player1 ? gameState.Player2 : gameState.Player1;
        }

        protected int positionValue(int position)
        {
            return _playerType == PlayerType.Player1 ? position : 11 - position;
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
					value += positionValue( piece.LanePosition.Value );
				}
			}
            //Debug.WriteLine(_playerType + value);
			return value;
		}
	}
}
