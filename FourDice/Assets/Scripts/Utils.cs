﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public static class Utils
	{

		/// <summary>
		///  This method can be called at any time to access all objects in the scene, and switch 
		/// </summary>
		public static void SwitchToMobileMaterials()
		{
			GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
			foreach ( var gameObject in allObjects ) {
				if ( gameObject.activeInHierarchy ) {



					var mesh = gameObject.GetComponent<MeshRenderer>();
					if ( mesh != null ) {
						var materials = mesh.materials;

						for ( int i = 0; i < materials.Length; i++ ) {

							var currentMaterial = materials[i];
							if ( currentMaterial.name.Contains( "_Hires" ) ) {
								// Find the equivalent _Mobile version of the material.
								var mobileMaterialName = currentMaterial.name.Replace( "_Hires", "_Mobile" ).Replace( " (Instance)", "" );
								var mobileMaterial = (Material)Resources.Load( "Materials/" + mobileMaterialName, typeof( Material ) );
								if ( mobileMaterial != null ) {
									materials[i] = mobileMaterial;
								}
							}
						}

						mesh.materials = materials;
					}


					// If this object has a component that implements ICachesMaterialsAtStart,
					// reinitialize. 
					var materialCacher = gameObject.GetComponent<ICachesMaterialsAtStart>();
					if ( materialCacher != null ) {
						materialCacher.InitializeMaterials();
					}
				}
			}
		}


		public static DateTime LastAdShownTime = DateTime.MinValue;
		public static void ShowAd( Action callback, Text debugText )
		{
			var minutesSinceLastAd = (DateTime.Now - LastAdShownTime).TotalMinutes;
			var shouldShowAd = minutesSinceLastAd > 10;

			debugText.text = string.Format ("{0}{1}{2}", debugText.text, Environment.NewLine, "Should show ad: " + shouldShowAd.ToString ());

			if ( shouldShowAd ) {
				debugText.text = string.Format ("{0}{1}{2}", debugText.text, Environment.NewLine, "Actually calling Ad service");

				Action updateCallback = () => {
					LastAdShownTime = DateTime.Now;
					callback();
				};
				AdUtils.ShowDefaultAd( updateCallback, debugText );
			}
			else {
				debugText.text = string.Format ("{0}{1}{2}", debugText.text, Environment.NewLine, "Ad was recently shown. Not showing ad.");

				Debug.Log( "Ad was recently shown. Not showing ad." );
				callback();
			}
		}

	}
}
