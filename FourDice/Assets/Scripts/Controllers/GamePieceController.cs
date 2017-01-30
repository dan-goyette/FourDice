using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DomainModel;
using UnityEngine;

public abstract class GamePieceController : MonoBehaviour
{
	public ParticleSystem SelectionParticleSystem;
	public PlayerType PlayerType;

	private bool _isSelected;

	protected virtual void Awake()
	{
		SelectionParticleSystem.Stop();
		SelectionParticleSystem.Clear();
	}


	// Use this for initialization
	protected virtual void Start()
	{

	}

	// Update is called once per frame
	protected virtual void Update()
	{

	}

	void OnMouseOver()
	{
		if ( Input.GetMouseButtonDown( 0 ) ) {
			_isSelected = !_isSelected;

			if ( _isSelected ) {
				if ( !SelectionParticleSystem.isPlaying ) {
					SelectionParticleSystem.Play();
					Debug.Log( "Playing Particle system for " + gameObject.name );
				}
			}
			else {
				if ( SelectionParticleSystem.isPlaying ) {
					SelectionParticleSystem.Stop();
					SelectionParticleSystem.Clear();
					Debug.Log( "Stopping Particle system for " + gameObject.name );
				}
			}
		}
	}
}
