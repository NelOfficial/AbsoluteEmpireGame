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


    private void Start()
    {
        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            for (int r = 0; r < ReferencesManager.Instance.countryManager.countries[i].myRegions.Count; r++)
            {
                ReferencesManager.Instance.countryManager.regions.Add(ReferencesManager.Instance.countryManager.countries[i].myRegions[r]);
            }
        }

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            ReferencesManager.Instance.countryManager.regions[i]._id = i;
        }

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            RegionManager region = ReferencesManager.Instance.countryManager.regions[i];
            region._id = i;
        }

        ReferencesManager.Instance.regionManager.UpdateRegions();

        if (PlayerPrefs.HasKey("CURRENT_EDITING_MODIFICATION"))
        {
            LoadMod();
        }

        _eventCreatorManager = FindObjectOfType<EventCreatorManager>();
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
        modID = PlayerPrefs.GetInt("CURRENT_EDITING_MODIFICATION");
        string loadedModName = PlayerPrefs.GetString($"MODIFICATION_{modID}");
        string loadedModPath = Path.Combine(Application.persistentDataPath, "savedMods", $"{loadedModName}");

        StreamReader reader = new StreamReader(Path.Combine(loadedModPath, $"{loadedModName}.AEMod"));
        modData = reader.ReadToEnd();

        reader.Close();

        if (Directory.Exists(loadedModPath))
        {
            string[] mainModDataLines = modData.Split("#REGIONS#")[0].Split(';');
            string[] regionsDataLines = modData.Split("#REGIONS#")[1].Split(';');
            string[] countriesDataLines = modData.Split("#COUNTRIES_SETTINGS#")[0].Split(';');
            string[] eventsIDsDataLines = modData.Split("#EVENTS#")[0].Split(';');

            try
            {
                string _line = mainModDataLines[0];
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
            nameInputField.text = $"{value}";

            List<int> countriesInRegionsIDs = new List<int>();

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
                        Debug.LogError($"ERROR: Mod loader error in value parser (MapEditor.cs)");
                    }
                }
            }

            for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
            {
                try
                {
                    string _line = regionsDataLines[i];
                    if (_line != "#REGIONS#")
                    {
                        string[] regionIdParts = _line.Split(' ');
                        regionValue = regionIdParts[0].Remove(0, 7);
                    }

                    if (ReferencesManager.Instance.countryManager.regions[i]._id == int.Parse(regionValue)) // - 1
                    {
                        for (int c = 0; c < globalCountries.Length; c++)
                        {
                            if (globalCountries[c].country._id == countriesInRegionsIDs[i])
                            {
                                ReferencesManager.Instance.PaintRegion(ReferencesManager.Instance.countryManager.regions[i], globalCountries[c]);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    if (ReferencesManager.Instance.gameSettings.developerMode)
                    {
                        Debug.LogError($"ERROR: Mod loader error in regionValue parser (MapEditor.cs)");
                    }
                }
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
        if (!PlayerPrefs.HasKey("CURRENT_EDITING_MODIFICATION"))
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
        else
        {
            modData = currentModText;
            CompileMod();

            if (!string.IsNullOrEmpty(modData))
            {
                ReferencesManager.Instance.gameSettings.jsonTest = true;

                PlayerPrefs.SetString("CURRENT_MOD_PLAYTESTING", nameInputField.text);

                SceneManager.LoadScene(1);
            }
        }
    }

    public void CreateEventAsset(int eventId)
    {
        EventScriptableObject asset = ScriptableObject.CreateInstance<EventScriptableObject>();

        CreateFolder(Path.Combine(Application.persistentDataPath), "localMods");
        CreateFolder(Path.Combine(Application.persistentDataPath, "localMods"), nameInputField.text);
        CreateFolder(Path.Combine(Application.persistentDataPath, "localMods",  nameInputField.text), "events");

        for (int i = 0; i < _eventCreatorManager.modEvents.Count; i++)
        {
            if (_eventCreatorManager.modEvents[i].id == eventId)
            {
                EventCreatorManager.ModEvent modEvent = _eventCreatorManager.modEvents[i];

                CreateFolder(Path.Combine(Application.persistentDataPath, "localMods", nameInputField.text, "events"), $"{modEvent.id}");

                asset.id = modEvent.id;
                asset._name = modEvent._name;
                asset.description = modEvent.description;

                for (int b = 0; b < modEvent.buttons.Count; b++)
                {
                    asset.buttons.Add(modEvent.buttons[b]);
                }

                for (int b = 0; b < modEvent.conditions.Count; b++)
                {
                    asset.conditions.Add(modEvent.conditions[b]);
                }

                string path = Path.Combine(Application.persistentDataPath, "localMods", $"{nameInputField.text}", "events", $"{eventId}");

                if (!File.Exists(Path.Combine(path, $"{eventId}.AEEvent")))
                {
                    if (modEvent.silentEvent) silentEvent = "1";
                    else silentEvent = "0";

                    string buttons_data = "";
                    string conditions_data = "";

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

                            actions_data += "[\n";

                            for (int a = 0; a < modEvent.buttons[b].actions.Count; a++)
                            {
                                string action = modEvent.buttons[b].actions[a];

                                if (!string.IsNullOrEmpty(action))
                                {
                                    actions_data += $"act = {action};\n";
                                }
                            }

                            actions_data += "];";
                        }
                        else if (modEvent.buttons[b].actions.Any() || modEvent.buttons[b].actions.Count <= 0)
                        {
                            actions_data = "0;\n";
                        }

                        buttons_data +=
                                $"{b}[\nname = {modEvent.buttons[b].name};\n" +
                                $"actions = {actions_data}\n" +
                                $"rejectUltimatum = {rejectUltimatum};\n];\n";
                    }

                    if (modEvent.conditions.Count > 0 || !modEvent.conditions.Any())
                    {
                        conditions_data = "";
                        conditions_data += "[\n";

                        for (int c = 0; c < modEvent.conditions.Count; c++)
                        {
                            string condition = modEvent.conditions[c];

                            if (!string.IsNullOrEmpty(condition))
                            {
                                conditions_data += $"cond = {condition};\n";
                            }
                        }

                        conditions_data += "]";
                    }
                    else if (modEvent.conditions.Any() || modEvent.conditions.Count <= 0)
                    {
                        conditions_data = "0;\n";
                    }

                    assetData =
                        $"eventID = {modEvent.id};\n" +
                        $"name = {modEvent._name};\n" +
                        $"description = {modEvent.description};\n" +
                        $"date = {modEvent.date};\n" +
                        $"silentEvent = {silentEvent};\n" +
                        $"conditions = {conditions_data};\n" +
                        $"buttons = {buttons_data};\n";

                    CreateFile(assetData, Path.Combine(path, $"{eventId}.AEEvent"));
                }
            }
        }
    }

    public void CheckPublish()
    {
        if (!PlayerPrefs.HasKey("CURRENT_EDITING_MODIFICATION"))
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
        PlayerPrefs.DeleteKey("CURRENT_EDITING_MODIFICATION");
        SceneManager.LoadScene(0);
    }

    private void CompileRegions()
    {
        currentModText += "#REGIONS#;";

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            RegionManager region = ReferencesManager.Instance.countryManager.regions[i];
            currentModText += $"REGION_{region._id} = [{region.currentCountry.country._id}];";
        }
    }

    private void CompileEvents()
    {
        currentModText += $"#EVENTS#;";

        for (int i = 0; i < _eventCreatorManager.modEvents.Count; i++)
        {
            CreateEventAsset(_eventCreatorManager.modEvents[i].id);

            currentModText += $"{_eventCreatorManager.modEvents[i].id};";
        }
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

            UploadFile(path, $"{modEvent.id}", "asset", nameInputField.text, modEvent.id);
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
                currentModText += $"COUNTRY = [{globalCountries[i].country._id}];";
            }
        }
    }

    private void CompileCountriesData()
    {
        currentModText += "#COUNTRIES_SETTINGS#;";

        foreach (CountryInfoData countryInfoData in localCountryInfoDataList)
        {
            currentModText += $"COUNTRY_DATA = [{countryInfoData.countryID}|{countryInfoData.newMoney}|{countryInfoData.newFood}|{countryInfoData.newRecroots}|{countryInfoData.newIdeology}];";
        }
    }

    private string GetValue(string line)
    {
        string[] parts = line.Split('[');

        string part = parts[1];
        string value = part.Remove(part.Length - 1);

        return value;
    }

    private void CheckModFolder()
    {
        string localModsPath = Path.Combine(Application.persistentDataPath, $"localmods");
        string modPath = Path.Combine(localModsPath, $"{nameInputField.text}");

        string modDataFilePath = Path.Combine(modPath, $"{nameInputField.text}_modData.AEMod");

        if (!Directory.Exists(modPath))
        {
            CreateFolder("", "localmods");
            CreateFolder("localmods", nameInputField.text);
        }

        CreateFile(modData, modDataFilePath);
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
}
