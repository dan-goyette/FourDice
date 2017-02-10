﻿
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DomainModel;
using Assets.Scripts.Interfaces;
using UnityEngine;


public abstract class SelectableObjectController : MonoBehaviour, ICachesMaterialsAtStart
{
	public ParticleSystem SelectionParticleSystem;

	public bool IsSelected { get; private set; }
	public bool CanSelect { get; private set; }
	public bool CanDeselect { get; private set; }

	protected virtual bool DisableCollisionWhenNotSelectable
	{
		get
		{
			return true;
		}
	}


	private Material[] _materials;
	private Color _targetEmissionColor;
	private float _emissionTimeElapsed;


	protected virtual void Awake()
	{
		SelectionParticleSystem.Stop();
		SelectionParticleSystem.Clear();
	}


	// Use this for initialization
	protected virtual void Start()
	{
		this.IsSelected = false;
		this.CanSelect = false;
		this.CanDeselect = true;

		InitializeMaterials();
	}

	public void InitializeMaterials()
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


	public void Select( bool force = false )
	{
		Select( true, force: force );
	}

	public void Deselect( bool force = false )
	{
		Select( false, force: force );
	}

	private void Select( bool isSelected, bool force = false )
	{
		if ( force || (isSelected && CanSelect) || (!isSelected && CanDeselect) ) {
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
			if ( DisableCollisionWhenNotSelectable ) {
				this.gameObject.GetComponent<Collider>().enabled = canSelect;
			}
		}
	}


	public void SetDeselectable( bool canDeselect )
	{
		CanDeselect = canDeselect;
	}


	protected virtual void UpdateParticleSystems()
	{
		if ( this.IsSelected ) {
			_targetEmissionColor = Color.black;
			_emissionTimeElapsed = 0;
			SelectionParticleSystem.Play();
		}
		else if ( this.CanSelect ) {
			SelectionParticleSystem.Stop();
			SelectionParticleSystem.Clear();

			_targetEmissionColor = _selectedEmissionColor;
			_emissionTimeElapsed = 0;
			//			SelectableParticleSystem.Play();
		}
		else {
			_targetEmissionColor = Color.black;
			_emissionTimeElapsed = 0;
			SelectionParticleSystem.Stop();
			SelectionParticleSystem.Clear();
		}
	}



	void OnMouseOver()
	{
		if ( Input.GetMouseButtonDown( 0 ) ) {
			Select( !IsSelected, force: false );
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
