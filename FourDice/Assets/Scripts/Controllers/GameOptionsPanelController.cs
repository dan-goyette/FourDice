using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DomainModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOptionsPanelController : MonoBehaviour
{
	public Slider AnimationSpeedSlider;
	public Text AnimationSpeedText;


	// Use this for initialization
	void Start()
	{
		var animationSpeed = PlayerPrefs.GetInt( "AnimationSpeed" );
		if ( animationSpeed >= 1 ) {
			AnimationSpeedSlider.value = animationSpeed;


		}

	}

	// Update is called once per frame
	void Update()
	{
		AnimationSpeedText.text = AnimationSpeedSlider.value.ToString();
	}


	public void AnimationSpeedOptionChanged()
	{
		Time.timeScale = AnimationSpeedSlider.value;

		PlayerPrefs.SetInt( "AnimationSpeed", (int)AnimationSpeedSlider.value );
	}


	public void CancelButtonPressed()
	{
		Destroy( this.gameObject );
	}

}
