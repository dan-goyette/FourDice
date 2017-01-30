using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.DomainModel.AI
{
	public interface IFourDiceAI
	{
		TurnAction[] GetNextMoves( GameState gameState );

	}
}
