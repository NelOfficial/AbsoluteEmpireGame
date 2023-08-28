using UnityEngine;
using System.Collections;

public class RegionLoader : MonoBehaviour
{

    private float regionsMax;
    private float regionsCompleted;
    private float regionsDeleted;

    public bool loaded = false;

    void Start()
    {

        regionsMax = ReferencesManager.Instance.countryManager.regions.Length;

        foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
        {
            region.isSelected = false;
            region.canSelect = true;
            region.GetComponent<SpriteRenderer>().color = region.currentCountry.country.countryColor;
            ReferencesManager.Instance.regionUI = FindObjectOfType<RegionUI>();
        }

        //StartCoroutine(LoadRegions_Co());

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Length; i++)
        {
            RegionManager region = ReferencesManager.Instance.countryManager.regions[i];
            region.GetComponent<SpriteRenderer>().color = region.currentCountry.country.countryColor;
        }

        ReferencesManager.Instance.mainCamera.Map_MoveTouch_IsAllowed = true;

        loaded = true;
    }

    private void UpdateLoadingBar()
    {
        ReferencesManager.Instance.regionUI.regionsLoadingBarInner.fillAmount = regionsCompleted / regionsMax;
        ReferencesManager.Instance.regionUI.regionsLoadingProgressText.text = (regionsCompleted / regionsMax * 100).ToString() + "%";
    }

    private void UpdateDeletingLoadingBar()
    {
        ReferencesManager.Instance.regionUI.regionsLoadingBarInner.fillAmount = regionsDeleted / regionsMax;
        ReferencesManager.Instance.regionUI.regionsLoadingProgressText.text = (regionsDeleted / regionsMax * 100).ToString() + "%";
    }

    private IEnumerator LoadRegions_Co()
    {
        ReferencesManager.Instance.regionUI.regionsLoadingPanel.SetActive(true);

        ReferencesManager.Instance.regionUI.regionsLoadingMainText.text = "Загружаю провинции из файлов...";

        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            for (int k = 0; k < ReferencesManager.Instance.countryManager.countries[i].myRegions.Count; k++)
            {
                RegionManager region = ReferencesManager.Instance.countryManager.countries[i].myRegions[k];

                region.gameObject.SetActive(false);
                regionsDeleted++;
                UpdateDeletingLoadingBar();
            }
        }

        ReferencesManager.Instance.regionUI.regionsLoadingMainText.text = "Назначаю данные в провинции...";

        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            foreach (RegionManager region in ReferencesManager.Instance.countryManager.countries[i].myRegions)
            {
                region.gameObject.SetActive(true);
                region.UpdateRegionGarrison();
                regionsCompleted++;
                UpdateLoadingBar();
            }
        }


        ReferencesManager.Instance.regionUI.regionsLoadingPanel.SetActive(false);

        yield break;
    }
}
