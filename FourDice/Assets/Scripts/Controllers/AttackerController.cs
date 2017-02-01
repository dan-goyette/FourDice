using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DomainModel;
using UnityEngine;

public class AttackerController : GamePieceController
{
	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
	}

	public override PieceType PieceType
	{
		get
		{
			return PieceType.Attacker;
		}
	}
}
