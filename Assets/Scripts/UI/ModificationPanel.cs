using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;
using System.IO;
using System.Net;
using System;
using UnityEngine.Networking;

public class ModificationPanel : MonoBehaviour
{
    public GameObject wallAnimationUI;
    public GameObject notifications_wallAnimationUI;
    public GameObject wallItemPrefab;
    public Transform wallContrainer;
    public Transform notifications_container;

    [SerializeField] Button refreshButton;
    [SerializeField] TMP_Text refreshButtonText;
    [SerializeField] Button notifications_refreshButton;
    [SerializeField] TMP_Text notifications_refreshButtonText;

    [SerializeField] TMP_Text modName_TMP;
    [SerializeField] TMP_Text modDesc_TMP;
    [SerializeField] TMP_Text modViews_TMP;
    [SerializeField] TMP_Text modDownloads_TMP;
    [SerializeField] TMP_Text modAuthor_TMP;

    public bool updatingFeed;
    public int lastScenarios;

    public Modification currentLoadedModification;

    public List<int> loadedModificationsIds = new List<int>();
    public List<string> notificationIds = new List<string>();
    public List<Modification> loadedModifications = new List<Modification>();

    [SerializeField] BoolValue jsonRead;

    [SerializeField] GameObject localModButtonPrefab;
    [SerializeField] Transform localModsContainer;
    [SerializeField] GameObject notification_buttonPrefab;

    public ModListValue downloadedModsIds;
    public BoolValue isPlayModObject;

    public MainMenu mainMenu;

    [SerializeField] Button downloadButton;

    [SerializeField] Transform countriesListContainer;
    public GameObject countriesListPanel;
    [SerializeField] GameObject selectCountryButtonPrefab;

    private int maxPage = 0;
    private int selectedPage = 0;

    public int postsPerPage = 10;
    public int maxLoadPostsPerTime = 20;

    [SerializeField] TMP_Text pageText;
    public List<CountryScriptableObject> currentModCountries = new List<CountryScriptableObject>();

    // coroutine values
    [SerializeField] string c_text;
    [SerializeField] byte[] c_bytes;

    [SerializeField] bool isTextNotEmpty = false;
    [SerializeField] bool isBytesNotEmpty = false;
    [SerializeField] bool modHaveDownloaded = false;

    private string mainInfo;
    private string regionsInfo;
    private string countriesInfo;
    private string eventsInfo;

    private string[] mainInfoLines;
    private string[] regionsInfoLines;
    private string[] countriesInfoLines;
    private string[] eventsInfoLines;

    private string loadedModsIds_data;
    private string[] loadedModsIds;

    private void Start()
    {
        selectedPage = 0;

        SetPage(selectedPage);

        UpdateDownloadedMods();
    }

    public void UpdateDownloadedMods()
    {
        loadedModsIds_data = PlayerPrefs.GetString("MODS_IDS");

        downloadedModsIds.list.Clear();

        if (!string.IsNullOrEmpty(loadedModsIds_data))
        {
            loadedModsIds = loadedModsIds_data.Split(';');

            if (loadedModsIds.Length > 0)
            {
                for (int i = 0; i < loadedModsIds.Length; i++)
                {
                    string modIdValue = loadedModsIds[i].Split('_')[0];
                    string modVerValue = loadedModsIds[i].Split('_')[1];

                    int.TryParse(modIdValue, out int modId);
                    int.TryParse(modVerValue, out int modVersion);

                    string modName = PlayerPrefs.GetString($"MODIFICATION_{modId}");

                    if (Directory.Exists(Path.Combine(Application.persistentDataPath, "savedMods", $"{modName}")))
                    {
                        ModListValue.LocalSavedModification mod = new ModListValue.LocalSavedModification();
                        mod.id = modId;
                        mod.version = modVersion;

                        downloadedModsIds.list.Add(mod);
                    }
                }
            }
        }
    }

    [Obsolete]
    public void UpdateScenariosList()
    {
        StartCoroutine(GetIds());
        StartCoroutine(WallUpdate());
        lastScenarios = loadedModificationsIds.Count;
        refreshButton.image.color = Color.white;
    }

    [System.Obsolete]
    private IEnumerator GetIds()
    {
        wallAnimationUI.SetActive(true);
        refreshButton.interactable = false;

        refreshButtonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Loading")}";

        WWWForm form = new WWWForm();
        form.AddField("maxPosts", maxLoadPostsPerTime + loadedModifications.Count);
        int offset = postsPerPage * selectedPage - postsPerPage;

        if (loadedModifications.Count <= 0)
        {
            offset = 0;
        }

        form.AddField("offset", offset);
        WWW getPostRequest = new WWW("https://absolute-empire.space/core/getPosts.php", form);

        yield return getPostRequest;

        string[] request = getPostRequest.text.Split('\t');

        loadedModificationsIds.Clear();

        for (int i = 0; i < request.Length; i++)
        {
            try
            {
                loadedModificationsIds.Add(int.Parse(request[i]));
            }
            catch (Exception) { }
        }

        StartCoroutine(WallUpdate());
    }

