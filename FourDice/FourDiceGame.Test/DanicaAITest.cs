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
        Danica danicaAI;

        [TestInitialize]
        public void TestInit()
        {
            danicaAI = new Danica(PlayerType.Player1);
        }

        [TestMethod]
        public void DanicasTest()
        {
            FourDice fourDice = new FourDice("danicaAI");
            fourDice.GameState.CurrentPlayer = fourDice.GameState.Player1;
            fourDice.GameState.Dice[0].Value = 1;
            fourDice.GameState.Dice[1].Value = 2;
            fourDice.GameState.Dice[2].Value = 3;
            fourDice.GameState.Dice[3].Value = 4;

            var nextMoves = danicaAI.GetNextMoves(fourDice.GameState);

        }
    }
}
