using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourDiceGame.AI
{
	public class Danica : IFourDiceAI
	{
		private PlayerType _playerType;
		public Danica( PlayerType playerType )
		{
			this._playerType = playerType;
		}

		public TurnAction[] GetNextMoves( GameState originalGameState )
		{
            var gameState = new GameState("");
            originalGameState.CopyTo(gameState);

            var actions = new TurnAction[2];
            actions[0] = GetNextMove(gameState, null);
            actions[1] = GetNextMove(gameState, actions[0]);
            return actions;
		}

        private TurnAction GetNextMove(GameState gameState, TurnAction prevAction)
        {
            var player = getMyPlayer(gameState);
            var copiedGameState = new GameState("");

            TurnAction bestAction = null;
            int bestValue = -10000;
            for (var dieIndex = 0; dieIndex < 4; dieIndex++)
            {
                if (gameState.Dice[dieIndex].IsChosen) continue;
                for (var pieceIndex = 0; pieceIndex < player.Attackers.Length; pieceIndex++)
                {
                    DoActionThing(gameState, prevAction, copiedGameState, ref bestAction, ref bestValue, dieIndex, pieceIndex, PieceType.Attacker);
                }
                for (var pieceIndex = 0; pieceIndex < player.Defenders.Length; pieceIndex++)
                {
                    DoActionThing(gameState, prevAction, copiedGameState, ref bestAction, ref bestValue, dieIndex, pieceIndex, PieceType.Defender);
                }
            }
            FourDice.ApplyTurnActionToGameState(gameState, bestAction, prevAction);
            return bestAction;

        }

        private void DoActionThing(GameState gameState, 
            TurnAction prevAction, 
            GameState copiedGameState, 
            ref TurnAction bestAction, 
            ref int bestValue, 
            int dieIndex, 
            int pieceIndex,
            PieceType pieceType)
        {
            foreach (PieceMovementDirection direction in Enum.GetValues(typeof(PieceMovementDirection)))
            {
                if (pieceType == PieceType.Attacker && direction == PieceMovementDirection.Backward) continue;
                gameState.CopyTo(copiedGameState);
                var turnAction = new TurnAction(dieIndex, direction, pieceType, pieceIndex);
                var validationResult = FourDice.ValidateTurnAction(copiedGameState, turnAction, prevAction);

                if (!validationResult.IsValidAction) continue;

                FourDice.ApplyTurnActionToGameState(copiedGameState, turnAction, prevAction);

                var value = GameStateValue(copiedGameState);
                if (value > bestValue)
                {
                    bestAction = turnAction;
                    bestValue = value;

                }
            }
        }

        private Player getMyPlayer( GameState gameState )
		{
			return _playerType == PlayerType.Player1 ? gameState.Player1 : gameState.Player2;
		}

		private int GameStateValue( GameState gameState )
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
			return value;
		}

		private int positionValue( int position )
		{
			return _playerType == PlayerType.Player1 ? position : 11 - position;
		}
	}
}
