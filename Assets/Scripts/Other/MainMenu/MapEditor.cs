using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

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
    [SerializeField] TMP_InputField nameInputField;
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
        modData = PlayerPrefs.GetString($"MODIFICATION_{modID}");

        string[] lines = modData.Split(';');

        try
        {
            string _line = lines[0];
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


        int regionsLineIndex = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == "##########")
            {
                regionsLineIndex = i + 1;
            }
        }

        List<int> countriesInRegionsIDs = new List<int>();

        for (int r = regionsLineIndex; r < lines.Length; r++)
        {
            try
            {
                string _line = lines[r];
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
                string _line = lines[i + regionsLineIndex];
                if (_line != "##########")
                {
                    string[] regionIdParts = _line.Split(' ');
                    regionValue = regionIdParts[0].Remove(0, 7);
                }

                //Debug.Log($"{regions[i]._id} ({regions[i].currentCountry.country._nameEN}) | {regionValue}");

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
        CheckPublish();
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
}
