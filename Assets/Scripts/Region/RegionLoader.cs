using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using Org.BouncyCastle.Asn1.Pkcs;

public class RegionLoader : MonoBehaviour
{
    public bool loaded = false;

    [HideInInspector] public string _currentScenarioData;
    public int _currentScenarioId;

    [SerializeField] private StringValue _currentScenarioData_Value;

    public List<ModRegionData> regionModIDs = new List<ModRegionData>();

    private void Start()
    {
        foreach (RegionManager province in ReferencesManager.Instance.countryManager.regions)
        {
            if (province.population == 0)
            {
                province.population = UnityEngine.Random.Range(1000, 70000);
            }

            province.currentDefenseUnits = ReferencesManager.Instance.gameSettings.currentDefenseUnits_FirstLevel;
        }

        if (SceneManager.GetActiveScene().buildIndex != 2)
        {
            if (!ReferencesManager.Instance.gameSettings.playMod.value &&
                !ReferencesManager.Instance.gameSettings.playTestingMod.value &&
                string.IsNullOrEmpty(ReferencesManager.Instance.gameSettings.editingModString.value))
            {
                _currentScenarioData = _currentScenarioData_Value.value;

                if (SceneManager.GetActiveScene().isLoaded)
                {
                    LoadMod(_currentScenarioData);
                }
            }
        }

        ReferencesManager.Instance.mainCamera.Map_MoveTouch_IsAllowed = true;

        loaded = true;

        //ReferencesManager.Instance.gameSettings._regionOpacity = 0.5f;
        //ReferencesManager.Instance.regionManager.UpdateRegions();
    }

    private void LoadMod(string modData)
    {
        string[] sections = modData.Split(new[] { "#REGIONS#", "#COUNTRIES_SETTINGS#", "#EVENTS#" }, StringSplitOptions.None);

        string[] mainModDataLines = sections[0].Split(';');
        string[] regionsDataLines = sections[1].Split(';');
        string[] countriesDataLines = sections[2].Split(';');

        LoadScenario(mainModDataLines);
        LoadCountries(mainModDataLines);
        var regionModIDs = ParseRegionData(regionsDataLines);
        UpdateRegions(regionModIDs);
        SetSelectedCountry();
        UpdateCountriesData(countriesDataLines);
    }

    private void LoadScenario(string[] mainModDataLines)
    {
        try
        {
            string scenarioID = ExtractID(mainModDataLines[0]);
            string value = ExtractID(mainModDataLines[1]);

            _currentScenarioId = int.Parse(scenarioID);

            var scenarioEvents = ReferencesManager.Instance.gameSettings._scenariosEvents
                .FirstOrDefault(se => _currentScenarioId == se._id);

            if (scenarioEvents != null)
            {
                foreach (var _event in scenarioEvents._events)
                {
                    _event._checked = false;
                    ReferencesManager.Instance.gameSettings.gameEvents.Add(_event);
                }
            }

            ReferencesManager.Instance.gameSettings.allowGameEvents = int.Parse(value) == 1;
        }
        catch (Exception ex)
        {
            if (ReferencesManager.Instance.gameSettings.developerMode)
            {
                Debug.LogError($"ERROR: Mod loader error in value parser: {ex.Message}");
            }
        }
    }

    private void LoadCountries(string[] mainModDataLines)
    {
        var existingCountryIds = new HashSet<int>(
            ReferencesManager.Instance.countryManager.countries.Select(c => c.country._id));
        var globalCountryIds = new HashSet<int>(
            ReferencesManager.Instance.globalCountries.Select(gc => gc._id));

        for (int i = 2; i < mainModDataLines.Length; i++)
        {
            string _line = mainModDataLines[i];
            if (!string.IsNullOrEmpty(_line))
            {
                int countryId = int.Parse(ReferencesManager.Instance.countryManager.GetValue(_line));

                if (!existingCountryIds.Contains(countryId) && globalCountryIds.Contains(countryId))
                {
                    var countryScriptableObject = ReferencesManager.Instance.globalCountries
                        .FirstOrDefault(gc => gc._id == countryId);

                    if (countryScriptableObject != null)
                    {
                        ReferencesManager.Instance.CreateCountry(countryScriptableObject, "Неопределено");
                    }
                }
            }
        }
    }

    private List<ModRegionData> ParseRegionData(string[] regionsDataLines)
    {
        var regionModIDs = new List<ModRegionData>();

        foreach (var _line in regionsDataLines)
        {
            if (!string.IsNullOrEmpty(_line))
            {
                string[] regionIdParts = _line.Split(' ');
                if (regionIdParts.Length < 3) continue;

                int regValue = int.Parse(regionIdParts[0].Substring(7));
                int regionCountryID = int.Parse(regionIdParts[2]);

                regionModIDs.Add(new ModRegionData
                {
                    countryID = regionCountryID,
                    regionId = regValue
                });
            }
        }

        return regionModIDs;
    }

    private void UpdateRegions(List<ModRegionData> regionModIDs)
    {
        var regionDict = ReferencesManager.Instance.countryManager.regions
            .ToDictionary(r => r._id, r => r);

        var countryDict = ReferencesManager.Instance.countryManager.countries
            .ToDictionary(c => c.country._id, c => c);

        foreach (var regValue in regionModIDs)
        {
            if (regionDict.TryGetValue(regValue.regionId, out var province) &&
                countryDict.TryGetValue(regValue.countryID, out var country))
            {
                if (province.currentCountry != country)
                {
                    ReferencesManager.Instance.AnnexRegion(province, country);
                }
            }
        }
    }

    private void SetSelectedCountry()
    {
        int selectedCountryId = int.Parse(ReferencesManager.Instance.gameSettings._playerCountrySelected.value);
        var countryDict = ReferencesManager.Instance.countryManager.countries
            .ToDictionary(c => c.country._id, c => c);

        if (countryDict.TryGetValue(selectedCountryId, out var selectedCountry))
        {
            ReferencesManager.Instance.countryManager.currentCountry = selectedCountry;
            ReferencesManager.Instance.countryManager.currentCountry.isPlayer = true;
        }
    }

    private void UpdateCountriesData(string[] countriesDataLines)
    {
        var countryDict = ReferencesManager.Instance.countryManager.countries
            .ToDictionary(c => c.country._id, c => c);

        foreach (var _line in countriesDataLines)
        {
            if (!string.IsNullOrEmpty(_line))
            {
                try
                {
                    string new_lineData = ReferencesManager.Instance.countryManager.GetValue(_line);

                    if (!string.IsNullOrEmpty(new_lineData))
                    {
                        string[] values = new_lineData.Split('|');

                        int countryID = int.Parse(values[0]);
                        if (countryDict.TryGetValue(countryID, out var country))
                        {
                            country.money = int.Parse(values[1]);
                            country.food = int.Parse(values[2]);
                            country.recroots = int.Parse(values[3]);
                            country.ideology = values[4];

                            country.UpdateCountryGraphics(country.ideology);
                        }
                    }
                }
                catch (Exception) { }
            }
        }
    }

    private string ExtractID(string line)
    {
        return line.Split('[')[1].TrimEnd(']');
    }

    [System.Serializable]
    public class ModRegionData
    {
        public int countryID;
        public int regionId;
    }
}
