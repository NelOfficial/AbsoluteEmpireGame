using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
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

    public CountryScriptableObject[] globalCountries;

    [SerializeField] GameObject[] languageToggles;

    [SerializeField] Button LoadGameButton;

    [SerializeField] AudioClip click01_WAV;

    [SerializeField] string[] quotes;

    private AsyncOperation loading;
    
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

        if (PlayerPrefs.HasKey("1_PLAYER_COUNTRY"))
        {
            LoadGameButton.interactable = true;
        }
    }

    public void UpdateNickname(string nickname)
    {
        nicknameText.text = nickname;
        secondNicknameText.text = nickname;
        Photon.Pun.PhotonNetwork.NickName = nickname;

        PlayerPrefs.SetString("nickname", nickname);
    }

    public void DeveloperMode(bool state)
    {
        devMode.value = state;
    }

    public void LoadThroughMenu()
    {
        PlayerPrefs.SetString("LOAD_GAME_THROUGH_MENU", "TRUE");
    }

    public void PlayGame()
    {
        PlayerPrefs.SetString("LOAD_GAME_THROUGH_MENU", "FALSE");
        PlayerPrefs.SetString("FIRST_LOAD", "TRUE");

        quoteText.text = quotes[Random.Range(0, quotes.Length)];
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
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
        PlayerPrefs.DeleteKey("currentCountryIndex");
    }

    public void QuitGame()
    {
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
