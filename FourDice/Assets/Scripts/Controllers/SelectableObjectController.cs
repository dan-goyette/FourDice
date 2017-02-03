
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DomainModel;
using UnityEngine;


public abstract class SelectableObjectController : MonoBehaviour
{
	public ParticleSystem SelectionParticleSystem;
	public ParticleSystem SelectableParticleSystem;

	public bool IsSelected { get; private set; }
	public bool CanSelect { get; private set; }
	public bool CanDeselect { get; private set; }


	private Material[] _materials;
	private Color _targetEmissionColor;
	private float _emissionTimeElapsed;


	protected virtual void Awake()
	{
		SelectionParticleSystem.Stop();
		SelectionParticleSystem.Clear();

		SelectableParticleSystem.Stop();
		SelectableParticleSystem.Clear();
	}


	// Use this for initialization
	protected virtual void Start()
	{
		this.IsSelected = false;
		this.CanSelect = false;
		this.CanDeselect = true;

		InitializeMaterials();
	}

	protected void InitializeMaterials()
	{

		_materials = gameObject.GetComponent<MeshRenderer>().materials;
	}

	// Update is called once per frame
	private Color _selectedEmissionColor = new Color( .6f, .1f, .6f );
	protected virtual void Update()
	{
		foreach ( var material in _materials ) {
			material.SetColor( "_EmissionColor", Color.Lerp( Color.black, _targetEmissionColor, (Mathf.Sin( _emissionTimeElapsed * Mathf.PI ) / 2.0f) + 0.5f ) );
		}
		_emissionTimeElapsed += Time.deltaTime;
	}


	public delegate void SelectableObjectSelectionChangedHandler( object myObject,
		SelectableObjectSelectionChangedEvent myArgs );

	public event SelectableObjectSelectionChangedHandler OnSelectionChanged;


	public void Select()
	{
		Select( true );
	}

	public void Deselect()
	{
		Select( false );
	}

	private void Select( bool isSelected )
	{
		if ( (isSelected && CanSelect) || (!isSelected && CanDeselect) ) {
			IsSelected = isSelected;
			UpdateParticleSystems();
			if ( OnSelectionChanged != null ) {
				OnSelectionChanged( this, new SelectableObjectSelectionChangedEvent( IsSelected ) );
			}
		}
	}




	public void SetSelectable( bool canSelect )
	{
		if ( (CanSelect && !canSelect) || (!CanSelect && canSelect) ) {
			CanSelect = canSelect;
			UpdateParticleSystems();
		}
	}


	public void SetDeselectable( bool canDeselect )
	{
		CanDeselect = canDeselect;
	}


	protected virtual void UpdateParticleSystems()
	{
		if ( this.IsSelected ) {
			SelectableParticleSystem.Stop();
			_targetEmissionColor = Color.black;
			_emissionTimeElapsed = 0;
			SelectionParticleSystem.Play();
		}
		else if ( this.CanSelect ) {
			SelectionParticleSystem.Stop();

			_targetEmissionColor = _selectedEmissionColor;
			_emissionTimeElapsed = 0;
			//			SelectableParticleSystem.Play();
		}
		else {
			SelectableParticleSystem.Stop();
			_targetEmissionColor = Color.black;
			_emissionTimeElapsed = 0;
			SelectionParticleSystem.Stop();
		}
	}



	void OnMouseOver()
	{
		if ( Input.GetMouseButtonDown( 0 ) ) {
			Select( !IsSelected );
		}
	}



	public void Dispose()
	{
	}
}

public class SelectableObjectSelectionChangedEvent : EventArgs
{

	public SelectableObjectSelectionChangedEvent( bool isSelected )
	{
		_isSelected = isSelected;
	}

	private bool _isSelected;
	public bool IsSelected { get { return _isSelected; } }
}
