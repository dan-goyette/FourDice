using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainBoardSceneController : MonoBehaviour
{

	// Use this for initialization
	void Start()
	{
		_dice = new List<DieController>();
	}

	// Update is called once per frame
	void Update()
	{

	}

	private List<DieController> _dice;

	Coroutine awaitFinalDiceRolls;
	public void RollDice()
	{
		if ( awaitFinalDiceRolls != null ) {
			StopCoroutine( awaitFinalDiceRolls );
		}

		foreach ( var existingDie in _dice.ToList() ) {
			GameObject.Destroy( existingDie.gameObject );
			_dice.Remove( existingDie );
		}

		for ( var i = 0; i < 1; i++ ) {
			GameObject die = (GameObject)Instantiate( Resources.Load( "Die" ) );
			die.transform.position = new Vector3( -2 + 4 * i, 4, -3 );
			die.transform.rotation = UnityEngine.Random.rotation;
			die.GetComponent<Rigidbody>().AddForce( new Vector3( UnityEngine.Random.Range( -20, 20 ), -5, UnityEngine.Random.Range( -5, 5 ) ) * 15, ForceMode.Impulse );
			die.GetComponent<Rigidbody>().rotation = UnityEngine.Random.rotation;
			_dice.Add( die.GetComponent<DieController>() );
		}


		awaitFinalDiceRolls = StartCoroutine( AwaitFinalDiceRolls() );
	}

	private IEnumerator AwaitFinalDiceRolls()
	{
		// Give is a moment to ensure the die has started moving. 
		yield return new WaitForSeconds( 0.25f );

		float secondsPassed = 0;
		float interval = 0.1f;
		var shouldReroll = false;
		while ( true ) {
			if ( secondsPassed > 5.0f ) {
				//Debug.Log( "Took too long. Rerolling." );
				shouldReroll = true;
				break;
			}
			if ( _dice.Any( d => d.IsMoving() ) ) {
				secondsPassed += interval;
				//Debug.Log( string.Format( "Still moving after {0} seconds", secondsPassed ) );
				yield return new WaitForSeconds( interval );
			}
			else {
				// The dice have stopped. 
				if ( _dice.Select( d => d.GetDieValue( Vector3.up ) ).Any( dv => dv == null ) ) {
					//Debug.Log( "Dice landed weird. Rerolling." );
					shouldReroll = true;
					break;
				}
				else {

				}
				break;
			}
		}

		if ( shouldReroll ) {
			RollDice();
		}

		Debug.Log( string.Format( "Dice values are: {0}", string.Join( ", ", _dice.Select( d => d.GetDieValue( Vector3.up ) ).Select( dv => dv == null ? "?" : dv.Value.ToString() ).ToArray() ) ) );

	}
}
