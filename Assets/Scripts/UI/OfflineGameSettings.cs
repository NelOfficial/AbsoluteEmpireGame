using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OfflineGameSettings : MonoBehaviour
{
    [SerializeField] Transform slideContent;
    [SerializeField] GameObject selectCountryButtonPrefab;
    [SerializeField] Image imagePreview;

    [SerializeField] int currentMapId;
    [SerializeField] int currentScenarioId;

    [SerializeField] Scenario[] scenarios;
    [SerializeField] List<Team> teams = new List<Team>();

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

    public void RemoveEditorKey()
    {
        PlayerPrefs.DeleteKey("CURRENT_EDITING_MODIFICATION");
    }

    private Scenario GetScenario(int id)
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

    [System.Serializable]
    public class Scenario
    {
        public CountryScriptableObject[] countries;
        public int id;
        public int date;
        public string _name;

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
