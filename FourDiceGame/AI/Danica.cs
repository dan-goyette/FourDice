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
        public Danica(PlayerType playerType)
        {
            this._playerType = playerType;
        }

        public TurnAction[] GetNextMoves(GameState gameState)
        {
            var currentGameValue = GameStateValue(gameState);
            var player = getMyPlayer(gameState);
            TurnAction bestAction = null;
            int bestValue = -10000;
            for (var dieIndex = 0; dieIndex < 4; dieIndex++)
            {
                //var die = gameState.Dice[dieIndex];
                for (var pieceIndex = 0; pieceIndex < player.Attackers.Length; pieceIndex++)
                {
                    var turnAction = new TurnAction()
                    {
                        DieIndex = dieIndex,
                        Direction = PieceMovementDirection.Forward,
                        PieceType = PieceType.Attacker,
                        PieceIndex = pieceIndex
                    };
                    TurnAction prevAction = null;
                    //FourDice.ValidateTurnAction(gameState, turnAction);
                    var copiedGameState = gameState.GetCopy();
                    FourDice.ApplyTurnActionToGameState(copiedGameState, turnAction, ref prevAction);

                    var value = GameStateValue(copiedGameState);
                    if (value > bestValue)
                    {
                        bestAction = turnAction;
                    }
                }
                foreach (var defender in player.Defenders)
                {

                }
            }
            //Debug.WriteLine(GameState);
            //throw new NotImplementedException();
            var actions = new TurnAction[2];
            actions[0] = bestAction;
            return actions;
        }
        
        private Player getMyPlayer(GameState gameState)
        {
            return _playerType == PlayerType.Player1 ? gameState.Player1 : gameState.Player2;
        }

        private int GameStateValue(GameState gameState)
        {
            var player = getMyPlayer(gameState);
            var value = 0;
            foreach (var piece in player.Attackers)
            {
                if (piece.BoardPositionType == BoardPositionType.OpponentGoal)
                {
                    value += 20;
                }
                else if (piece.BoardPositionType == BoardPositionType.Lane && piece.LanePosition != null)
                {
                    value += positionValue(piece.LanePosition.Value);
                }
            }
            return value;
        }

        private int positionValue (int position)
        {
            return _playerType == PlayerType.Player1 ? position : 11 - position;
        }
    }
}
