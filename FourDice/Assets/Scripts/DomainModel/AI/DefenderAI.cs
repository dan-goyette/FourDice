using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.DomainModel.AI
{
	public class DefenderAI : AIBase
	{
		public DefenderAI( PlayerType playerType, bool simulateOpponent) : base( playerType, simulateOpponent) { }

		protected override int GameStateValue( GameState gameState )
		{
			var myPlayer = getMyPlayer( gameState );
			var opponentPlayer = getOpponentPlayer( gameState );
			var value = 0;

			// Offense
			foreach ( var piece in myPlayer.Attackers ) {
				if ( piece.BoardPositionType == BoardPositionType.OpponentGoal ) {
					value += 30;
				}
				else if ( piece.BoardPositionType == BoardPositionType.Lane && piece.LanePosition != null ) {
					value += PositionValue( piece.LanePosition.Value );
				}
			}

			// Keep defendered apart
			if ( myPlayer.Defenders[0].LanePosition != myPlayer.Defenders[1].LanePosition ) {
				value += 5;
			}

			// Defense
			foreach ( var piece in opponentPlayer.Attackers ) {
				if ( piece.BoardPositionType == BoardPositionType.OwnGoal ) {
					value += 300;
				}
			}

			for ( var position = 1; position <= FourDice.Player1ThresholdLanePosition; position++ ) {
				var myPosition = PositionValue( position );
				var playersAtPosition = FourDice.GetGamePiecesAtLanePosition( gameState, myPosition );
				if ( playersAtPosition.Count() == 2 && (playersAtPosition.ElementAt( 0 ).PlayerType != _playerType || playersAtPosition.ElementAt( 1 ).PlayerType != _playerType) ) {
					value += 100;
				}
			}
			//Debug.WriteLine(string.Format("{0} ",value));
			return value;
		}
	}
}
