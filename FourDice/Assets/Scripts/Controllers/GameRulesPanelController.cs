using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameRulesPanelController : MonoBehaviour
{
	public RawImage Page1;
	public RawImage Page2;
	public RawImage Page3;
	public RawImage Page4;
	public RawImage Page5;
	public RawImage Page6;

	public Button PreviousButton;
	public Button NextButton;

	private int _currentPage = 1;

	// Use this for initialization
	void Start()
	{
		UpdateButtonStates();
		UpdateImageStates();
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void PreviousButtonPressed()
	{
		_currentPage--;
		UpdateButtonStates();
		UpdateImageStates();
	}

	public void NextButtonPressed()
	{
		_currentPage++;
		UpdateButtonStates();
		UpdateImageStates();
	}


	private void UpdateButtonStates()
	{
		PreviousButton.interactable = _currentPage != 1;
		NextButton.interactable = _currentPage != 6;
	}

	private void UpdateImageStates()
	{
		Page1.enabled = _currentPage == 1;
		Page2.enabled = _currentPage == 2;
		Page3.enabled = _currentPage == 3;
		Page4.enabled = _currentPage == 4;
		Page5.enabled = _currentPage == 5;
		Page6.enabled = _currentPage == 6;
	}



	public void CancelButtonPressed()
	{
		Destroy( this.gameObject );
	}
}
