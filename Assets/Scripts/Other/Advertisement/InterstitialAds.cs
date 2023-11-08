//using UnityEngine;
//using YandexMobileAds;
//using YandexMobileAds.Base;
//using TMPro;
//using System;

//public class InterstitialAds : MonoBehaviour
//{
//    private string message = "";

//    private Interstitial interstitial;

//    public TMP_Text textToTest;

//    private void Start()
//    {
//        RequestInterstitial();
//    }

//    private void RequestInterstitial()
//    {
//        //Sets COPPA restriction for user age under 13 demo-interstitial-yandex
//        MobileAds.SetAgeRestrictedUser(true);

//        // Replace demo Unit ID 'demo-interstitial-yandex' with actual Ad Unit ID
//        string adUnitId = "R-M-2659272-2"; //R-M-2515342-1 demo-interstitial-yandex

//        if (this.interstitial != null)
//        {
//            this.interstitial.Destroy();
//        }

//        this.interstitial = new Interstitial(adUnitId);

//        this.interstitial.LoadAd(this.CreateAdRequest());
//        interstitial.OnInterstitialLoaded += HandleInterstitialLoaded;
//        interstitial.OnInterstitialShown += HandleInterstitialShown;
//        this.DisplayMessage("Interstitial is requested");
//        textToTest.text = "Interstitial is requested";
//    }

//    public void ShowInterstitial()
//    {
//        textToTest.text = "loading";

//        this.interstitial.Show();

//        textToTest.text = "showing";
//    }
//    public void HandleInterstitialLoaded(object sender, EventArgs args)
//    {
//        ShowInterstitial();
//    }
//    public void HandleInterstitialShown(object sender, EventArgs args)
//    {
//        RequestInterstitial();
//    }
//    private AdRequest CreateAdRequest()
//    {
//        return new AdRequest.Builder().Build();
//    }

//    private void DisplayMessage(string message)
//    {
//        this.message = message + (this.message.Length == 0 ? "" : "\n--------\n" + this.message);
//        MonoBehaviour.print(message);
//    }
//}
