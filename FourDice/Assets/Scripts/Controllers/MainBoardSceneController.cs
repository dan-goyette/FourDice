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

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void RollDice()
	{
		foreach ( var existingDie in GameObject.FindObjectsOfType<DieController>().ToList() ) {
			GameObject.Destroy( existingDie.gameObject );
		}

		for ( var i = 0; i < 4; i++ ) {
			GameObject die = (GameObject)Instantiate( Resources.Load( "Die" ) );
			die.transform.position = new Vector3( -10 + 4 * i, 8, 15 );
			die.transform.rotation = UnityEngine.Random.rotation;
			die.GetComponent<Rigidbody>().velocity = new Vector3( UnityEngine.Random.Range( -5, 5 ), -20, UnityEngine.Random.Range( -5, 5 ) );
			die.GetComponent<Rigidbody>().rotation = UnityEngine.Random.rotation;
		}

	}
}
