using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public IntListValue localSavesIds;

    [SerializeField] Transform savesContainer;
    [SerializeField] GameObject savePrefab;

    public StringValue currentSave;


    private void Awake()
    {
        UpdateSavesList();
    }

    public void UpdateSavesList()
    {
        try
        {
            localSavesIds.list.Clear();

            string saves = PlayerPrefs.GetString("SAVES_IDS");
            string[] _saves = saves.Split(';');

            for (int i = 0; i < _saves.Length; i++)
            {
                int value = int.Parse(_saves[i]);

                localSavesIds.list.Add(value);
            }
        }
        catch (Exception) { }
    }

    public void SetSavesIDs()
    {
        string _saves = "";

        for (int i = 0; i < localSavesIds.list.Count; i++)
        {
            _saves += $"{localSavesIds.list[i]};";
        }

        PlayerPrefs.SetString("SAVES_IDS", _saves);
    }

    public void Save()
    {
        try
        {
            PlayerPrefs.SetString("FIRST_LOAD", "FALSE");

            int saveId = localSavesIds.list.Count > 0 ? localSavesIds.list.Max() + 1 : 1;

            string dateTime = DateTime.Now.ToString();

            PlayerPrefs.SetString($"{saveId}_DATETIME", dateTime);
            PlayerPrefs.SetString($"{saveId}_DIFFICULTY", ReferencesManager.Instance.gameSettings.difficultyValue.value);
            PlayerPrefs.SetString($"{saveId}_GAMEMODE", ReferencesManager.Instance.gameSettings._currentGameMode.value);

            #region CountrySave

            string countries = string.Join(";", ReferencesManager.Instance.countryManager.countries.Select(c => c.country._id));
            PlayerPrefs.SetInt($"{saveId}_PLAYER_COUNTRY", ReferencesManager.Instance.countryManager.currentCountry.country._id);
            PlayerPrefs.SetString($"{saveId}_COUNTRIES", countries);
            PlayerPrefs.SetString($"{saveId}_TOURNAMENT_COUNTRIES", ReferencesManager.Instance.gameSettings._currentTournamentCountries.value);

            foreach (var country in ReferencesManager.Instance.countryManager.countries)
            {
                var countryId = country.country._id;
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{countryId}_IDEOLOGY", country.ideology);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_MONEY", country.money);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_FOOD", country.food);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_RECROOTS", country.recroots);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_RESEARCH_POINTS", country.researchPoints);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_RECROOTS_LIMIT", country.recruitsLimit);

                #region MONEY_INCOME

                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_startMoneyIncome", country.startMoneyIncome);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_moneyNaturalIncome", country.moneyNaturalIncome);

                #endregion

                #region FOOD_INCOME

                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_startFoodIncome", country.startFoodIncome);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_foodNaturalIncome", country.foodNaturalIncome);

                #endregion

                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_recrootsIncome", country.recrootsIncome);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_researchPointsIncome", country.researchPointsIncome);
                PlayerPrefs.SetFloat($"{saveId}_COUNTRY_{countryId}_fuel", country.fuel);

                if (country.mobilasing)
                {
                    PlayerPrefs.SetString($"{saveId}_COUNTRY_{countryId}_MOBILASING", "TRUE");
                }

                if (country.deMobilasing)
                {
                    PlayerPrefs.SetString($"{saveId}_COUNTRY_{countryId}_DEMOBILASING", "TRUE");
                }

                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_civFactories", country.civFactories);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_farms", country.farms);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_cheFarms", country.chemicalFarms);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{countryId}_resLabs", country.researchLabs);
            }

            #endregion

            #region RegionSave

            foreach (var region in ReferencesManager.Instance.countryManager.regions)
            {
                var regionId = region._id;
                PlayerPrefs.SetInt($"{saveId}_REGION_{regionId}_CURRENTCOUNTRY_ID", region.currentCountry.country._id);
                PlayerPrefs.SetInt($"{saveId}_REGION_{regionId}_INFRASTRUCTURES", region.infrastructure_Amount);
                PlayerPrefs.SetInt($"{saveId}_REGION_{regionId}_CIV_FABRICS", region.civFactory_Amount);
                PlayerPrefs.SetInt($"{saveId}_REGION_{regionId}_FARMS", region.farms_Amount);
                PlayerPrefs.SetInt($"{saveId}_REGION_{regionId}_CHEFARMS", region.cheFarms);
                PlayerPrefs.SetInt($"{saveId}_REGION_{regionId}_RESLABS", region.researchLabs);
                PlayerPrefs.SetInt($"{saveId}_REGION_{regionId}_DOCKYARDS", region.dockyards);
                PlayerPrefs.SetInt($"{saveId}_REGION_{regionId}_POPULATION", region.population);
                PlayerPrefs.SetInt($"{saveId}_REGION_{regionId}_FORTS", region.fortifications_Amount);
            }

            #region Army

            foreach (var region in ReferencesManager.Instance.countryManager.regions)
            {
                region.CheckRegionUnits(region);

                PlayerPrefs.SetString($"{saveId}_REGION_{region._id}_HAS_ARMY", region.hasArmy ? "TRUE" : "FALSE");

                if (region.hasArmy)
                {
                    var unitMovement = region.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();
                    var unitCounts = new Dictionary<string, int>
                {
                    {"INF_LVL1", 0}, {"INF_LVL2", 0}, {"INF_LVL3", 0},
                    {"ART_LVL1", 0}, {"ART_LVL2", 0},
                    {"TANK_LVL1", 0}, {"TANK_LVL2", 0}, {"TANK_LVL3", 0},
                    {"MOTO_LVL1", 0}, {"MOTO_LVL2", 0},
                    {"ATI_LVL1", 0}, {"ATI_LVL2", 0},
                    {"CAV_LVL1", 0}, {"CAV_LVL2", 0}
                };

                    foreach (var unitHealth in unitMovement.unitsHealth)
                    {
                        var unit = unitHealth.unit;
                        switch (unit.type)
                        {
                            case UnitScriptableObject.Type.SOLDIER:
                                unitCounts[$"INF_LVL{unit.level}"]++;
                                break;
                            case UnitScriptableObject.Type.ARTILERY:
                                unitCounts[$"ART_LVL{unit.level}"]++;
                                break;
                            case UnitScriptableObject.Type.TANK:
                                unitCounts[$"TANK_LVL{unit.level}"]++;
                                break;
                            case UnitScriptableObject.Type.SOLDIER_MOTORIZED:
                                unitCounts[$"MOTO_LVL{unit.level}"]++;
                                break;
                        }

                        if (unit.unitName == "ATI_01")
                        {
                            unitCounts["ATI_LVL1"]++;
                        }
                        else if (unit.unitName == "ATI_02")
                        {
                            unitCounts["ATI_LVL2"]++;
                        }
                        else if (unit.unitName == "CAV_01")
                        {
                            unitCounts["CAV_LVL1"]++;
                        }
                        else if (unit.unitName == "CAV_02")
                        {
                            unitCounts["CAV_LVL2"]++;
                        }
                    }

                    foreach (var unitCount in unitCounts)
                    {
                        PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_{unitCount.Key}", unitCount.Value);
                    }
                }
            }

            #endregion

            #endregion

            #region TechnologySave

            foreach (var country in ReferencesManager.Instance.countryManager.countries)
            {
                for (int i = 0; i < ReferencesManager.Instance.gameSettings.technologies.Length; i++)
                {
                    var tech = ReferencesManager.Instance.gameSettings.technologies[i];
                    if (!tech.startReasearched)
                    {
                        PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_TECH_{i}", HasTech(country, tech) ? "TRUE" : "FALSE");
                    }
                }
            }

            #endregion

            #region Events and Date

            for (int i = 0; i < 3; i++)
            {
                PlayerPrefs.SetInt($"{saveId}_DATE_{i}", ReferencesManager.Instance.dateManager.currentDate[i]);
            }

            for (int i = 0; i < ReferencesManager.Instance.gameSettings.gameEvents.Count; i++)
            {
                PlayerPrefs.SetString($"{saveId}_EVENT_{i}", ReferencesManager.Instance.gameSettings.gameEvents[i]._checked ? "TRUE" : "FALSE");
            }

            #endregion

            #region Relations

            foreach (var country in ReferencesManager.Instance.countryManager.countries)
            {
                string countryTrades = "", countryPacts = "", countryUnions = "", countryRights = "", countryWars = "", countryVassals = "";

                foreach (var otherCountry in ReferencesManager.Instance.countryManager.countries)
                {
                    var relation = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(country, otherCountry);

                    if (relation.trade) countryTrades += $"{otherCountry.country._id};";
                    if (relation.pact) countryPacts += $"{otherCountry.country._id};";
                    if (relation.union) countryUnions += $"{otherCountry.country._id};";
                    if (relation.right) countryRights += $"{otherCountry.country._id};";
                    if (relation.war) countryWars += $"{otherCountry.country._id};";
                    if (relation.vassal) countryVassals += $"{otherCountry.country._id};";
                }

                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_TRADES", countryTrades);
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_RIGHTS", countryRights);
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_PACTS", countryPacts);
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_UNIONS", countryUnions);
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_WARS", countryWars);
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_VASSALS", countryVassals);
            }

            #endregion

            PlayerPrefs.SetInt($"{saveId}_SCENARIO_ID", ReferencesManager.Instance.regionLoader._currentScenarioId);

            localSavesIds.list.Add(saveId);

            string saves = string.Join(";", localSavesIds.list);
            PlayerPrefs.SetString("SAVES_IDS", saves);
        }
        catch (Exception ex)
        {
            WarningManager.Instance.Warn($"Error: {ex.Message}");
            ReferencesManager.Instance.regionUI._autoSaveMessage.SetActive(false);
        }
    }

    public void UpdateUI()
    {
        foreach (Transform child in savesContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < localSavesIds.list.Count; i++)
        {
            try
            {
                int saveID = localSavesIds.list[i];

                GameObject spawnedSavePrefab = Instantiate(savePrefab, savesContainer);

                spawnedSavePrefab.GetComponent<SaveItem>().saveID = saveID;

                CountryScriptableObject _country = null;

                int countryId = PlayerPrefs.GetInt($"{saveID}_PLAYER_COUNTRY");

                foreach (CountryScriptableObject country in ReferencesManager.Instance.globalCountries)
                {
                    if (country._id == countryId)
                    {
                        _country = country;
                    }
                }

                string _scenarioName = ReferencesManager.Instance.offlineGameSettings.GetScenario(PlayerPrefs.GetInt($"{saveID}_SCENARIO_ID"))._name;

                spawnedSavePrefab.GetComponent<SaveItem>().country = _country;
                spawnedSavePrefab.GetComponent<SaveItem>().date = PlayerPrefs.GetString($"{saveID}_DATETIME");
                spawnedSavePrefab.GetComponent<SaveItem>().scenarioName = _scenarioName;
                spawnedSavePrefab.GetComponent<SaveItem>().scenarioId = PlayerPrefs.GetInt($"{saveID}_SCENARIO_ID");

                spawnedSavePrefab.GetComponent<SaveItem>().UpdateUI();

            }
            catch (System.Exception) { }
        }
    }

    private bool HasTech(CountrySettings country, TechnologyScriptableObject tech)
    {
        if (country.countryTechnologies.Contains(tech))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UpdateLocalSavesUI()
    {

    }

    public void Load()
    {
        PlayerPrefs.SetString("FIRST_LOAD", "FALSE");

        SceneManager.LoadScene("EuropeSceneOffline");
    }
}
