using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements; // only compile Ads code on supported platforms
#endif

public static class AdUtils
{
	public static void ShowDefaultAd()
	{
#if UNITY_ADS
        if (!Advertisement.IsReady())
        {
            Debug.Log("Ads not ready for default placement");
            return;
        }

        Advertisement.Show();
#endif
	}
}