    public void GetNotifications()
    {
        StartCoroutine(GetNotifications_Co());
    }

    [Obsolete]
    private IEnumerator GetNotifications_Co()
    {
        ReferencesManager.Instance.profileManager.loadedNotifications.Clear();
        loadedModificationsIds.Clear();

        notifications_wallAnimationUI.SetActive(true);
        notifications_refreshButton.interactable = false;

        notifications_refreshButtonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Loading")}";

        WWWForm form = new WWWForm();
        form.AddField("receiver", ReferencesManager.Instance.profileManager.userId);

        WWW getPostRequest = new WWW("https://absolute-empire.space/core/getNotifications.php", form);

        yield return getPostRequest;

        notificationIds = getPostRequest.text.Split('\t').ToList();

        for (int i = 0; i < notificationIds.Count; i++)
        {
            WWWForm _form = new WWWForm();
            _form.AddField("id", int.Parse(notificationIds[i]));

            WWW getPostRequestById = new WWW("https://absolute-empire.space/core/getNotificationById.php", _form);
            yield return getPostRequestById;

            string[] request = getPostRequestById.text.Split('\t');

            if (!string.IsNullOrEmpty(request[0]))
            {
                ProfileManager.ProfileNotification newNotification = new ProfileManager.ProfileNotification();

                try
                {
                    newNotification.title = request[0];
                    newNotification.description = request[1];
                    newNotification.moderator_id = int.Parse(request[2]);
                    newNotification.date = request[3];

                    ReferencesManager.Instance.profileManager.loadedNotifications.Add(newNotification);
                }
                catch (System.Exception)
                {
                    Debug.Log(getPostRequest.text);
                }
            }
        }

        yield return new WaitForSeconds(0.1f);
        UpdateNotifications_UI();

        notifications_wallAnimationUI.SetActive(false);
        notifications_refreshButton.interactable = true;

        yield return new WaitForSeconds(0.1f);

        notifications_refreshButtonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("RefreshButton")}";
    }

    [Obsolete]
    private IEnumerator WallUpdate()
    {
        wallAnimationUI.SetActive(true);
        updatingFeed = true;
        refreshButton.interactable = false;

        refreshButtonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Loading")}";

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

                // ������ ������ �� ���� ��� � ������ ��� ���������� ������ �� ���������� �����
                WWW getPostRequest = new WWW("https://absolute-empire.space/core/getPostById.php", form);

                yield return getPostRequest;

                string[] request = getPostRequest.text.Split('\t');

                //����� �� �� ���������� ������� ���� � ��� ��� ����� �� �����������
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
                    //WWW wwwAuthorRequest = new WWW("https://absolute-empire.space/core/getAuthorById.php", authorForm);

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

            refreshButtonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.ModsList.LoadMore")}";

            UpdateUI();
        }
    }

    private void UpdateModInfo(int id)
    {
        StartCoroutine(UpdateModInfo_Co(id));
    }

    private IEnumerator UpdateModInfo_Co(int id)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", id);

        // ������ ������ �� ���� ��� � ������ ��� ���������� ������ �� ���������� �����
        WWW getPostRequest = new WWW("https://absolute-empire.space/core/getPostById.php", form);

        yield return getPostRequest;

        string[] request = getPostRequest.text.Split('\t');

        //����� �� �� ���������� ������� ���� � ��� ��� ����� �� �����������
        if (!string.IsNullOrEmpty(request[0]))
        {
            Modification newModification = new Modification();

            try
            {
                newModification.id = int.Parse(request[0]);
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

            foreach (var mod in loadedModifications)
            {
                if (mod.id == newModification.id)
                {
                    mod.currentScenarioName = newModification.currentScenarioName;
                    mod.currentScenarioDescription = newModification.currentScenarioDescription;
                    mod.currentScenarioDate = newModification.currentScenarioDate;
                    mod.currentScenarioData = newModification.currentScenarioData;
                    mod.currentScenarioAuthorId = newModification.currentScenarioAuthorId;
                    mod.verified = newModification.verified;
                    mod.views = newModification.views;
                    mod.likes = newModification.likes;
                    mod.version = newModification.version;

                    currentLoadedModification = mod;

                    modName_TMP.text = currentLoadedModification.currentScenarioName;
                    modDesc_TMP.text = $"{currentLoadedModification.currentScenarioDescription}";
                    modViews_TMP.text = currentLoadedModification.views.ToString();
                    modDownloads_TMP.text = currentLoadedModification.likes.ToString();
                    modAuthor_TMP.text = $"{currentLoadedModification.currentScenarioAuthorId}";
                }
            }
        }
    }

    public void UpdateModUI()
    {
        UpdateViews(currentLoadedModification.id, ReferencesManager.Instance.profileManager.userId);

        UpdateModInfo(currentLoadedModification.id);

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

                        downloadButton.transform.GetComponentInChildren<TMP_Text>().text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.ModsList.DownloadUpdate")}";
                    }
                    else if (localMod.version >= currentLoadedModification.version)
                    {
                        downloadButton.transform.GetComponentInChildren<TMP_Text>().text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.ModsList.Save")}";

                        downloadButton.interactable = false;
                    }
                }
            }
        }
        else
        {
            downloadButton.interactable = true;

            downloadButton.transform.GetComponentInChildren<TMP_Text>().text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.ModsList.Save")}";
        }
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

    public void UpdateCountriesList()
    {
        mainMenu.ScrollEffect(countriesListContainer.GetComponent<RectTransform>());

        // Get countries in list from mod data
        mainInfo = currentLoadedModification.currentScenarioData.Split("#REGIONS#")[0];
        mainInfoLines = mainInfo.Split(';');

        currentModCountries.Clear();

        for (int i = 2; i < mainInfoLines.Length; i++)
        {
            string _line = mainInfoLines[i];
            string value = "";

            try
            {
                value = GetValue(_line);
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

        currentModCountries = currentModCountries.OrderBy(x => x._name).ToList();

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

        Stream data = client.OpenRead(@$"https://absolute-empire.space/media/uploads/mods/{currentLoadedModification.currentScenarioName}/{currentLoadedModification.currentScenarioName}.AEMod");
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

        mainInfo = modData.Split("#REGIONS#")[0];
        regionsInfo = modData.Split("#REGIONS#")[1];
        countriesInfo = modData.Split("#COUNTRIES_SETTINGS#")[1];
        eventsInfo = modData.Split("#EVENTS#")[1];

        mainInfoLines = mainInfo.Split(';');
        regionsInfoLines = regionsInfo.Split(';');
        countriesInfoLines = countriesInfo.Split(';');
        eventsInfoLines = eventsInfo.Split(';');

        try
        {
            List<int> eventsIDS = new List<int>();

            foreach (string eventData in eventsInfoLines)
            {
                if (!string.IsNullOrEmpty(eventData) && !string.IsNullOrWhiteSpace(eventData))
                {
                    eventsIDS.Add(int.Parse(eventData));
                }
            }

            for (int i = 0; i < eventsIDS.Count; i++)
            {
                string imageUrl = @$"https://absolute-empire.space/media/uploads/mods/{currentLoadedModification.currentScenarioName}/events/{eventsIDS[i]}/{eventsIDS[i]}.jpg";
                string textUrl = @$"https://absolute-empire.space/media/uploads/mods/{currentLoadedModification.currentScenarioName}/events/{eventsIDS[i]}/{eventsIDS[i]}.AEEvent";

                StartCoroutine(CreateEventData_Co(ModPath, eventsIDS[i], imageUrl, textUrl));
            }
        }
        catch (Exception)
        {
            Debug.LogError("No events found");
        }

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

            UpdateDownloads(currentLoadedModification.id, ReferencesManager.Instance.profileManager.userId);

            UpdateModInfo(currentLoadedModification.id);
        }

        PlayerPrefs.SetString($"MODIFICATION_{currentLoadedModification.id}", $"{currentLoadedModification.currentScenarioName}");

        modHaveDownloaded = true;

        UpdateSavedIds();

        UpdateModUI();
    }

    public void UpdateViews(int modId, int playerId)
    {
        StartCoroutine(SendViewRequest(modId, playerId));
    }

    private IEnumerator SendViewRequest(int modId, int playerId)
    {
        // �������������� ������ ��� ��������
        WWWForm form = new WWWForm();
        form.AddField("mod_id", modId);
        form.AddField("player_id", playerId);

        // ���������� ������ �� ������
        using (UnityWebRequest www = UnityWebRequest.Post("https://absolute-empire.space/core/update_views.php", form))
        {
            yield return www.SendWebRequest();

            // ��������� ����������
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("response: " + www.downloadHandler.text);
            }
            else
            {
                Debug.Log("error: " + www.error);
            }
        }
    }

    public void UpdateDownloads(int modId, int playerId)
    {
        StartCoroutine(SendDownloadRequest(modId, playerId));
    }

    private IEnumerator SendDownloadRequest(int modId, int playerId)
    {
        // �������������� ������ ��� ��������
        WWWForm form = new WWWForm();
        form.AddField("mod_id", modId);
        form.AddField("player_id", playerId);

        // ���������� ������ �� ������
        using (UnityWebRequest www = UnityWebRequest.Post("https://absolute-empire.space/core/update_downloads.php", form))
        {
            yield return www.SendWebRequest();

            // ��������� ����������
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("response: " + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("error: " + www.error);
            }
        }
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

        pageText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.ModsList.PageText")} {selectedPage + 1}/{maxPage + 1}";

        UpdateUI();
    }

    public void UpdateUI()
    {
        foreach (Transform child in wallContrainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < loadedModifications.Count; i++)
        {
            if (loadedModifications[i].page == selectedPage)
            {
                GameObject spawnedWall = Instantiate(wallItemPrefab, wallContrainer);
                spawnedWall.transform.localScale = Vector3.one;

                spawnedWall.transform.GetChild(0).GetComponent<TMP_Text>().text = loadedModifications[i].currentScenarioName;
                spawnedWall.GetComponent<ModButton>().id = loadedModifications[i].id;
            }
        }

        pageText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.ModsList.PageText")} {selectedPage + 1}/{maxPage + 1}";


        if (selectedPage < maxPage)
        {
            refreshButton.interactable = false;
        }
        else
        {
            refreshButton.interactable = true;
        }
    }

    public void StartMod()
    {
        isPlayModObject.value = true;

        ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
        ReferencesManager.Instance.gameSettings.playMod.value = true;
        ReferencesManager.Instance.gameSettings.loadGame.value = false;

        PlayerPrefs.SetInt("CURRENT_MOD_PLAYING", currentLoadedModification.id);
        PlayerPrefs.DeleteKey("CURRENT_EDITING_MODIFICATION");

        mainMenu.LoadScene("EuropeSceneOffline");
    }

    public void CreateFolder(string _path, string folderName)
    {
        string path = Path.Combine(Application.persistentDataPath, $"{_path}");
        path = Path.Combine(path, $"{folderName}");

        Directory.CreateDirectory(path);
        Debug.Log($"Created folder in {path} with name: {folderName}");
    }

    public void CreateFile(string fileText, string path)
    {
        StreamWriter streamWriter;
        FileInfo file = new FileInfo(path);
        streamWriter = file.CreateText();

        streamWriter.Write(fileText);
        streamWriter.Close();

        Debug.Log($"Created file in {path} with text: {fileText}");
    }

    public bool IsNullOrEmpty(Array array)
    {
        return (array == null || array.Length == 0);
    }

    public IEnumerator GetImageByURL_Co(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Or retrieve results as binary data
            c_bytes = www.downloadHandler.data;
        }

        isBytesNotEmpty = !IsNullOrEmpty(c_bytes);
    }

    private IEnumerator GetTextByURL_Co(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Or retrieve results as binary data
            c_text = www.downloadHandler.text;
        }

        isTextNotEmpty = !string.IsNullOrEmpty(c_text);
    }

    public IEnumerator CreateEventData_Co(string ModPath, int eventID, string imageUrl, string textUrl)
    {
        StartCoroutine(GetImageByURL_Co(imageUrl));
        StartCoroutine(GetTextByURL_Co(textUrl));

        yield return new WaitUntil(() => isTextNotEmpty == true);

        CreateFolder(Path.Combine(ModPath, "events"), $"{eventID}");
        CreateFile($"{c_text}", Path.Combine(ModPath, "events", $"{eventID}", $"{eventID}.AEEvent"));

        yield return new WaitUntil(() => isBytesNotEmpty == true);

        File.WriteAllBytes(Path.Combine(ModPath, "events", $"{eventID}", $"{eventID}.jpg"), c_bytes);
    }

    public void UpdateNotifications_UI()
    {
        foreach (Transform child in notifications_container)
        {
            Destroy(child.gameObject);
        }

        foreach (ProfileManager.ProfileNotification notification in ReferencesManager.Instance.profileManager.loadedNotifications)
        {
            GameObject localModButtonSpawned = Instantiate(notification_buttonPrefab, notifications_container);
            localModButtonSpawned.GetComponent<ProfileNotificationItem>().title = notification.title;
            localModButtonSpawned.GetComponent<ProfileNotificationItem>().description = notification.description;
            localModButtonSpawned.GetComponent<ProfileNotificationItem>().date = notification.date;
            localModButtonSpawned.GetComponent<ProfileNotificationItem>().SetUp();
        }
    }

    [Serializable]
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