using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string androidGameID = "5348275";
    [SerializeField] string iOSGameID = "5348275";

    [SerializeField] bool testMode = true;

    private string gameID;

    private void Awake()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
        gameID = (Application.platform == RuntimePlatform.IPhonePlayer) ? iOSGameID : androidGameID;
        Advertisement.Initialize(gameID, testMode, this);
    }

    public void OnInitializationComplete()
    {
        if (testMode)
        {
            Debug.Log($"Unity Ads Initialization complete.");
        }
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        if (testMode)
        {
            Debug.LogError($"Unity Ads Initialization failed: {error} ({message})");
        }
    }
}
