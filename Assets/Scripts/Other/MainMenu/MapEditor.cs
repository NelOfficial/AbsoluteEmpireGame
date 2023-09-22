using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using UnityEditor;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditor.VersionControl;
using System.Net;
using System;
using UnityEngine.Networking;

public class MapEditor : MonoBehaviour
{
    [SerializeField] GameObject countrySelectionPrefab;
    [SerializeField] GameObject countrySelectionPanelGrid;
    [SerializeField] GameObject container;

    public bool paintMapMode;

    public CountrySettings selectedCountry;

    [SerializeField] CountrySettings[] globalCountries;

    [SerializeField] GameObject[] tabs;

    private bool isOpen;

    [SerializeField] Button publishButton;
    public TMP_InputField nameInputField;
    [SerializeField] TMP_InputField descInputField;
    [SerializeField] Toggle allowEventsToggle;

    [SerializeField] GameObject successPanel;
    [SerializeField] GameObject errorPanel;

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


    private void Start()
    {
        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Length; i++)
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
            string[] dataParts = modData.Split("##########");
            string[] mainModDataLines = dataParts[0].Split(';');
            string[] regionsDataLines = dataParts[1].Split(';');

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

            for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Length; i++)
            {
                try
                {
                    string _line = regionsDataLines[i];
                    if (_line != "##########")
                    {
                        string[] regionIdParts = _line.Split(' ');
                        regionValue = regionIdParts[0].Remove(0, 7);
                    }

                    if ((ReferencesManager.Instance.countryManager.regions[i]._id) == int.Parse(regionValue)) // - 1
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
                catch (System.Exception)
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
                WarningManager.Instance.Warn("¬ведите им€ мода.");
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
            }
        }

        string path = Path.Combine(Application.persistentDataPath, "localMods", $"{nameInputField.text}", "events", $"{eventId}");
        string assetsPath = Path.Combine("Assets", "localEvents");

        AssetDatabase.CreateAsset(asset, Path.Combine(assetsPath, $"{eventId}.asset"));
        AssetDatabase.SaveAssets();

        if (File.Exists(Path.Combine(assetsPath, $"{eventId}.asset")))
        {
            File.Move(Path.Combine(assetsPath, $"{eventId}.asset"), Path.Combine(path, $"{eventId}.asset"));
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
        currentModText += "##########;";

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Length; i++)
        {
            RegionManager region = ReferencesManager.Instance.countryManager.regions[i];
            currentModText += $"REGION_{region._id} = [{region.currentCountry.country._id}];";
        }
    }

    private void CompileEvents()
    {
        for (int i = 0; i < _eventCreatorManager.modEvents.Count; i++)
        {
            CreateEventAsset(_eventCreatorManager.modEvents[i].id);
        }

        UploadEventFiles();
        UploadPictures();
    }

    private void UploadPictures()
    {
        for (int i = 0; i < _eventCreatorManager.modEvents.Count; i++)
        {
            EventCreatorManager.ModEvent modEvent = _eventCreatorManager.modEvents[i];

            string path = Path.Combine(Application.persistentDataPath, "localMods", $"{nameInputField.text}", "events", $"{modEvent.id}");
            string filePath = $"{Path.Combine(path, $"{modEvent.id}.jpg")}";

            string destinationPath = $"http://absolute-empire.7m.pl/media/uploads/mods/{nameInputField.text}/events/{modEvent.id}";

            StartCoroutine(UploadImage($"{modEvent.id}", modEvent.texture, nameInputField.text, modEvent.id));
        }
    }

    private IEnumerator UploadImage(string fileName, Texture2D source, string modName, int eventID)
    {
        WWWForm form = new WWWForm();
        Texture2D imageTexture_copy = GetTextureCopy(source);

        byte[] fileBytes = imageTexture_copy.EncodeToPNG();

        form.AddBinaryData("file", fileBytes, $"{fileName}.jpg", $"image/jpg");
        form.AddField("modname", modName);
        form.AddField("eventID", eventID);

        WWW w = new WWW("http://absolute-empire.7m.pl/uploadImage.php", form);

        yield return w;

        if (w.error != null)
        {
            //error : 
            Debug.Log(w.error);
        }
        else
        {
            //success
            Debug.Log(w.text);
        }

        w.Dispose();
    }

    private void UploadEventFiles()
    {
        for (int i = 0; i < _eventCreatorManager.modEvents.Count; i++)
        {
            EventCreatorManager.ModEvent modEvent = _eventCreatorManager.modEvents[i];

            string path = Path.Combine(Application.persistentDataPath, "localMods", $"{nameInputField.text}", "events", $"{modEvent.id}");
            string filePath = $"{Path.Combine(path, $"{modEvent.id}.asset")}";

            string destinationPath = $"http://absolute-empire.7m.pl/media/uploads/mods/{nameInputField.text}/events/{modEvent.id}";

            //StartCoroutine(UploadFile(filePath, $"{modEvent.id}.asset", destinationPath));
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

    private string GetValue(string[] _lines, int line)
    {
        string _line = _lines[line];
        string part = _line.Split('[')[1];
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
    }

    public void CreateFolder(string _path, string folderName)
    {
        string path = Path.Combine(Application.persistentDataPath, $"{_path}");
        path = Path.Combine(path, $"{folderName}");

        Directory.CreateDirectory(path);
    }
}
