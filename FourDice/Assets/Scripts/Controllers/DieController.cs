using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DieController : SelectableObjectController
{

	private Rigidbody _rigidbody;

	public bool IsRolling;


	public List<Vector3> _directions;
	public List<int> _sideValues;



	// Use this for initialization
	protected override void Start()
	{
		base.Start();
	}

	protected override void Awake()
	{
		base.Awake();

		SelectionParticleSystem.Stop();
		SelectionParticleSystem.Clear();

		_rigidbody = gameObject.GetComponent<Rigidbody>();

		// For the sake of this example we assume a regular cube dice if 
		// directions haven't been specified in the editor. Sum of opposite
		// sides is 7, haven't consider exact real layout though.
		if ( _directions.Count == 0 ) {
			// Object space directions
			_directions.Add( Vector3.up );
			_sideValues.Add( 2 ); // up
			_directions.Add( Vector3.down );
			_sideValues.Add( 5 ); // down

			_directions.Add( Vector3.left );
			_sideValues.Add( 3 ); // left
			_directions.Add( Vector3.right );
			_sideValues.Add( 4 ); // right

			_directions.Add( Vector3.forward );
			_sideValues.Add( 6 ); // fw
			_directions.Add( Vector3.back );
			_sideValues.Add( 1 ); // back
		}
	}

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();
	}

	public bool IsMoving()
	{
		//Debug.Log( _rigidbody.velocity.magnitude );
		return _rigidbody.velocity.magnitude > 0.001f;
	}

	public int? GetDieValue( float epsilonDeg = 5f )
	{
		return GetDieValue( Vector3.up, epsilonDeg );
	}

	public int? GetDieValue( Vector3 referenceVectorUp, float epsilonDeg = 5f )
	{

		// Code found here: http://answers.unity3d.com/questions/1215416/rolling-a-3d-dice-detect-which-number-faces-up.html

		Vector3 referenceObjectSpace = gameObject.transform.InverseTransformDirection( referenceVectorUp );

		// Find smallest difference to object space direction
		float min = float.MaxValue;
		int mostSimilarDirectionIndex = -1;
		for ( int i = 0; i < _directions.Count; ++i ) {
			float a = Vector3.Angle( referenceObjectSpace, _directions[i] );
			if ( a <= epsilonDeg && a < min ) {
				min = a;
				mostSimilarDirectionIndex = i;
			}
		}

		// -1 as error code for not within bounds

		return (mostSimilarDirectionIndex >= 0) ? (int?)_sideValues[mostSimilarDirectionIndex] : null;


	}
}
