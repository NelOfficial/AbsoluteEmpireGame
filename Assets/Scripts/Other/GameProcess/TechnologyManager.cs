using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TechnologyManager : MonoBehaviour
{
    [SerializeField] GameObject container;
    [SerializeField] GameObject[] tabs;
    [SerializeField] Image[] buttons;
    
    [SerializeField] GameObject selectionPanel;
    [SerializeField] TMP_Text techName;
    [SerializeField] TMP_Text techDescription;
    [SerializeField] TMP_Text techMoneyCost;
    [SerializeField] TMP_Text techReserchingPointsCost;
    [SerializeField] TMP_Text techMoves;

    [SerializeField] TMP_Text recearchButtonText;
    [SerializeField] Button recearchButton;
    [SerializeField] Button cancelButton;

    private TechnologyScriptableObject selectedTechnology;
    private CountryManager countryManager;
    private RegionUI regionUI;

    [SerializeField] Color defaultColor;
    [SerializeField] Color defaultButtonColor;
    [SerializeField] Color selectedColor;

    [SerializeField] Color greenColor;
    [SerializeField] Color yellowColor;
    [SerializeField] Color redColor;

    [SerializeField] Image notificationImage;
    [SerializeField] TMP_Text movesLeftText;

    [SerializeField] Transform neededTechsGrid;
    [SerializeField] GameObject neededTechsPanel;

    [SerializeField] TMP_Text neededTechPrefab;

    public TechQueue currentTech;

    [SerializeField] TechnologyButton[] technologyButtons;

    [SerializeField] int lastTab;


    private void Awake()
    {
        countryManager = FindObjectOfType<CountryManager>();
        regionUI = FindObjectOfType<RegionUI>();
        OpenTab(0);

        SetResearchState(false);
    }

    public bool HasTech(CountrySettings country, TechnologyScriptableObject tech)
    {
        if (country.countryTechnologies.Contains(tech))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HasUnitTech(CountrySettings country, UnitScriptableObject unit)
    {
        if (HasTech(country, ReferencesManager.Instance.gameSettings.technologies[unit.unlockLevel]))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OpenTab(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
            tabs[index].SetActive(true);

            buttons[i].color = defaultColor;
            buttons[index].color = selectedColor;
        }

        technologyButtons = FindObjectsOfType<TechnologyButton>();

        for (int i = 0; i < technologyButtons.Length; i++)
        {
            technologyButtons[i].SetUp();
            CheckButtonColor(technologyButtons[i]);
        }

        selectionPanel.SetActive(false);
        lastTab = index;
    }

    public void OpenUI()
    {
        regionUI.CloseAllUI();
        BackgroundUI_Overlay.Instance.CloseOverlay();

        container.SetActive(true);
        OpenTab(lastTab);
    }

    public void SelectTechnology(TechnologyScriptableObject tech)
    {
        selectedTechnology = tech;

        selectionPanel.SetActive(true);

        foreach (Transform child in neededTechsGrid)
        {
            Destroy(child.gameObject);
        }

        if (selectedTechnology.techsNeeded.Length > 0)
        {
            neededTechsPanel.SetActive(true);

            for (int i = 0; i < selectedTechnology.techsNeeded.Length; i++)
            {
                TMP_Text spawnedNeededTech = Instantiate(neededTechPrefab);
                spawnedNeededTech.transform.SetParent(neededTechsGrid);
                spawnedNeededTech.transform.localScale = new Vector3(1, 1, 1);

                spawnedNeededTech.text = ReferencesManager.Instance.languageManager.GetTranslation(selectedTechnology.techsNeeded[i]._name);

                if (Researched(selectedTechnology.techsNeeded[i]))
                {
                    spawnedNeededTech.color = ReferencesManager.Instance.gameSettings.greenColor;
                    spawnedNeededTech.fontStyle = FontStyles.Strikethrough;
                }
                else
                {
                    spawnedNeededTech.color = ReferencesManager.Instance.gameSettings.redColor;
                }
            }
        }
        else
        {
            neededTechsPanel.SetActive(false);
        }

        techName.text = ReferencesManager.Instance.languageManager.GetTranslation(selectedTechnology._name);
        techDescription.text = ReferencesManager.Instance.languageManager.GetTranslation($"{selectedTechnology._name}.desc");

        if (selectedTechnology.moneyCost >= 5000)
        {
            techMoneyCost.text = $"{ReferencesManager.Instance.GoodNumberString(selectedTechnology.moneyCost)}";
        }
        else
        {
            techMoneyCost.text = $"{selectedTechnology.moneyCost}";
        }

        techMoves.text = selectedTechnology.moves.ToString();
        techReserchingPointsCost.text = selectedTechnology.researchPointsCost.ToString();

        CheckConnectors();

        if (currentTech != null && currentTech != null)
        {
            if (selectedTechnology == currentTech.tech)
            {
                recearchButton.gameObject.SetActive(false);
                cancelButton.gameObject.SetActive(true);
            }
            else
            {
                recearchButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(false);
            }
        }
        else
        {
            recearchButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(false);
        }
    }

    private void CheckButtonColor(TechnologyButton button)
    {
        if (Researched(button.technology))
        {
            button.GetComponent<Button>().targetGraphic.GetComponent<Image>().color = ReferencesManager.Instance.gameSettings.greenColor;
        }
        else if (!Researched(button.technology))
        {
            button.GetComponent<Button>().targetGraphic.GetComponent<Image>().color = defaultButtonColor;
        }
    }

    public bool Researched(TechnologyScriptableObject tech)
    {
        return countryManager.currentCountry.countryTechnologies.Contains(tech);
    }

    public void SetResearchState(bool researching)
    {
        if (researching)
        {
            notificationImage.color = yellowColor;
            movesLeftText.text = $"{currentTech.moves}";
        }
        else if (!researching)
        {
            notificationImage.color = greenColor;
            movesLeftText.text = "";
        }
    }

    public void StartRecearch()
    {
        if (currentTech == null || currentTech.tech == null)
        {
            TechQueue techQueue = new TechQueue();
            techQueue.tech = selectedTechnology;
            techQueue.moves = selectedTechnology.moves;

            currentTech = techQueue;
            countryManager.currentCountry.money -= currentTech.tech.moneyCost;
            countryManager.currentCountry.researchPoints -= currentTech.tech.researchPointsCost;
            countryManager.UpdateValuesUI();

            SelectTechnology(currentTech.tech);
            SetResearchState(true);
        }
        else
        {
            WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.technologyAlready"));
        }

        Multiplayer.Instance.SetCountryValues(
            countryManager.currentCountry.country._id,
            countryManager.currentCountry.money,
            countryManager.currentCountry.food,
            countryManager.currentCountry.recruits);
    }

    private void CheckConnectors()
    {
        int neededTechResearched = 0;

        if (selectedTechnology.techsNeeded.Length > 0)
        {
            for (int i = 0; i < selectedTechnology.techsNeeded.Length; i++)
            {
                if (Researched(selectedTechnology.techsNeeded[i]))
                {
                    neededTechResearched++;
                }
            }
        }
        else
        {
            recearchButton.interactable = true;
        }

        if (countryManager.currentCountry.money >= selectedTechnology.moneyCost && countryManager.currentCountry.researchPoints >= selectedTechnology.researchPointsCost)
        {
            if (selectedTechnology.techsNeeded.Length > 0)
            {
                if (!selectedTechnology.optional)
                {
                    if (neededTechResearched == selectedTechnology.techsNeeded.Length)
                    {
                        recearchButton.interactable = true;
                    }
                    else
                    {
                        recearchButton.interactable = false;
                    }
                }
                else
                {
                    if (neededTechResearched == 1)
                    {
                        recearchButton.interactable = true;
                    }
                    else
                    {
                        recearchButton.interactable = false;
                    }
                }
            }
            else
            {
                recearchButton.interactable = true;
            }
        }
        else
        {
            recearchButton.interactable = false;
        }

        if (Researched(selectedTechnology))
        {
            recearchButton.interactable = false;
            recearchButtonText.text = ReferencesManager.Instance.languageManager.GetTranslation("TechnologyPanel.Researched");

        }
        else
        {
            recearchButtonText.text = ReferencesManager.Instance.languageManager.GetTranslation("TechnologyPanel.Research");

        }
    }

    public void CancelRecearch()
    {
        countryManager.currentCountry.money += currentTech.tech.moneyCost;
        countryManager.currentCountry.researchPoints += currentTech.tech.researchPointsCost;
        countryManager.UpdateValuesUI();

        currentTech.tech = null;
        selectionPanel.SetActive(false);

        SetResearchState(false);

        Multiplayer.Instance.SetCountryValues(
            countryManager.currentCountry.country._id,
            countryManager.currentCountry.money,
            countryManager.currentCountry.food,
            countryManager.currentCountry.recruits);
    }

    [System.Serializable]
    public class TechQueue
    {
        public TechnologyScriptableObject tech;
        public int moves;
    }
}
