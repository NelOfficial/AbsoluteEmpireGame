using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    // û
    public GameObject currentRoomMenu;
    public GameObject loadingMenu;

    public TMP_Text connectionText;
    public TMP_Text currentRoomText;
    public TMP_Text currentScenariyText;

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
    
    private void Awake()
    {
        Application.targetFrameRate = 120;
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("buttonDesign"))
        {
            SetButtonsDesign(PlayerPrefs.GetInt("buttonDesign"));
        }

        SetLanguage(PlayerPrefs.GetInt("languageId"));

        PlayerPrefs.DeleteKey("currentCountryIndex");
        ReferencesManager.Instance.gameSettings._playerCountrySelected.value = "";
        ReferencesManager.Instance.gameSettings.developerCheats = false;

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

        quoteText.text = quotes[Random.Range(0, quotes.Length)];

        LoadScene("EuropeSceneOffline");
    }

    public void UpdateNickname(string nickname)
    {
        nicknameText.text = nickname;
        secondNicknameText.text = nickname;

        PlayerPrefs.SetString("nickname", nickname);
    }

    public void DeveloperMode(bool state)
    {
        devMode.value = state;
    }

    public void RegionsDebugMode(bool state)
    {
        ReferencesManager.Instance.gameSettings._regionsDebugMode.value = state;
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

        quoteText.text = quotes[Random.Range(0, quotes.Length)];
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

        LocalLocalisationText[] localisationTexts = FindObjectsOfType<LocalLocalisationText>();
        for (int i = 0;i < localisationTexts.Length; i++)
        {
            localisationTexts[i].SetUp();
        }

        loadingMenu.SetActive(false);
        PlayerPrefs.SetInt("languageId", id);
    }

    public void SetButtonsDesign(int data)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].targetImage.sprite = buttonDesigns[data];
            PlayerPrefs.SetInt("buttonDesign", data);
        }
    }
}
