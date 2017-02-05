using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.DomainModel.AI
{
	public class OffensiveAI : AIBase
	{
		public OffensiveAI(PlayerType playerType, bool simulateOpponent) : base(playerType, simulateOpponent) { }

        protected override int GameStateValue( GameState gameState )
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
                    value += PositionValue(piece.LanePosition.Value);
                }
            }
            return value;
        }
	}
}
