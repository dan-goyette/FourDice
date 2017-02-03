using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DomainModel;
using UnityEngine;


public abstract class GamePieceController : SelectableObjectController
{
	public PlayerType PlayerType;
	public int PieceIndex;
	public int? LanePosition;
	public BoardPositionType BoardPositionType;
	public bool? InUpperSlot;

	public Material[] MaterialsToReplace;
	public Material[] Player1ReplacementMaterials;
	public Material[] Player2ReplacementMaterials;

	public Vector3 TurnStartPosition;
	public Quaternion TurnStartRotation;
	public bool? TurnStartInUpperSlot;

	public abstract PieceType PieceType { get; }

	protected override void Awake()
	{
		base.Awake();
	}


	// Use this for initialization
	protected override void Start()
	{
		base.Start();

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

		InitializeMaterials();
	}

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();
	}

}
