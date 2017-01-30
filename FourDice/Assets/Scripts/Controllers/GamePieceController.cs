using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DomainModel;
using UnityEngine;

public abstract class GamePieceController : MonoBehaviour
{
	public ParticleSystem SelectionParticleSystem;
	public PlayerType PlayerType;

	protected virtual void Awake()
	{
		SelectionParticleSystem.Stop();

	}


	// Use this for initialization
	protected virtual void Start()
	{

	}

	// Update is called once per frame
	protected virtual void Update()
	{

	}
}
