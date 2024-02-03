using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class RegionLoader : MonoBehaviour
{

    private float regionsMax;
    private float regionsCompleted;
    private float regionsDeleted;

    public bool loaded = false;

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "Editor")
        {
            ReferencesManager.Instance.countryManager.regions.Clear();

            for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
            {
                for (int r = 0; r < ReferencesManager.Instance.countryManager.countries[i].myRegions.Count; r++)
                {
                    ReferencesManager.Instance.countryManager.regions.Add(ReferencesManager.Instance.countryManager.countries[i].myRegions[r]);
                }
            }

            for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
            {
                if (ReferencesManager.Instance.countryManager.regions[i]._id == 0)
                {
                    ReferencesManager.Instance.countryManager.regions[i]._id = i;
                }
            }
        }
        else
        {
            ReferencesManager.Instance.countryManager.regions = FindObjectsOfType<RegionManager>().ToList();
        }


        regionsMax = ReferencesManager.Instance.countryManager.regions.Count;
        StartCoroutine(LoadRegions_Co());

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

        ReferencesManager.Instance.regionUI.regionsLoadingMainText.text = "Загрузка провинций...";
        foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
        {
            region.isSelected = false;
            region.canSelect = true;

            if (ReferencesManager.Instance.mEditor != null)
            {
                region.currentRegionManager = null;
            }

            int random = Random.Range(2000, 12000);

            region.currentDefenseUnits = ReferencesManager.Instance.gameSettings.currentDefenseUnits_FirstLevel;

            if (region.population == 0)
            {
                if (region.capital)
                {
                    region.population = Random.Range(100000, 800000);
                }
                else if (!region.capital)
                {
                    region.population = random;
                }
            }
            region.GetComponent<SpriteRenderer>().color = region.currentCountry.country.countryColor;

            regionsCompleted++;
            UpdateLoadingBar();

            yield return new WaitForSecondsRealtime(0.000001f);
        }

        ReferencesManager.Instance.regionUI.regionsLoadingPanel.SetActive(false);

        yield break;
    }
}
