using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ADS
using UnityEngine.Advertisements; // only compile Ads code on supported platforms
#endif

public static class AdUtils
{
	public static void ShowDefaultAd( Action playedAdCallback, Action didNotPlayAdCallback )
	{
#if UNITY_ADS

		if ( !Advertisement.IsReady() ) {
			Debug.Log( "Ads not ready for default placement" );
			didNotPlayAdCallback(); 
			return;
		}

		Advertisement.Show( new ShowOptions() { 
			resultCallback = ( r ) => { 
				playedAdCallback(); 
			} 
		} );
#else
		Debug.Log( "Ads not supported. Not showing ad." );
		callback();
#endif
	}
}