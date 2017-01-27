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

		public TurnAction[] GetNextMoves( GameState gameState )
		{
            var copiedGameState = new GameState("");
            gameState.CopyTo(copiedGameState);

            var actions = new TurnAction[2];
            actions[0] = GetNextMove(copiedGameState, null);
            actions[1] = GetNextMove(copiedGameState, actions[0]);
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
                    gameState.CopyTo(copiedGameState);
                    var turnAction = new TurnAction(dieIndex, PieceMovementDirection.Forward, PieceType.Attacker, pieceIndex);
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
                for (var pieceIndex = 0; pieceIndex < player.Defenders.Length; pieceIndex++)
                {
                    foreach (PieceMovementDirection direction in Enum.GetValues(typeof(PieceMovementDirection)))
                    {
                        gameState.CopyTo(copiedGameState);
                        var turnAction = new TurnAction(dieIndex, direction, PieceType.Defender, pieceIndex);
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
            }
            FourDice.ApplyTurnActionToGameState(gameState, bestAction, prevAction);
            return bestAction;

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
