using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System;

public class CountryManager : MonoBehaviour
{
    [SerializeField] TMP_Text moneyText, moneyIncomeText, foodText, foodIncomeText, recrootsText, recrootsIncomeText, debtText, researchPointsText, researchIncomeText, fuelIncomeText, fuelText;
    [SerializeField] Image[] countryFlagImages;
    [SerializeField] TMP_Text[] countryNameTexts;

    public List<CountrySettings> countries = new List<CountrySettings>();
    public List<RegionManager> regions = new List<RegionManager>();

    public CountrySettings currentCountry;
    [HideInInspector] public PlayerData currentPlayerData;
    [HideInInspector] public string playerNickname;

    public string[] parts;
    public string secondPart;
    public string value;
    public string regionValue;

    public List<int> countriesInRegionsIDs = new List<int>();

    [SerializeField] StringValue currentSaveIndex;
    [SerializeField] StringValue _currentDate;

    private void Start()
    {
        ReferencesManager.Instance.gameSettings.allowGameEvents = true;

        ReferencesManager.Instance.dateManager.currentDate[0] = 1;
        ReferencesManager.Instance.dateManager.currentDate[1] = 1;

        ReferencesManager.Instance.dateManager.currentDate[2] = int.Parse(_currentDate.value);
        ReferencesManager.Instance.dateManager.UpdateUI();

        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            ReferencesManager.Instance.gameSettings.playMod.value = false;
            ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
            ReferencesManager.Instance.gameSettings.loadGame.value = false;

            currentCountry.country._name = $"{currentCountry.country._uiName} ({playerNickname})";

            foreach (PlayerData player in ReferencesManager.Instance.gameSettings.multiplayer.roomPlayers)
            {
                player.country = countries[player.countryIndex];
                if (player.currentNickname == playerNickname)
                {
                    currentPlayerData = player;
                }
            }

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

            if (ReferencesManager.Instance.gameSettings.loadGame.value)
            {
                if (SceneManager.GetActiveScene().name != "Editor")
                {
                    Load();
                }
            }

            if (ReferencesManager.Instance.gameSettings.playMod.value)
            {
                int currentModID = PlayerPrefs.GetInt("CURRENT_MOD_PLAYING");

                LoadMod(PlayerPrefs.GetString($"MODIFICATION_{currentModID}"), currentModID, "saved");
            }

            if (ReferencesManager.Instance.gameSettings.playTestingMod.value)
            {
                LoadMod(PlayerPrefs.GetString($"CURRENT_MOD_PLAYTESTING"), 0, "local");
            }
        }


        ReferencesManager.Instance.aiManager.AICountries.Clear();

