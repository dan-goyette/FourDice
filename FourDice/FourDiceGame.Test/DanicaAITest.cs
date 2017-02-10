using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.DomainModel.AI;
using Assets.Scripts.DomainModel;

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
		int numberOfGames = 1;
		string startState = "";

		[TestInitialize]
		public void TestInit()
		{
			danicaAI1 = new BestAI( PlayerType.Player1, true );
			danicaAI2 = new BestAI( PlayerType.Player2, true );
		}

		[TestMethod]
		public void DanicasTest()
		{
			var player1Wins = 0;
			for ( var i = 0; i < numberOfGames; i++ ) {
				if ( PlayAGame( danicaAI1, danicaAI2 ) )
					player1Wins++;
			}

			Debug.WriteLine( string.Format( "{0} wins {1}\n{2} wins {3}", danicaAI1.GetType().Name, player1Wins, danicaAI2.GetType().Name, numberOfGames - player1Wins ) );
		}

		public bool PlayAGame( AIBase AI1, AIBase AI2 )
		{
			FourDice fourDice = new FourDice( null, "danicaAI" );
			fourDice.GameState.CurrentPlayerType = fourDice.RollToSeeWhoGoesFirst();
			var nextMoves = new TurnAction[2];

			while ( !FourDice.GetGameEndResult( fourDice.GameState ).IsFinished && startState != "end" ) {
				if ( startState.Length > 0 ) {
					fourDice.GameState.InitializeFromSerializationCode( startState );
					Debug.WriteLine( fourDice.GameState.GetAsciiState() );
					startState = "end";
				}
				if ( fourDice.GameState.CurrentPlayerType == PlayerType.Player1 ) {
					nextMoves = danicaAI1.GetNextMoves( fourDice.GameState );
				}
				else {
					nextMoves = danicaAI2.GetNextMoves( fourDice.GameState );
				}


				if ( numberOfGames == 1 ) {
					Debug.WriteLine( fourDice.ApplyTurnAction( nextMoves[0] ) );
					Debug.WriteLine( fourDice.ApplyTurnAction( nextMoves[1] ) );
					Debug.WriteLine( fourDice.GameState.GetAsciiState() );
				}
				else {
					fourDice.ApplyTurnAction( nextMoves[0] );
					fourDice.ApplyTurnAction( nextMoves[1] );
				}
				fourDice.RerollDice();

				if ( numberOfGames == 1 ) {
					Debug.WriteLine( fourDice.GameState.GetSerializationCode() );
				}
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
