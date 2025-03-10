using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System;
using System.Collections;

public class CountryManager : MonoBehaviour
{
    [SerializeField] TMP_Text moneyText, moneyIncomeText, foodText, foodIncomeText, recrootsText, recrootsIncomeText, debtText, researchPointsText, researchIncomeText, fuelIncomeText, fuelText;
    [SerializeField] Image[] countryFlagImages;
    [SerializeField] TMP_Text[] countryNameTexts;

    public List<CountrySettings> countries = new List<CountrySettings>();
    public List<RegionManager> regions = new List<RegionManager>();

    public CountrySettings currentCountry;
    //[HideInInspector] public PlayerData currentPlayerData;
    [HideInInspector] public string playerNickname;
    [HideInInspector] public CountrySettings botCountry;

    public string[] parts;
    public string secondPart;
    public string value;
    public string regionValue;

    public List<int> countriesInRegionsIDs = new List<int>();

    [SerializeField] StringValue currentSaveIndex;
    [SerializeField] StringValue _currentDate;

    private void Start()
    {
        ReferencesManager.Instance.dateManager.currentDate[0] = 1;
        ReferencesManager.Instance.dateManager.currentDate[1] = 1;

        ReferencesManager.Instance.dateManager.currentDate[2] = int.Parse(_currentDate.value);

        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            ReferencesManager.Instance.gameSettings.playMod.value = false;
            ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
            ReferencesManager.Instance.gameSettings.loadGame.value = false;

            currentCountry.country._name = $"{currentCountry.country._uiName} ({playerNickname})";

            //foreach (PlayerData player in ReferencesManager.Instance.gameSettings.multiplayer.roomPlayers)
            //{
            //    player.country = countries[player.countryIndex];
            //    if (player.currentNickname == playerNickname)
            //    {
            //        currentPlayerData = player;
            //    }
            //}

            //Debug.Log($"countryManager nickname: {playerNickname}");
            //Debug.Log($"currentPlayerData: {currentPlayerData.currentNickname}");
            //Debug.Log($"localPlayer: {PhotonNetwork.LocalPlayer}");
        }
        else
        {
            if (ReferencesManager.Instance.gameSettings.loadGame.value)
            {
                if (!ReferencesManager.Instance.gameSettings.spectatorMode)
                {
                    for (int i = 0; i < countries.Count; i++)
                    {
                        if (countries[i].country._id == PlayerPrefs.GetInt("PLAYER_COUNTRY"))
                        {
                            currentCountry = countries[i];
                            currentCountry.isPlayer = true;

                            if (ReferencesManager.Instance.gameSettings.developerMode) Debug.Log(currentCountry.country);
                            if (ReferencesManager.Instance.gameSettings.developerMode) Debug.Log($"SPECTATOR MODE: {ReferencesManager.Instance.gameSettings.spectatorMode}");
                        }
                    }
                }
                else
                {
                    currentCountry = null;
                    if (ReferencesManager.Instance.gameSettings.developerMode) Debug.Log($"SPECTATOR MODE: {ReferencesManager.Instance.gameSettings.spectatorMode}");
                }
            }
            else
            {
                if (!ReferencesManager.Instance.gameSettings.spectatorMode)
                {
                    if (ReferencesManager.Instance.gameSettings._playerCountrySelected.value != "")
                    {
                        for (int i = 0; i < countries.Count; i++)
                        {
                            if (countries[i].country._id == int.Parse(ReferencesManager.Instance.gameSettings._playerCountrySelected.value))
                            {
                                currentCountry = countries[i];
                                currentCountry.isPlayer = true;
                                if (ReferencesManager.Instance.gameSettings.developerMode) Debug.Log(currentCountry.country);
                            }
                        }
                    }
                    else
                    {
                        ReferencesManager.Instance.gameSettings._playerCountrySelected.value = $"{UnityEngine.Random.Range(0, countries.Count)}";

                        for (int i = 0; i < countries.Count; i++)
                        {
                            if (countries[i].country._id == int.Parse(ReferencesManager.Instance.gameSettings._playerCountrySelected.value))
                            {
                                currentCountry = countries[i];
                                currentCountry.isPlayer = true;
                                if (ReferencesManager.Instance.gameSettings.developerMode) Debug.Log(currentCountry.country);
                            }
                        }
                    }

                    currentCountry.country._name = $"{currentCountry.country._uiName}";
                }
                else
                {
                    currentCountry = null;
                    if (ReferencesManager.Instance.gameSettings.developerMode) Debug.Log($"SPECTATOR MODE: {ReferencesManager.Instance.gameSettings.spectatorMode}");
                }
            }

            if (ReferencesManager.Instance.gameSettings.playMod.value)
            {
                int currentModID = PlayerPrefs.GetInt("CURRENT_MOD_PLAYING");

                LoadMod(PlayerPrefs.GetString($"MODIFICATION_{currentModID}"), currentModID, "saved");
            }

            else if (ReferencesManager.Instance.gameSettings.playTestingMod.value)
            {
                LoadMod(PlayerPrefs.GetString($"CURRENT_MOD_PLAYTESTING"), 0, "local");
            }

            else if (ReferencesManager.Instance.gameSettings.loadGame.value)
            {
                if (SceneManager.GetActiveScene().name != "Editor")
                {
                    Load();
                }
            }
        }

