using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class OfflineGameSettings : MonoBehaviour
{
    [SerializeField] private Transform slideContent;
    [SerializeField] private Transform _tournamentCountriesContainer;
    public GameObject selectCountryButtonPrefab;
    [SerializeField] private GameObject selectCountryButtonPrefabWithCheckMark;
    [SerializeField] private Image imagePreview;

    [SerializeField] int currentMapId;
    [HideInInspector] public int currentScenarioId;

    [SerializeField] private Scenario[] scenarios;
    [SerializeField] private List<Team> teams = new(0);

    [SerializeField] private StringValue _currentScenarioData;
    [SerializeField] private StringValue _currentDate;

    [HideInInspector] public List<CountryScriptableObject> _tournamentCountries = new();

    [SerializeField] private Button _tournament_ConfirmButton;
    [SerializeField] private TMP_Text _tournament_ConfirmButton_Text;

    [SerializeField] private Image[] _gameTypeButtonTargetImages;

    [SerializeField] private Color _gameType_Color_selected;

    private MainMenu mainMenu;

    private void Start()
    {
        mainMenu = FindObjectOfType<MainMenu>();

        SelectScenario(0);
    }

    private void UpdateSelectionCountriesUI()
    {
        foreach (Transform child in slideContent)
        {
            Destroy(child.gameObject);
        }

        Scenario currentScenario = GetScenario(currentScenarioId);

        for (int i = 0; i < currentScenario.countries.Length; i++)
        {
            GameObject spawnedSelectionButton = Instantiate(selectCountryButtonPrefab, slideContent);

            spawnedSelectionButton.transform.localScale = new Vector3(1, 1, 1);
            spawnedSelectionButton.GetComponent<SelectCountryButton>().country_ScriptableObject = currentScenario.countries[i];
            spawnedSelectionButton.GetComponent<SelectCountryButton>().UpdateUI();
        }

        mainMenu.ScrollEffect(slideContent.GetComponent<RectTransform>());
    }

    public void UpdateTournamentCountries()
    {
        foreach (Transform child in _tournamentCountriesContainer)
        {
            Destroy(child.gameObject);
        }

        Scenario currentScenario = GetScenario(currentScenarioId);

        for (int i = 0; i < currentScenario.countries.Length; i++)
        {
            GameObject spawnedSelectionButton = Instantiate(selectCountryButtonPrefabWithCheckMark, _tournamentCountriesContainer);

            spawnedSelectionButton.transform.localScale = new Vector3(1, 1, 1);
            spawnedSelectionButton.GetComponent<SelectCountryButton>().country_ScriptableObject = currentScenario.countries[i];
            spawnedSelectionButton.GetComponent<SelectCountryButton>().UpdateUI();
            spawnedSelectionButton.GetComponent<SelectCountryButton>()._checkmark.SetActive(_tournamentCountries.Contains(currentScenario.countries[i]));

            spawnedSelectionButton.GetComponent<Button>().onClick.RemoveAllListeners();

            spawnedSelectionButton.GetComponent<Button>().onClick.AddListener(
                spawnedSelectionButton.GetComponent<SelectCountryButton>().TournamentCountryToggle
                );
        }
    }

    public void CheckConfirmButton()
    {
        if (_tournamentCountries.Count >= 4)
        {
            _tournament_ConfirmButton.interactable = true;

            string confirmButton_DisplayText = $"{ReferencesManager.Instance.languageManager.GetTranslation("ConfirmButton")}";

            _tournament_ConfirmButton_Text.text = confirmButton_DisplayText;
        }
        else
        {
            _tournament_ConfirmButton.interactable = false;

            string confirmButton_DisplayText = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.SingleMode.Tournament.LimitCountries")}";

            _tournament_ConfirmButton_Text.text = confirmButton_DisplayText;
        }

        string _tournamentCountries_string = "";

        for (int i = 0; i < _tournamentCountries.Count; i++)
        {
            _tournamentCountries_string += $"{_tournamentCountries[i]._id};";
        }

        ReferencesManager.Instance.gameSettings._currentTournamentCountries.value = _tournamentCountries_string;
    }

    public void RemoveEditorKey()
    {
        ReferencesManager.Instance.gameSettings.editingModString.value = "";
    }

    public Scenario GetScenario(int id)
    {
        Scenario result = null;

        for (int i = 0; i < scenarios.Length; i++)
        {
            if (scenarios[i].id == id)
            {
                result = scenarios[i];
            }
        }

        return result;
    }

    public void SelectScenario(int id)
    {
        Scenario currentScenario = null;

        currentScenarioId = id;

        foreach (Scenario scenario in scenarios)
        {
            if (scenario.id == currentScenarioId)
            {
                currentScenario = scenario;
            }
        }

        imagePreview.sprite = currentScenario.mapPreview;
        _currentScenarioData.value = currentScenario._data;
        _currentDate.value = currentScenario.date.ToString();

        UpdateSelectionCountriesUI();
    }

    public void SelectMap(int id)
    {
        currentMapId = id;
        UpdateSelectionCountriesUI();
    }

    public void AddTeam(Team _team)
    {
        teams.Add(_team);
    }

    public void SelectGameMode(string gameMode)
    {
        ReferencesManager.Instance.gameSettings._currentGameMode.value = gameMode;

        for (int i = 0; i < _gameTypeButtonTargetImages.Length; i++)
        {
            _gameTypeButtonTargetImages[i].color = Color.white;
        }

        string gameModeDisplay = "";

        if (gameMode == "historic")
        {
            gameModeDisplay = ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.SingleMode.SelectMode.Historic");
            _gameTypeButtonTargetImages[0].color = _gameType_Color_selected;
        }
        else if (gameMode == "nonhistoric")
        {
            gameModeDisplay = ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.SingleMode.SelectMode.NonHistoric");
            _gameTypeButtonTargetImages[1].color = _gameType_Color_selected;
        }
        else if (gameMode == "tournament")
        {
            _gameTypeButtonTargetImages[2].color = _gameType_Color_selected;
        }

        string warnText = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.SelectModeWarn_1")} {gameModeDisplay}. {ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.SelectModeWarn_2")}";
        if (gameMode != "tournament")
        {
            WarningManager.Instance.Warn(warnText);
        }
    }

    [System.Serializable]
    public class Scenario
    {
        public CountryScriptableObject[] countries;
        public int id;
        public int date;
        public string _name;

        [TextArea(1, 100)]
        public string _data;

        public Sprite mapPreview;
    }

    [System.Serializable]
    public class Team
    {
        public CountryScriptableObject[] countries;
        public int id;
        public string _name;
        [Header("Internal relations")]
        public bool war;
        public bool pact;
        public bool trade;
        public bool right;
        public bool union;
        [Header("External relations")]
        public TeamRelations[] otherTeamsRelations;
    }

    [System.Serializable]
    public class TeamRelations
    {
        public CountryScriptableObject[] team;
        public bool war;
        public bool pact;
        public bool trade;
        public bool right;
        public bool union;
    }
}
