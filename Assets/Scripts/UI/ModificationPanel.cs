using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;
using System.IO;
using System.Net;
using System;

public class ModificationPanel : MonoBehaviour
{
    public GameObject wallAnimationUI;
    public GameObject wallItemPrefab;
    public Transform wallContrainer;

    [SerializeField] Button refreshButton;
    [SerializeField] TMP_Text refreshButtonText;

    [SerializeField] TMP_Text modName_TMP;
    [SerializeField] TMP_Text modDesc_TMP;

    public bool updatingFeed;
    public int lastScenarios;

    public Modification currentLoadedModification;

    public List<int> loadedModificationsIds = new List<int>();
    public List<Modification> loadedModifications = new List<Modification>();

    [SerializeField] BoolValue jsonRead;

    [SerializeField] GameObject localModButtonPrefab;
    [SerializeField] Transform localModsContainer;

    public ModListValue downloadedModsIds;
    public BoolValue isPlayModObject;

    public MainMenu mainMenu;

    [SerializeField] Button downloadButton;

    [SerializeField] Transform countriesListContainer;
    public GameObject countriesListPanel;
    [SerializeField] GameObject selectCountryButtonPrefab;

    private int maxPage = 1;
    private int selectedPage = 0;

    public int postsPerPage = 10;
    public int maxLoadPostsPerTime = 20;

    [SerializeField] TMP_Text pageText;
    public List<CountryScriptableObject> currentModCountries = new List<CountryScriptableObject>();

    private void Start()
    {
        SetPage(0);

        UpdateDownloadedMods();
    }

    public void UpdateDownloadedMods()
    {
        string loadedModsIds_data = PlayerPrefs.GetString("MODS_IDS");

        downloadedModsIds.list.Clear();

        if (!string.IsNullOrEmpty(loadedModsIds_data))
        {
            string[] loadedModsIds = loadedModsIds_data.Split(';');

            for (int i = 0; i < loadedModsIds.Length; i++)
            {
                string modName = PlayerPrefs.GetString($"MODIFICATION_{int.Parse(loadedModsIds[i].Split('_')[0])}");

                if (Directory.Exists(Path.Combine(Application.persistentDataPath, "savedMods", $"{modName}")))
                {
                    ModListValue.LocalSavedModification mod = new ModListValue.LocalSavedModification();
                    mod.id = int.Parse(loadedModsIds[i].Split('_')[0]);
                    mod.version = int.Parse(loadedModsIds[i].Split('_')[1]);

                    downloadedModsIds.list.Add(mod);
                }
            }
        }
    }

    public void UpdateScenariosList()
    {
        StartCoroutine(GetIds(true));
        StartCoroutine(WallUpdate());
        lastScenarios = loadedModificationsIds.Count;
        refreshButton.image.color = Color.white;
    }

    [System.Obsolete]
    private IEnumerator GetIds(bool useUI)
    {
        if (useUI)
        {
            wallAnimationUI.SetActive(true);
            refreshButton.interactable = false;

            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                refreshButtonText.text = "Loading IDs...";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                refreshButtonText.text = "Загрузка id сценариев...";
            }
        }

        WWWForm form = new WWWForm();
        form.AddField("maxPosts", maxLoadPostsPerTime + loadedModifications.Count);
        int offset = postsPerPage * selectedPage - postsPerPage;

        if (loadedModifications.Count <= 0)
        {
            offset = 0;
        }

        form.AddField("offset", offset);
        WWW getPostRequest = new WWW("http://our-empire.7m.pl/core/getPosts.php", form);

        yield return getPostRequest;

        string[] request = getPostRequest.text.Split('\t');

        loadedModificationsIds.Clear();

        for (int i = 0; i < request.Length; i++)
        {
            try
            {
                loadedModificationsIds.Add(int.Parse(request[i]));
            }
            catch (System.Exception)
            {
                Debug.Log(getPostRequest.text);
            }
        }

