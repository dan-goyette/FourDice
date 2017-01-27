﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FourDiceGame.Test
{
	[TestClass]
	public class UnitTest1
	{
		FourDice fourDice;

		[TestInitialize]
		public void TestInit()
		{
			fourDice = new FourDice( "danicaAI" );
		}


		[TestMethod]
		public void SampleUnitTest()
		{
			Assert.AreEqual( 4, fourDice.GameState.Dice.Length );
			Assert.AreEqual( 5, fourDice.GameState.Player1.Attackers.Length );
			Assert.AreEqual( 2, fourDice.GameState.Player1.Defenders.Length );
			Assert.AreEqual( 5, fourDice.GameState.Player2.Attackers.Length );
			Assert.AreEqual( 2, fourDice.GameState.Player2.Defenders.Length );

			var gameEndResult = FourDice.GetGameEndResult( fourDice.GameState );
			Assert.IsFalse( gameEndResult.IsFinished );
			Assert.IsNull( gameEndResult.WinningPlayer );
		}



		[TestMethod]
		public void TurnActionTest1()
		{
			fourDice.GameState.CurrentPlayerType = PlayerType.Player1;

			fourDice.GameState.Dice[0].Value = 1;
			fourDice.GameState.Dice[1].Value = 2;
			fourDice.GameState.Dice[2].Value = 3;
			fourDice.GameState.Dice[3].Value = 4;


			{
				var turn1 = new TurnAction( 0, PieceMovementDirection.Forward, PieceType.Defender, 0 );
				fourDice.ApplyTurnAction( turn1 );


				Assert.AreEqual( fourDice.GameState.Player1, fourDice.GameState.GetCurrentPlayer() );
				Assert.IsTrue( fourDice.GameState.Dice[0].IsChosen );
				Assert.IsFalse( fourDice.GameState.Dice[1].IsChosen );
				Assert.IsFalse( fourDice.GameState.Dice[2].IsChosen );
				Assert.IsFalse( fourDice.GameState.Dice[3].IsChosen );

				Assert.AreEqual( BoardPositionType.Lane, fourDice.GameState.Player1.Defenders[0].BoardPositionType );
				Assert.AreEqual( FourDice.Player1DefenderCircleLanePosition, fourDice.GameState.Player1.Defenders[0].LanePosition );


				var turn2 = new TurnAction( 1, PieceMovementDirection.Forward, PieceType.Attacker, 0 );
				fourDice.ApplyTurnAction( turn2 );

				Assert.AreEqual( fourDice.GameState.Player2, fourDice.GameState.GetCurrentPlayer() );
				Assert.IsTrue( fourDice.GameState.Dice[0].IsChosen );
				Assert.IsTrue( fourDice.GameState.Dice[1].IsChosen );
				Assert.IsFalse( fourDice.GameState.Dice[2].IsChosen );
				Assert.IsFalse( fourDice.GameState.Dice[3].IsChosen );

				Assert.AreEqual( BoardPositionType.Lane, fourDice.GameState.Player1.Attackers[0].BoardPositionType );
				Assert.AreEqual( fourDice.GameState.Dice[1].Value, fourDice.GameState.Player1.Attackers[0].LanePosition );

			}

			// Reroll, but then hardcode.
			fourDice.RerollDice();
			fourDice.GameState.Dice[0].Value = 1;
			fourDice.GameState.Dice[1].Value = 2;
			fourDice.GameState.Dice[2].Value = 3;
			fourDice.GameState.Dice[3].Value = 4;

			{

				var turn1 = new TurnAction( 3, PieceMovementDirection.Forward, PieceType.Attacker, 0 );
				fourDice.ApplyTurnAction( turn1 );


				Assert.AreEqual( fourDice.GameState.Player2, fourDice.GameState.GetCurrentPlayer() );
				Assert.IsFalse( fourDice.GameState.Dice[0].IsChosen );
				Assert.IsFalse( fourDice.GameState.Dice[1].IsChosen );
				Assert.IsFalse( fourDice.GameState.Dice[2].IsChosen );
				Assert.IsTrue( fourDice.GameState.Dice[3].IsChosen );

				Assert.AreEqual( BoardPositionType.Lane, fourDice.GameState.Player2.Attackers[0].BoardPositionType );
				Assert.AreEqual( FourDice.Player2GoalLanePosition - fourDice.GameState.Dice[3].Value, fourDice.GameState.Player2.Attackers[0].LanePosition );


				var turn2 = new TurnAction( 2, PieceMovementDirection.Forward, PieceType.Attacker, 1 );
				fourDice.ApplyTurnAction( turn2 );

				Assert.AreEqual( fourDice.GameState.Player1, fourDice.GameState.GetCurrentPlayer() );
				Assert.IsFalse( fourDice.GameState.Dice[0].IsChosen );
				Assert.IsFalse( fourDice.GameState.Dice[1].IsChosen );
				Assert.IsTrue( fourDice.GameState.Dice[2].IsChosen );
				Assert.IsTrue( fourDice.GameState.Dice[3].IsChosen );

				Assert.AreEqual( BoardPositionType.Lane, fourDice.GameState.Player2.Attackers[1].BoardPositionType );
				Assert.AreEqual( FourDice.Player2GoalLanePosition - fourDice.GameState.Dice[2].Value, fourDice.GameState.Player2.Attackers[1].LanePosition );

			}

		}



		[TestMethod]
		public void CopyGameStateTest()
		{
			var gameState = new GameState( "AI" );
			gameState.Player1.Attackers[0].BoardPositionType = BoardPositionType.OpponentGoal;

			var copiedGameState = new GameState( "AI" );
			gameState.CopyTo( copiedGameState );

			Assert.AreEqual( gameState.Player1.Attackers[0].BoardPositionType, copiedGameState.Player1.Attackers[0].BoardPositionType );
		}
	}
}
