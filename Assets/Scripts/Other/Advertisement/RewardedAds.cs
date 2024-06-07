using UnityEngine;
using UnityEngine.UI;
using YandexMobileAds;
using YandexMobileAds.Base;
using System;

public class RewardedAds : MonoBehaviour
{
    public string reward;
    private RewardedAdLoader rewardedAdLoader;
    private RewardedAd rewardedAd;
    [SerializeField] Button[] buttons;

    private void Awake()
    {
        SetupLoader();
        RequestRewardedAd();
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].onClick.AddListener(ShowRewardedAd);
        }
    }

    private void SetupLoader()
    {
        rewardedAdLoader = new RewardedAdLoader();
        rewardedAdLoader.OnAdLoaded += HandleAdLoaded;
        rewardedAdLoader.OnAdFailedToLoad += HandleAdFailedToLoad;
    }

    private void RequestRewardedAd()
    {
        string adUnitId = "R-M-2659272-5"; // R-M-2659272-3
        AdRequestConfiguration adRequestConfiguration = new AdRequestConfiguration.Builder(adUnitId).Build();
        rewardedAdLoader.LoadAd(adRequestConfiguration);
    }

    private void ShowRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Show();
        }
    }

    public void HandleAdLoaded(object sender, RewardedAdLoadedEventArgs args)
    {
        // The ad was loaded successfully. Now you can handle it.
        rewardedAd = args.RewardedAd;

        // Add events handlers for ad actions
        rewardedAd.OnAdClicked += HandleAdClicked;
        rewardedAd.OnAdShown += HandleAdShown;
        rewardedAd.OnAdFailedToShow += HandleAdFailedToShow;
        rewardedAd.OnAdImpression += HandleImpression;
        rewardedAd.OnAdDismissed += HandleAdDismissed;
        rewardedAd.OnRewarded += HandleRewarded;
    }

    public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log($"Ad {args.AdUnitId} failed to load with {args.Message}");
        // Ad {args.AdUnitId} failed for to load with {args.Message}
        // Attempting to load a new ad from the OnAdFailedToLoad event is strongly discouraged.
    }

    public void HandleAdDismissed(object sender, EventArgs args)
    {
        // Called when an ad is dismissed.

        // Clear resources after an ad dismissed.
        DestroyRewardedAd();

        // Now you can preload the next rewarded ad.
        RequestRewardedAd();
    }

    public void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
    {
        Debug.Log($"Ad failed to show with {args.Message}");
        // Called when rewarded ad failed to show.

        // Clear resources after an ad dismissed.
        DestroyRewardedAd();

        // Now you can preload the next rewarded ad.
        RequestRewardedAd();
    }

    public void HandleAdClicked(object sender, EventArgs args)
    {
        // Called when a click is recorded for an ad.
    }

    public void HandleAdShown(object sender, EventArgs args)
    {
        // Called when an ad is shown.
        RequestRewardedAd();
    }

    public void HandleImpression(object sender, ImpressionData impressionData)
    {
        // Called when an impression is recorded for an ad.
    }

    public void HandleRewarded(object sender, Reward args)
    {
        UserGotReward(this.reward);
        // Called when the user can be rewarded with {args.type} and {args.amount}.
    }

    public void DestroyRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }

    public void SetReward(string reward)
    {
        this.reward = reward;
    }

    public void UserGotReward(string reward)
    {
        if (this.reward == reward)
        {
            string[] rewardDatas = this.reward.Split(';');

            if (rewardDatas[0] == "money")
            {
                ReferencesManager.Instance.countryManager.currentCountry.money += int.Parse(rewardDatas[1]);
            }
            else if (rewardDatas[0] == "food")
            {
                ReferencesManager.Instance.countryManager.currentCountry.food += int.Parse(rewardDatas[1]);
            }
            else if (rewardDatas[0] == "recroots")
            {
                ReferencesManager.Instance.countryManager.currentCountry.recroots += int.Parse(rewardDatas[1]);
            }
            else if (rewardDatas[0] == "points")
            {
                ReferencesManager.Instance.countryManager.currentCountry.researchPoints += int.Parse(rewardDatas[1]);
            }

            ReferencesManager.Instance.countryManager.UpdateValuesUI();
        }

        if (ReferencesManager.Instance.settings.paused)
        {
            ReferencesManager.Instance.settings.Play();
        }

        AudioListener.pause = false;
    }
}