        foreach (CountrySettings country in countries)
        {
            if (!ReferencesManager.Instance.gameSettings.spectatorMode)
            {
                if (country.country != currentCountry.country)
                {
                    ReferencesManager.Instance.aiManager.AICountries.Add(country);
                    country.gameObject.AddComponent<CountryAIManager>();
                }
            }
            else
            {
                country.gameObject.AddComponent<CountryAIManager>();
            }
            country.UpdateCapitulation();
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

        UpdateValuesUI();
        UpdateIncomeValuesUI();
        UpdateCountryInfo();

        foreach (CountrySettings country in countries)
        {
            country.country.countryColor.a = ReferencesManager.Instance.gameSettings._regionOpacity;
        }

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

    private void Load()
    {
        int currentSaveIndex_INT = int.Parse(currentSaveIndex.value);

        ReferencesManager.Instance.gameSettings.difficultyValue.value = PlayerPrefs.GetString($"{currentSaveIndex_INT}_DIFFICULTY");
        ReferencesManager.Instance.gameSettings._currentGameMode.value = PlayerPrefs.GetString($"{currentSaveIndex_INT}_GAMEMODE");

        ReferencesManager.Instance.gameSettings._currentTournamentCountries.value = PlayerPrefs.GetString($"{currentSaveIndex_INT}_TOURNAMENT_COUNTRIES");

        try
        {
            string[] _countries = PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRIES").Split(';');

            for (int c = 0; c < _countries.Length; c++)
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
        catch (Exception) { }

        foreach (CountrySettings country in countries)
        {
            if (country.country._id == PlayerPrefs.GetInt($"{currentSaveIndex_INT}_PLAYER_COUNTRY"))
            {
                currentCountry = country;
                currentCountry.isPlayer = true;
            }
        }

        for (int i = 0; i < countries.Count; i++)
        {
            countries[i].ideology = PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_IDEOLOGY");
            countries[i].money = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_MONEY");
            countries[i].food = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_FOOD");
            countries[i].recroots = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_RECROOTS");
            countries[i].recruitsLimit = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_RECROOTS_LIMIT");
            countries[i].researchPoints = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_RESEARCH_POINTS");
            countries[i].fuel = PlayerPrefs.GetFloat($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_fuel");

            bool isMob = false;
            bool isDeMob = false;

            if (PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_MOBILASING") == "TRUE")
            {
                isMob = true;
            }

            if (PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_DEMOBILASING") == "TRUE")
            {
                isDeMob = true;
            }

            countries[i].mobilasing = isMob;
            countries[i].deMobilasing = isDeMob;

            countries[i].startMoneyIncome = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_startMoneyIncome");
            countries[i].moneyNaturalIncome = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_moneyNaturalIncome");

            countries[i].startFoodIncome = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_startFoodIncome");
            countries[i].foodNaturalIncome = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_foodNaturalIncome");

            countries[i].recrootsIncome = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_recrootsIncome");
            countries[i].researchPointsIncome = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_researchPointsIncome");

            countries[i].civFactories = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_civFactories");
            countries[i].farms = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_farms");
            countries[i].chemicalFarms = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_cheFarms");
            countries[i].researchLabs = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_resLabs");

            countries[i].UpdateCountryGraphics(countries[i].ideology);

            List<string> _country_trades = new List<string>();
            List<string> _country_wars = new List<string>();
            List<string> _country_vassals = new List<string>();
            List<string> _country_pacts = new List<string>();
            List<string> _country_rights = new List<string>();
            List<string> _country_unions = new List<string>();

            _country_trades = PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_TRADES").Split(';').ToList();
            _country_wars = PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_WARS").Split(';').ToList();
            _country_vassals = PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_VASSALS").Split(';').ToList();
            _country_pacts = PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_PACTS").Split(';').ToList();
            _country_rights = PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_RIGHTS").Split(';').ToList();
            _country_unions = PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_UNIONS").Split(';').ToList();

            _country_trades.RemoveAll(item => item == "");
            _country_wars.RemoveAll(item => item == "");
            _country_vassals.RemoveAll(item => item == "");
            _country_pacts.RemoveAll(item => item == "");
            _country_rights.RemoveAll(item => item == "");
            _country_unions.RemoveAll(item => item == "");

            ReferencesManager.Instance.diplomatyUI.senderId = countries[i].country._id;
            ReferencesManager.Instance.diplomatyUI.sender = countries[i];

            foreach (string _country in _country_trades)
            {
                int countryID = int.Parse(_country.ToString());

                ReferencesManager.Instance.diplomatyUI.AISendOffer("��������", countries[i], FindCountryByID(countryID), false);
            }

            foreach (string _country in _country_wars)
            {
                int countryID = int.Parse(_country.ToString());

                ReferencesManager.Instance.diplomatyUI.AISendOffer("�������� �����", countries[i], FindCountryByID(countryID), false);
            }

            foreach (string _country in _country_vassals)
            {
                int countryID = int.Parse(_country.ToString());

                ReferencesManager.Instance.diplomatyUI.AISendOffer("������� ��������", countries[i], FindCountryByID(countryID), false);
            }

            foreach (string _country in _country_pacts)
            {
                int countryID = int.Parse(_country.ToString());

                ReferencesManager.Instance.diplomatyUI.AISendOffer("���� � �����������", countries[i], FindCountryByID(countryID), false);
            }

            foreach (string _country in _country_rights)
            {
                int countryID = int.Parse(_country.ToString());

                ReferencesManager.Instance.diplomatyUI.AISendOffer("����� � ������� �����", countries[i], FindCountryByID(countryID), false);
            }

            foreach (string _country in _country_unions)
            {
                int countryID = int.Parse(_country.ToString());

                ReferencesManager.Instance.diplomatyUI.AISendOffer("����", countries[i], FindCountryByID(countryID), false);
            }
        }

        for (int i = 0; i < regions.Count; i++)
        {
            RegionManager region = regions[i];

            foreach (CountrySettings country in countries)
            {
                if (country.country._id == PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_CURRENTCOUNTRY_ID"))
                {
                    ReferencesManager.Instance.AnnexRegion(region, country);
                    
                    // OLD
                    //region.currentCountry = country;
                }
            }

            int infrastructure_Amount = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_INFRASTRUCTURES");
            int civFactory_Amount = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_CIV_FABRICS");
            int farms_Amount = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_FARMS");
            int cheFarms = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_CHEFARMS");
            int resLabs = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_RESLABS");
            int dockyards = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_DOCKYARDS");
            int forts = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_FORTS");

            region.buildings.Clear();
            region.infrastructure_Amount = 0;
            region.fortifications_Amount = forts;

            for (int f = 0; f < civFactory_Amount; f++)
            {
                region.buildings.Add(ReferencesManager.Instance.gameSettings.fabric);
                region.currentCountry.civFactories++;
            }


            for (int f = 0; f < farms_Amount; f++)
            {
                region.buildings.Add(ReferencesManager.Instance.gameSettings.farm);
                region.currentCountry.farms++;
            }

            for (int f = 0; f < cheFarms; f++)
            {
                region.buildings.Add(ReferencesManager.Instance.gameSettings.chefarm);
                region.currentCountry.chemicalFarms++;
            }

            for (int f = 0; f < resLabs; f++)
            {
                region.buildings.Add(ReferencesManager.Instance.gameSettings.researchLab);
                region.currentCountry.researchLabs++;
            }

            for (int f = 0; f < dockyards; f++)
            {
                region.buildings.Add(ReferencesManager.Instance.gameSettings.dockyard);
                region.currentCountry.dockyards++;
            }

            for (int f = 0; f < infrastructure_Amount; f++)
            {
                region.UpgradeInfrastructureForce(region);
            }

            //Debug.Log($"{PlayerPrefs.GetString($"1_REGION_{i}_HAS_ARMY")} {i} {regions[i].currentCountry.country._name}");
            if (PlayerPrefs.GetString($"{currentSaveIndex_INT}_REGION_{region._id}_HAS_ARMY") == "TRUE")
            {
                regions[i].hasArmy = true;
                ReferencesManager.Instance.army.CreateUnit_NoCheck(regions[i]);

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_INF_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.soldierLVL1, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_INF_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.soldierLVL2, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_INF_LVL3"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.soldierLVL3, region);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_ART_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.artileryLVL1, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_ART_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.artileryLVL2, region);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_TANK_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.tankLVL1, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_TANK_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.tankLVL2, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_TANK_LVL3"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.tankLVL3, region);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_MOTO_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.motoLVL1, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_MOTO_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.motoLVL2, region);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_ATI_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.antitankLVL1, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_ATI_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.antitankLVL2, region);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_CAV_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.cavLVL1, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"{currentSaveIndex_INT}_REGION_{region._id}_CAV_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.cavLVL2, region);
                }
            }
        }

        UpdateRegionsColor();

        for (int i = 0; i < countries.Count; i++)
        {
            for (int techIndex = 0; techIndex < ReferencesManager.Instance.gameSettings.technologies.Length; techIndex++)
            {
                if (PlayerPrefs.GetString($"{currentSaveIndex_INT}_COUNTRY_{countries[i].country._id}_TECH_{techIndex}") == "TRUE")
                {
                    countries[i].countryTechnologies.Add(ReferencesManager.Instance.gameSettings.technologies[techIndex]);
                }
            }
        }

        ReferencesManager.Instance.dateManager.currentDate[0] = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_DATE_0");
        ReferencesManager.Instance.dateManager.currentDate[1] = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_DATE_1");
        ReferencesManager.Instance.dateManager.currentDate[2] = PlayerPrefs.GetInt($"{currentSaveIndex_INT}_DATE_2");
        ReferencesManager.Instance.dateManager.UpdateUI();

        for (int i = 0; i < ReferencesManager.Instance.gameSettings.gameEvents.Count; i++)
        {
            if (PlayerPrefs.GetString($"{currentSaveIndex_INT}_EVENT_{i}") == "TRUE")
            {
                ReferencesManager.Instance.gameSettings.gameEvents[i]._checked = true;
            }
            else
            {
                ReferencesManager.Instance.gameSettings.gameEvents[i]._checked = false;
            }
        }
    }

    public CountrySettings FindCountryByID(int id)
    {
        CountrySettings _foundedCountry = new CountrySettings();

        foreach (CountrySettings country in countries)
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

        using (StreamReader reader = new StreamReader(path))
        {
            modData = reader.ReadToEnd();
        }

        string[] sections = modData.Split(new[] { "#REGIONS#", "#COUNTRIES_SETTINGS#", "#EVENTS#" }, StringSplitOptions.None);
        string[] mainModDataLines = sections[0].Split(';');
        string[] regionsDataLines = sections[1].Split(';');
        string[] countriesDataLines = sections[2].Split(';');
        string[] eventsIDsDataLines = sections.Length > 3 ? sections[3].Split(';') : Array.Empty<string>();

        try
        {
            string _line = mainModDataLines[1];
            string[] parts = _line.Split('[');
            string secondPart = parts[1];
            string value = secondPart.Remove(secondPart.Length - 1);

            int isModAllowsGameEvents = int.Parse(value);
            ReferencesManager.Instance.gameSettings.allowGameEvents = isModAllowsGameEvents == 1;
        }
        catch (Exception)
        {
            if (ReferencesManager.Instance.gameSettings.developerMode)
            {
                Debug.LogError("ERROR: Mod loader error in value parser (CountryManager.cs)");
            }
        }

        HashSet<int> existingCountryIds = new HashSet<int>(
            ReferencesManager.Instance.countryManager.countries.Select(c => c.country._id));
        HashSet<int> globalCountryIds = new HashSet<int>(
            ReferencesManager.Instance.globalCountries.Select(gc => gc._id));

        for (int i = 2; i < mainModDataLines.Length; i++)
        {
            string _line = mainModDataLines[i];
            if (!string.IsNullOrEmpty(_line))
            {
                string value = GetValue(_line);
                int countryId = int.Parse(value);

                if (!existingCountryIds.Contains(countryId) && globalCountryIds.Contains(countryId))
                {
                    var countryScriptableObject = ReferencesManager.Instance.globalCountries
                        .FirstOrDefault(gc => gc._id == countryId);

                    if (countryScriptableObject != null)
                    {
                        ReferencesManager.Instance.CreateCountry(countryScriptableObject, "������������");
                    }
                }
            }
        }

        List<int> countriesInRegionsIDs = new List<int>();
        foreach (var _line in regionsDataLines)
        {
            try
            {
                if (!string.IsNullOrEmpty(_line))
                {
                    int value = int.Parse(GetValue(_line));
                    countriesInRegionsIDs.Add(value);
                }
            }
            catch (Exception) { }
        }

        List<RegionLoader.ModRegionData> regionModIDs = new List<RegionLoader.ModRegionData>();
        for (int i = 0; i < Math.Min(regionsDataLines.Length, ReferencesManager.Instance.countryManager.regions.Count + 2); i++)
        {
            try
            {
                string _line = regionsDataLines[i];
                if (!string.IsNullOrEmpty(_line))
                {
                    string[] regionIdParts = _line.Split(' ');
                    int regValue = int.Parse(regionIdParts[0].Remove(0, 7));
                    int regionCountryID = int.Parse(regionIdParts[2]);

                    regionModIDs.Add(new RegionLoader.ModRegionData
                    {
                        countryID = regionCountryID,
                        regionId = regValue
                    });
                }
            }
            catch (Exception) { }
        }

        var regionDict = ReferencesManager.Instance.countryManager.regions
            .ToDictionary(r => r._id, r => r);
        var countryDict = ReferencesManager.Instance.countryManager.countries
            .ToDictionary(c => c.country._id, c => c);

        foreach (var regValue in regionModIDs)
        {
            if (regionDict.TryGetValue(regValue.regionId, out var province) &&
                countryDict.TryGetValue(regValue.countryID, out var country))
            {
                ReferencesManager.Instance.AnnexRegion(province, country);
            }
        }

        int selectedCountryId = int.Parse(ReferencesManager.Instance.gameSettings._playerCountrySelected.value);
        if (countryDict.TryGetValue(selectedCountryId, out var selectedCountry))
        {
            ReferencesManager.Instance.countryManager.currentCountry = selectedCountry;
            ReferencesManager.Instance.countryManager.currentCountry.isPlayer = true;
        }

        if (!IsNullOrEmpty(countriesDataLines))
        {
            foreach (var _line in countriesDataLines)
            {
                if (!string.IsNullOrEmpty(_line))
                {
                    try
                    {
                        string new_lineData = GetValue(_line);
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

        List<int> eventsIDs = new List<int>();
        if (!IsNullOrEmpty(eventsIDsDataLines))
        {
            foreach (var eventData in eventsIDsDataLines)
            {
                if (!string.IsNullOrEmpty(eventData))
                {
                    eventsIDs.Add(int.Parse(eventData));
                }
            }
        }

        foreach (var eventId in eventsIDs)
        {
            string eventPath = Path.Combine(Application.persistentDataPath, pathType == "local" ? "localMods" : "savedMods", $"{modName}", "events", $"{eventId}", $"{eventId}.AEEvent");

            if (!File.Exists(eventPath)) continue;

            string eventData = "";
            using (StreamReader _reader = new StreamReader(eventPath))
            {
                eventData = _reader.ReadToEnd();
            }

            string[] eventLines = eventData.Split(';');

            EventScriptableObject _event = new EventScriptableObject
            {
                id = int.Parse(GetValue(eventLines[0])),
                _name = GetValue(eventLines[1]),
                name = GetValue(eventLines[1]),
                _nameEN = GetValue(eventLines[1]),
                description = GetValue(eventLines[2]),
                descriptionEN = GetValue(eventLines[2]),
                date = GetValue(eventLines[3]),
                silentEvent = GetValue(eventLines[4]) == "1",
                receivers = GetValue(eventLines[5]).Split('-').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList(),
                exceptionsReceivers = GetValue(eventLines[6]).Split('-').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList(),
                conditions = GetValue(eventLines[7]).Split('@').ToList()
            };

            string _localEvent_imagePath = Path.Combine(Application.persistentDataPath, pathType == "local" ? "localMods" : "savedMods", $"{modName}", "events", $"{_event.id}", $"{_event.id}.jpg");
            if (File.Exists(_localEvent_imagePath))
            {
                Texture2D finalTexture = NativeGallery.LoadImageAtPath(_localEvent_imagePath);
                _event.image = Sprite.Create(finalTexture, new Rect(0, 0, finalTexture.width, finalTexture.height), Vector2.zero);
            }

            var conditionsData = eventData.Split(new[] { "conditions:" }, StringSplitOptions.None);
            if (conditionsData.Length > 1)
            {
                _event.conditions = conditionsData[1].Split(';').SelectMany(c => c.Split('@').Select(cd => cd.Replace('-', ';'))).Where(x => !string.IsNullOrEmpty(x)).ToList();
            }

            var buttonsData = eventData.Split(new[] { "buttons:" }, StringSplitOptions.None);
            if (buttonsData.Length > 1)
            {
                var buttonsLines = buttonsData[1].Split(';');
                for (int i = 0; i < buttonsLines.Length - 1; i++)
                {
                    var buttonData = buttonsLines[i].Split('|');
                    var newButton = new EventScriptableObject.EventButton
                    {
                        name = buttonData[0],
                        nameEN = buttonData[0],
                        rejectUltimatum = buttonData[2] == "1",
                        actions = buttonData[1].Split('@').Select(a => a.Replace('-', ';')).Where(x => !string.IsNullOrEmpty(x)).ToList()
                    };

                    _event.buttons.Add(newButton);
                }
            }

            ReferencesManager.Instance.gameSettings.gameEvents.Add(_event);
        }
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
                foodText.text = currentCountry.food.ToString();
            }
            if (recrootsText != null)
            {
                if (currentCountry.recroots >= 10000)
                {
                    recrootsText.text = ReferencesManager.Instance.GoodNumberString((int)currentCountry.recroots);
                }
                else
                {
                    recrootsText.text = currentCountry.recroots.ToString();
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

            if (currentCountry.recrootsIncome >= 0)
            {
                recrootsIncomeText.text = "+" + currentCountry.recrootsIncome.ToString();
                recrootsIncomeText.color = ReferencesManager.Instance.gameSettings.greenColor;
            }
            else
            {
                recrootsIncomeText.text = currentCountry.recrootsIncome.ToString();
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
