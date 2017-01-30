using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.DomainModel.AI
{
	public class OffensiveAI : AIBase
	{
		public OffensiveAI( PlayerType playerType ) : base( playerType ) { }

		protected override int GameStateValue( GameState gameState )
		{
			var player = getMyPlayer( gameState );
			var value = 0;
			foreach ( var piece in player.Attackers ) {
				if ( piece.BoardPositionType == BoardPositionType.OpponentGoal ) {
					value += 30;
				}
				else if ( piece.BoardPositionType == BoardPositionType.Lane && piece.LanePosition != null ) {
					var positionValue = PositionValue( piece.LanePosition.Value );
					if ( positionValue > 6 ) {
						value += 10;
					}
					else {
						value += positionValue;
					}
				}
			}
			//Debug.WriteLine(string.Format("{0} {1}",_playerType,  value));
			return value;
		}
	}
}
