using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.DomainModel
{
	/// <summary>
	/// The type of the game piece.
	/// </summary>
	public enum PieceType
	{
		Attacker,
		Defender
	}

	/// <summary>
	/// Whether the player is human-controlled or computer controlled.
	/// </summary>
	public enum PlayerControlType
	{
		Human,
		Computer
	}


	/// <summary>
	/// Whether the player is Player 1 or Player 2.
	/// </summary>
	public enum PlayerType
	{
		Player1,
		Player2
	}

	/// <summary>
	/// Indicates the region of the board on which the piece is currently placed.
	/// </summary>
	public enum BoardPositionType
	{
		OwnGoal,
		OpponentGoal,
		DefenderCircle,
		Lane
	}

	public enum PieceMovementDirection
	{
		Forward,
		Backward
	}
}
