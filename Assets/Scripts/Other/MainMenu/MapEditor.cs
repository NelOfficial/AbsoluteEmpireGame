using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System;
using System.Text;
using UnityEngine.Networking;
using Photon.Realtime;


public class MapEditor : MonoBehaviour
{
    [SerializeField] GameObject countrySelectionPrefab;
    [SerializeField] GameObject countryInModSelectionPrefab;
    [SerializeField] GameObject countrySelectionPanelGrid;
    [SerializeField] GameObject container;

    public bool paintMapMode;

    public CountrySettings selectedCountry;
    public CountrySettings editingCountry;

    [SerializeField] CountrySettings[] globalCountries;

    [SerializeField] GameObject[] tabs;

    private bool isOpen;

    [SerializeField] Button publishButton;
    public TMP_InputField nameInputField;
    [SerializeField] TMP_InputField descInputField;
    [SerializeField] Toggle allowEventsToggle;

    [SerializeField] TMP_InputField countryMoneyInputfield;
    [SerializeField] TMP_InputField countryFoodInputfield;
    [SerializeField] TMP_InputField countryRecrootsInputfield;
    [SerializeField] TMP_Dropdown countryIdeologyDropdown;

    [SerializeField] TMP_Text countryNameText;
    [SerializeField] Image countryFlagImage;

    [SerializeField] GameObject successPanel;
    [SerializeField] GameObject errorPanel;

    [SerializeField] GameObject countryInfoPanel;

    [SerializeField] List<int> modCountriesList = new List<int>();

    [TextArea(0, 50)]
    public string currentModText;

    [Header("EDITING EXISTING MOD")]
    public int modID;
    public string modData;

    private string[] parts;
    private string secondPart;

    private string value;
    private string regionValue;

    [SerializeField] EventCreatorManager _eventCreatorManager;

    private List<CountryInfoData> localCountryInfoDataList = new List<CountryInfoData>();

    string silentEvent = "";
    string rejectUltimatum = "";

    public string assetData;

    [SerializeField] IntListValue regionList;

    private Texture2D EventImageTexture;
    public List<int> countriesInRegionsIDs = new List<int>();

    public List<ModCustomCountry> modCustomCountries = new List<ModCustomCountry>();

    [Header("# Create Country UI:")]
    [SerializeField] TMP_InputField _countryName_RU_Inputfield;
    [SerializeField] TMP_InputField _countryName_EN_Inputfield;

    [SerializeField] TMP_InputField _countryTag_Inputfield;
    [SerializeField] TMP_InputField _countryCapLimit_Inputfield;

    [SerializeField] TMP_InputField _countryIdeology_Dropdown;

    [SerializeField] Image[] _countryImages;

    [SerializeField] string _debugString;

    private void Start()
    {
        ReferencesManager.Instance.countryManager.regions = FindObjectsOfType<RegionManager>().ToList();

        ReferencesManager.Instance.regionManager.UpdateRegions();

        bool test = string.IsNullOrEmpty(ReferencesManager.Instance.gameSettings.editingModString.value);

        if (test == false)
        {
            StartCoroutine(LoadMod_Co());
        }

        _eventCreatorManager = FindObjectOfType<EventCreatorManager>();

        for (int i = 0; i < globalCountries.Length; i++)
        {
            _debugString += $"{globalCountries[i].country._name} - {globalCountries[i].country._id}\n";
        }
    }

    public void OpenUI()
    {
        UpdateCountriesList();

        container.SetActive(true);

        isOpen = true;
    }

    public void OpenCountryEditPanel()
    {
        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            countryNameText.text = editingCountry.country._nameEN;
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            countryNameText.text = editingCountry.country._name;
        }

        bool alreadyContainsCountryData = localCountryInfoDataList.Any(item => item.countryID == editingCountry.country._id);

