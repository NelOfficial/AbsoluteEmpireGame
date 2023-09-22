using TMPro;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;

public class CountryManager : MonoBehaviour
{
    [SerializeField] TMP_Text moneyText, moneyIncomeText, foodText, foodIncomeText, recrootsText, recrootsIncomeText, debtText;
    [SerializeField] Image[] countryFlagImages;
    [SerializeField] TMP_Text[] countryNameTexts;

    public List<CountrySettings> countries = new List<CountrySettings>();
    public RegionManager[] regions;

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

        for (int i = 0; i < regions.Length; i++)
        {
            regions[i]._id = i;
        }

        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            PlayerPrefs.SetString("LOAD_GAME_THROUGH_MENU", "FALSE");

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

            Debug.Log($"countryManager nickname: {playerNickname}");
            Debug.Log($"currentPlayerData: {currentPlayerData.currentNickname}");
            Debug.Log($"localPlayer: {PhotonNetwork.LocalPlayer}");
        }
        else
        {
            if (PlayerPrefs.GetString("LOAD_GAME_THROUGH_MENU") == "TRUE")
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
                        PlayerPrefs.SetInt("currentCountryIndex", Random.Range(0, countries.Count));

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

            if (PlayerPrefs.GetString("LOAD_GAME_THROUGH_MENU") == "TRUE" && !ReferencesManager.Instance.gameSettings.jsonTest)
            {
                if (SceneManager.GetActiveScene().name != "Editor")
                {
                    Load();
                }
            }
            else if (ReferencesManager.Instance.gameSettings.jsonTest)
            {
                LoadMod();
            }
            else if (ReferencesManager.Instance.gameSettings.playTestingMod)
            {
                PlayTestMod(PlayerPrefs.GetString("CURRENT_MOD_PLAYTESTING"));
                ReferencesManager.Instance.gameSettings.developerCheats = true;
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

        for (int i = 0; i < regions.Length; i++)
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

        for (int i = 0; i < ReferencesManager.Instance.gameSettings.gameEvents.Length; i++)
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

    private void LoadMod()
    {
        int currentModID = PlayerPrefs.GetInt("CURRENT_MOD_PLAYING");

        string modName = PlayerPrefs.GetString($"MODIFICATION_{currentModID}");
        string modData = "";

        StreamReader reader = new StreamReader(Path.Combine(Application.persistentDataPath, "savedMods", $"{modName}", $"{modName}.AEMod"));
        modData = reader.ReadToEnd();

        reader.Close();

        string[] dataParts = modData.Split("##########");
        string[] mainModDataLines = dataParts[0].Split(';');
        string[] regionsDataLines = dataParts[1].Split(';');

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
            try
            {
                parts = _line.Split('[');
                secondPart = parts[1];
                value = secondPart.Remove(secondPart.Length - 1);
            }
            catch (System.Exception)
            {

            }

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

        for (int r = 0; r < regionsDataLines.Length; r++)
        {
            try
            {
                string _line = regionsDataLines[r];
                parts = _line.Split('[');

                secondPart = parts[1];

                value = secondPart.Remove(secondPart.Length - 1);

                countriesInRegionsIDs.Add(int.Parse(value));
            }
            catch
            {
                if (ReferencesManager.Instance.gameSettings.developerMode)
                {
                    Debug.LogError($"ERROR: Mod loader error in value parser (CountryManager.cs)");
                }
            }
        }

        for (int i = 0; i < regions.Length; i++)
        {
            try
            {
                string _line = regionsDataLines[i];
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

                //for (int r = 0; r < modRegionsIds.Count; r++)
                //{
                //    //Debug.Log($"Real {regions[i]._id} ({regions[i].currentCountry.country._nameEN}) | {regValue}");

                //    if (regions[i]._id == modRegionsIds[r])
                //    {
                //        for (int c = 0; c < countries.Count; c++)
                //        {
                //            if (countries[c].country._id == countriesInRegionsIDs[i])
                //            {
                //                ReferencesManager.Instance.AnnexRegion(regions[i], countries[c]);
                //            }
                //        }
                //    }
                //}
            }
            catch (System.Exception)
            {
                if (ReferencesManager.Instance.gameSettings.developerMode)
                {
                    Debug.LogError($"ERROR: Mod loader error in regionValue parser (CountryManager.cs)");
                }
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
    }

    private void PlayTestMod(string _name)
    {
        string modName = _name;
        string modData = "";

        StreamReader reader = new StreamReader(Path.Combine(Application.persistentDataPath, "localMods", $"{modName}", $"{modName}_modData.AEMod"));
        modData = reader.ReadToEnd();

        reader.Close();

        string[] dataParts = modData.Split("##########");
        string[] mainModDataLines = dataParts[0].Split(';');
        string[] regionsDataLines = dataParts[1].Split(';');

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
            try
            {
                parts = _line.Split('[');
                secondPart = parts[1];
                value = secondPart.Remove(secondPart.Length - 1);
            }
            catch (System.Exception)
            {

            }

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

        for (int r = 0; r < regionsDataLines.Length; r++)
        {
            try
            {
                string _line = regionsDataLines[r];
                parts = _line.Split('[');

                secondPart = parts[1];

                value = secondPart.Remove(secondPart.Length - 1);

                countriesInRegionsIDs.Add(int.Parse(value));
            }
            catch
            {
                if (ReferencesManager.Instance.gameSettings.developerMode)
                {
                    Debug.LogError($"ERROR: Mod loader error in value parser (CountryManager.cs)");
                }
            }
        }

        for (int i = 0; i < regions.Length; i++)
        {
            try
            {
                string _line = regionsDataLines[i];
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
            catch (System.Exception)
            {
                if (ReferencesManager.Instance.gameSettings.developerMode)
                {
                    Debug.LogError($"ERROR: Mod loader error in regionValue parser (CountryManager.cs)");
                }
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
            if (currentCountry.moneyIncomeUI >= 0)
            {
                moneyIncomeText.text = "+" + currentCountry.moneyIncomeUI.ToString();
                moneyIncomeText.color = ReferencesManager.Instance.gameSettings.greenColor;
            }
            else
            {
                moneyIncomeText.text = currentCountry.moneyIncomeUI.ToString();
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
