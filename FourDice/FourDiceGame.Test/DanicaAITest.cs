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
            danicaAI = new Danica();
        }

        [TestMethod]
        public void TestMethod1()
        {
            FourDice fourDice = new FourDice("danicaAI");
            //fourDice.GameState

            var nextMoves = danicaAI.GetNextMoves(fourDice.GameState);
            //
            // TODO: Add test logic here
            //
        }
    }
}
