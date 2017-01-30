using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FourDiceGame.AI;
using System.Diagnostics;
using System.Linq;

namespace FourDiceGame.Test
{
	/// <summary>
	/// Summary description for UnitTest2
	/// </summary>
	[TestClass]
	public class DanicaAITest
	{
		AIBase danicaAI1;
		AIBase danicaAI2;
        int numberOfGames = 100;

		[TestInitialize]
		public void TestInit()
		{
			danicaAI1 = new AIBase( PlayerType.Player1 );
			danicaAI2 = new OffensiveAI( PlayerType.Player2 );
		}

		[TestMethod]
		public void DanicasTest()
		{
			var player1Wins = 0;
			for ( var i = 0; i < numberOfGames; i++ ) {
				if ( PlayAGame( danicaAI1, danicaAI2 ) )
					player1Wins++;
			}

			Debug.WriteLine( "Player 1 wins " + player1Wins );
		}

		public bool PlayAGame( AIBase AI1, AIBase AI2 )
		{
			FourDice fourDice = new FourDice( "danicaAI" );
			fourDice.RollToSeeWhoGoesFirst();
			fourDice.GameState.CurrentPlayerType = PlayerType.Player1;
			/*fourDice.GameState.Dice[0].Value = 1;
            fourDice.GameState.Dice[1].Value = 2;
            fourDice.GameState.Dice[2].Value = 3;
            fourDice.GameState.Dice[3].Value = 4;*/
			var nextMoves = new TurnAction[2];

			while ( !FourDice.GetGameEndResult( fourDice.GameState ).IsFinished ) {
				if ( fourDice.GameState.CurrentPlayerType == PlayerType.Player1 ) {
					nextMoves = danicaAI1.GetNextMoves( fourDice.GameState );
				}
				else {
					nextMoves = danicaAI2.GetNextMoves( fourDice.GameState );
				}

				Debug.WriteLine( fourDice.ApplyTurnAction( nextMoves[0] ) );
				Debug.WriteLine( fourDice.ApplyTurnAction( nextMoves[1] ) );
				Debug.WriteLine( fourDice.GameState.GetAsciiState() );
				fourDice.RerollDice();
			}
			var winner = FourDice.GetGameEndResult( fourDice.GameState ).WinningPlayer;
			if ( numberOfGames == 1 ) {
				Debug.WriteLine( "WINNER!! " + winner );

				foreach ( var logEntry in fourDice.GameLog ) {
					Debug.WriteLine( logEntry );
				}
				var p1Captures = fourDice.GameLog.Where( l => l.PlayerType == PlayerType.Player1 ).Count( l => l.CapturedAttackerIndex != null );
				var p2Captures = fourDice.GameLog.Where( l => l.PlayerType == PlayerType.Player2 ).Count( l => l.CapturedAttackerIndex != null );
				Debug.WriteLine( string.Format( "player 1 captured: {0}\nplayer 2 captured: {1}", p1Captures, p2Captures ) );
				Debug.WriteLine( fourDice.GameState.GetPrettyState() );
			}
			return winner == PlayerType.Player1;
		}
	}
}
