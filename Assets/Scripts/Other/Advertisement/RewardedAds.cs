//using UnityEngine;

//public class RewardedAds : MonoBehaviour
//{
//    public string reward;
//    public YandexSDKFixed yandexSDKFixed;

//    private void Start()
//    {
//        yandexSDKFixed = FindObjectOfType<YandexSDKFixed>();
//    }

//    private void OnEnable()
//    {
//        YandexSDKFixed.instance.onRewardedAdReward += UserGotReward;
//    }

//    private void OnDisable()
//    {
//        YandexSDKFixed.instance.onRewardedAdReward -= UserGotReward;
//    }

//    public void ShowRewardedAd()
//    {
//        if (!ReferencesManager.Instance.settings.paused)
//        {
//            ReferencesManager.Instance.settings.PlayPauseMusic();
//        }
//        yandexSDKFixed.ShowRewarded(reward);
//    }

//    public void ShowInterstitial()
//    {
//        if (!ReferencesManager.Instance.settings.paused)
//        {
//            ReferencesManager.Instance.settings.PlayPauseMusic();
//        }
//        yandexSDKFixed.ShowInterstitial();
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
