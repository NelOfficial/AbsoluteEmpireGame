using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using UnityEditor;

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

            int saveId = 0;

            if (localSavesIds.list.Count > 0)
            {
                saveId = localSavesIds.list.Max() + 1;
            }
            else
            {
                saveId = 1;
            }

            string date_time = $"{DateTime.Now}";

            PlayerPrefs.SetString($"{saveId}_DATETIME", date_time);
            PlayerPrefs.SetInt($"{saveId}_SCENARIO", ReferencesManager.Instance.regionLoader._currentScenarioId);
            PlayerPrefs.SetString($"{saveId}_DIFFICULTY", ReferencesManager.Instance.gameSettings.difficultyValue.value);

            PlayerPrefs.SetString($"{saveId}_GAMEMODE", ReferencesManager.Instance.gameSettings._currentGameMode.value);

            #region CountrySave

            string _countries = "";

            for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
            {
                _countries += $"{ReferencesManager.Instance.countryManager.countries[i].country._id};";
            }

            PlayerPrefs.SetInt($"{saveId}_PLAYER_COUNTRY", ReferencesManager.Instance.countryManager.currentCountry.country._id);
            PlayerPrefs.SetString($"{saveId}_COUNTRIES", _countries);
            PlayerPrefs.SetString($"{saveId}_TOURNAMENT_COUNTRIES", ReferencesManager.Instance.gameSettings._currentTournamentCountries.value);

            for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
            {
                CountrySettings country = ReferencesManager.Instance.countryManager.countries[i];

                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_IDEOLOGY", country.ideology);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_MONEY", country.money);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_FOOD", country.food);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_RECROOTS", country.recruits);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_RESEARCH_POINTS", country.researchPoints);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_RECROOTS_LIMIT", country.recruitsLimit);

                #region MONEY_INCOME

                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_startMoneyIncome", country.startMoneyIncome);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_moneyNaturalIncome", country.moneyNaturalIncome);

                #endregion

                #region FOOD_INCOME

                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_startFoodIncome", country.startFoodIncome);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_foodNaturalIncome", country.foodNaturalIncome);

                #endregion

                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_recrootsIncome", country.recruitsIncome);

                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_researchPointsIncome", country.researchPointsIncome);

                PlayerPrefs.SetFloat($"{saveId}_COUNTRY_{country.country._id}_fuel", country.fuel);

                if (country.mobilasing)
                {
                    PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_MOBILASING", "TRUE");
                }

                if (country.deMobilasing)
                {
                    PlayerPrefs.SetString($"{saveId} _COUNTRY_ {country.country._id}_DEMOBILASING", "TRUE");
                }

                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_civFactories", country.civFactories);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_farms", country.farms);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_cheFarms", country.chemicalFarms);
                PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_resLabs", country.researchLabs);
            }

            #endregion

            #region RegionSave

            for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
            {
                RegionManager region = ReferencesManager.Instance.countryManager.regions[i];
                if (region._airBaseLevel > 0)
                {
                    Aviation_Storage airbase = region.gameObject.GetComponent<Aviation_Storage>();

                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_PLANES", airbase.planes.Count);

                    for (int p = 0; p < airbase.planes.Count; p++)
                    {
                        PlayerPrefs.SetString($"{saveId}_REGION_{region._id}_PLANE_{p}_TYPE", airbase.planes[p].AirPlane.tag);
                        PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_PLANE_{p}_OWNER", airbase.planes[p].Owner.country._id);
                        PlayerPrefs.SetFloat($"{saveId}_REGION_{region._id}_PLANE_{p}_HP", airbase.planes[p].hp);
                        PlayerPrefs.SetFloat($"{saveId}_REGION_{region._id}_PLANE_{p}_FUEL", airbase.planes[p].fuel);
                    }
                }

                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_CURRENTCOUNTRY_ID", region.currentCountry.country._id);

                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_INFRASTRUCTURES", region.infrastructure_Amount);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_CIV_FABRICS", region.civFactory_Amount);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_FARMS", region.farms_Amount);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_CHEFARMS", region.cheFarms);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_RESLABS", region.researchLabs);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_DOCKYARDS", region.dockyards);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_POPULATION", region.population);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_FORTS", region.fortifications_Amount);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_MARINEBASES", region._marineBaseLevel);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_AIRBASES", region._airBaseLevel);
            }

            #region Army

            for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
            {
                RegionManager region = ReferencesManager.Instance.countryManager.regions[i];

                region.CheckRegionUnits(region);

                if (region.hasArmy)
                {
                    PlayerPrefs.SetString($"{saveId}_REGION_{region._id}_HAS_ARMY", "TRUE");

                    UnitMovement unitMovement = region.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();

                    int infLVL1 = 0;
                    int infLVL2 = 0;
                    int infLVL3 = 0;

                    int artLVL1 = 0;
                    int artLVL2 = 0;

                    int atiLVL1 = 0;
                    int atiLVL2 = 0;

                    int aaaLVL1 = 0;
                    int aaaLVL2 = 0;

                    int tankLVL1 = 0;
                    int tankLVL2 = 0;
                    int tankLVL3 = 0;

                    int motoLVL1 = 0;
                    int motoLVL2 = 0;

                    int cavLVL1 = 0;
                    int cavLVL2 = 0;

                    for (int unitIndex = 0; unitIndex < unitMovement.unitsHealth.Count; unitIndex++)
                    {
                        UnitScriptableObject unit = unitMovement.unitsHealth[unitIndex].unit;

                        if (unit.type == UnitScriptableObject.Type.SOLDIER)
                        {
                            if (unit.level == 1)
                            {
                                infLVL1++;
                            }
                            else if (unit.level == 2)
                            {
                                infLVL2++;
                            }
                            else if (unit.level == 3)
                            {
                                infLVL3++;
                            }
                        }
                        else if (unit.type == UnitScriptableObject.Type.ARTILERY)
                        {
                            if (unit.level == 1)
                            {
                                artLVL1++;
                            }
                            else if (unit.level == 2)
                            {
                                artLVL2++;
                            }
                        }
                        else if (unit.type == UnitScriptableObject.Type.TANK)
                        {
                            if (unit.level == 1)
                            {
                                tankLVL1++;
                            }
                            else if (unit.level == 2)
                            {
                                tankLVL2++;
                            }
                            else if (unit.level == 3)
                            {
                                tankLVL3++;
                            }
                        }
                        else if (unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                        {
                            if (unit.level == 1)
                            {
                                motoLVL1++;
                            }
                            else if (unit.level == 2)
                            {
                                motoLVL2++;
                            }
                        }

                        else if (unit.unitName == "ATI_01")
                        {
                            artLVL1++;
                        }
                        else if (unit.unitName == "ATI_02")
                        {
                            artLVL2++;
                        }

                        else if (unit.unitName == "CAV_01")
                        {
                            cavLVL1++;
                        }
                        else if (unit.unitName == "CAV_02")
                        {
                            cavLVL2++;
                        }

                        else if (unit.unitName == "AAA_01")
                        {
                            aaaLVL1++;
                        }
                        else if (unit.unitName == "AAA_02")
                        {
                            aaaLVL2++;
                        }
                    }

                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_INF_LVL1", infLVL1);
                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_INF_LVL2", infLVL2);
                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_INF_LVL3", infLVL3);

                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_ART_LVL1", artLVL1);
                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_ART_LVL2", artLVL2);

                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_TANK_LVL1", tankLVL1);
                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_TANK_LVL2", tankLVL2);
                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_TANK_LVL3", tankLVL3);

                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_MOTO_LVL1", motoLVL1);
                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_MOTO_LVL2", motoLVL2);

                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_ATI_LVL1", atiLVL1);
                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_ATI_LVL2", atiLVL2);

                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_CAV_LVL1", cavLVL1);
                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_CAV_LVL2", cavLVL2);

                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_AAA_LVL1", aaaLVL1);
                    PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_AAA_LVL2", aaaLVL2);
                }
                else if (!region.hasArmy)
                {
                    PlayerPrefs.SetString($"{saveId}_REGION_{region._id}_HAS_ARMY", "FALSE");
                }
            }

            #endregion

            #endregion

            #region TechnologySave

            for (int c = 0; c < ReferencesManager.Instance.countryManager.countries.Count; c++)
            {
                CountrySettings country = ReferencesManager.Instance.countryManager.countries[c];

                for (int i = 0; i < ReferencesManager.Instance.gameSettings.technologies.Length; i++)
                {
                    TechnologyScriptableObject tech = ReferencesManager.Instance.gameSettings.technologies[i];

                    if (!tech.startReasearched)
                    {
                        if (HasTech(country, tech))
                        {
                            PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_TECH_{i}", "TRUE");
                        }
                        else
                        {
                            PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_TECH_{i}", "FALSE");
                        }
                    }
                }
            }

            #endregion

            #region Events and Date

            PlayerPrefs.SetInt($"{saveId}_DATE_0", ReferencesManager.Instance.dateManager.currentDate[0]);
            PlayerPrefs.SetInt($"{saveId}_DATE_1", ReferencesManager.Instance.dateManager.currentDate[1]);
            PlayerPrefs.SetInt($"{saveId}_DATE_2", ReferencesManager.Instance.dateManager.currentDate[2]);

            for (int i = 0; i < ReferencesManager.Instance.gameSettings.gameEvents.Count; i++)
            {
                if (ReferencesManager.Instance.gameSettings.gameEvents[i]._checked)
                {
                    PlayerPrefs.SetString($"{saveId}_EVENT_{i}", "TRUE");
                }
                else
                {
                    PlayerPrefs.SetString($"{saveId}_EVENT_{i}", "FALSE");
                }
            }

            #endregion

            #region Relations

            foreach (CountrySettings country in ReferencesManager.Instance.countryManager.countries)
            {
                string country_trades = "";
                string country_pacts = "";
                string country_unions = "";
                string country_rights = "";
                string country_wars = "";
                string country_vassals = "";

                foreach (CountrySettings otherCountry in ReferencesManager.Instance.countryManager.countries)
                {
                    Relationships.Relation relation = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(country, otherCountry);

                    if (relation.trade)
                    {
                        country_trades += $"{otherCountry.country._id};";
                    }
                    if (relation.pact)
                    {
                        country_pacts += $"{otherCountry.country._id};";
                    }
                    if (relation.union)
                    {
                        country_unions += $"{otherCountry.country._id};";
                    }
                    if (relation.right)
                    {
                        country_rights += $"{otherCountry.country._id};";
                    }
                    if (relation.war)
                    {
                        country_wars += $"{otherCountry.country._id};";
                    }
                    if (relation.vassal)
                    {
                        country_vassals += $"{otherCountry.country._id};";
                    }
                }

                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_TRADES", country_trades);
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_RIGHTS", country_rights);
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_PACTS", country_pacts);
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_UNIONS", country_unions);
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_WARS", country_wars);
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_VASSALS", country_vassals);
            }

            #endregion

            #region Guilds

            if (Guild._guilds.Count > 0)
            {
                string _guilds = "";

                foreach (Guild _guild in Guild._guilds)
                {
                    if (_guild.GetCountries().Count > 0)
                    {
                        _guilds = string.Join(";", _guild.GetCountries().Select(_ => $"{_guild._id}"));

                        string hasUnion = "FALSE";
                        string hasRights = "FALSE";
                        string hasPact = "FALSE";
                        string hasTrade = "FALSE";

                        string countries = "";
                        string offers = "";

                        if (_guild._countries.Count > 0)
                        {
                            countries = string.Join(";", _guild._countries.Select(_country =>
                                $"{_country.country.country._id}role{(int)_country.role}"
                            ));
                        }

                        if (_guild._offers.Count > 0)
                        {
                            for (int i = 0; i < _guild._offers.Count; i++)
                            {
                                Guild.Offer offer = _guild._offers[i];

                                int argCountryId = 0;
                                int starterCountryId = offer.starter.country._id;

                                string _agreeCountries = "";
                                string _disagreeCountries = "";

                                string arg_type = "";

                                int action = (int)offer.action;

                                if (offer.agree.Count > 0)
                                {
                                    _agreeCountries = string.Join(";", offer.agree.Select(country =>
                                        $"{country.country.country._id}"
                                    ));
                                }

                                if (offer.disagree.Count > 0)
                                {
                                    _disagreeCountries = string.Join(";", offer.disagree.Select(country =>
                                        $"{country.country.country._id}"
                                    ));
                                }

                                if (offer.arg is Guild.Country _guildCountryArg)
                                {
                                    argCountryId = _guildCountryArg.country.country._id;
                                    arg_type = "country";
                                }
                                else if (offer.arg is CountrySettings _countryArg)
                                {
                                    argCountryId = _countryArg.country._id;
                                    arg_type = "countrySettings";
                                }

                                string _offerData = $"{argCountryId},{starterCountryId},{_agreeCountries},{_disagreeCountries},{action},{arg_type}";

                                PlayerPrefs.SetString($"{saveId}_GUILD_{_guild._id}_OFFER_{i}", $"{_offerData}");
                            }
                        }

                        if (_guild._relations.trade)
                        {
                            hasTrade = "TRUE";
                        }
                        if (_guild._relations.union)
                        {
                            hasUnion = "TRUE";
                        }
                        if (_guild._relations.pact)
                        {
                            hasPact = "TRUE";
                        }
                        if (_guild._relations.right)
                        {
                            hasRights = "TRUE";
                        }

                        PlayerPrefs.SetString($"{saveId}_GUILD_{_guild._id}_NAME", $"{_guild._name}");
                        PlayerPrefs.SetString($"{saveId}_GUILD_{_guild._id}_IDEOLOGY", $"{_guild._ideology}");
                        PlayerPrefs.SetInt($"{saveId}_GUILD_{_guild._id}_TYPE", (int)_guild._type);
                        PlayerPrefs.SetString($"{saveId}_GUILD_{_guild._id}_TRADE", hasTrade);
                        PlayerPrefs.SetString($"{saveId}_GUILD_{_guild._id}_RIGHTS", hasRights);
                        PlayerPrefs.SetString($"{saveId}_GUILD_{_guild._id}_UNION", hasUnion);
                        PlayerPrefs.SetString($"{saveId}_GUILD_{_guild._id}_PACT", hasPact);
                        PlayerPrefs.SetString($"{saveId}_GUILD_{_guild._id}_countries", countries);
                        PlayerPrefs.SetInt($"{saveId}_GUILD_{_guild._id}_money", _guild._storage.gold);
                        PlayerPrefs.SetInt($"{saveId}_GUILD_{_guild._id}_food", _guild._storage.food);
                        PlayerPrefs.SetInt($"{saveId}_GUILD_{_guild._id}_recruits", _guild._storage.recruits);
                        PlayerPrefs.SetInt($"{saveId}_GUILD_{_guild._id}_fuel", _guild._storage.fuel);
                        PlayerPrefs.SetInt($"{saveId}_GUILD_{_guild._id}_OWNER", Guild.GetGuildOwner(_guild._id).country._id);
                        PlayerPrefs.SetString($"{saveId}_GUILD_{_guild._id}_OFFERS", $"{_guild._offers.Count}");
                    }
                }

                PlayerPrefs.SetString($"{saveId}_GUILDS", _guilds);
            }

            #endregion

            PlayerPrefs.SetInt($"{saveId}_SCENARIO_ID", ReferencesManager.Instance.regionLoader._currentScenarioId);

            localSavesIds.list.Add(saveId);

            string _saves = "";

            for (int i = 0; i < localSavesIds.list.Count; i++)
            {
                _saves += $"{localSavesIds.list[i]};";
            }

            PlayerPrefs.SetString("SAVES_IDS", _saves);
        }
        catch (Exception ex)
        {
            WarningManager.Instance.Warn($"Error on save: {ex.Message}");
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
