using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DomainModel;
using UnityEngine;

public class LanePositionController : SelectableObjectController
{
	public int LanePosition;
	public PlayerType PlayerType;
	public bool IsAlternate;

	public Material[] MaterialsToReplace;
	public Material[] Player1ReplacementMaterials;
	public Material[] Player2ReplacementMaterials;

	// Use this for initialization
	protected override void Start()
	{
		base.Start();

		if ( IsAlternate ) {
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
	}

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();
	}
}
