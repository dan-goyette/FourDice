using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FourDiceGame.AI;

namespace FourDiceGame.Test
{
    /// <summary>
    /// Summary description for UnitTest2
    /// </summary>
    [TestClass]
    public class DanicaAITest
    {
        Danica danicaAI1;
        Danica danicaAI2;

        [TestInitialize]
        public void TestInit()
        {
            danicaAI1 = new Danica(PlayerType.Player1);
            danicaAI2 = new Danica(PlayerType.Player2);
        }

        [TestMethod]
        public void DanicasTest()
        {
            FourDice fourDice = new FourDice("danicaAI");
			fourDice.GameState.CurrentPlayerType = fourDice.RollToSeeWhoGoesFirst();
            /*fourDice.GameState.Dice[0].Value = 1;
            fourDice.GameState.Dice[1].Value = 2;
            fourDice.GameState.Dice[2].Value = 3;
            fourDice.GameState.Dice[3].Value = 4;*/
            var nextMoves = new TurnAction[2];

            while (!FourDice.GetGameEndResult(fourDice.GameState).IsFinished)
            {
                if (fourDice.GameState.CurrentPlayerType == PlayerType.Player1)
                {
                    nextMoves = danicaAI1.GetNextMoves(fourDice.GameState);
                } else
                {
                    nextMoves = danicaAI2.GetNextMoves(fourDice.GameState);
                }
                Console.WriteLine(nextMoves);

                fourDice.ApplyTurnAction(nextMoves[0]);
                fourDice.ApplyTurnAction(nextMoves[1]);
                fourDice.RerollDice();
            }
        }
    }
}
