using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.DomainModel.AI
{
	public class OpponentAI : AIBase
	{
		public OpponentAI( PlayerType playerType, bool simulateOpponent ) : base( playerType, simulateOpponent ) { }

		protected override int GameStateValue( GameState gameState )
		{
			var myPlayer = getMyPlayer( gameState );
			var opponentPlayer = getOpponentPlayer( gameState );
			var value = 0;

			var allPiecesAtPositions = FourDice.GetGamePiecesAtAllLanePosition( gameState );

			// Offense
			foreach ( var piece in myPlayer.Attackers ) {
				if ( piece.BoardPositionType == BoardPositionType.OpponentGoal ) {
					value += 600;
				}
				else if ( piece.BoardPositionType == BoardPositionType.Lane && piece.LanePosition != null ) {
					value += PositionValue( piece.LanePosition.Value );
				}
			}

			// Defense
			foreach ( var piece in opponentPlayer.Attackers ) {
				if ( piece.BoardPositionType == BoardPositionType.OwnGoal ) {
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
						value += 0;
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
