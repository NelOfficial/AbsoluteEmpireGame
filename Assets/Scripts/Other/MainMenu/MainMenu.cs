using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

public class MainMenu : MonoBehaviour
{
    public string newsJson;

    public CurrentRoomInfo currentRoomMenu;
    public GameObject loadingMenu;

    public TMP_Text connectionText;

    public TMP_Text nicknameText;
    public TMP_Text secondNicknameText;
    public TMP_Text quoteText;

    public GameObject[] menus;
    public Image progressbar;

    public UI_Button[] buttons;
    public Sprite[] buttonDesigns;

    public BoolValue devMode;
    public GameObject devmodecheck;
    public StringValue difficultyValue;

    public CountryScriptableObject[] globalCountries;

    [SerializeField] GameObject[] languageToggles;

    [SerializeField] Button LoadGameButton;

    [SerializeField] AudioClip click01_WAV;

    [SerializeField] string[] quotes;

    private AsyncOperation loading;

    [SerializeField] TMP_Dropdown difficultyDropdown;
    [SerializeField] GameObject loadingPanel;

    [Header("News Settings")]
    [SerializeField] private GameObject _newsLoadingAnimation;
    [SerializeField] private GameObject _newsItemPrefab;
    [SerializeField] private Transform _newsContainer;
    
    private void Awake()
    {
        Application.targetFrameRate = 120;
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("buttonDesign"))
        {
            PlayerPrefs.DeleteKey("buttonDesign");
        }

        SetLanguage(PlayerPrefs.GetInt("languageId", 1));

        PlayerPrefs.DeleteKey("currentCountryIndex");
        ReferencesManager.Instance.gameSettings._playerCountrySelected.value = "";
        ReferencesManager.Instance.gameSettings.developerCheats = false;
        ReferencesManager.Instance.gameSettings.editingModString.value = string.Empty;

