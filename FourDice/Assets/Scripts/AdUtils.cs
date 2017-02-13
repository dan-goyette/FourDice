using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ADS
using UnityEngine.Advertisements; // only compile Ads code on supported platforms
#endif

public static class AdUtils
{
	public static void ShowDefaultAd( Action callback, Text debugText )
	{
#if UNITY_ADS

		InitAdService();

		if ( !Advertisement.IsReady() ) {
			debugText.text = string.Format("{0}{1}{2}", debugText.text, Environment.NewLine, "Ads not ready for default placement");

			Debug.Log( "Ads not ready for default placement" );
			return;
		}

		debugText.text = string.Format("{0}{1}{2}", debugText.text, Environment.NewLine, "About to call .Show()");
		Advertisement.Show( new ShowOptions() { 
			resultCallback = ( r ) => { 
				debugText.text = string.Format("{0}{1}{2}", debugText.text, Environment.NewLine, "Inside resultCallback");
				callback(); 
			} 
		} );
#else
		debugText.text = string.Format("{0}{1}{2}", debugText.text, Environment.NewLine, "Ads not supported. Not showing ad.");

		Debug.Log( "Ads not supported. Not showing ad." );
		callback();
#endif
	}


	public static void InitAdService() {
		#if UNITY_ADS

		if ( Advertisement.isSupported ) { // If runtime platform is supported...
			Advertisement.Initialize( "1302939", false ); // ...initialize.
		}
		#endif
	}
}