        ReferencesManager.Instance.aiManager.AICountries.Clear();

        foreach (CountrySettings country in countries)
        {
            if (!ReferencesManager.Instance.gameSettings.spectatorMode)
            {
                if (country.country._id != currentCountry.country._id && !country.isPlayer)
                {
                    ReferencesManager.Instance.aiManager.AICountries.Add(country);
                    country.gameObject.AddComponent<CountryAIManager>();
                }
            }
        }

        if (ReferencesManager.Instance.gameSettings._currentGameMode.value == "tournament")
        {
            string[] _tournamentCountries_data = ReferencesManager.Instance.gameSettings._currentTournamentCountries.value.Split(';');

            for (int i = 0; i < _tournamentCountries_data.Length; i++)
            {
                if (_tournamentCountries_data[i] != "")
                {
                    int countryId = int.Parse(_tournamentCountries_data[i]);

                    foreach (CountrySettings country in ReferencesManager.Instance.countryManager.countries)
                    {
                        if (country.country._id == countryId)
                        {
                            ReferencesManager.Instance.aiManager.DisableAI(country);
                        }
                    }
                }
            }
        }
        else if (ReferencesManager.Instance.gameSettings._currentGameMode.value == "historic")
        {
            ReferencesManager.Instance.gameSettings.allowGameEvents = true;
        }
        else if (ReferencesManager.Instance.gameSettings._currentGameMode.value == "nonhistoric")
        {
            ReferencesManager.Instance.gameSettings.allowGameEvents = false;
        }

        foreach (CountrySettings country in countries)
        {
            country.country.countryColor.a = ReferencesManager.Instance.gameSettings._regionOpacity;
        }

        if (ReferencesManager.Instance.gameSettings._DEBUG_REGIONS_IDS)
        {
            foreach (RegionManager region in regions)
            {
                region.selectedColor.r = region.currentCountry.country.countryColor.r + 0.2f;
                region.selectedColor.g = region.currentCountry.country.countryColor.g + 0.2f;
                region.selectedColor.b = region.currentCountry.country.countryColor.b + 0.2f;
                region.selectedColor.a = ReferencesManager.Instance.gameSettings._regionOpacity;

                region.hoverColor.r = region.currentCountry.country.countryColor.r + 0.3f;
                region.hoverColor.g = region.currentCountry.country.countryColor.g + 0.3f;
                region.hoverColor.b = region.currentCountry.country.countryColor.b + 0.3f;
                region.hoverColor.a = ReferencesManager.Instance.gameSettings._regionOpacity;

                if (ReferencesManager.Instance.gameSettings._DEBUG_REGIONS_IDS)
                {
                    GameObject regionID_text = Instantiate(ReferencesManager.Instance.gameSettings.debugText, region.transform);
                    regionID_text.GetComponentInChildren<TMP_Text>().text = $"{region._id}";
                    if (!region.currentCountry.myRegions.Contains(region))
                    {
                        regionID_text.GetComponentInChildren<TMP_Text>().color = ReferencesManager.Instance.gameSettings.redColor;
                    }
                }
            }
        }

