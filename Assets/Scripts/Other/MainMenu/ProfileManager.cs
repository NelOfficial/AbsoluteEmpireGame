using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public bool _LOGGED_IN;

    [SerializeField] int maxLevels;

    [SerializeField] TMP_Text playerProgressExp;
    [SerializeField] TMP_Text playerLevel;

    [SerializeField] TMP_InputField login_AccountNameInputField;
    [SerializeField] TMP_InputField login_AccountPasswordInputField;

    [SerializeField] TMP_Text login_MessageText;

    [SerializeField] TMP_Text[] acountStatusTexts;

    [SerializeField] GameObject loginAnimationUI;

    [SerializeField] GameObject loginButton;
    [SerializeField] GameObject profileButton;

    public string[] accountData;

    [HideInInspector] public int userId;
    [HideInInspector] public string _profileName;
    private string _profilePassword;
    private string _profileEmail;
    private string _profileRegDate;
    private string _profileIp;
    private int isPremium;
    private int isBanned;
    private string _profileStatus;

    public int currentExp;
    public int currentLvl;

    [SerializeField] BoolValue loggedInValue;

    public List<int> loadedModificationsIds = new List<int>();
    public List<ModificationPanel.Modification> loadedModifications = new List<ModificationPanel.Modification>();
    public List<ProfileNotification> loadedNotifications = new List<ProfileNotification>();

    [SerializeField] GameObject wallAnimationUI;
    [SerializeField] Button refreshButton;
    [SerializeField] TMP_Text refreshButtonText;
    [SerializeField] GameObject wallAnimationUISecond;
    [SerializeField] Button refreshButtonSecond;
    [SerializeField] TMP_Text refreshButtonTextSecond;
    private bool updatingFeed;

    [SerializeField] Transform wallContrainer;
    [SerializeField] Transform onVerifyWallContrainer;
    [SerializeField] GameObject modItemPrefab;

    [SerializeField] Button createModButton;
    [SerializeField] TMP_Text createModButtonText;

    private void Start()
    {
        createModButton.interactable = false;

        if (PlayerPrefs.GetString("LOGGED_IN") == "TRUE")
        {
            loggedInValue.value = true;

            string password = PlayerPrefs.GetString("PASSWORD");
            string nickname = PlayerPrefs.GetString("nickname");

            StartCoroutine(LoginAccount_Co(nickname, password, true));
        }
        else if (PlayerPrefs.GetString("LOGGED_IN") == "FALSE")
        {
            loggedInValue.value = false;
        }
    }

    public void UpdateUI()
    {
        playerProgressExp.text = $"{currentExp} / 100 {ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Account.Role.Exp")}";
        playerLevel.text = $"{currentLvl}/{maxLevels} {ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Account.Role.Level")}";


        if (isPremium == 0)
        {
            acountStatusTexts[0].text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Account.Premium")}: <color=red>{ReferencesManager.Instance.languageManager.GetTranslation("No")}</color>";
        }
        else if (isPremium == 1)
        {
            acountStatusTexts[0].text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Account.Premium")}: <color=green>{ReferencesManager.Instance.languageManager.GetTranslation("Yes")}</color>";
        }

        if (isBanned == 0)
        {
            acountStatusTexts[1].text = $"<color=green>{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Account.BanState")}</color>";
        }
        else if (isBanned == 1)
        {
            acountStatusTexts[1].text = $"<color=red>{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Account.BanState.True")}</color>";
        }

        if (_profileStatus == "default")
        {
            acountStatusTexts[2].text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Account.Role.Player")}";
        }
        else if (_profileStatus == "yt")
        {
            acountStatusTexts[2].text = $"<color=red>{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Account.Role.ContentMaker")}</color>";
        }
        else if (_profileStatus == "admin")
        {
            acountStatusTexts[2].text = $"<color=green>{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Account.Role.Admin")}</color>";
        }
        else if (_profileStatus == "moderator")
        {
            acountStatusTexts[2].text = $"<color=lightblue>{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Account.Role.Moderator")}</color>";
        }

        acountStatusTexts[3].text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Account.Email")}: {_profileEmail}";

        foreach (Transform child in wallContrainer)
        {
            if (string.IsNullOrEmpty(child.GetComponentInChildren<TMP_Text>().text))
            {
                Destroy(child.gameObject);
            }
        }

        foreach (Transform child in onVerifyWallContrainer)
        {
            if (string.IsNullOrEmpty(child.GetComponentInChildren<TMP_Text>().text))
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < loadedModifications.Count; i++)
        {
            if (!string.IsNullOrEmpty(loadedModifications[i].currentScenarioData))
            {
                if (loadedModifications[i].verified == 0)
                {
                    GameObject _spawnedWall = Instantiate(modItemPrefab, onVerifyWallContrainer);
                    _spawnedWall.transform.localScale = Vector3.one;

                    _spawnedWall.GetComponent<LocalModButton>().modNameText.text = loadedModifications[i].currentScenarioName;
                    _spawnedWall.GetComponent<LocalModButton>().id = loadedModifications[i].id;
                    _spawnedWall.GetComponent<LocalModButton>().version = loadedModifications[i].version;
                    _spawnedWall.GetComponent<LocalModButton>().modName = loadedModifications[i].currentScenarioName;
                    _spawnedWall.GetComponent<LocalModButton>().description = loadedModifications[i].currentScenarioDescription;
                }
                else
                {
                    GameObject spawnedWall = Instantiate(modItemPrefab, wallContrainer);
                    spawnedWall.transform.localScale = Vector3.one;

                    spawnedWall.GetComponent<LocalModButton>().modNameText.text = loadedModifications[i].currentScenarioName;
                    spawnedWall.GetComponent<LocalModButton>().id = loadedModifications[i].id;
                    spawnedWall.GetComponent<LocalModButton>().version = loadedModifications[i].version;
                    spawnedWall.GetComponent<LocalModButton>().modName = loadedModifications[i].currentScenarioName;
                    spawnedWall.GetComponent<LocalModButton>().description = loadedModifications[i].currentScenarioDescription;
                }
            }
        }
    }

    public void LoginAccount()
    {
        if (!string.IsNullOrEmpty(login_AccountNameInputField.text) && !string.IsNullOrEmpty(login_AccountPasswordInputField.text))
        {
            StartCoroutine(LoginAccount_Co(login_AccountNameInputField.text, login_AccountPasswordInputField.text, false));
        }
    }

    private IEnumerator LoginAccount_Co(string username, string password, bool authInStart)
    {
        loginAnimationUI.SetActive(true);

        WWWForm loginForm = new WWWForm();
        loginForm.AddField("login", username);
        loginForm.AddField("password", password);

        WWW accountLoginRequest = new WWW("https://absolute-empire.space/auth/signin.php", loginForm);
        yield return accountLoginRequest;

        if (accountLoginRequest.isDone)
        {
            if (accountLoginRequest.text == "AUTH_ERROR.INCORRECT_DATA")
            {
                login_MessageText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("AUTH_ERROR.INCORRECT_DATA")}";
            }
            else if (accountLoginRequest.text == "AUTH_ERROR.ACCOUNT_NOT_EXIST")
            {
                login_MessageText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("AUTH_ERROR.ACCOUNT_NOT_EXIST")}";
            }
            else if (accountLoginRequest.text == "AUTH_ERROR.NO_HASHED_PASSWORD")
            {
                login_MessageText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("AUTH_ERROR.NO_HASHED_PASSWORD")}";
            }

            if (accountLoginRequest.text != "AUTH_ERROR.INCORRECT_DATA" && accountLoginRequest.text != "AUTH_ERROR.ACCOUNT_NOT_EXIST" &&
                accountLoginRequest.text != "AUTH_ERROR.NO_HASHED_PASSWORD")
            {
                accountData = accountLoginRequest.text.Split('\t');

                try
                {
                    userId = int.Parse(accountData[0]);
                    _profileName = accountData[1];
                    _profileEmail = accountData[2];
                    _profilePassword = accountData[3];
                    _profileRegDate = accountData[4];
                    _profileIp = accountData[5];
                    isPremium = int.Parse(accountData[6]);
                    isBanned = int.Parse(accountData[7]);
                    _profileStatus = accountData[8];
                    currentExp = int.Parse(accountData[9]);
                    currentLvl = int.Parse(accountData[10]);

                    _LOGGED_IN = true;
                    loggedInValue.value = true;
                    PlayerPrefs.SetString("LOGGED_IN", "TRUE");

                    PlayerPrefs.SetString("PASSWORD", $"{_profilePassword}");
                    PlayerPrefs.SetString("nickname", $"{_profileName}");

                    ReferencesManager.Instance.mainMenu.UpdateNickname(_profileName);

                    loginButton.SetActive(false);
                    profileButton.SetActive(true);

                    createModButton.interactable = true;
                }
                catch (System.Exception)
                {
                    login_MessageText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("Error")}: \n {accountLoginRequest.text}";
                }
            }

            loginAnimationUI.SetActive(false);
        }
    }

    public void LoadMyMods(bool verified)
    {
        StartCoroutine(GetIds(verified));
    }

    public void LogOut()
    {
        _LOGGED_IN = false;
        loggedInValue.value = false;
        PlayerPrefs.SetString("LOGGED_IN", "FALSE");

        PlayerPrefs.DeleteKey("PASSWORD");
        PlayerPrefs.DeleteKey("nickname");

        createModButton.interactable = false;

        UpdateUI();
    }

    [System.Obsolete]
    private IEnumerator GetIds(bool verified)
    {
        wallAnimationUI.SetActive(true);
        refreshButton.interactable = false;

        refreshButtonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.Loading")}";

        WWWForm form = new WWWForm();
        form.AddField("authorName", _profileName);

        if (verified) form.AddField("verified", "TRUE");
        else form.AddField("verified", "FALSE");

        WWW getPostRequest = new WWW("http://our-empire.7m.pl/core/getPostsWithAuthor.php", form);

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

        StartCoroutine(WallUpdate(verified));
    }

    private IEnumerator WallUpdate(bool verified)
    {
        updatingFeed = true;
        if (verified)
        {
            wallAnimationUI.SetActive(true);
            refreshButton.interactable = false;
        }
        else
        {
            wallAnimationUISecond.SetActive(true);
            refreshButtonSecond.interactable = false;
        }

        loadedModifications.Clear();

        if (loadedModifications != null)
        {
            updatingFeed = true;
            for (int i = 0; i < loadedModificationsIds.Count; i++)
            {
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
                    ModificationPanel.Modification newModification = new ModificationPanel.Modification();

                    try
                    {
                        newModification.id = int.Parse(request[0]);
                        newModification.page = 0;
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

                    loadedModifications.Add(newModification);
                }

            }
            updatingFeed = false;

            yield return new WaitForSeconds(2f);
            wallAnimationUI.SetActive(false);
            wallAnimationUISecond.SetActive(false);
            refreshButton.interactable = true;
            refreshButtonSecond.interactable = true;

            refreshButtonText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("RefreshButton")}";
            refreshButtonTextSecond.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("RefreshButton")}";
        }
        UpdateUI();
    }

    [System.Serializable]
    public class ProfileNotification
    {
        public int id;
        public string title;

        [TextArea(1, 100)]
        public string description;

        public int moderator_id;
        public string date;
    }
}