        if (alreadyContainsCountryData)
        {
            foreach (CountryInfoData countryInfoData in localCountryInfoDataList)
            {
                if (countryInfoData.countryID == editingCountry.country._id)
                {
                    countryFlagImage.sprite = editingCountry.country.countryFlag;

                    countryMoneyInputfield.text = countryInfoData.newMoney.ToString();
                    countryFoodInputfield.text = countryInfoData.newFood.ToString();
                    countryRecrootsInputfield.text = countryInfoData.newRecroots.ToString();

                    if (countryInfoData.newIdeology == "Неопределено" || countryInfoData.newIdeology == "Неопределённый")
                    {
                        countryIdeologyDropdown.value = 0;
                    }

                    if (countryInfoData.newIdeology == "Демократия")
                    {
                        countryIdeologyDropdown.value = 1;
                    }

                    if (countryInfoData.newIdeology == "Монархия")
                    {
                        countryIdeologyDropdown.value = 2;
                    }

                    if (countryInfoData.newIdeology == "Фашизм")
                    {
                        countryIdeologyDropdown.value = 3;
                    }

                    if (countryInfoData.newIdeology == "Коммунизм")
                    {
                        countryIdeologyDropdown.value = 4;
                    }
                }
            }
        }
        else
        {
            countryFlagImage.sprite = editingCountry.country.countryFlag;

            countryMoneyInputfield.text = editingCountry.money.ToString();
            countryFoodInputfield.text = editingCountry.food.ToString();
            countryRecrootsInputfield.text = editingCountry.recroots.ToString();

            if (editingCountry.ideology == "Неопределено" || editingCountry.ideology == "Неопределённый")
            {
                countryIdeologyDropdown.value = 0;
            }

            if (editingCountry.ideology == "Демократия")
            {
                countryIdeologyDropdown.value = 1;
            }

            if (editingCountry.ideology == "Монархия")
            {
                countryIdeologyDropdown.value = 2;
            }

            if (editingCountry.ideology == "Фашизм")
            {
                countryIdeologyDropdown.value = 3;
            }

            if (editingCountry.ideology == "Коммунизм")
            {
                countryIdeologyDropdown.value = 4;
            }
        }