        if (loadedModificationsIds.Count > lastScenarios)
        {
            refreshButton.image.color = Color.yellow;
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                refreshButtonText.text = $"{loadedModificationsIds.Count - lastScenarios} new mods | Update";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                refreshButtonText.text = $"{loadedModificationsIds.Count - lastScenarios} новых модов | Обновить";
            }
        }

        //yield return new WaitForSeconds(5f);
        //StartCoroutine(GetIds(false));
    }

    private IEnumerator WallUpdate()
    {
        wallAnimationUI.SetActive(true);
        updatingFeed = true;
        refreshButton.interactable = false;

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            refreshButtonText.text = $"Loading data...";
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            refreshButtonText.text = $"Загрузка данных...";
        }

        loadedModifications.Clear();

        if (loadedModifications != null)
        {
            updatingFeed = true;
            for (int i = 0; i < loadedModificationsIds.Count; i++)
            {
                int currentPage = Mathf.CeilToInt(i / postsPerPage);

                maxPage = currentPage;

                // Getting Post with id

                WWWForm form = new WWWForm();
                form.AddField("id", loadedModificationsIds[i]);

                // делаем запрос на сайт мой и оттуда нам возвращают данные об айдишниках модов
                WWW getPostRequest = new WWW("http://our-empire.7m.pl/core/getPostById.php", form);

                yield return getPostRequest;

                string[] request = getPostRequest.text.Split('\t');

                //далее мы по айдишникам находим моды и еще тут можно их выкладывать
                if (!string.IsNullOrEmpty(request[0]))
                {
                    Modification newModification = new Modification();

                    try
                    {
                        newModification.id = int.Parse(request[0]);
                        newModification.page = currentPage;
                        newModification.currentScenarioName = request[1];
                        newModification.currentScenarioDescription = request[2];
                        newModification.currentScenarioDate = request[3];
                        newModification.currentScenarioData = request[4];
                        newModification.currentScenarioAuthorId = request[5];
                        newModification.verified = int.Parse(request[6]);
                        newModification.views = int.Parse(request[7]);
                        newModification.likes = int.Parse(request[8]);
                        newModification.version = int.Parse(request[9]);
                    }
                    catch (System.Exception)
                    {
                        Debug.Log(getPostRequest.text);
                    }

                    // Get Author data with id

                    //WWWForm authorForm = new WWWForm();
                    //authorForm.AddField("id", request[4]);
                    //WWW wwwAuthorRequest = new WWW("http://our-empire.7m.pl/core/getAuthorById.php", authorForm);

                    //yield return wwwAuthorRequest;
                    //string[] postAuthorValues = wwwAuthorRequest.text.Split('\t');

                    //newModification.currentScenarioAuthorName = postAuthorValues[0];

                    loadedModifications.Add(newModification);
                    // Get AuthorAvatar data with url

                    //UnityWebRequest wwwAvatarRequest = UnityWebRequestTexture.GetTexture(postAuthorValues[4]);
                    //yield return wwwAvatarRequest.SendWebRequest();

                    //Texture avatarTexture = DownloadHandlerTexture.GetContent(wwwAvatarRequest);

                    //Sprite avatarSprite = Sprite.Create((Texture2D)avatarTexture, new Rect(0.0f, 0.0f, avatarTexture.width, avatarTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    //walls[i].authorAvatar.sprite = avatarSprite; // apply the new sprite to the image
                }

            }
            updatingFeed = false;

            yield return new WaitForSeconds(2f);
            wallAnimationUI.SetActive(false);
            refreshButton.interactable = true;

            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                refreshButtonText.text = $"Load more";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                refreshButtonText.text = $"Загрузить ещё";
            }
        }
        UpdateUI();
    }

    public void UpdateModUI()
    {
        modName_TMP.text = currentLoadedModification.currentScenarioName;
        modDesc_TMP.text = $"{currentLoadedModification.currentScenarioDescription} \nMade by: {currentLoadedModification.currentScenarioAuthorId}";

        bool alreadyDownloaded = downloadedModsIds.list.Any(item => item.id == currentLoadedModification.id);

        if (alreadyDownloaded)
        {
            foreach (ModListValue.LocalSavedModification localMod in downloadedModsIds.list)
            {
                if (localMod.id == currentLoadedModification.id)
                {
                    if (localMod.version < currentLoadedModification.version)
                    {
                        downloadButton.interactable = true;

                        if (PlayerPrefs.GetInt("languageId") == 0)
                        {
                            downloadButton.transform.GetChild(2).GetComponent<TMP_Text>().text = "Download an update of mod";
                        }
                        else if (PlayerPrefs.GetInt("languageId") == 1)
                        {
                            downloadButton.transform.GetChild(2).GetComponent<TMP_Text>().text = "Скачать обновление";
                        }
                    }
                    else if (localMod.version >= currentLoadedModification.version)
                    {
                        if (PlayerPrefs.GetInt("languageId") == 0)
                        {
                            downloadButton.transform.GetChild(2).GetComponent<TMP_Text>().text = "Save";
                        }
                        else if (PlayerPrefs.GetInt("languageId") == 1)
                        {
                            downloadButton.transform.GetChild(2).GetComponent<TMP_Text>().text = "Сохранить";
                        }

                        downloadButton.interactable = false;
                    }
                }
            }
        }
        else
        {
            downloadButton.interactable = true;

            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                downloadButton.transform.GetChild(2).GetComponent<TMP_Text>().text = "Save";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                downloadButton.transform.GetChild(2).GetComponent<TMP_Text>().text = "Сохранить";
            }
        }
    }

    public void UpdateCountriesList()
    {
        mainMenu.ScrollEffect(countriesListContainer.GetComponent<RectTransform>());

        string[] dataParts = currentLoadedModification.currentScenarioData.Split("##########");
        string[] mainModDataLines = dataParts[0].Split(';');
        string[] regionsDataLines = dataParts[1].Split(';');

        // Get countries in list from mod data

        currentModCountries.Clear();

        for (int i = 2; i < mainModDataLines.Length; i++)
        {
            string _line = mainModDataLines[i];
            string value = "";

            try
            {
                string part = _line.Split('[')[1];
                value = part.Remove(part.Length - 1);
            }
            catch (Exception)
            {
                //Debug.Log($"line error: {_line}");
            }

            for (int c = 0; c < ReferencesManager.Instance.globalCountries.Length; c++)
            {
                CountryScriptableObject country = ReferencesManager.Instance.globalCountries[c];

                bool hasCountryInList = currentModCountries.Any(item => item._id == country._id);

                try
                {
                    if (country._id == int.Parse(value) && !hasCountryInList)
                    {
                        currentModCountries.Add(country);
                    }
                }
                catch (Exception)
                {
                    //Debug.Log($"parse error: {value}");
                }
            }
        }

        // Updating the UI

        foreach (Transform child in countriesListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (CountryScriptableObject country in currentModCountries)
        {
            GameObject countryButton = Instantiate(selectCountryButtonPrefab, countriesListContainer);

            countryButton.GetComponent<SelectCountryButton>().country_ScriptableObject = country;
            countryButton.GetComponent<SelectCountryButton>().UpdateUI();
        }
    }

    public void DownloadMod()
    {
        bool alreadyDownloaded = downloadedModsIds.list.Any(item => item.id == currentLoadedModification.id);

        WebClient client = new WebClient();

        Stream data = client.OpenRead(@$"http://absolute-empire.7m.pl/media/uploads/mods/{currentLoadedModification.currentScenarioName}/{currentLoadedModification.currentScenarioName}.AEMod");
        StreamReader reader = new StreamReader(data);

        string modData = reader.ReadToEnd();

        string ModPath = Path.Combine($"{Application.persistentDataPath}", "savedMods", $"{currentLoadedModification.currentScenarioName}");
        string ModsPath = Path.Combine($"{Application.persistentDataPath}", "savedMods");

        if (!Directory.Exists(ModsPath))
        {
            CreateFolder("", "savedMods");
        }

        CreateFolder(ModsPath, $"{currentLoadedModification.currentScenarioName}");
        CreateFolder(ModPath, $"events");
        CreateFolder(ModPath, $"countries");

        CreateFile(modData, Path.Combine(ModPath, $"{currentLoadedModification.currentScenarioName}.AEMod"));

        data.Close();
        reader.Close();

        if (alreadyDownloaded)
        {
            foreach (ModListValue.LocalSavedModification localMod in downloadedModsIds.list)
            {
                if (localMod.id == currentLoadedModification.id)
                {
                    localMod.version = currentLoadedModification.version;
                }
            }
        }
        else
        {
            ModListValue.LocalSavedModification mod = new ModListValue.LocalSavedModification();
            mod.id = currentLoadedModification.id;
            mod.version = currentLoadedModification.version;

            downloadedModsIds.list.Add(mod);
        }

        PlayerPrefs.SetString($"MODIFICATION_{currentLoadedModification.id}", $"{currentLoadedModification.currentScenarioName}");

        UpdateSavedIds();

        UpdateModUI();
    }

    public void UpdateSavedIds()
    {
        PlayerPrefs.SetString("MODS_IDS", "");

        string modsIds = "";

        for (int i = 0; i < downloadedModsIds.list.Count; i++)
        {
            modsIds += $"{downloadedModsIds.list[i].id}_{downloadedModsIds.list[i].version};";
        }

        PlayerPrefs.SetString("MODS_IDS", modsIds);
    }

    public void UpdateLocalMods()
    {
        foreach (Transform child in localModsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (ModListValue.LocalSavedModification mod in downloadedModsIds.list)
        {
            if (PlayerPrefs.HasKey($"MODIFICATION_{mod.id}"))
            {
                GameObject localModButtonSpawned = Instantiate(localModButtonPrefab, localModsContainer);
                localModButtonSpawned.GetComponent<LocalModButton>().id = mod.id;
                localModButtonSpawned.GetComponent<LocalModButton>().SetUp();
            }
        }
    }

    public void SetPage(int increment)
    {
        foreach (Transform child in wallContrainer)
        {
            Destroy(child.gameObject);
        }

        if (selectedPage + increment >= 0 && selectedPage + increment <= maxPage)
        {
            selectedPage += increment;
        }

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            pageText.text = $"Page {selectedPage + 1} of {maxPage + 1}";
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            pageText.text = $"Страница {selectedPage + 1} из {maxPage + 1}";
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        foreach (Transform child in wallContrainer)
        {
            if (string.IsNullOrEmpty(child.GetComponentInChildren<TMP_Text>().text))
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < loadedModifications.Count; i++)
        {
            if (loadedModifications[i].page == selectedPage)
            {
                if (!string.IsNullOrEmpty(loadedModifications[i].currentScenarioData))
                {
                    GameObject spawnedWall = Instantiate(wallItemPrefab, wallContrainer);
                    spawnedWall.transform.localScale = Vector3.one;

                    spawnedWall.transform.GetChild(0).GetComponent<TMP_Text>().text = loadedModifications[i].currentScenarioName;
                    spawnedWall.GetComponent<ModButton>().id = loadedModifications[i].id;
                }
            }
        }

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            pageText.text = $"Page {selectedPage + 1} of {maxPage + 1}";
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            pageText.text = $"Страница {selectedPage + 1} из {maxPage + 1}";
        }
    }

    public void StartMod()
    {
        isPlayModObject.value = true;

        PlayerPrefs.SetInt("CURRENT_MOD_PLAYING", currentLoadedModification.id);

        mainMenu.LoadScene("EuropeSceneOffline");
    }

    private void CreateFolder(string _path, string folderName)
    {
        string path = Path.Combine(Application.persistentDataPath, $"{_path}");
        path = Path.Combine(path, $"{folderName}");

        Directory.CreateDirectory(path);
        Debug.Log($"Created folder in {path} with name: {folderName}");
    }

    private void CreateFile(string fileText, string path)
    {
        StreamWriter streamWriter;
        FileInfo file = new FileInfo(path);
        streamWriter = file.CreateText();

        streamWriter.Write(fileText);
        streamWriter.Close();

        Debug.Log($"Created file in {path} with text: {fileText}");
    }

    [System.Serializable]
    public class Modification
    {
        public int id;
        public int page;
        public string currentScenarioName;
        public string currentScenarioDescription;
        public string currentScenarioDate;
        public string currentScenarioData;
        public string currentScenarioAuthorId;
        public string currentScenarioAuthorName;
        public int verified;
        public int views;
        public int likes;
        public int version;
    }
}