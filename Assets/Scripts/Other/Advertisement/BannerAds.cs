using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using YandexMobileAds;
using YandexMobileAds.Base;

public class BannerAds : MonoBehaviour
{
    private string message = "";

    [HideInInspector] public Banner banner;

    private void Awake()
    {
        if (ReferencesManager.Instance.gameSettings._isPremium.value == false)
        {
            RequestBanner();
        }
    }

    private void RequestBanner()
    {
        if (ReferencesManager.Instance.gameSettings._isPremium.value == false)
        {
            //Sets COPPA restriction for user age under 13
            MobileAds.SetAgeRestrictedUser(true);

            string adUnitId = "R-M-2659272-6";

            if (this.banner != null)
            {
                this.banner.Destroy();
            }

            // Set sticky banner width
            //BannerAdSize bannerSize = BannerAdSize.StickySize(GetScreenWidthDp());

            // Or set inline banner maximum width and height
            BannerAdSize bannerSize = BannerAdSize.InlineSize(GetScreenWidthDp() / 3, 50);
            this.banner = new Banner(adUnitId, bannerSize, AdPosition.BottomRight);

            this.banner.OnAdLoaded += this.HandleAdLoaded;
            this.banner.OnAdFailedToLoad += this.HandleAdFailedToLoad;
            this.banner.OnReturnedToApplication += this.HandleReturnedToApplication;
            this.banner.OnLeftApplication += this.HandleLeftApplication;
            this.banner.OnAdClicked += this.HandleAdClicked;
            this.banner.OnImpression += this.HandleImpression;

            this.banner.LoadAd(this.CreateAdRequest());
        }
    }

    // Example how to get screen width for request
    private int GetScreenWidthDp()
    {
        int screenWidth = (int)Screen.safeArea.width;
        return ScreenUtils.ConvertPixelsToDp(screenWidth);
    }

    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder().Build();
    }

    private void DisplayMessage(String message)
    {
        this.message = message + (this.message.Length == 0 ? "" : "\n--------\n" + this.message);
        MonoBehaviour.print(message);
    }

    #region Banner callback handlers

    public void HandleAdLoaded(object sender, EventArgs args)
    {
        this.banner.Show();
    }

    public void HandleAdFailedToLoad(object sender, AdFailureEventArgs args)
    {
    }

    public void HandleLeftApplication(object sender, EventArgs args)
    {
    }

    public void HandleReturnedToApplication(object sender, EventArgs args)
    {
    }

    public void HandleAdLeftApplication(object sender, EventArgs args)
    {
    }

    public void HandleAdClicked(object sender, EventArgs args)
    {
        if (SceneManager.GetActiveScene().name == "EuropeSceneOffline")
        {
            StartCoroutine(LoadAd_Co());
        }
        else
        {
            RequestBanner();
        }
    }

    private IEnumerator LoadAd_Co()
    {
        yield return new WaitForSecondsRealtime(210f);
        RequestBanner();
    }

    public void HandleImpression(object sender, ImpressionData impressionData)
    {
        var data = impressionData == null ? "null" : impressionData.rawData;
    }

    #endregion
}
