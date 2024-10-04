using TMPro;
using UnityEngine;

public class DeveloperMode : MonoBehaviour
{
    [SerializeField] private GameObject RegionDevOptions;
    [SerializeField] private GameObject CountryCheatsPanel;
    [SerializeField] private GameObject SettingsCheats;

    [SerializeField] private TMP_InputField moneyField;
    [SerializeField] private TMP_InputField foodField;
    [SerializeField] private TMP_InputField recrootsField;
    [SerializeField] private TMP_InputField _researchPoints_Inputfield;
    [SerializeField] private TMP_InputField _fuel_Inputfield;


    private void Start()
    {
        if (ReferencesManager.Instance.gameSettings.developerCheats ||
            ReferencesManager.Instance.gameSettings._isPremium.value)
        {
            RegionDevOptions.SetActive(true);
            CountryCheatsPanel.SetActive(true);
            SettingsCheats.SetActive(true);
        }
        else if (!ReferencesManager.Instance.gameSettings.developerCheats ||
            !ReferencesManager.Instance.gameSettings._isPremium.value)
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
        ReferencesManager.Instance.countryManager.currentCountry.isPlayer = false;
        ReferencesManager.Instance.countryManager.currentCountry.gameObject.AddComponent<CountryAIManager>();
        ReferencesManager.Instance.aiManager.AICountries.Add(ReferencesManager.Instance.countryManager.currentCountry);

        ReferencesManager.Instance.countryManager.currentCountry = ReferencesManager.Instance.regionManager.currentRegionManager.currentCountry;

        ReferencesManager.Instance.aiManager.AICountries.Remove(ReferencesManager.Instance.countryManager.currentCountry);
        Destroy(ReferencesManager.Instance.countryManager.currentCountry.gameObject.GetComponent<CountryAIManager>());

        ReferencesManager.Instance.countryManager.currentCountry.isPlayer = true;

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
            if (int.Parse(moneyField.text) <= 999999)
            {
                ReferencesManager.Instance.countryManager.currentCountry.money = int.Parse(moneyField.text);
            }
            else if (int.Parse(moneyField.text) > 999999)
            {
                moneyField.text = "999999";
            }
        }
        if (!string.IsNullOrEmpty(foodField.text))
        {
            if (int.Parse(foodField.text) < 0)
            {
                foodField.text = 0.ToString();
            }
            if (int.Parse(foodField.text) <= 999999)
            {
                ReferencesManager.Instance.countryManager.currentCountry.food = int.Parse(foodField.text);
            }
            else if (int.Parse(foodField.text) > 999999)
            {
                foodField.text = "999999";
            }
        }
        if (!string.IsNullOrEmpty(recrootsField.text))
        {
            if (int.Parse(recrootsField.text) < 0)
            {
                recrootsField.text = 0.ToString();
            }

            if (int.Parse(recrootsField.text) <= 999999)
            {
                ReferencesManager.Instance.countryManager.currentCountry.recruits = int.Parse(recrootsField.text);
            }
            else if (int.Parse(recrootsField.text) > 999999)
            {
                recrootsField.text = "999999";
            }
        }

        if (!string.IsNullOrEmpty(_researchPoints_Inputfield.text))
        {
            if (int.Parse(_researchPoints_Inputfield.text) < 0)
            {
                _researchPoints_Inputfield.text = 0.ToString();
            }

            if (int.Parse(_researchPoints_Inputfield.text) <= 999999)
            {
                ReferencesManager.Instance.countryManager.currentCountry.researchPoints = int.Parse(_researchPoints_Inputfield.text);
            }
            else if (int.Parse(_researchPoints_Inputfield.text) > 999999)
            {
                _researchPoints_Inputfield.text = "999999";
            }
        }

        if (!string.IsNullOrEmpty(_fuel_Inputfield.text))
        {
            if (int.Parse(_fuel_Inputfield.text) < 0)
            {
                _fuel_Inputfield.text = 0.ToString();
            }

            if (int.Parse(_fuel_Inputfield.text) <= 999999)
            {
                ReferencesManager.Instance.countryManager.currentCountry.fuel = int.Parse(_fuel_Inputfield.text);
            }
            else if (int.Parse(_fuel_Inputfield.text) > 999999)
            {
                _fuel_Inputfield.text = "999999";
            }
        }

        Multiplayer.Instance.SetCountryValues(
            ReferencesManager.Instance.countryManager.currentCountry.country._id,
            ReferencesManager.Instance.countryManager.currentCountry.money,
            ReferencesManager.Instance.countryManager.currentCountry.food,
            ReferencesManager.Instance.countryManager.currentCountry.recruits);

        ReferencesManager.Instance.countryManager.UpdateValuesUI();
    }
}
