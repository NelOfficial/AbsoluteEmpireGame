using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class RegionLoader : MonoBehaviour
{
    private float regionsMax;
    private float regionsCompleted;
    private float regionsDeleted;

    public bool loaded = false;

    [HideInInspector] public string _currentScenarioData;
    public int _currentScenarioId;

    [SerializeField] private StringValue _currentScenarioData_Value;

    public List<ModRegionData> regionModIDs = new List<ModRegionData>();

    void Start()
    {
        ReferencesManager.Instance.countryManager.regions.Clear();
        ReferencesManager.Instance.countryManager.regions = FindObjectsOfType<RegionManager>().ToList();

        List<int> regionIds = new List<int>();

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            regionIds.Add(ReferencesManager.Instance.countryManager.regions[i]._id);

            if (ReferencesManager.Instance.countryManager.regions[i].population == 0)
            {
                ReferencesManager.Instance.countryManager.regions[i].population = UnityEngine.Random.Range(1000, 70000);
            }

            ReferencesManager.Instance.countryManager.regions[i].currentDefenseUnits =
                ReferencesManager.Instance.gameSettings.currentDefenseUnits_FirstLevel;
        }

        if (SceneManager.GetActiveScene().buildIndex != 2)
        {
            if (!ReferencesManager.Instance.gameSettings.playMod.value &&
                !ReferencesManager.Instance.gameSettings.playTestingMod.value &&
                string.IsNullOrEmpty(ReferencesManager.Instance.gameSettings.editingModString.value))
            {
                _currentScenarioData = _currentScenarioData_Value.value;

                LoadMod(_currentScenarioData);
            }
        }

        regionsMax = ReferencesManager.Instance.countryManager.regions.Count;
        //StartCoroutine(LoadRegions_Co());

        ReferencesManager.Instance.mainCamera.Map_MoveTouch_IsAllowed = true;

        loaded = true;
    }

    private void LoadMod(string modData)
    {
        string[] parts = new string[0];
        string secondPart = "";
        string value = "";

        string[] mainModDataLines = modData.Split("#REGIONS#")[0].Split(';');
        string[] regionsDataLines = modData.Split("#REGIONS#")[1].Split(';');
        string[] countriesDataLines = modData.Split("#COUNTRIES_SETTINGS#")[1].Split(';');
        string[] eventsIDsDataLines = modData.Split("#EVENTS#")[1].Split(';');

        try
        {
            string _line = mainModDataLines[1];
            parts = _line.Split('[');

            secondPart = parts[1];

            value = secondPart.Remove(secondPart.Length - 1);

            string scenarioID = mainModDataLines[0].Split('[')[1];
            scenarioID = scenarioID.Remove(scenarioID.Length - 1);

            _currentScenarioId = int.Parse(scenarioID);

            foreach (GameSettings.ScenarioEvents scenarioEvents in ReferencesManager.Instance.gameSettings._scenariosEvents)
            {
                if (_currentScenarioId == scenarioEvents._id)
                {
                    foreach (EventScriptableObject _event in scenarioEvents._events)
                    {
                        _event._checked = false;
                        ReferencesManager.Instance.gameSettings.gameEvents.Add(_event);
                    }
                }
            }
        }
        catch (Exception)
        {
            if (ReferencesManager.Instance.gameSettings.developerMode)
            {
                Debug.LogError($"ERROR: Mod loader error in value parser");
            }
        }

        int isModAllowsGameEvents = int.Parse(value);

        if (isModAllowsGameEvents == 0)
        {
            ReferencesManager.Instance.gameSettings.allowGameEvents = false;
        }
        else if (isModAllowsGameEvents == 1)
        {
            ReferencesManager.Instance.gameSettings.allowGameEvents = true;
        }

        for (int i = 2; i < mainModDataLines.Length; i++)
        {
            string _line = mainModDataLines[i];
            if (!string.IsNullOrEmpty(_line))
            {
                value = ReferencesManager.Instance.countryManager.GetValue(_line);

                bool _hasCountry = ReferencesManager.Instance.countryManager.countries.Any(item => item.country._id == int.Parse(value));

                if (!_hasCountry)
                {
                    foreach (CountryScriptableObject countryScriptableObject in ReferencesManager.Instance.globalCountries)
                    {
                        if (countryScriptableObject._id == int.Parse(value))
                        {
                            ReferencesManager.Instance.CreateCountry(countryScriptableObject, "Неопределено");
                        }
                    }
                }
            }
        }

        List<int> countriesInRegionsIDs = new List<int>();

        for (int r = 0; r < regionsDataLines.Length; r++)
        {
            try
            {
                string _line = regionsDataLines[r];
                int _value = int.Parse(ReferencesManager.Instance.countryManager.GetValue(_line));

                countriesInRegionsIDs.Add(_value);
            }
            catch (Exception) { }
        }

        string regionValue = "";


        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count + 2; i++)
        {
            try
            {
                string _line = regionsDataLines[i];
                if (!string.IsNullOrEmpty(_line))
                {
                    string[] regionIdParts = _line.Split(' ');
                    regionValue = regionIdParts[0].Remove(0, 7);
                    int regValue = int.Parse(regionValue);
                    int regionCountryID = int.Parse(regionIdParts[2]);

                    ModRegionData modRegionData = new ModRegionData();

                    modRegionData.countryID = regionCountryID;
                    modRegionData.regionId = regValue;

                    regionModIDs.Add(modRegionData);
                }
            }
            catch (Exception) { }
        }

        foreach (ModRegionData regValue in regionModIDs)
        {
            foreach (RegionManager province in ReferencesManager.Instance.countryManager.regions)
            {
                if (regValue.regionId == province._id)
                {
                    for (int c = 0; c < ReferencesManager.Instance.countryManager.countries.Count; c++)
                    {
                        if (regValue.countryID == ReferencesManager.Instance.countryManager.countries[c].country._id)
                        {
                            ReferencesManager.Instance.AnnexRegion(province, ReferencesManager.Instance.countryManager.countries[c]);
                        }
                    }
                }
            }
        }


        bool hasCountry = ReferencesManager.Instance.countryManager.countries.Any(item => item.country._id == PlayerPrefs.GetInt("currentCountryIndex"));

        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            if (hasCountry)
            {
                if (ReferencesManager.Instance.countryManager.countries[i].country._id == PlayerPrefs.GetInt("currentCountryIndex"))
                {
                    ReferencesManager.Instance.countryManager.currentCountry = ReferencesManager.Instance.countryManager.countries[i];
                    ReferencesManager.Instance.countryManager.currentCountry.isPlayer = true;
                }
            }
        }

        #region countriesSettings

        if (!ReferencesManager.Instance.countryManager.IsNullOrEmpty(countriesDataLines))
        {
            for (int i = 0; i < countriesDataLines.Length; i++)
            {
                if (!string.IsNullOrEmpty(countriesDataLines[i]))
                {
                    try
                    {
                        string new_lineData = ReferencesManager.Instance.countryManager.GetValue(countriesDataLines[i]);

                        if (!string.IsNullOrEmpty(new_lineData))
                        {
                            string[] values = new_lineData.Split('|');

                            int countryID = int.Parse(values[0]);
                            int money = int.Parse(values[1]);
                            int food = int.Parse(values[2]);
                            int recroots = int.Parse(values[3]);

                            string ideology = values[4];

                            foreach (CountrySettings country in ReferencesManager.Instance.countryManager.countries)
                            {
                                if (country.country._id == countryID)
                                {
                                    country.money = money;
                                    country.food = food;
                                    country.recroots = recroots;

                                    country.ideology = ideology;

                                    country.UpdateCountryGraphics(country.ideology);
                                }
                            }
                        }
                    }
                    catch (Exception except)
                    {

                    }
                }
            }
        }

        #endregion

        #region events

        

        #endregion
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

            int random = UnityEngine.Random.Range(2000, 12000);

            region.currentDefenseUnits = ReferencesManager.Instance.gameSettings.currentDefenseUnits_FirstLevel;

            if (region.population == 0)
            {
                if (region.capital)
                {
                    region.population = UnityEngine.Random.Range(100000, 800000);
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

    [System.Serializable]
    public class ModRegionData
    {
        public int countryID;
        public int regionId;
    }
}
