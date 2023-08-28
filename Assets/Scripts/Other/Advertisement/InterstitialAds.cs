using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    private string androidAdID = "Interstitial_Android";
    private string iOSAdID = "Interstitial_iOS";

    private string adID;

    private void Awake()
    {
        adID = (Application.platform == RuntimePlatform.IPhonePlayer) ? iOSAdID : androidAdID;
        LoadAd();
    }

    public void LoadAd()
    {
        Debug.Log($"Loading ad: {adID}");
        Advertisement.Load(adID, this);
    }

    public void ShowAd()
    {
        Debug.Log($"Showing ad: {adID}");
        Advertisement.Show(adID, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Unity Ads failed to load advertisement: {error} ({message})");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Unity Ads failed to show advertisement: {error} ({message})");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        // Start showing an advertisement
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        // Clicked on showing advertisement
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        // Completed
        LoadAd();
    }
}
