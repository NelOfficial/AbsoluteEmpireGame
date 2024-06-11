using TMPro;
using UnityEngine;

public class DeveloperMode : MonoBehaviour
{
    [SerializeField] GameObject RegionDevOptions;
    [SerializeField] GameObject CountryCheatsPanel;
    [SerializeField] GameObject SettingsCheats;

    [SerializeField] TMP_InputField moneyField;
    [SerializeField] TMP_InputField foodField;
    [SerializeField] TMP_InputField recrootsField;


    private void Start()
    {

        if (ReferencesManager.Instance.gameSettings.developerCheats)
        {
            RegionDevOptions.SetActive(true);
            CountryCheatsPanel.SetActive(true);
            SettingsCheats.SetActive(true);
        }
        else if (!ReferencesManager.Instance.gameSettings.developerCheats)
        {
            RegionDevOptions.SetActive(false);
            CountryCheatsPanel.SetActive(false);
            SettingsCheats.SetActive(false);
        }
    }

    public void AnnexButton()
    {
        ReferencesManager.Instance.AnnexRegion(ReferencesManager.Instance.regionManager.currentRegionManager, ReferencesManager.Instance.countryManager.currentCountry);
    }

    public void AddFabric()
    {
        ReferencesManager.Instance.regionManager.BuildBuilding(ReferencesManager.Instance.gameSettings.fabric, ReferencesManager.Instance.regionManager.currentRegionManager, true);
        ReferencesManager.Instance.regionManager.civFactory_Amount++;

        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
    }

    public void DeleteSaves()
    {
        PlayerPrefs.DeleteAll();
    }

    public void DiplomatyCheats(bool state)
    {
        ReferencesManager.Instance.gameSettings.diplomatyCheats = state;
    }

    public void PlayAs()
    {
        ReferencesManager.Instance.countryManager.currentCountry = ReferencesManager.Instance.regionManager.currentRegionManager.currentCountry;

        ReferencesManager.Instance.countryManager.UpdateCountryInfo();
        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
        ReferencesManager.Instance.countryManager.UpdateValuesUI();
    }

    public void CheckInputField()
    {
        if (!string.IsNullOrEmpty(moneyField.text))
        {
            if (int.Parse(moneyField.text) < 0)
            {
                moneyField.text = 0.ToString();
            }
            if (int.Parse(moneyField.text) <= 99999)
            {
                ReferencesManager.Instance.countryManager.currentCountry.money = int.Parse(moneyField.text);
            }
            else if (int.Parse(moneyField.text) > 99999)
            {
                moneyField.text = "99999";
            }
        }
        if (!string.IsNullOrEmpty(foodField.text))
        {
            if (int.Parse(foodField.text) < 0)
            {
                foodField.text = 0.ToString();
            }
            if (int.Parse(foodField.text) <= 99999)
            {
                ReferencesManager.Instance.countryManager.currentCountry.food = int.Parse(foodField.text);
            }
            else if (int.Parse(foodField.text) > 99999)
            {
                foodField.text = "99999";
            }
        }
        if (!string.IsNullOrEmpty(recrootsField.text))
        {
            if (int.Parse(recrootsField.text) < 0)
            {
                recrootsField.text = 0.ToString();
            }

            if (int.Parse(recrootsField.text) <= 99999)
            {
                ReferencesManager.Instance.countryManager.currentCountry.recroots = int.Parse(recrootsField.text);
            }
            else if (int.Parse(recrootsField.text) > 99999)
            {
                recrootsField.text = "99999";
            }
        }

        Multiplayer.Instance.SetCountryValues(
            ReferencesManager.Instance.countryManager.currentCountry.country._id,
            ReferencesManager.Instance.countryManager.currentCountry.money,
            ReferencesManager.Instance.countryManager.currentCountry.food,
            ReferencesManager.Instance.countryManager.currentCountry.recroots);

        ReferencesManager.Instance.countryManager.UpdateValuesUI();
    }
}
