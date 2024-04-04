using UnityEngine;


[RequireComponent(typeof(RectTransform))]
public class UI_OnlinePosition : MonoBehaviour
{
    public Vector2 onlinePosition;

    private BannerAds _bannerAd;


    private void Awake()
    {
        if (_bannerAd.banner != null)
        {
            this.GetComponent<RectTransform>().position = onlinePosition;
        }
    }
}
