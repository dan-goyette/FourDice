using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FourDiceGame.Test
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			GameState gameState = new GameState( "danicaAI" );

			Assert.AreEqual( 4, gameState.Dice.Length );
			Assert.AreEqual( 5, gameState.Player1.Attackers.Length );
			Assert.AreEqual( 2, gameState.Player1.Defenders.Length );
			Assert.AreEqual( 5, gameState.Player2.Attackers.Length );
			Assert.AreEqual( 2, gameState.Player2.Defenders.Length );

			var gameEndResult = FourDice.GetGameEndResult( gameState );
			Assert.IsFalse( gameEndResult.IsFinished );
			Assert.IsNull( gameEndResult.WinningPlayer );
		}
	}
}
