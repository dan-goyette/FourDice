using System;
using System.Collections.Generic;
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

		[TestMethod]
		public void TurnValidationTest()
		{

			{
				// Ensure you can pick the same two dice on each turn.
				var gameState = new GameState( "AI" );
				TurnAction turnAction = new TurnAction( 0 );
				TurnAction lastTurnAction = new TurnAction( 0 );
				var validationResult = FourDice.ValidateTurnAction( gameState, turnAction, lastTurnAction );
				Assert.IsFalse( validationResult.IsValidAction );
				Assert.IsNull( validationResult.NewBoardPositionType );
				Assert.IsNull( validationResult.NewLanePosition );
				Assert.IsNull( validationResult.PieceToMove );
				Assert.AreEqual( "The same DieIndex may not be chosen in both actions.", validationResult.ValidationFailureReason );
			}

			{
				// Ensure the same piece can't be moved on both turns.
				var gameState = new GameState( "AI" );
				TurnAction turnAction = new TurnAction( 0, PieceMovementDirection.Forward, PieceType.Attacker, 0 );
				TurnAction lastTurnAction = new TurnAction( 1, PieceMovementDirection.Forward, PieceType.Attacker, 0 );
				var validationResult = FourDice.ValidateTurnAction( gameState, turnAction, lastTurnAction );
				Assert.IsFalse( validationResult.IsValidAction );
				Assert.IsNull( validationResult.NewBoardPositionType );
				Assert.IsNull( validationResult.NewLanePosition );
				Assert.IsNull( validationResult.PieceToMove );
				Assert.AreEqual( "The same PieceIndex may not be chosen for the same PieceType in both actions.", validationResult.ValidationFailureReason );
			}

			{
				// Ensure defenders can't move into the goal.
				var gameState = new GameState( "AI" );
				gameState.Dice[0].Value = 4;
				TurnAction turnAction = new TurnAction( 0, PieceMovementDirection.Backward, PieceType.Defender, 0 );
				TurnAction lastTurnAction = null;
				var validationResult = FourDice.ValidateTurnAction( gameState, turnAction, lastTurnAction );
				Assert.IsFalse( validationResult.IsValidAction );
				Assert.IsNull( validationResult.NewBoardPositionType );
				Assert.IsNull( validationResult.NewLanePosition );
				Assert.IsNull( validationResult.PieceToMove );
				Assert.AreEqual( "Defenders may not move into, or past, the goal.", validationResult.ValidationFailureReason );
			}

			{
				// Ensure defenders can't leave their half of the board. 
				var gameState = new GameState( "AI" );
				gameState.Dice[0].Value = 5;
				TurnAction turnAction = new TurnAction( 0, PieceMovementDirection.Forward, PieceType.Defender, 0 );
				TurnAction lastTurnAction = null;
				var validationResult = FourDice.ValidateTurnAction( gameState, turnAction, lastTurnAction );
				Assert.IsFalse( validationResult.IsValidAction );
				Assert.IsNull( validationResult.NewBoardPositionType );
				Assert.IsNull( validationResult.NewLanePosition );
				Assert.IsNull( validationResult.PieceToMove );
				Assert.AreEqual( "Defenders may not be moved into the opponent's half of the board.", validationResult.ValidationFailureReason );
			}

			{
				// Ensure an attacker can't move into a space with two defenders in it. 
				var gameState = new GameState( "AI" );
				gameState.Dice[0].Value = 1;
				gameState.Player1.Defenders[0].BoardPositionType = BoardPositionType.Lane;
				gameState.Player1.Defenders[0].LanePosition = 3;
				gameState.Player1.Defenders[1].BoardPositionType = BoardPositionType.Lane;
				gameState.Player1.Defenders[1].LanePosition = 3;
				gameState.Player1.Attackers[0].BoardPositionType = BoardPositionType.Lane;
				gameState.Player1.Attackers[0].LanePosition = 2;
				TurnAction turnAction = new TurnAction( 0, PieceMovementDirection.Forward, PieceType.Attacker, 0 );
				TurnAction lastTurnAction = null;
				var validationResult = FourDice.ValidateTurnAction( gameState, turnAction, lastTurnAction );
				Assert.IsFalse( validationResult.IsValidAction );
				Assert.IsNull( validationResult.NewBoardPositionType );
				Assert.IsNull( validationResult.NewLanePosition );
				Assert.IsNull( validationResult.PieceToMove );
				Assert.AreEqual( "The selected location is already occupied by two of the player's pieces.", validationResult.ValidationFailureReason );
			}

		}
	}
}
