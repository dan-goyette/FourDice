﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourDiceGame.AI
{
    public interface IFourDiceAI
    {
        TurnAction[] GetNextMoves(GameState gameState);

    }
}
