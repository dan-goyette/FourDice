using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

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


		public static DateTime LastAddShownTime = DateTime.MinValue;
		public static IEnumerator ShowAd( Action callback )
		{
			var minutesSinceLastAd = (DateTime.Now - LastAddShownTime).TotalMinutes;
			var shouldShowAd = minutesSinceLastAd > 15;

			if ( shouldShowAd ) {
				if ( Advertisement.isSupported ) { // If runtime platform is supported...
												   //	Advertisement.Initialize( "1302939", false, ); // ...initialize.


					// Wait until Unity Ads is initialized,
					//  and the default ad placement is ready.
					while ( !Advertisement.isInitialized || !Advertisement.IsReady() ) {
						yield return new WaitForSeconds( 0.5f );
					}

					var showOptions = new ShowOptions() {
						resultCallback = ( o ) => {
							LastAddShownTime = DateTime.Now;
							callback();

						}
					};
					// Show the default ad placement.
					Advertisement.Show( showOptions );

				}
			}
			else {
				callback();
			}
		}

	}
}