        OpenTab(countryInfoPanel);
    }

    public void SaveCountryInfoChanges()
    {
        bool alreadyContainsCountryData = localCountryInfoDataList.Any(item => item.countryID == editingCountry.country._id);

        if (alreadyContainsCountryData)
        {
            foreach (CountryInfoData countryInfoData in localCountryInfoDataList)
            {
                if (countryInfoData.countryID == editingCountry.country._id)
                {
                    countryInfoData.newMoney = int.Parse(countryMoneyInputfield.text);
                    countryInfoData.newFood = int.Parse(countryFoodInputfield.text);
                    countryInfoData.newRecroots = int.Parse(countryRecrootsInputfield.text);

                    if (countryIdeologyDropdown.value == 0) countryInfoData.newIdeology = "Неопределено";

                    if (countryIdeologyDropdown.value == 1) countryInfoData.newIdeology = "Демократия";

                    if (countryIdeologyDropdown.value == 2) countryInfoData.newIdeology = "Монархия";

                    if (countryIdeologyDropdown.value == 3) countryInfoData.newIdeology = "Фашизм";

                    if (countryIdeologyDropdown.value == 4) countryInfoData.newIdeology = "Коммунизм";
                }
            }
        }
        else
        {
            CountryInfoData countryInfoData = new CountryInfoData();

            countryInfoData.countryID = editingCountry.country._id;

            countryInfoData.newMoney = int.Parse(countryMoneyInputfield.text);
            countryInfoData.newFood = int.Parse(countryFoodInputfield.text);
            countryInfoData.newRecroots = int.Parse(countryRecrootsInputfield.text);

            if (countryIdeologyDropdown.value == 0) countryInfoData.newIdeology = "Неопределено";

            if (countryIdeologyDropdown.value == 1) countryInfoData.newIdeology = "Демократия";

            if (countryIdeologyDropdown.value == 2) countryInfoData.newIdeology = "Монархия";

            if (countryIdeologyDropdown.value == 3) countryInfoData.newIdeology = "Фашизм";

            if (countryIdeologyDropdown.value == 4) countryInfoData.newIdeology = "Коммунизм";

            localCountryInfoDataList.Add(countryInfoData);
        }
    }

    private class CountryInfoData
    {
        public int countryID;

        public int newMoney;
        public int newFood;
        public int newRecroots;

        public string newIdeology;
    }

    public void CloseUI()
    {
        container.SetActive(false);
        isOpen = false;
    }

    public void ToggleUI()
    {
        if (isOpen) // Close
        {
            CloseUI();
        }
        else // Open
        {
            OpenUI();
        }
    }

    private void LoadMod()
    {
        string loadedModName = ReferencesManager.Instance.gameSettings.editingModString.value;
        string loadedModPath = Path.Combine(Application.persistentDataPath, "localMods", $"{loadedModName}");

        StreamReader reader = new StreamReader(Path.Combine(loadedModPath, $"{loadedModName}.AEMod"));
        modData = reader.ReadToEnd();
        currentModText = modData;

        reader.Close();

        if (Directory.Exists(loadedModPath))
        {
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
                    Debug.LogError($"ERROR: Mod loader error in value parser (MapEditor.cs)");
                }
            }

            nameInputField.interactable = false;
            descInputField.interactable = false;
            nameInputField.text = $"{loadedModName}";

            bool allowEventsBoolean = false;

            if (int.Parse(value) == 0) allowEventsBoolean = false;
            else allowEventsBoolean = true;

            allowEventsToggle.isOn = allowEventsBoolean;

            for (int r = 1; r < regionsDataLines.Length - 1; r++)
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

            for (int i = 0; i < countriesDataLines.Length; i++)
            {
                if (!string.IsNullOrEmpty(countriesDataLines[i]))
                {
                    try
                    {
                        string pre_lineData = countriesDataLines[i].Remove(countriesDataLines[i].Length - 1);
                        string[] new_lineData = pre_lineData.Split('[');
                        string lineData = new_lineData[1];

                        if (!string.IsNullOrEmpty(lineData))
                        {
                            string[] values = lineData.Split('|');

                            int countryID = int.Parse(values[0]);
                            int money = int.Parse(values[1]);
                            int food = int.Parse(values[2]);
                            int recroots = int.Parse(values[3]);

                            string ideology = values[4];

                            foreach (CountryInfoData countryInfoData in localCountryInfoDataList)
                            {
                                if (countryInfoData.countryID == countryID)
                                {
                                    countryInfoData.newMoney = money;
                                    countryInfoData.newFood = food;
                                    countryInfoData.newRecroots = recroots;

                                    countryInfoData.newIdeology = ideology;
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }

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
                string eventPath = Path.Combine(Application.persistentDataPath, "localMods", $"{loadedModName}", "events", $"{eventId}", $"{eventId}.AEEvent");
                string eventData = "";

                EventCreatorManager.ModEvent _event = new EventCreatorManager.ModEvent();

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
                catch (Exception) {}

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
                catch (Exception) {}

                _localEvent_conditions = GetValue(eventLines[7]).Split('@');
                _localEvent_imagePath = $"{Application.persistentDataPath}/savedMods/{loadedModName}/events/{_localEvent_ID}/{_localEvent_ID}.jpg";

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

                _event.id = _localEvent_ID;
                _event._name = _localEvent_Name;
                _event.description = _localEvent_Description;
                _event.date = _localEvent_Date;
                _event.receivers = _localEvent_receivers;
                _event.exceptionsReceivers = _localEvent_receiversBlacklist;

                if (EventImageTexture != null)
                {
                    StartCoroutine(_localEvent_imagePath);

                    _event.texture = EventImageTexture;
                }

                _event.silentEvent = _localEvent_silentEventBoolean;
                _event.conditions = _localEvent_conditions.ToList();

                _eventCreatorManager.modEvents.Add(_event);
            }
        }
    }

    public void OpenTab(GameObject currentTab)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
            currentTab.SetActive(true);
        }
    }

    public void UpdateCountriesList()
    {
        foreach (Transform child in countrySelectionPanelGrid.transform)
        {
            Destroy(child.gameObject);
        }

        globalCountries = globalCountries.OrderBy(x => x.country._name).ToArray();

        for (int i = 0; i < globalCountries.Length; i++)
        {
            GameObject spawnedPrefab = Instantiate(countrySelectionPrefab, countrySelectionPanelGrid.transform);

            spawnedPrefab.GetComponent<SelectCountryButton>().country = globalCountries[i];
            spawnedPrefab.GetComponent<SelectCountryButton>().map_editor = true;
            spawnedPrefab.GetComponent<SelectCountryButton>().UpdateUI();
        }
    }

    public void UpdateCountriesInModList()
    {
        List<CountrySettings> countriesInMod = new List<CountrySettings>();

        foreach (RegionManager province in ReferencesManager.Instance.countryManager.regions)
        {
            if (!countriesInMod.Contains(province.currentCountry))
            {
                countriesInMod.Add(province.currentCountry);
            }
        }

        countriesInMod.Distinct();

        foreach (Transform child in countrySelectionPanelGrid.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < countriesInMod.Count; i++)
        {
            GameObject spawnedPrefab = Instantiate(countryInModSelectionPrefab, countrySelectionPanelGrid.transform);

            spawnedPrefab.GetComponent<SelectCountryButton>().country = countriesInMod[i];
            spawnedPrefab.GetComponent<SelectCountryButton>().map_editor = true;
            spawnedPrefab.GetComponent<SelectCountryButton>().UpdateUI();
        }
    }

    private IEnumerator DownloadEventImage(string url)
    {
        UnityWebRequest wwwEventImageRequest = UnityWebRequestTexture.GetTexture(url);
        yield return wwwEventImageRequest.SendWebRequest();

        EventImageTexture = DownloadHandlerTexture.GetContent(wwwEventImageRequest);
    }

    private IEnumerator LoadMod_Co()
    {
        yield return new WaitForSecondsRealtime(2f);
        LoadMod();
    }

    public void CompileMod()
    {
        int eventsState;

        if (allowEventsToggle.isOn)
        {
            eventsState = 1;
        }
        else
        {
            eventsState = 0;
        }

        currentModText = $"NAME = [{nameInputField.text}];ALLOW_EVENTS = [{eventsState}];";
        CompileCountries();
        CompileRegions();
        CompileCountriesData();
        CompileEvents();
        CheckPublish();

        CheckModFolder();
    }

    public void Publish()
    {
        StartCoroutine(Publish_Requiest());
    }

    private IEnumerator Publish_Requiest()
    {
        if (string.IsNullOrEmpty(ReferencesManager.Instance.gameSettings.editingModString.value))
        {
            WWWForm publishRequiestForm = new WWWForm();

            publishRequiestForm.AddField("title", nameInputField.text);
            publishRequiestForm.AddField("description", descInputField.text);
            publishRequiestForm.AddField("data", currentModText);
            publishRequiestForm.AddField("author_id", PlayerPrefs.GetString("nickname"));

            WWW publishRequiest = new WWW("http://our-empire.7m.pl/core/publishMod.php", publishRequiestForm);

            yield return publishRequiest;

            UploadEventFiles();
            UploadPictures();

            if (publishRequiest.isDone)
            {
                successPanel.SetActive(true);
            }
            else
            {
                errorPanel.SetActive(true);
            }

            yield break;
        }
        else
        {
            WWWForm updateRequiestForm = new WWWForm();

            updateRequiestForm.AddField("id", modID);
            updateRequiestForm.AddField("data", currentModText);
            updateRequiestForm.AddField("author_id", PlayerPrefs.GetString("nickname"));

            WWW uploadRequiest = new WWW("http://our-empire.7m.pl/core/updateMod.php", updateRequiestForm);

            yield return uploadRequiest;

            UploadEventFiles();
            UploadPictures();

            if (uploadRequiest.isDone)
            {
                successPanel.SetActive(true);
            }
            else
            {
                errorPanel.SetActive(true);
            }

            yield break;
        }
    }

    public void PlayTestMod()
    {
        if (string.IsNullOrEmpty(nameInputField.text))
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                WarningManager.Instance.Warn("Enter mod name.");
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                WarningManager.Instance.Warn("Введите имя мода.");
            }
        }
        if (string.IsNullOrEmpty(currentModText))
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                WarningManager.Instance.Warn("Save the mod first.");
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                WarningManager.Instance.Warn("Сначала сохраните мод.");
            }
        }
        else if (!string.IsNullOrEmpty(currentModText) && !string.IsNullOrEmpty(nameInputField.text))
        {
            modData = currentModText;
            CompileMod();

            if (!string.IsNullOrEmpty(modData))
            {
                ReferencesManager.Instance.gameSettings.jsonTest = false;

                PlayerPrefs.DeleteKey("CURRENT_MOD_PLAYING");

                PlayerPrefs.SetString("CURRENT_MOD_PLAYTESTING", nameInputField.text);
                ReferencesManager.Instance.gameSettings.playTestingMod.value = true;
                ReferencesManager.Instance.gameSettings.playMod.value = false;
                ReferencesManager.Instance.gameSettings.loadGame.value = false;

                SceneManager.LoadScene(1);
            }
        }
    }

    public void CreateEventAsset(int eventId)
    {
        CreateFolder(Path.Combine(Application.persistentDataPath), "localMods");
        CreateFolder(Path.Combine(Application.persistentDataPath, "localMods"), nameInputField.text);
        CreateFolder(Path.Combine(Application.persistentDataPath, "localMods",  nameInputField.text), "events");

        for (int i = 0; i < _eventCreatorManager.modEvents.Count; i++)
        {
            if (_eventCreatorManager.modEvents[i].id == eventId)
            {
                EventCreatorManager.ModEvent modEvent = _eventCreatorManager.modEvents[i];

                CreateFolder(Path.Combine(Application.persistentDataPath, "localMods", nameInputField.text, "events"), $"{modEvent.id}");

                string path = Path.Combine(Application.persistentDataPath, "localMods", $"{nameInputField.text}", "events", $"{eventId}");

                if (!File.Exists(Path.Combine(path, $"{eventId}.AEEvent")))
                {
                    if (modEvent.silentEvent) silentEvent = "1";
                    else silentEvent = "0";

                    string buttons_data = "";
                    string conditions_data = "";
                    string receivers_data = "";
                    string receiversBlacklist_data = "";

                    for (int r = 0; r < modEvent.receivers.Count; r++)
                    {
                        string endline = "";

                        if (modEvent.receivers.Count != r)
                        {
                            endline = "-";
                        }

                        receivers_data += $"{modEvent.receivers[r]}{endline}";
                    }

                    for (int rb = 0; rb < modEvent.exceptionsReceivers.Count; rb++)
                    {
                        string endline = "";

                        if (modEvent.receivers.Count != rb)
                        {
                            endline = "-";
                        }

                        receiversBlacklist_data += $"{modEvent.exceptionsReceivers[rb]}{endline}";
                    }

                    for (int b = 0; b < modEvent.buttons.Count; b++)
                    {
                        string actions_data = "";

                        if (modEvent.buttons[b].rejectUltimatum)
                        {
                            rejectUltimatum = "1";
                        }
                        else
                        {
                            rejectUltimatum = "0";
                        }

                        if (modEvent.buttons[b].actions.Count > 0)
                        {
                            actions_data = "";

                            for (int a = 0; a < modEvent.buttons[b].actions.Count; a++)
                            {
                                string action = modEvent.buttons[b].actions[a];

                                if (!string.IsNullOrEmpty(action))
                                {
                                    actions_data += $"{action.Replace(';', '-')}@";
                                }
                            }
                        }
                        else if (modEvent.buttons[b].actions.Any() || modEvent.buttons[b].actions.Count <= 0)
                        {
                            actions_data = "@";
                        }

                        buttons_data +=
                                $"{modEvent.buttons[b].name}|" +
                                $"{actions_data}|" +
                                $"{rejectUltimatum};";
                    }

                    if (modEvent.conditions.Count > 0 || !modEvent.conditions.Any())
                    {
                        conditions_data = "";

                        for (int c = 0; c < modEvent.conditions.Count; c++)
                        {
                            string condition = modEvent.conditions[c];

                            if (!string.IsNullOrEmpty(condition))
                            {
                                conditions_data += $"{condition.Replace(';', '-')}@";
                            }
                        }
                    }
                    else if (modEvent.conditions.Any() || modEvent.conditions.Count <= 0)
                    {
                        conditions_data = "@";
                    }

                    assetData =
                        $"eventID = {modEvent.id};\n" +
                        $"name = {modEvent._name};\n" +
                        $"description = {modEvent.description};\n" +
                        $"date = {modEvent.date};\n" +
                        $"silentEvent = {silentEvent};\n" +
                        $"receivers = {receivers_data};\n" +
                        $"receiversBlacklist = {receiversBlacklist_data};\n" +
                        $"conditions:\n{conditions_data};\n" +
                        $"buttons:\n{buttons_data}\n";

                    CreateFile(assetData, Path.Combine(path, $"{eventId}.AEEvent"));
                }
            }
        }
    }

    public void CheckPublish()
    {
        if (string.IsNullOrEmpty(ReferencesManager.Instance.gameSettings.editingModString.value))
        {
            if (!string.IsNullOrEmpty(currentModText) && !string.IsNullOrWhiteSpace(currentModText))
            {
                if (!string.IsNullOrEmpty(nameInputField.text) && !string.IsNullOrWhiteSpace(nameInputField.text))
                {
                    if (!string.IsNullOrEmpty(descInputField.text) && !string.IsNullOrWhiteSpace(descInputField.text))
                    {
                        publishButton.interactable = true;
                    }
                }
            }
        }
        else
        {
            publishButton.interactable = true;
        }
    }


    public void QuitToMenu()
    {
        SceneManager.LoadScene(0);

        ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
        ReferencesManager.Instance.gameSettings.playMod.value = false;
        ReferencesManager.Instance.gameSettings.loadGame.value = false;
    }

    private void CompileRegions()
    {
        currentModText += "#REGIONS#;";

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            RegionManager region = ReferencesManager.Instance.countryManager.regions[i];
            currentModText += $"REGION_{region._id} = {region.currentCountry.country._id};";
        }

        currentModText += "#REGIONS#;";
    }

    private void CompileEvents()
    {
        currentModText += $"#EVENTS#;";

        for (int i = 0; i < _eventCreatorManager.modEvents.Count; i++)
        {
            try
            {
                File.Delete(Path.Combine(Application.persistentDataPath, "localmods", nameInputField.text, "events", $"{_eventCreatorManager.modEvents[i].id}", $"{_eventCreatorManager.modEvents[i].id}.AEEvent"));
            }
            catch (Exception)
            {

            }
            CreateEventAsset(_eventCreatorManager.modEvents[i].id);

            currentModText += $"{_eventCreatorManager.modEvents[i].id};";
        }

        currentModText += $"#EVENTS#;";
    }

    public void UploadFile(string filepath, string filename, string fileextension, string modName, int eventID)
    {
        StartCoroutine(Upload(filepath, filename, fileextension, modName, eventID));
    }

    private IEnumerator Upload(string filepath, string filename, string fileextension, string modName, int eventID)
    {
        string fileBytes = File.ReadAllText(filepath);

        byte[] myData = Encoding.UTF8.GetBytes(fileBytes);

        if (fileextension == "jpg")
        {
            myData = GetTextureCopy(NativeGallery.LoadImageAtPath(filepath)).EncodeToJPG();
        }

        WWWForm form = new WWWForm();

        form.AddField("modName", modName);
        form.AddField("eventID", eventID);
        form.AddBinaryData("file", myData);
        form.AddField("filename", filename);
        form.AddField("fileextension", fileextension);

        WWW www = new WWW("http://absolute-empire.7m.pl/uploadFile.php", form);

        yield return www;

        if (!www.isDone)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log($"Upload complete! {www.text}");
        }
    }

    private void UploadPictures()
    {
        for (int i = 0; i < _eventCreatorManager.modEvents.Count; i++)
        {
            EventCreatorManager.ModEvent modEvent = _eventCreatorManager.modEvents[i];

            string path = Path.Combine(Application.persistentDataPath, "localMods", $"{nameInputField.text}", "events", $"{modEvent.id}", $"{modEvent.id}.jpg");
            UploadFile(path, $"{modEvent.id}", "jpg", nameInputField.text, modEvent.id);
        }
    }

    private void UploadEventFiles()
    {
        for (int i = 0; i < _eventCreatorManager.modEvents.Count; i++)
        {
            EventCreatorManager.ModEvent modEvent = _eventCreatorManager.modEvents[i];

            string path = Path.Combine(Application.persistentDataPath, "localMods", $"{nameInputField.text}", "events", $"{modEvent.id}", $"{modEvent.id}.AEEvent");

            UploadFile(path, $"{modEvent.id}", "AEEvent", nameInputField.text, modEvent.id);
        }
    }

    private Texture2D GetTextureCopy(Texture2D source)
    {
        //Create a RenderTexture
        RenderTexture rt = RenderTexture.GetTemporary(
                               source.width,
                               source.height,
                               0,
                               RenderTextureFormat.Default,
                               RenderTextureReadWrite.Linear
                           );

        //Copy source texture to the new render (RenderTexture) 
        Graphics.Blit(source, rt);

        //Store the active RenderTexture & activate new created one (rt)
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        //Create new Texture2D and fill its pixels from rt and apply changes.
        Texture2D readableTexture = new Texture2D(source.width, source.height);
        readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTexture.Apply();

        //activate the (previous) RenderTexture and release texture created with (GetTemporary( ) ..)
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return readableTexture;
    }

    private void CompileCountries()
    {
        for (int i = 0; i < globalCountries.Length; i++)
        {
            if (globalCountries[i].myRegions.Count > 0)
            {
                currentModText += $"COUNTRY = {globalCountries[i].country._id};";
            }
        }
    }

    private void CompileCountriesData()
    {
        currentModText += "#COUNTRIES_SETTINGS#;";

        foreach (CountryInfoData countryInfoData in localCountryInfoDataList)
        {
            currentModText += $"COUNTRY_DATA ={countryInfoData.countryID}|{countryInfoData.newMoney}|{countryInfoData.newFood}|{countryInfoData.newRecroots}|{countryInfoData.newIdeology};";
        }

        currentModText += "#COUNTRIES_SETTINGS#;";
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

    private void CheckModFolder()
    {
        string localModsPath = Path.Combine(Application.persistentDataPath, $"localmods");
        string modPath = Path.Combine(localModsPath, $"{nameInputField.text}");

        string modDataFilePath = Path.Combine(modPath, $"{nameInputField.text}.AEMod");

        if (!Directory.Exists(modPath))
        {
            CreateFolder("", "localmods");
            CreateFolder("localmods", nameInputField.text);
        }

        CreateFile(currentModText, modDataFilePath);
    }

    private void CreateFile(string fileText, string path)
    {
        StreamWriter streamWriter;
        FileInfo file = new FileInfo(path);
        streamWriter = file.CreateText();

        streamWriter.Write(fileText);
        streamWriter.Close();
    }

    public void CreateFolder(string _path, string folderName)
    {
        string path = Path.Combine(Application.persistentDataPath, $"{_path}");
        path = Path.Combine(path, $"{folderName}");

        Directory.CreateDirectory(path);
    }

    public bool IsNullOrEmpty(Array array)
    {
        return (array == null || array.Length == 0);
    }


    public void CreateCountry()
    {
        ModCustomCountry newCountry = new ModCustomCountry();

        string _name = "";
        string _nameEN = "";
        string _ideology = "";
        string _tag = "";
        int _capLimit = 0;

        if (!string.IsNullOrEmpty(_countryName_RU_Inputfield.text))
        {
            _name = _countryName_RU_Inputfield.text;
        }
        else
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                WarningManager.Instance.Warn("Please enter the country name in Russian language");
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                WarningManager.Instance.Warn("Пожалуйста, введите название страны на русском языке");
            }
        }

        if (!string.IsNullOrEmpty(_countryName_EN_Inputfield.text))
        {
            _nameEN = _countryName_EN_Inputfield.text;
        }
        else
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                WarningManager.Instance.Warn("Please enter the country name in English language");
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                WarningManager.Instance.Warn("Пожалуйста, введите название страны на английском языке");
            }
        }

        if (!string.IsNullOrEmpty(_countryTag_Inputfield.text))
        {
            _tag = _countryTag_Inputfield.text;
        }
        else
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                WarningManager.Instance.Warn("Please enter the country tag");
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                WarningManager.Instance.Warn("Пожалуйста, введите тэг страны");
            }
        }

        if (!string.IsNullOrEmpty(_countryCapLimit_Inputfield.text))
        {
            _capLimit = 100 - int.Parse(_countryCapLimit_Inputfield.text);
        }
        else
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                WarningManager.Instance.Warn("Please enter the country capitulation percent");
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                WarningManager.Instance.Warn("Пожалуйста, укажите процент капитуляции страны");
            }
        }

        newCountry._name = _name;
        newCountry._nameEN = _nameEN;
        newCountry.tag = _tag;
        newCountry.capitulationLimit = _capLimit;

        modCustomCountries.Add(newCountry);
    }

    public void LoadCountryFlag(string ideology)
    {
        int currentImageIndex = 0;

        if (ideology == "Неопределено" || ideology == "Неопределённый")
        {
            currentImageIndex = 0;
        }

        if (ideology == "Демократия")
        {
            currentImageIndex = 1;
        }

        if (ideology == "Монархия")
        {
            currentImageIndex = 2;
        }

        if (ideology == "Фашизм")
        {
            currentImageIndex = 3;
        }

        if (ideology == "Коммунизм")
        {
            currentImageIndex = 4;
        }

        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            NativeGallery.GetImageFromGallery((path) =>
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path);
                if (texture == null)
                {
                    WarningManager.Instance.Warn($"Couldn't load texture from + {path}");
                    return;
                }

                string fileName = $"{_countryName_EN_Inputfield.text}_{currentImageIndex}.jpg";

                string eventImageOriginPath = Path.Combine(path);
                string eventImageDestinationPath = Path.Combine(Application.persistentDataPath, "localMods", nameInputField.text, "countries", $"{_countryName_EN_Inputfield.text}", fileName);

                CreateFolder(Path.Combine(Application.persistentDataPath), "localMods");
                CreateFolder(Path.Combine(Application.persistentDataPath, "localMods"), nameInputField.text);
                CreateFolder(Path.Combine(Application.persistentDataPath, "localMods", nameInputField.text), "events");
                CreateFolder(Path.Combine(Application.persistentDataPath, "localMods", nameInputField.text, "events"), $"{_eventCreatorManager.modEvents[_eventCreatorManager.currentModEventIndex].id}");

                if (File.Exists(eventImageOriginPath))
                {
                    File.Copy(eventImageOriginPath, eventImageDestinationPath);
                }

                Texture2D finalTexture = NativeGallery.LoadImageAtPath(eventImageDestinationPath);

                _countryImages[currentImageIndex].sprite = Sprite.Create(finalTexture, new Rect(0, 0, finalTexture.width, finalTexture.height), Vector2.zero);
            });
        }
        else
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                WarningManager.Instance.Warn("Enter mod name.");
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                WarningManager.Instance.Warn("Введите имя мода.");
            }
        }
    }


    public class ModCustomCountry
    {
        public int id;
        public string _name;
        public string _nameEN;
        public string tag;
        public int capitulationLimit;

        public Sprite countryFlag;

        public Color color;

        public Color[] ideologyColors;
        public Sprite[] ideologyFlags;
    }
}
