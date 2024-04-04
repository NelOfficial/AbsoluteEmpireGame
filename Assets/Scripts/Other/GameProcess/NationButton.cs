using UnityEngine.UI;
using UnityEngine;
using TMPro;


public class NationButton : MonoBehaviour
{
    public CountryInfoAdvanced.Nation currentNation;
    [SerializeField] Image countryFlag;
    [SerializeField] TMP_Text countryName;

    private CountryManager countryManager;

    public void SetUp()
    {
        countryManager = FindObjectOfType<CountryManager>();

        countryFlag.sprite = currentNation.country.countryFlag;
        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            countryName.text = currentNation.country._nameEN;
        }
        if (PlayerPrefs.GetInt("languageId") == 1)
        {
            countryName.text = currentNation.country._name;
        }

        if (countryManager.currentCountry.country._id == currentNation.country._id) // Formed
        {
            Destroy(this.gameObject);
        }
    }

    public void CheckFormNation()
    {
        RegionUI regionUI = FindObjectOfType<RegionUI>();
        UISoundEffect uISoundEffect = FindObjectOfType<UISoundEffect>();

        uISoundEffect.PlayAudio(regionUI.click_01);

        if (countryManager.currentCountry.country._id != currentNation.country._id) // Isn't formed
        {
            if (HasNeededRegions())
            {
                FormNation();
            }
            else
            {
                ShowRegions();
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private bool HasNeededRegions()
    {
        bool result = false;

        int neededRegionsHas = 0;

        foreach (RegionManager region in currentNation.regionsNeeded)
        {
            if (countryManager.currentCountry.myRegions.Contains(region))
            {
                neededRegionsHas++;
            }
        }

        if (neededRegionsHas == currentNation.regionsNeeded.Count)
        {
            result = true;
        }
        else
        {
            result = false;
        }

        return result;
    }

    private void ShowRegions()
    {
        foreach (RegionManager region in currentNation.regionsNeeded)
        {
            if (region.currentCountry.country._id != countryManager.currentCountry.country._id)
            {
                region.GetComponent<SpriteRenderer>().color = ReferencesManager.Instance.gameSettings.redColor;
            }
        }

        ReferencesManager.Instance.countryInfo.ToggleUI();
    }

    private void FormNation()
    {
        countryManager.currentCountry.country = currentNation.country;

        countryManager.UpdateCountryInfo();
    }
}