        ReferencesManager.Instance.dateManager.UpdateUI();
        UpdateValuesUI();
        UpdateIncomeValuesUI();
        UpdateCountryInfo();
    }

    private void Load()
    {
        GameSettings gameSettings = ReferencesManager.Instance.gameSettings;
        RegionLoader regionLoader = ReferencesManager.Instance.regionLoader;
        DateManager dateManager = ReferencesManager.Instance.dateManager;

        int currentSaveIndex_INT = int.Parse(currentSaveIndex.value);

        regionLoader._currentScenarioId = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_SCENARIO");
        gameSettings.difficultyValue.value = PlayerPrefs.GetString($"{currentSaveIndex_INT}_DIFFICULTY");
        gameSettings._currentGameMode.value = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GAMEMODE", "historic");

        gameSettings._currentTournamentCountries.value = PlayerPrefs.GetString($"{currentSaveIndex_INT}_TOURNAMENT_COUNTRIES");

        try
        {
            gameSettings.gameEvents.Clear();

            GameSettings.ScenarioEvents scenarioEvents = null;

            foreach (var eventData in gameSettings._scenariosEvents)
            {
                if (eventData._id == regionLoader._currentScenarioId)
                {
                    scenarioEvents = eventData;
                }
            }

            if (scenarioEvents != null)
            {
                foreach (var _event in scenarioEvents._events)
                {
                    _event._checked = false;
                    gameSettings.gameEvents.Add(_event);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading events: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
        }

        try
        {
            List<string> _countries = PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRIES").Split(';').ToList();

            _countries.RemoveAll(item => item == "");

            for (int c = 0; c < _countries.Count; c++)
            {
                if (_countries[c] != "")
                {
                    bool _hasCountry = countries.Any(item => item.country._id == int.Parse(_countries[c]));

                    if (!_hasCountry)
                    {
                        foreach (CountryScriptableObject country in ReferencesManager.Instance.globalCountries)
                        {
                            if (int.Parse(_countries[c]) == country._id)
                            {
                                ReferencesManager.Instance.CreateCountry(country, "������������");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating countries: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
        }

        try
        {
            foreach (CountrySettings country in countries)
            {
                if (country.country._id == PlayerPrefs.GetInt($"{currentSaveIndex_INT}_PLAYER_COUNTRY"))
                {
                    currentCountry = country;
                    currentCountry.isPlayer = true;

                    if (currentCountry.gameObject.GetComponent<CountryAIManager>())
                    {
                        ReferencesManager.Instance.aiManager.AICountries.Remove(currentCountry);
                        Destroy(currentCountry.gameObject.GetComponent<CountryAIManager>());
                    }
                }

                string GetPlayerPrefString(string key) => PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{country.country._id}_{key}");

                int GetPlayerPrefInt(string key) => PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{country.country._id}_{key}");

                float GetPlayerPrefFloat(string key) => PlayerPrefs.GetFloat($"{currentSaveIndex_INT}_COUNTRY_{country.country._id}_{key}");

                country.ideology = GetPlayerPrefString("IDEOLOGY");
                country.money = GetPlayerPrefInt("MONEY");
                country.food = GetPlayerPrefInt("FOOD");
                country.recruits = GetPlayerPrefInt("RECROOTS");
                country.recruitsLimit = GetPlayerPrefInt("RECROOTS_LIMIT");
                country.researchPoints = GetPlayerPrefInt("RESEARCH_POINTS");
                country.fuel = GetPlayerPrefFloat("fuel");

                country.mobilasing = GetPlayerPrefString("MOBILASING") == "TRUE";
                country.deMobilasing = GetPlayerPrefString("DEMOBILASING") == "TRUE";

                country.startMoneyIncome = GetPlayerPrefInt("startMoneyIncome");
                country.moneyNaturalIncome = GetPlayerPrefInt("moneyNaturalIncome");
                country.startFoodIncome = GetPlayerPrefInt("startFoodIncome");
                country.foodNaturalIncome = GetPlayerPrefInt("foodNaturalIncome");
                country.recruitsIncome = GetPlayerPrefInt("recrootsIncome");
                country.researchPointsIncome = GetPlayerPrefInt("researchPointsIncome");
                country.civFactories = GetPlayerPrefInt("civFactories");
                country.farms = GetPlayerPrefInt("farms");
                country.chemicalFarms = GetPlayerPrefInt("cheFarms");
                country.researchLabs = GetPlayerPrefInt("resLabs");

                country.UpdateCountryFlagOnIdeology(country.ideology);

                for (int techIndex = 0; techIndex < gameSettings.technologies.Length; techIndex++)
                {
                    if (PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{country.country._id}_TECH_{techIndex}") == "TRUE")
                    {
                        country.countryTechnologies.Add(gameSettings.technologies[techIndex]);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading countries: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
        }

        WarningManager.Instance.Warn($"Countries and techs loaded");

        foreach (RegionManager region in regions)
        {
            foreach (CountrySettings country in countries)
            {
                if (country.country._id == PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_CURRENTCOUNTRY_ID"))
                {
                    if (region.currentCountry != country)
                    {
                        ReferencesManager.Instance.AnnexRegion(region, country);
                    }
                }
            }

            int infrastructure_Amount = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_INFRASTRUCTURES");
            int civFactory_Amount = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_CIV_FABRICS");
            int farms_Amount = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_FARMS");
            int cheFarms = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_CHEFARMS");
            int resLabs = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_RESLABS");
            int dockyards = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_DOCKYARDS");
            int forts = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_FORTS");
            int marinebases = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_MARINEBASES");
            int airbases = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_AIRBASES");

            region.buildings.Clear();
            region.infrastructure_Amount = infrastructure_Amount;
            region.fortifications_Amount = forts;
            region._marineBaseLevel = marinebases;
            region._airBaseLevel = airbases;

            if (airbases > 0)
            {
                region.gameObject.AddComponent<Aviation_Storage>();

                Aviation_Storage airStorage = region.gameObject.GetComponent<Aviation_Storage>();

                for (int p = 0; p < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_PLANES"); p++)
                {
                    Aviation_ScriptableObj plane = null;

                    for (int f = 0; f < ReferencesManager.Instance.gameSettings._planes.Length; f++)
                    {
                        Aviation_ScriptableObj _plane = ReferencesManager.Instance.gameSettings._planes[f];

                        if (_plane.tag == PlayerPrefs.GetString($"{currentSaveIndex_INT}_REGION_{region._id}_PLANE_{p}_TYPE"))
                        {
                            plane = _plane;
                            break;
                        }
                    }

                    CountrySettings owner = FindCountryByID(PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_PLANE_{p}_OWNER"));

                    Aviation_Cell aviation_Cell = new Aviation_Cell(plane, owner)
                    {
                        hp = PlayerPrefs.GetFloat($"{currentSaveIndex_INT}_REGION_{region._id}_PLANE_{p}_HP"),
                        fuel = PlayerPrefs.GetFloat($"{currentSaveIndex_INT}_REGION_{region._id}_PLANE_{p}_FUEL")
                    };

                    airStorage.planes.Add(aviation_Cell);
                }
            }

            for (int f = 0; f < civFactory_Amount; f++)
            {
                region.buildings.Add(gameSettings.fabric);
                region.currentCountry.civFactories++;
            }

            for (int f = 0; f < farms_Amount; f++)
            {
                region.buildings.Add(gameSettings.farm);
                region.currentCountry.farms++;
            }

            for (int f = 0; f < cheFarms; f++)
            {
                region.buildings.Add(gameSettings.chefarm);
                region.currentCountry.chemicalFarms++;
            }

            for (int f = 0; f < resLabs; f++)
            {
                region.buildings.Add(gameSettings.researchLab);
                region.currentCountry.researchLabs++;
            }

            for (int f = 0; f < dockyards; f++)
            {
                region.buildings.Add(gameSettings.dockyard);
                region.currentCountry.dockyards++;
            }

            if (PlayerPrefs.GetString($"{currentSaveIndex_INT}_REGION_{region._id}_HAS_ARMY") == "TRUE")
            {
                region.hasArmy = true;
                ReferencesManager.Instance.army.CreateUnit_NoCheck(region);

                UnitMovement division = region.GetDivision(region);

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_INF_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.soldierLVL1, division);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_INF_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.soldierLVL2, division);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_INF_LVL3"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.soldierLVL3, division);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_ART_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.artileryLVL1, division);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_ART_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.artileryLVL2, division);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_AAA_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.antiairLVL1, division);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_AAA_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.antiairLVL2, division);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_TANK_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.tankLVL1, division);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_TANK_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.tankLVL2, division);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_TANK_LVL3"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.tankLVL3, division);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_MOTO_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.motoLVL1, division);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_MOTO_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.motoLVL2, division);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_ATI_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.antitankLVL1, division);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_ATI_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.antitankLVL2, division);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_CAV_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.cavLVL1, division);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_CAV_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_Save(ReferencesManager.Instance.gameSettings.cavLVL2, division);
                }
            }
        }

        WarningManager.Instance.Warn($"Loaded regions");

        try
        {
            StartCoroutine(LoadDiplomatyRelations(currentSaveIndex_INT));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading deiplomaty: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
        }

        try
        {
            dateManager.currentDate[0] = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_DATE_0");
            dateManager.currentDate[1] = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_DATE_1");
            dateManager.currentDate[2] = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_DATE_2");
            dateManager.UpdateUI();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading date manager: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
        }

        try
        {
            for (int i = 0; i < gameSettings.gameEvents.Count; i++)
            {
                if (PlayerPrefs.GetString($"{currentSaveIndex_INT}_EVENT_{i}") == "TRUE")
                {
                    gameSettings.gameEvents[i]._checked = true;
                }
                else
                {
                    gameSettings.gameEvents[i]._checked = false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading game events: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
        }

        #region Guilds

        try
        {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILDS")))
            {
                string[] _guilds_data_items = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILDS").Split(';');

                foreach (string guild_id in _guilds_data_items)
                {
                    CountrySettings _guildOwner = FindCountryByID(PlayerPrefs.GetInt($"{currentSaveIndex_INT}_GUILD_{guild_id}_OWNER"));

                    int guild_id_int = int.Parse(guild_id);

                    string[] _countries_in_guild = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_countries").Split(';');
                    int _offers_in_guild = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_GUILD_{guild_id}_OFFERS");

                    string _guildName = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_NAME");
                    string _guildIdeology = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_IDEOLOGY");
                    int _guildType = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_GUILD_{guild_id}_TYPE");
                    string _guildTrade = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_TRADE");
                    string _guildRights = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_RIGHTS");
                    string _guildUnion = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_UNION");
                    string _guildPact = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_PACT");

                    int _guildMoney = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_GUILD_{guild_id}_money");
                    int _guildFood = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_GUILD_{guild_id}_food");
                    int _guildRecruits = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_GUILD_{guild_id}_recruits");
                    int _guildFuel = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_GUILD_{guild_id}_fuel");


                    string hasUnion = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_UNION");
                    string hasRights = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_RIGHTS");
                    string hasPact = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_PACT");
                    string hasTrade = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_TRADE");

                    Guild.Relations _relations = new();

                    if (hasUnion == "TRUE")
                    {
                        _relations.union = true;
                    }
                    if (hasRights == "TRUE")
                    {
                        _relations.right = true;
                    }
                    if (hasPact == "TRUE")
                    {
                        _relations.pact = true;
                    }
                    if (hasTrade == "TRUE")
                    {
                        _relations.trade = true;
                    }

                    Guild.GuildType _guildType_Object = (Guild.GuildType)_guildType;

                    Guild guild = new Guild(_guildName, _guildOwner.country.countryFlag, _guildOwner, _guildType_Object, _relations, _guildIdeology);

                    guild._storage.gold = _guildMoney;
                    guild._storage.food = _guildFood;
                    guild._storage.recruits = _guildRecruits;
                    guild._storage.fuel = _guildFuel;

                    foreach (string _countryData in _countries_in_guild)
                    {
                        string[] _countryInfo = _countryData.Split("role");
                        int _countryId = int.Parse(_countryInfo[0]);
                        int _countryRole = int.Parse(_countryInfo[1]);

                        Guild.Country _country = new Guild.Country();

                        _country.country = FindCountryByID(_countryId);
                        _country.role = (Guild.Role)_countryRole;

                        guild._countries.Add(_country);

                        CountrySettings _countrySettings = FindCountryByID(_countryId);
                        _countrySettings.guilds.Add(guild);
                    }

                    for (int i = 0; i < _offers_in_guild; i++)
                    {
                        string[] _offerData = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GUILD_{guild_id}_OFFER_{i}").Split(',');

                        int _arg_countryId = int.Parse(_offerData[0]);
                        int _starter_CountryId = int.Parse(_offerData[1]);
                        string[] _agreeCountries = _offerData[2].Split(';');
                        string[] _disagreeCountries = _offerData[3].Split(';');

                        string _argType = _offerData[5];

                        CountrySettings _starterCountry = FindCountryByID(_starter_CountryId);

                        Guild.Action _action = (Guild.Action)int.Parse(_offerData[4]);

                        List<Guild.Country> _agreeCountries_Objects = new();
                        List<Guild.Country> _disagreeCountries_Objects = new();

                        foreach (string _agreeCountry_Id in _agreeCountries)
                        {
                            Guild.Country _agreeCountry = guild._countries.Find(item => item.country.country._id == int.Parse(_agreeCountry_Id));

                            _agreeCountries_Objects.Add(_agreeCountry);
                        }

                        foreach (string _disagreeCountry_Id in _disagreeCountries)
                        {
                            Guild.Country _disagreeCountry = guild._countries.Find(item => item.country.country._id == int.Parse(_disagreeCountry_Id));

                            _disagreeCountries_Objects.Add(_disagreeCountry);
                        }

                        object arg = new object();

                        if (_argType == "country")
                        {
                            arg = guild._countries.Find(x => x.country.country._id == _arg_countryId);
                        }
                        else if (_argType == "countrySettings")
                        {
                            arg = FindCountryByID(_arg_countryId);
                        }

                        Guild.Offer _offer = new Guild.Offer(guild, _starterCountry, arg, _action);

                        _offer.agree = _agreeCountries_Objects;
                        _offer.disagree = _disagreeCountries_Objects;

                        guild._offers.Add(_offer);
                    }

                    Guild._guilds.Add(guild);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading guilds: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
        }

        #endregion

        foreach (CountrySettings countrySettings in countries)
        {
            countrySettings.stability.buffs.Clear();
        }

        #region Stability

        // TODO

        #endregion
    }

    private IEnumerator LoadDiplomatyRelations(int currentSaveIndex_INT)
    {
        foreach (CountrySettings country in countries)
        {
            var relationshipTypes = new[] { "TRADES", "WARS", "VASSALS", "PACTS", "RIGHTS", "UNIONS" };
            var countryRelations = new Dictionary<string, List<string>>();

            foreach (var type in relationshipTypes)
            {
                var key = $"{currentSaveIndex_INT}_COUNTRY_{country.country._id}_{type}";
                var relationList = PlayerPrefs.GetString(key).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                countryRelations[type] = relationList;
            }

            var diplomaty = ReferencesManager.Instance.diplomatyUI;
            diplomaty.senderId = country.country._id;
            diplomaty.sender = country;

            var offerTypes = new Dictionary<string, string>
            {
                { "TRADES", "��������" },
                { "WARS", "�������� �����" },
                { "VASSALS", "������� ��������" },
                { "PACTS", "���� � �����������" },
                { "RIGHTS", "����� � ������� �����" },
                { "UNIONS", "����" }
            };

            foreach (var relationType in relationshipTypes)
            {
                if (countryRelations.TryGetValue(relationType, out var relationList))
                {
                    foreach (var _country in relationList)
                    {
                        if (int.TryParse(_country, out int countryID))
                        {
                            diplomaty.AISendOffer(offerTypes[relationType], country, FindCountryByID(countryID), false);
                        }
                    }
                }
            }

            yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    public CountrySettings FindCountryByID(int id)
    {
        return FindCountryByID_CustomList(id, countries);
    }

    public CountrySettings FindCountryByID_CustomList(int id, List<CountrySettings> _countries)
    {
        CountrySettings _foundedCountry = null;

        foreach (CountrySettings country in _countries)
        {
            if (country.country._id == id)
            {
                _foundedCountry = country;
                break;
            }
        }

        return _foundedCountry;
    }

    private void LoadMod(string _modname, int modID, string pathType)
    {
        int currentModID = modID;

        string modName = _modname;
        string modData = "";

        string path = "";

        if (pathType == "local")
        {
            path = Path.Combine(Application.persistentDataPath, "localMods", $"{modName}", $"{modName}.AEMod");
        }
        else if (pathType == "saved")
        {
            path = Path.Combine(Application.persistentDataPath, "savedMods", $"{modName}", $"{modName}.AEMod");
        }

        StreamReader reader = new StreamReader(path);

        modData = reader.ReadToEnd();

        reader.Close();

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
        }
        catch (System.Exception)
        {
            if (ReferencesManager.Instance.gameSettings.developerMode)
            {
                Debug.LogError($"ERROR: Mod loader error in value parser (CountryManager.cs)");
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

        string year = "1936";

        try
        {
            string _line = mainModDataLines[2];
            string[] _parts = _line.Split('[');

            string valuePart = _parts[1];

            year = valuePart.Remove(valuePart.Length - 1);
        }
        catch (Exception) { }

        _currentDate.value = year;

        for (int i = 3; i < mainModDataLines.Length; i++)
        {
            string _line = mainModDataLines[i];
            if (!string.IsNullOrEmpty(_line))
            {
                value = GetValue(_line);

                bool _hasCountry = countries.Any(item => item.country._id == int.Parse(value));

                if (!_hasCountry)
                {
                    foreach (CountryScriptableObject countryScriptableObject in ReferencesManager.Instance.globalCountries)
                    {
                        if (countryScriptableObject._id == int.Parse(value))
                        {
                            ReferencesManager.Instance.CreateCountry(countryScriptableObject, "������������");
                        }
                    }
                }
            }
        }

        for (int r = 0; r < regionsDataLines.Length; r++)
        {
            try
            {
                string _line = regionsDataLines[r];
                int value = int.Parse(GetValue(_line));

                countriesInRegionsIDs.Add(value);
            }
            catch (System.Exception) { }
        }

        List<RegionLoader.ModRegionData> regionModIDs = new List<RegionLoader.ModRegionData>();

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

                    RegionLoader.ModRegionData modRegionData = new RegionLoader.ModRegionData();

                    modRegionData.countryID = regionCountryID;
                    modRegionData.regionId = regValue;

                    regionModIDs.Add(modRegionData);
                }
            }
            catch (Exception) { }
        }

        foreach (RegionLoader.ModRegionData regValue in regionModIDs)
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

        bool hasCountry = countries.Any(item => item.country._id == int.Parse(ReferencesManager.Instance.gameSettings._playerCountrySelected.value));

        for (int i = 0; i < countries.Count; i++)
        {
            if (hasCountry)
            {
                if (countries[i].country._id == int.Parse(ReferencesManager.Instance.gameSettings._playerCountrySelected.value))
                {
                    currentCountry = countries[i];
                    currentCountry.isPlayer = true;
                }
            }
        }

        #region countriesSettings

        if (!IsNullOrEmpty(countriesDataLines))
        {
            for (int i = 0; i < countriesDataLines.Length; i++)
            {
                if (!string.IsNullOrEmpty(countriesDataLines[i]))
                {
                    try
                    {
                        string new_lineData = GetValue(countriesDataLines[i]);

                        if (!string.IsNullOrEmpty(new_lineData))
                        {
                            string[] values = new_lineData.Split('|');

                            int countryID = int.Parse(values[0]);
                            int money = int.Parse(values[1]);
                            int food = int.Parse(values[2]);
                            int recroots = int.Parse(values[3]);

                            string ideology = values[4];

                            foreach (CountrySettings country in countries)
                            {
                                if (country.country._id == countryID)
                                {
                                    country.money = money;
                                    country.food = food;
                                    country.recruits = recroots;

                                    country.ideology = ideology;

                                    country.UpdateCountryGraphics(country.ideology);
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        #endregion

        #region events

        List<int> eventsIDs = new List<int>();

        if (!IsNullOrEmpty(eventsIDsDataLines))
        {
            foreach (string eventData in eventsIDsDataLines)
            {
                if (!string.IsNullOrEmpty(eventData) && !string.IsNullOrWhiteSpace(eventData))
                {
                    eventsIDs.Add(int.Parse(eventData));
                }
            }
        }

        foreach (int eventId in eventsIDs)
        {
            string eventPath = "";

            if (pathType == "local")
            {
                eventPath = Path.Combine(Application.persistentDataPath, "localMods", $"{modName}", "events", $"{eventId}", $"{eventId}.AEEvent");
            }
            else if (pathType == "saved")
            {
                eventPath = Path.Combine(Application.persistentDataPath, "savedMods", $"{modName}", "events", $"{eventId}", $"{eventId}.AEEvent");
            }

            string eventData = "";

            EventScriptableObject _event = new EventScriptableObject();

            StreamReader _reader = new StreamReader(eventPath);
            eventData = _reader.ReadToEnd();

            string[] eventLines = eventData.Split(';');

            int _localEvent_ID = 0;
            string _localEvent_Name = "";
            string _localEvent_Description = "";
            string _localEvent_Date = "";
            string _localEvent_silentEvent = "";

            List<int> _localEvent_receivers = new List<int>();
            List<int> _localEvent_receiversBlacklist = new List<int>();

            bool _localEvent_silentEventBoolean = false;
            string[] _localEvent_conditions;
            string _localEvent_imagePath = "";


            _localEvent_ID = int.Parse(GetValue(eventLines[0]));
            _localEvent_Name = GetValue(eventLines[1]);
            _localEvent_Description = GetValue(eventLines[2]);
            _localEvent_Date = GetValue(eventLines[3]);
            _localEvent_silentEvent = GetValue(eventLines[4]);

            string[] receiversString = GetValue(eventLines[5]).Split('-');
            string[] receiversBlacklistString = GetValue(eventLines[6]).Split('-');

            try
            {
                for (int i = 0; i < receiversString.Length; i++)
                {
                    if (!string.IsNullOrEmpty(receiversString[i]))
                    {
                        _localEvent_receivers.Add(int.Parse(receiversString[i]));
                    }
                }
            }
            catch (Exception) { }

            try
            {
                for (int i = 0; i < receiversBlacklistString.Length; i++)
                {
                    if (!string.IsNullOrEmpty(receiversBlacklistString[i]))
                    {
                        _localEvent_receiversBlacklist.Add(int.Parse(receiversBlacklistString[i]));
                    }
                }
            }
            catch (Exception) { }

            _localEvent_conditions = GetValue(eventLines[7]).Split('@');

            if (pathType == "local")
            {
                _localEvent_imagePath = $"{Application.persistentDataPath}/localMods/{modName}/events/{_localEvent_ID}/{_localEvent_ID}.jpg";
            }
            else if (pathType == "saved")
            {
                _localEvent_imagePath = $"{Application.persistentDataPath}/savedMods/{modName}/events/{_localEvent_ID}/{_localEvent_ID}.jpg";
            }

            if (_localEvent_silentEvent == "0") _localEvent_silentEventBoolean = false;
            else if (_localEvent_silentEvent == "1") _localEvent_silentEventBoolean = true;

            string[] conditionsData = eventData.Split("conditions:");
            string[] conditionsLines = conditionsData[1].Split(';');

            for (int c = 0; c < conditionsLines.Length; c++)
            {
                string[] conditions = conditionsLines[c].Split('@');

                for (int cd = 0; cd < conditions.Length; cd++)
                {
                    if (!string.IsNullOrEmpty(conditions[cd]))
                    {
                        conditions[cd] = conditions[cd].Replace('-', ';');
                    }
                }

                conditions = conditions.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                _event.conditions = conditions.ToList();
            }

            #region EventButton

            string[] buttonsData = eventData.Split("buttons:");
            string[] buttonsLines = buttonsData[1].Split(';');

            for (int i = 0; i < buttonsLines.Length - 1; i++)
            {
                EventScriptableObject.EventButton newButton = new EventScriptableObject.EventButton();
                string[] buttonData = buttonsLines[i].Split('|');

                newButton.name = buttonData[0];
                newButton.nameEN = buttonData[0];

                if (buttonData[2] == "0") newButton.rejectUltimatum = false;
                else if (buttonData[2] == "1") newButton.rejectUltimatum = true;

                string[] actions = buttonData[1].Split('@');

                for (int a = 0; a < actions.Length; a++)
                {
                    if (!string.IsNullOrEmpty(actions[a]))
                    {
                        actions[a] = actions[a].Replace('-', ';');
                    }
                }

                actions = actions.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                newButton.actions = actions.ToList();

                _event.buttons.Add(newButton);
            }

            #endregion

            reader.Close();

            _event.name = _localEvent_Name;

            _event.id = _localEvent_ID;
            _event._name = _localEvent_Name;
            _event._nameEN = _localEvent_Name;
            _event.description = _localEvent_Description;
            _event.descriptionEN = _localEvent_Description;
            _event.date = _localEvent_Date;
            _event.receivers = _localEvent_receivers;
            _event.exceptionsReceivers = _localEvent_receiversBlacklist;

            if (File.Exists(_localEvent_imagePath))
            {
                Texture2D finalTexture = NativeGallery.LoadImageAtPath(_localEvent_imagePath);

                _event.image = Sprite.Create(finalTexture, new Rect(0, 0, finalTexture.width, finalTexture.height), Vector2.zero);
            }

            _event.silentEvent = _localEvent_silentEventBoolean;
            _event.conditions = _localEvent_conditions.ToList();

            ReferencesManager.Instance.gameSettings.gameEvents.Add(_event);
        }

        #endregion
    }

    public bool IsNullOrEmpty(Array array)
    {
        return (array == null || array.Length == 0);
    }

    public string GetValue(string line)
    {
        string value = "";

        try
        {
            string[] parts = line.Split('=');

            string part = parts[1];
            value = part;
        }
        catch (Exception)
        {
        }

        return value;
    }

    public void UpdateValuesUI()
    {
        if (!ReferencesManager.Instance.gameSettings.spectatorMode)
        {
            if (moneyText != null)
            {
                if (currentCountry.money >= 30000)
                {
                    moneyText.text = ReferencesManager.Instance.GoodNumberString((int)currentCountry.money);
                }
                else
                {
                    moneyText.text = currentCountry.money.ToString();
                }

            }
            if (foodText != null)
            {
                if (currentCountry.food >= 30000)
                {
                    foodText.text = ReferencesManager.Instance.GoodNumberString((int)currentCountry.food);
                }
                else
                {
                    foodText.text = currentCountry.food.ToString();
                }
            }
            if (recrootsText != null)
            {
                if (currentCountry.recruits >= 10000)
                {
                    recrootsText.text = ReferencesManager.Instance.GoodNumberString((int)currentCountry.recruits);
                }
                else
                {
                    recrootsText.text = currentCountry.recruits.ToString();
                }
            }
            if (debtText != null)
            {
                debtText.text = ReferencesManager.Instance.countryInfo.currentCredit.ToString();
            }
            if (researchPointsText != null)
            {
                researchPointsText.text = currentCountry.researchPoints.ToString();
            }
            if (fuelText != null)
            {
                if (currentCountry.fuel >= 10000)
                {
                    fuelText.text = ReferencesManager.Instance.GoodNumberString((int)currentCountry.fuel);
                }
                else
                {
                    fuelText.text = currentCountry.fuel.ToString();
                }
            }
        }
    }

    public void UpdateIncomeValuesUI()
    {
        if (!ReferencesManager.Instance.gameSettings.spectatorMode)
        {
            int moneyIncomeFull = currentCountry.moneyIncomeUI;
            if (currentCountry.moneyIncomeUI >= 0)
            {
                moneyIncomeText.text = "+" + moneyIncomeFull.ToString();
                moneyIncomeText.color = ReferencesManager.Instance.gameSettings.greenColor;
            }
            else
            {
                moneyIncomeText.text = moneyIncomeFull.ToString();
                moneyIncomeText.color = ReferencesManager.Instance.gameSettings.redColor;
            }

            if (currentCountry.foodIncomeUI >= 0)
            {
                foodIncomeText.text = "+" + currentCountry.foodIncomeUI.ToString();
                foodIncomeText.color = ReferencesManager.Instance.gameSettings.greenColor;
            }
            else
            {
                foodIncomeText.text = currentCountry.foodIncomeUI.ToString();
                foodIncomeText.color = ReferencesManager.Instance.gameSettings.redColor;
            }

            if (currentCountry.recruitsIncome >= 0)
            {
                recrootsIncomeText.text = "+" + currentCountry.recruitsIncome.ToString();
                recrootsIncomeText.color = ReferencesManager.Instance.gameSettings.greenColor;
            }
            else
            {
                recrootsIncomeText.text = currentCountry.recruitsIncome.ToString();
                recrootsIncomeText.color = ReferencesManager.Instance.gameSettings.redColor;
            }

            if (currentCountry.researchPointsIncome >= 0)
            {
                researchIncomeText.text = "+" + currentCountry.researchPointsIncome.ToString();
                researchIncomeText.color = ReferencesManager.Instance.gameSettings.greenColor;
            }
            else
            {
                researchIncomeText.text = currentCountry.researchPointsIncome.ToString();
                researchIncomeText.color = ReferencesManager.Instance.gameSettings.redColor;
            }

            if (currentCountry.fuelIncome >= 0)
            {
                fuelIncomeText.text = "+" + currentCountry.fuelIncome.ToString();
                fuelIncomeText.color = ReferencesManager.Instance.gameSettings.greenColor;
            }
            else
            {
                fuelIncomeText.text = currentCountry.fuelIncome.ToString();
                fuelIncomeText.color = ReferencesManager.Instance.gameSettings.redColor;
            }
        }
    }

    public void UpdateCountryInfo()
    {
        if (!ReferencesManager.Instance.gameSettings.spectatorMode)
        {
            foreach (Image countryFlagImage in countryFlagImages)
            {
                countryFlagImage.sprite = currentCountry.country.countryFlag;
            }

            foreach (TMP_Text countryText in countryNameTexts)
            {
                countryText.text = ReferencesManager.Instance.languageManager.GetTranslation(currentCountry.country._nameEN);
            }
        }
    }

    public void UpdateCountryGraphics(string ideology)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.M_UpdateCountryGraphics(currentCountry.country._id, currentCountry.ideology);
        }
        else
        {
            if (ideology == "�������������")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[1];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[1];
            }
            else if (ideology == "����������")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[1];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[1];
            }
            else if (ideology == "��������")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[2];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[2];
            }
            else if (ideology == "������")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[4];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[4];
            }
            else if (ideology == "���������")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[5];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[5];
            }

            // ����� ����
            foreach (RegionManager region in regions)
            {
                region.selectedColor.r = region.currentCountry.country.countryColor.r + 0.1f;
                region.selectedColor.g = region.currentCountry.country.countryColor.g + 0.1f;
                region.selectedColor.b = region.currentCountry.country.countryColor.b + 0.1f;
            }

            UpdateRegionsColor();
        }
    }

    public void UpdateRegionsColor()
    {
        foreach (RegionManager region in regions)
        {
            Color provinceColor = new Color(
                region.currentCountry.country.countryColor.r,
                region.currentCountry.country.countryColor.g,
                region.currentCountry.country.countryColor.b,
                ReferencesManager.Instance.gameSettings._regionOpacity);

            region.GetComponent<SpriteRenderer>().color = provinceColor;
        }
    }
}