        DeveloperMode(false);
    }

    public void StartTutorial()
    {
        ReferencesManager.Instance.gameSettings.isTutorialMode.value = false;
        ReferencesManager.Instance.gameSettings.marchEvent.value = false;

        ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
        ReferencesManager.Instance.gameSettings.playMod.value = false;
        ReferencesManager.Instance.gameSettings.loadGame.value = false;

        PlayerPrefs.DeleteKey("CURRENT_EDITING_MODIFICATION");

        PlayerPrefs.SetString("FIRST_LOAD", "TRUE");

        quoteText.text = ReferencesManager.Instance.languageManager.GetTranslation($"{quotes[Random.Range(0, quotes.Length)]}");

        LoadScene("EuropeSceneOffline");
    }

    public void UpdateNickname(string nickname)
    {
        nicknameText.text = nickname;
        secondNicknameText.text = nickname;

        PlayerPrefs.SetString("nickname", nickname);
        Photon.Pun.PhotonNetwork.NickName = nickname;
    }

    public void DeveloperMode(bool state)
    {
        devMode.value = state;
    }

    public void RegionsDebugMode(bool state)
    {
        ReferencesManager.Instance.gameSettings._regionsDebugMode.value = state;
        ReferencesManager.Instance.gameSettings._gameConsole.value = state;
    }

    public void SetDifficulty(int difficulty)
    {
        // 0 - easy 1 - normal 2 - hard 3 - insane 4 - hardcore

        if (difficulty == 0) difficultyValue.value = "EASY";
        else if (difficulty == 1) difficultyValue.value = "NORMAL";
        else if (difficulty == 2) difficultyValue.value = "HARD";
        else if (difficulty == 3) difficultyValue.value = "INSANE";
        else if (difficulty == 4) difficultyValue.value = "HARDCORE";
    }

    public void SetMarchEvent(bool state)
    {
        if (state)
        {
            SetDifficulty(4);
            difficultyDropdown.value = 4;
            difficultyDropdown.interactable = false;

            ReferencesManager.Instance.gameSettings.marchEvent.value = true;
        }
        else
        {
            SetDifficulty(2);
            difficultyDropdown.value = 2;
            difficultyDropdown.interactable = true;

            ReferencesManager.Instance.gameSettings.marchEvent.value = false;
        }
    }

    public void LoadThroughMenu()
    {
        ReferencesManager.Instance.gameSettings.isTutorialMode.value = false;
        ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
        ReferencesManager.Instance.gameSettings.playMod.value = false;
        ReferencesManager.Instance.gameSettings.loadGame.value = true;

        PlayerPrefs.DeleteKey("CURRENT_EDITING_MODIFICATION");
    }

    public void PlayGame()
    {
        ReferencesManager.Instance.gameSettings.isTutorialMode.value = false;
        ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
        ReferencesManager.Instance.gameSettings.playMod.value = false;
        ReferencesManager.Instance.gameSettings.loadGame.value = false;

        PlayerPrefs.DeleteKey("CURRENT_EDITING_MODIFICATION");

        PlayerPrefs.SetString("FIRST_LOAD", "TRUE");

        quoteText.text = ReferencesManager.Instance.languageManager.GetTranslation($"{quotes[Random.Range(0, quotes.Length)]}");
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        loadingPanel.SetActive(true);

        loading = SceneManager.LoadSceneAsync(sceneName);
        loading.allowSceneActivation = true;

        while (loading.progress != 0.9f)
        {
            progressbar.fillAmount = loading.progress;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void OpenMenu(GameObject openMenu)
    {
        CloseMenus();
        openMenu.SetActive(true);
    }

    public void CloseMenus()
    {
        for (int i = 0; i < menus.Length; i++)
        {
            menus[i].SetActive(false);
        }
    }

    private void OnApplicationQuit()
    {
        ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
        ReferencesManager.Instance.gameSettings.playMod.value = false;
        ReferencesManager.Instance.gameSettings.loadGame.value = false;

        PlayerPrefs.DeleteKey("currentCountryIndex");
    }

    public void QuitGame()
    {
        ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
        ReferencesManager.Instance.gameSettings.playMod.value = false;
        ReferencesManager.Instance.gameSettings.loadGame.value = false;

        Application.Quit();
    }

    public void ScrollEffect(RectTransform rectTransform)
    {
        rectTransform.position = new Vector3(rectTransform.position.x, -rectTransform.sizeDelta.y * 2, 0);
    }

    public void AdaptiveHeight(RectTransform rectTransform)
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.childCount * 100);
    }

    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

    public void SetLanguage(int id)
    {
        for (int i = 0; i < languageToggles.Length; i++)
        {
            languageToggles[i].SetActive(false);
        }

        languageToggles[id].SetActive(true);

        UISoundEffect.Instance.PlayAudio(click01_WAV);

        loadingMenu.SetActive(true);

        TextTranslate[] localisationTextsNew = FindObjectsOfType<TextTranslate>();
        for (int i = 0; i < localisationTextsNew.Length; i++)
        {
            localisationTextsNew[i].SetUp();
        }

        loadingMenu.SetActive(false);
        PlayerPrefs.SetInt("languageId", id);
    }

    public void GetNews()
    {
        StartCoroutine(GetGameNews());
    }

    IEnumerator GetGameNews()
    {
        _newsLoadingAnimation.SetActive(true);

        UnityWebRequest request = UnityWebRequest.Get(newsJson);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // �������� �����
            string jsonResponse = request.downloadHandler.text;
            UpdateNewsUI(jsonResponse);
            _newsLoadingAnimation.SetActive(false);
        }
    }

    private void UpdateNewsUI(string json)
    {
        foreach (Transform newsItem in _newsContainer)
        {
            Destroy(newsItem.gameObject);
        }

        NewsResponse newsResponse = JsonUtility.FromJson<NewsResponse>(json);

        foreach (NewsItem newsItem in newsResponse.news)
        {
            GameObject spawnedNewsItem = Instantiate(_newsItemPrefab, _newsContainer);

            spawnedNewsItem.GetComponent<NewsItemUI>()._text = newsItem.content;
            spawnedNewsItem.GetComponent<NewsItemUI>()._url = newsItem.url;
            spawnedNewsItem.GetComponent<NewsItemUI>().SetUp();
        }
    }

    [System.Serializable]
    public class NewsResponse
    {
        public NewsItem[] news;
    }

    [System.Serializable]
    public class NewsItem
    {
        public string content;
        public string url;
    }
}
