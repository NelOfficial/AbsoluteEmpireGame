using TMPro;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System;


public class CountryManager : MonoBehaviour
{
    [SerializeField] TMP_Text moneyText, moneyIncomeText, foodText, foodIncomeText, recrootsText, recrootsIncomeText, debtText;
    [SerializeField] Image[] countryFlagImages;
    [SerializeField] TMP_Text[] countryNameTexts;

    public List<CountrySettings> countries = new List<CountrySettings>();
    public List<RegionManager> regions = new List<RegionManager>();

    [HideInInspector] public CountrySettings currentCountry;
    [HideInInspector] public PlayerData currentPlayerData;
    [HideInInspector] public string playerNickname;

    public string[] parts;
    public string secondPart;
    public string value;
    public string regionValue;

    public List<int> countriesInRegionsIDs = new List<int>();

    private void Start()
    {
        ReferencesManager.Instance.gameSettings.allowGameEvents = true;

        regions.Clear();

        for (int i = 0; i < countries.Count; i++)
        {
            for (int r = 0; r < countries[i].myRegions.Count; r++)
            {
                regions.Add(countries[i].myRegions[r]);
            }
        }

        for (int i = 0; i < regions.Count; i++)
        {
            regions[i]._id = i;
        }

        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            ReferencesManager.Instance.gameSettings.playMod.value = false;
            ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
            ReferencesManager.Instance.gameSettings.loadGame.value = false;

            currentCountry = countries[(int)PhotonNetwork.LocalPlayer.CustomProperties["playerCountryIndex"]];
            currentCountry.isPlayer = true;

            playerNickname = PhotonNetwork.LocalPlayer.NickName;

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
                    if (PlayerPrefs.HasKey("currentCountryIndex"))
                    {
                        for (int i = 0; i < countries.Count; i++)
                        {
                            if (countries[i].country._id == PlayerPrefs.GetInt("currentCountryIndex"))
                            {
                                currentCountry = countries[i];
                                currentCountry.isPlayer = true;
                                if (ReferencesManager.Instance.gameSettings.developerMode) Debug.Log(currentCountry.country);
                            }
                        }
                    }
                    else
                    {
                        PlayerPrefs.SetInt("currentCountryIndex", UnityEngine.Random.Range(0, countries.Count));

                        for (int i = 0; i < countries.Count; i++)
                        {
                            if (countries[i].country._id == PlayerPrefs.GetInt("currentCountryIndex"))
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

            if (ReferencesManager.Instance.gameSettings.playTestingMod.value == true)
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

        UpdateValuesUI();
        UpdateIncomeValuesUI();
        UpdateCountryInfo();

        foreach (CountrySettings country in countries)
        {
            country.country.countryColor.a = 0.5f;
        }

        foreach (RegionManager region in regions)
        {
            region.selectedColor.r = region.currentCountry.country.countryColor.r + 0.2f;
            region.selectedColor.g = region.currentCountry.country.countryColor.g + 0.2f;
            region.selectedColor.b = region.currentCountry.country.countryColor.b + 0.2f;
            region.selectedColor.a = 0.5f;

            region.hoverColor.r = region.currentCountry.country.countryColor.r + 0.3f;
            region.hoverColor.g = region.currentCountry.country.countryColor.g + 0.3f;
            region.hoverColor.b = region.currentCountry.country.countryColor.b + 0.3f;
            region.hoverColor.a = 0.5f;

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
        foreach (CountrySettings country in countries)
        {
            if (country.country._id == PlayerPrefs.GetInt($"1_PLAYER_COUNTRY"))
            {
                currentCountry = country;
            }
        }

        for (int i = 0; i < countries.Count; i++)
        {
            countries[i].ideology = PlayerPrefs.GetString($"1_COUNTRY_{i}_IDEOLOGY");
            countries[i].money = PlayerPrefs.GetInt($"1_COUNTRY_{i}_MONEY");
            countries[i].food = PlayerPrefs.GetInt($"1_COUNTRY_{i}_FOOD");
            countries[i].recroots = PlayerPrefs.GetInt($"1_COUNTRY_{i}_RECROOTS");

            countries[i].startMoneyIncome = PlayerPrefs.GetInt($"1_COUNTRY_{i}_startMoneyIncome");
            countries[i].moneyNaturalIncome = PlayerPrefs.GetInt($"1_COUNTRY_{i}_moneyNaturalIncome");

            countries[i].startFoodIncome = PlayerPrefs.GetInt($"1_COUNTRY_{i}_startFoodIncome");
            countries[i].foodNaturalIncome = PlayerPrefs.GetInt($"1_COUNTRY_{i}_foodNaturalIncome");

            countries[i].civFactories = PlayerPrefs.GetInt($"1_COUNTRY_{i}_civFactories");
            countries[i].milFactories = PlayerPrefs.GetInt($"1_COUNTRY_{i}_milFactories");
        }

        for (int i = 0; i < regions.Count; i++)
        {
            RegionManager region = regions[i];

            foreach (CountrySettings country in countries)
            {
                if (country.country._id == PlayerPrefs.GetInt($"1_REGION_{i}_CURRENTCOUNTRY_ID"))
                {
                    region.currentCountry = country;
                }
            }
            region.infrastructure_Amount = PlayerPrefs.GetInt($"1_REGION_{i}_INFRASTRUCTURES");
            region.civFactory_Amount = PlayerPrefs.GetInt($"1_REGION_{i}_CIV_FABRICS");
            region.farms_Amount = PlayerPrefs.GetInt($"1_REGION_{i}_FARMS");
            region.cheFarms = PlayerPrefs.GetInt($"1_REGION_{i}_CHEFARMS");

            //Debug.Log($"{PlayerPrefs.GetString($"1_REGION_{i}_HAS_ARMY")} {i} {regions[i].currentCountry.country._name}");
            if (PlayerPrefs.GetString($"1_REGION_{i}_HAS_ARMY") == "TRUE")
            {
                regions[i].hasArmy = true;
                ReferencesManager.Instance.army.CreateUnit_NoCheck(regions[i]);

                for (int index = 0; index < PlayerPrefs.GetInt($"1_REGION_{i}_INF_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.soldierLVL1, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"1_REGION_{i}_INF_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.soldierLVL2, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"1_REGION_{i}_INF_LVL3"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.soldierLVL3, region);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"1_REGION_{i}_ART_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.artileryLVL1, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"1_REGION_{i}_ART_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.artileryLVL2, region);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"1_REGION_{i}_TANK_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.tankLVL1, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"1_REGION_{i}_TANK_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.tankLVL2, region);
                }

                for (int index = 0; index < PlayerPrefs.GetInt($"1_REGION_{i}_MOTO_LVL1"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.motoLVL1, region);
                }
                for (int index = 0; index < PlayerPrefs.GetInt($"1_REGION_{i}_MOTO_LVL2"); index++)
                {
                    ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(ReferencesManager.Instance.gameSettings.motoLVL2, region);
                }
            }
        }

        UpdateRegionsColor();

        for (int i = 0; i < countries.Count; i++)
        {
            for (int techIndex = 0; techIndex < ReferencesManager.Instance.gameSettings.technologies.Length; techIndex++)
            {
                if (PlayerPrefs.GetString($"1_COUNTRY_{i}_TECH_{techIndex}") == "TRUE")
                {
                    countries[i].countryTechnologies.Add(ReferencesManager.Instance.gameSettings.technologies[techIndex]);
                }
            }
        }

        ReferencesManager.Instance.dateManager.currentDate[0] = PlayerPrefs.GetInt($"1_DATE_0");
        ReferencesManager.Instance.dateManager.currentDate[1] = PlayerPrefs.GetInt($"1_DATE_1");
        ReferencesManager.Instance.dateManager.currentDate[2] = PlayerPrefs.GetInt($"1_DATE_2");
        ReferencesManager.Instance.dateManager.UpdateUI();

        for (int i = 0; i < ReferencesManager.Instance.gameSettings.gameEvents.Count; i++)
        {
            if (PlayerPrefs.GetString($"1_EVENT_{i}") == "TRUE")
            {
                ReferencesManager.Instance.gameSettings.gameEvents[i]._checked = true;
            }
            else
            {
                ReferencesManager.Instance.gameSettings.gameEvents[i]._checked = false;
            }
        }
    }

    private void LoadMod(string _modname, int modID, string pathType)
    {
        int currentModID = modID;

        string modName = _modname;
        string modData = "";

        string path = "";

        string eventPartPath = "";

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

        for (int i = 2; i < mainModDataLines.Length; i++)
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
                            ReferencesManager.Instance.CreateCountry(countryScriptableObject, "Неопределено");
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
            catch (System.Exception) {}
        }

        for (int i = 0; i < regions.Count; i++)
        {
            try
            {
                string _line = regionsDataLines[i];
                if (!string.IsNullOrEmpty(_line))
                {
                    string[] regionIdParts = _line.Split(' ');
                    regionValue = regionIdParts[0].Remove(0, 7);
                    int regValue = int.Parse(regionValue);

                    for (int c = 0; c < countries.Count; c++)
                    {
                        if (countries[c].country._id == countriesInRegionsIDs[0])
                        {
                            ReferencesManager.Instance.AnnexRegion(regions[0], countries[c]);
                        }
                    }
                    for (int c = 0; c < countries.Count; c++)
                    {
                        if (countries[c].country._id == countriesInRegionsIDs[i])
                        {
                            ReferencesManager.Instance.AnnexRegion(regions[i], countries[c]);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        bool hasCountry = countries.Any(item => item.country._id == PlayerPrefs.GetInt("currentCountryIndex"));

        for (int i = 0; i < countries.Count; i++)
        {
            if (hasCountry)
            {
                if (countries[i].country._id == PlayerPrefs.GetInt("currentCountryIndex"))
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
                        string lineData = new_lineData.Split('|')[1];

                        if (!string.IsNullOrEmpty(lineData))
                        {
                            string[] values = lineData.Split('|');

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
                                    country.recroots = recroots;

                                    country.ideology = ideology;
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
            bool _localEvent_silentEventBoolean = false;
            string[] _localEvent_conditions;
            string _localEvent_imagePath = "";


            _localEvent_ID = int.Parse(GetValue(eventLines[0]));
            _localEvent_Name = GetValue(eventLines[1]);
            _localEvent_Description = GetValue(eventLines[2]);
            _localEvent_Date = GetValue(eventLines[3]);
            _localEvent_silentEvent = GetValue(eventLines[4]);
            _localEvent_conditions = GetValue(eventLines[5]).Split('!');

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
                string[] conditions = conditionsLines[c].Split('!');

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

                if (buttonData[2] == "0") newButton.rejectUltimatum = false;
                else if (buttonData[2] == "1") newButton.rejectUltimatum = true;

                string[] actions = buttonData[1].Split('!');

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
            _event.description = _localEvent_Description;
            _event.date = _localEvent_Date;
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

    private string GetValue(string line)
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
                moneyText.text = currentCountry.money.ToString();
            }
            if (foodText != null)
            {
                foodText.text = currentCountry.food.ToString();
            }
            if (recrootsText != null)
            {
                recrootsText.text = currentCountry.recroots.ToString();
            }
            if (debtText != null)
            {
                debtText.text = ReferencesManager.Instance.countryInfo.currentCredit.ToString();
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
        }
    }

    public void UpdateCountryInfo()
    {
        string currentLanguage = "";

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            currentLanguage = "EN";
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            currentLanguage = "RU";
        }

        if (!ReferencesManager.Instance.gameSettings.spectatorMode)
        {
            foreach (Image countryFlagImage in countryFlagImages)
            {
                countryFlagImage.sprite = currentCountry.country.countryFlag;
            }

            foreach (TMP_Text countryText in countryNameTexts)
            {
                if (string.IsNullOrEmpty(currentLanguage))
                {
                    currentLanguage = "EN";
                }
                if (currentLanguage == "EN")
                {
                    countryText.text = currentCountry.country._nameEN;
                }
                else if (currentLanguage == "RU")
                {
                    countryText.text = currentCountry.country._name;
                }
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
            if (ideology == "Неопределённый")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[1];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[1];
            }
            else if (ideology == "Демократия")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[1];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[1];
            }
            else if (ideology == "Монархия")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[2];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[2];
            }
            else if (ideology == "Фашизм")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[4];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[4];
            }
            else if (ideology == "Коммунизм")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[5];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[5];
            }

            // Новый цвет
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
            region.GetComponent<SpriteRenderer>().color = region.currentCountry.country.countryColor;
        }
    }
}
