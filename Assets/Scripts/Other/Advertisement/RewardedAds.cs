//using UnityEngine;
//using YandexMobileAds;
//using YandexMobileAds.Base;
//using System;

//public class RewardedAds : MonoBehaviour
//{
//    public string reward;
//    private string message = "";

//    private RewardedAd rewardedAd;

//    private void Start()
//    {
//        RequestRewarded();
//    }

//    public void RequestRewarded()
//    {
//        MobileAds.SetAgeRestrictedUser(true);

//        string adUnitId = "R-M-2659272-3";

//        if (this.rewardedAd != null)
//        {
//            this.rewardedAd.Destroy();
//        }

//        this.rewardedAd = new RewardedAd(adUnitId);

//        this.rewardedAd.LoadAd(this.CreateAdRequest());

//        rewardedAd.OnRewardedAdLoaded += HandleRewardedAdlLoaded;
//        rewardedAd.OnRewardedAdShown += HandleRewardedAdShown;
//        rewardedAd.OnRewarded += HandleRewarded;
//        rewardedAd.OnRewardedAdFailedToLoad += HandleRewardedAdlFailedToLoad;
//        rewardedAd.OnRewardedAdFailedToShow += HandleRewardedAdlFailedToShow;

//        this.DisplayMessage("RewardedAd is requested");
//    }

//    public void ShowRewardedAd()
//    {
//        this.rewardedAd.Show();
//    }

//    public void HandleRewardedAdlLoaded(object sender, EventArgs args)
//    {
//        Debug.Log("Ad loaded");
//    }

//    public void HandleRewardedAdlFailedToLoad(object sender, EventArgs args)
//    {
//        Debug.Log($"FailedToLoad | Sender ({sender}) | Args ({args})");
//    }

//    public void HandleRewardedAdlFailedToShow(object sender, EventArgs args)
//    {
//        Debug.Log($"FailedToShow | Sender ({sender}) | Args ({args})");
//    }

//    public void HandleRewardedAdShown(object sender, EventArgs args)
//    {
//        RequestRewarded();
//    }

//    public void HandleRewarded(object sender, EventArgs args)
//    {
//        UserGotReward(reward);
//    }

//    private AdRequest CreateAdRequest()
//    {
//        return new AdRequest.Builder().Build();
//    }

//    private void DisplayMessage(string message)
//    {
//        this.message = message + (this.message.Length == 0 ? "" : "\n--------\n" + this.message);
//        MonoBehaviour.print(this.message);
//    }

//    public void SetReward(string reward)
//    {
//        this.reward = reward;
//    }

//    public void UserGotReward(string reward)
//    {
//        if (this.reward == reward)
//        {
//            string[] rewardDatas = this.reward.Split(';');

//            if (rewardDatas[0] == "money")
//            {
//                ReferencesManager.Instance.countryManager.currentCountry.money += int.Parse(rewardDatas[1]);
//            }
//            else if (rewardDatas[0] == "food")
//            {
//                ReferencesManager.Instance.countryManager.currentCountry.food += int.Parse(rewardDatas[1]);
//            }
//            else if (rewardDatas[0] == "recroots")
//            {
//                ReferencesManager.Instance.countryManager.currentCountry.recroots += int.Parse(rewardDatas[1]);
//            }

//            ReferencesManager.Instance.countryManager.UpdateValuesUI();
//        }

//        if (ReferencesManager.Instance.settings.paused)
//        {
//            ReferencesManager.Instance.settings.Play();
//        }

//        AudioListener.pause = false;
//    }
//}
