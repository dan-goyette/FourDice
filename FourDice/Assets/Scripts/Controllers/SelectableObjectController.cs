
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DomainModel;
using UnityEngine;


public abstract class SelectableObjectController : MonoBehaviour
{
	public ParticleSystem SelectionParticleSystem;
	public ParticleSystem SelectableParticleSystem;
	public bool IsSelected;
	public bool CanSelect;
	public bool CanDeselect;





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
	}

	// Update is called once per frame
	protected virtual void Update()
	{

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


	private void UpdateParticleSystems()
	{
		if ( this.IsSelected ) {
			SelectableParticleSystem.Stop();

			SelectionParticleSystem.Play();
		}
		else if ( this.CanSelect ) {
			SelectionParticleSystem.Stop();

			SelectableParticleSystem.Play();
		}
		else {
			SelectableParticleSystem.Stop();

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
