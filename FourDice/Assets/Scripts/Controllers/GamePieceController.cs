using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DomainModel;
using UnityEngine;


public abstract class GamePieceController : MonoBehaviour
{
	public ParticleSystem SelectionParticleSystem;
	public PlayerType PlayerType;

	public Material[] MaterialsToReplace;
	public Material[] Player1ReplacementMaterials;
	public Material[] Player2ReplacementMaterials;

	private bool _isSelected;

	protected virtual void Awake()
	{
		SelectionParticleSystem.Stop();
		SelectionParticleSystem.Clear();
	}


	// Use this for initialization
	protected virtual void Start()
	{
		// Find all player-based surfaces and set the appropriate material.

		var mesh = this.gameObject.GetComponent<MeshRenderer>();

		var materials = mesh.materials;

		for ( int i = 0; i < MaterialsToReplace.Length; i++ ) {

			var newMaterial = PlayerType == PlayerType.Player1 ? Player1ReplacementMaterials[i] : Player2ReplacementMaterials[i];
			for ( var j = 0; j < materials.Length; j++ ) {
				if ( materials[j].name.StartsWith( MaterialsToReplace[i].name ) ) {
					materials[j] = newMaterial;
				}
			}
		}

		mesh.materials = materials;


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
