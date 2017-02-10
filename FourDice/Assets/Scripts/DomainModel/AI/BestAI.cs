﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.DomainModel.AI
{
	public class BestAI : AIBase
	{


		public BestAI( PlayerType playerType, bool simulateOpponent, AIOptions aiOptions = null ) : base( playerType, simulateOpponent: simulateOpponent, aiOptions: aiOptions ) { }

		protected override int GameStateValue( GameState gameState )
		{
			var myPlayer = getMyPlayer( gameState );
			var opponentPlayer = getOpponentPlayer( gameState );
			var value = 0;

			var allPiecesAtPositions = FourDice.GetGamePiecesAtAllLanePosition( gameState );

			// Offense
			foreach ( var piece in myPlayer.Attackers ) {
				if ( piece.BoardPositionType == BoardPositionType.OpponentGoal ) {
					value += this._aiOptions.AttackerReachesGoalPoints;
				}
				else if ( piece.BoardPositionType == BoardPositionType.Lane && piece.LanePosition != null ) {
					value += PositionValue( piece.LanePosition.Value ) * this._aiOptions.AttackerLanePositionPointMultiplier;
				}
			}

			// Get defenders out
			if ( myPlayer.Defenders[0].BoardPositionType != BoardPositionType.DefenderCircle ) {
				value += this._aiOptions.DefenderLeavingDefenderCirclePoints;
			}
			if ( myPlayer.Defenders[1].BoardPositionType != BoardPositionType.DefenderCircle ) {
				value += this._aiOptions.DefenderLeavingDefenderCirclePoints;
			}

			// Stomp on opponents
			foreach ( var piece in opponentPlayer.Attackers ) {
				if ( piece.BoardPositionType == BoardPositionType.OwnGoal ) {
					value += this._aiOptions.OpponentAttackersInOwnGoalPoints;
				}
			}

			for ( var position = 1; position < FourDice.Player2GoalLanePosition; position++ ) {
				var myPosition = PositionValue( position );
				List<GamePiece> piecesAtLocation;

				if ( !allPiecesAtPositions.TryGetValue( myPosition, out piecesAtLocation ) ) {
					piecesAtLocation = new List<GamePiece>();
				}
				if ( piecesAtLocation.Count() == 2 ) {

					PlayerType PT0 = piecesAtLocation.ElementAt( 0 ).PlayerType;
					PlayerType PT1 = piecesAtLocation.ElementAt( 1 ).PlayerType;
					if ( PT0 == _playerType || PT1 == _playerType ) {
						// It's good to double up next to an opponent piece on my side of the board
						if ( position <= FourDice.Player1ThresholdLanePosition && (PT0 != _playerType || PT1 != _playerType) ) {
							value += this._aiOptions.PutOpponentAtRiskOnSelfSidePoints;
						}
						// It's bad to double up on the opponent's side of the board
						else if ( position > FourDice.Player1ThresholdLanePosition ) {
							if ( PT0 == _playerType ) {
								value -= this._aiOptions.PenaltyForDoublingUpOnOpponentsSidePoints;
							}
							if ( PT1 == _playerType ) {
								value -= this._aiOptions.PenaltyForDoublingUpOnOpponentsSidePoints;
							}
						}
						// Don't double up with my own piece on my side of the board
						else {
							value -= this._aiOptions.PenaltyForDoublingUpOnSelfSidePoints;
						}
					}
				}
			}
			//Debug.WriteLine(string.Format("{0} ",value));
			return value;
		}
	}
}
