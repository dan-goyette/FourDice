using System;
using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements; // only compile Ads code on supported platforms
#endif

public static class AdUtils
{
	public static void ShowDefaultAd( Action callback )
	{
#if UNITY_ADS

		if ( Advertisement.isSupported ) { // If runtime platform is supported...
			Advertisement.Initialize( "1302939", false ); // ...initialize.
		}


		if ( !Advertisement.IsReady() ) {
			Debug.Log( "Ads not ready for default placement" );
			return;
		}

		Advertisement.Show( new ShowOptions() { resultCallback = ( r ) => { callback(); } } );
#else
		Debug.Log( "Ads not supported. Not showing ad." );
		callback();
#endif
	}
}