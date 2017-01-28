using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourDiceGame.AI
{
    public class DefenderAI : AIBase
    {
        public DefenderAI(PlayerType playerType) : base(playerType) { }

        protected override int GameStateValue(GameState gameState)
        {
            var opponentPlayer = getOpponentPlayer(gameState);
            var value = 0;
            foreach (var piece in opponentPlayer.Attackers)
            {
                if (piece.BoardPositionType == BoardPositionType.OwnGoal)
                {
                    value += 20;
                }
            }
            //Debug.WriteLine(_playerType + value);
            return value;
        }
    }

}
