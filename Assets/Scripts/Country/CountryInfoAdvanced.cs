using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class CountryInfoAdvanced : MonoBehaviour
{
    private CameraMovement cameraMovement;
    private CountryManager countryManager;
    private RegionManager regionManager;
    private RegionUI regionUI;

    public GameObject countryInfoPanelAdvanced;

    [SerializeField] TMP_Text countryNameText;
    [SerializeField] TMP_Text countryIdeologyText;
    [SerializeField] TMP_Text countryRegionCountText;
    [SerializeField] TMP_Text countryPopulationCountText;

    [SerializeField] TMP_Text[] horizontalSenderCount;
    [SerializeField] GameObject[] horizontalSenderScrolls;

    [SerializeField] GameObject countryItem;

    [SerializeField] GameObject itemPrefab;
    [SerializeField] GameObject formableNationsGrid;

    public int currentCredit;
    public int currentCreditIncome;
    public int currentCreditMoves;
    public int l;

    [SerializeField] Image[] pieCharts;

    [SerializeField] TMP_Text[] factoriesCount;
    [SerializeField] TMP_Text[] goldIncomes;
    [SerializeField] TMP_Text[] foodIncomes;

    [SerializeField] TMP_Text oilCount;
    [SerializeField] TMP_Text fuelIncome;

    [SerializeField] TMP_Text[] expenses;

    [SerializeField] IdeologyFlagFill[] ideologyFlagFills;

    [Header("# Template Settings: ")]
    [SerializeField] Transform _templatesUIContainer;
    [SerializeField] Transform _templateUnitsUIContainer;
    [SerializeField] GameObject _templateUnitPrefab;
    public GameObject _templatePrefab;

    [SerializeField] GameObject _templatePanel;
    [SerializeField] GameObject _templateBatalionPanel;

    [HideInInspector] public int _currentTemplateIndex;

    [SerializeField] Transform countriesPanel;
    [SerializeField] GameObject countryListItem;

    [HideInInspector] public CountrySettings newVassal;

    private void Awake()
    {
        cameraMovement = FindObjectOfType<CameraMovement>();
        regionManager = FindObjectOfType<RegionManager>();
        countryManager = FindObjectOfType<CountryManager>();
        regionUI = FindObjectOfType<RegionUI>();
    }

    public void SetValues(float[] valuesToSet)
    {
        float totalValues = 0;
        for (int i = 0; i < pieCharts.Length; i++)
        {
            totalValues += FindPercentage(valuesToSet, i);
            pieCharts[i].fillAmount = totalValues;
        }
    }

    private float FindPercentage(float[] valuesToSet, int index)
    {
        float totalAmount = 0;
        for (int i = 0; i < valuesToSet.Length; i++)
        {
            totalAmount += valuesToSet[i];
        }

        return valuesToSet[index] / totalAmount;
    }

    private float ReturnPercentage(float value)
    {
        float percentage = value * 100 / 42; // 42 - max value

        return percentage;
    }

    private float ConvertPercentage(float Percentage)
    {
        float value = Percentage * 42 / 100; // 42 - max value

        return value;
    }

    public void ToggleUI()
    {
        if (!countryInfoPanelAdvanced.activeSelf) // Open
        {
            regionUI.CloseAllUI();
            regionManager.DeselectRegions();
            UpdateIncomesPanel();
            UpdateIdeologyFlagUI();

            countryInfoPanelAdvanced.SetActive(true);

            SetValues(countryManager.currentCountry.popularities);

            BackgroundUI_Overlay.Instance.OpenOverlay(countryInfoPanelAdvanced);
        }
        else if (countryInfoPanelAdvanced.activeSelf)
        {
            regionUI.CloseAllUI();
            regionManager.DeselectRegions();

            countryInfoPanelAdvanced.SetActive(false);
            BackgroundUI_Overlay.Instance.CloseOverlay();
        }

        UpdateUI();
    }

    public void TakeCredit()
    {
        currentCredit += 1000;
        currentCreditMoves = 10;
        currentCreditIncome = currentCredit / currentCreditMoves;

        countryManager.currentCountry.money += 1000;
        countryManager.currentCountry.moneyIncomeUI -= 100;

        WarningManager.Instance.Warn("Вы взяли в долг 1000 золота на 10 ходов.");

        Multiplayer.Instance.SetCountryValues(
            countryManager.currentCountry.country._id,
            countryManager.currentCountry.money,
            countryManager.currentCountry.food,
            countryManager.currentCountry.recroots);

        countryManager.UpdateValuesUI();
        countryManager.UpdateIncomeValuesUI();
    }

    public void CheckCredit()
    {
        if (currentCreditMoves <= 0)
        {
            currentCredit = 0;
            currentCreditMoves = 0;
            countryManager.currentCountry.moneyIncomeUI += currentCreditIncome;
            currentCreditIncome = 0;
        }
        currentCredit -= currentCreditIncome;


        countryManager.UpdateValuesUI();
        countryManager.UpdateIncomeValuesUI();
    }

    private void UpdateIncomesPanel()
    {
        factoriesCount[0].text = $"x{countryManager.currentCountry.civFactories}";
        factoriesCount[1].text = $"x{countryManager.currentCountry.farms}";
        factoriesCount[2].text = $"x{countryManager.currentCountry.farms}";
        factoriesCount[3].text = $"x{countryManager.currentCountry.chemicalFarms}";
        factoriesCount[4].text = $"x{countryManager.currentCountry.chemicalFarms}";

        goldIncomes[0].text = $"{countryManager.currentCountry.civFactories * ReferencesManager.Instance.gameSettings.fabric.goldIncome}";
        goldIncomes[1].text = $"{countryManager.currentCountry.farms * ReferencesManager.Instance.gameSettings.farm.goldIncome}";
        goldIncomes[2].text = $"{countryManager.currentCountry.chemicalFarms * ReferencesManager.Instance.gameSettings.chefarm.goldIncome}";

        foodIncomes[0].text = $"{countryManager.currentCountry.farms * ReferencesManager.Instance.gameSettings.farm.foodIncome}";
        foodIncomes[1].text = $"{countryManager.currentCountry.chemicalFarms * ReferencesManager.Instance.gameSettings.chefarm.foodIncome}";

        expenses[0].text = $"-{countryManager.currentCountry.regionCosts + ReferencesManager.Instance.resourcesMarketManager.CountAllCustomerExpenses(countryManager.currentCountry.country)}";
        expenses[1].text = $"{countryManager.currentCountry.inflation}%";
        expenses[2].text = $"-{countryManager.currentCountry.inflationDebuff}";

        oilCount.text = $"{countryManager.currentCountry.oil}";
        fuelIncome.text = $"{countryManager.currentCountry.fuelIncome}";
    }

    private void UpdateUI()
    {
        if (!ReferencesManager.Instance.gameSettings.spectatorMode)
        {
            countryNameText.text = ReferencesManager.Instance.languageManager.GetTranslation(countryManager.currentCountry.country._nameEN);

            if (countryManager.currentCountry.ideology != "Неопределено" &&
                countryManager.currentCountry.ideology != "Неопределённый" &&
                countryManager.currentCountry.ideology != "Коммунизм" &&
                countryManager.currentCountry.ideology != "Демократия" &&
                countryManager.currentCountry.ideology != "Монархия" &&
                countryManager.currentCountry.ideology != "Фашизм")
            {
                countryIdeologyText.text = countryManager.currentCountry.ideology;
            }

            countryIdeologyText.text = ReferencesManager.Instance.languageManager.GetTranslation(countryManager.currentCountry.ideology);

            countryRegionCountText.text = countryManager.currentCountry.myRegions.Count.ToString();
            countryPopulationCountText.text = ReferencesManager.Instance.GoodNumberString(countryManager.currentCountry.population);

            UpdateDiplomatyUI(countryManager.currentCountry);
        }
    }

    public void UpdateDiplomatyUI(CountrySettings sender)
    {
        for (int i = 0; i < horizontalSenderScrolls.Length; i++)
        {
            DestroyChildrens(horizontalSenderScrolls[i].transform);
        }

        UpdateCountryRelationsUI(horizontalSenderScrolls[0], sender, "vassal");
        UpdateCountryRelationsUI(horizontalSenderScrolls[1], sender, "union");
        UpdateCountryRelationsUI(horizontalSenderScrolls[2], sender, "trade");
        UpdateCountryRelationsUI(horizontalSenderScrolls[3], sender, "wars");


        for (int i = 0; i < horizontalSenderCount.Length; i++)
        {
            UpdateCountryCountTextUI(horizontalSenderCount[i], horizontalSenderScrolls[i].transform);
        }
    }

    private void UpdateCountryCountTextUI(TMP_Text text, Transform parent)
    {
        int childcount = parent.childCount;
        text.text = childcount.ToString();
        parent.GetComponent<RectTransform>().sizeDelta = new Vector2(80 * parent.transform.childCount, 75);
    }

    private void DestroyChildrens(Transform parent)
    {
        List<GameObject> childrens = new List<GameObject>();

        foreach (Transform child in parent.Cast<Transform>().ToArray())
        {
            childrens.Add(child.gameObject);
        }

        foreach (GameObject child in childrens)
        {
            child.transform.SetParent(null);
            DestroyImmediate(child);
        }
    }

    public void DestroyChildrens()
    {
        for (int i = 0; i < horizontalSenderScrolls.Length; i++)
        {
            DestroyChildrens(horizontalSenderScrolls[i].transform);
        }
    }

    private void UpdateIdeologyFlagUI()
    {
        for (int i = 0; i < ideologyFlagFills.Length; i++)
        {
            ideologyFlagFills[i].SetUp();
        }
    }

    private void UpdateCountryRelationsUI(GameObject horizontalScroll, CountrySettings country, string data)
    {
        if (data == "vassal")
        {
            foreach (Relationships.Relation relation in country.GetComponent<Relationships>().relationship)
            {
                if (relation.vassal)
                {
                    GameObject spawnedItem = Instantiate(countryItem, horizontalScroll.transform);
                    spawnedItem.GetComponent<FillCountryFlag>().country = relation.country.country;
                    spawnedItem.GetComponent<FillCountryFlag>().InDiplomatyUI = true;
                    spawnedItem.GetComponent<FillCountryFlag>().FillInfo();
                }
            }

            horizontalScroll.GetComponent<RectTransform>().sizeDelta = new Vector2(80 * horizontalScroll.transform.childCount, 75);
        }
        else if (data == "union")
        {
            foreach (Relationships.Relation relation in country.GetComponent<Relationships>().relationship)
            {
                if (relation.union)
                {
                    GameObject spawnedItem = Instantiate(countryItem, horizontalScroll.transform);
                    spawnedItem.GetComponent<FillCountryFlag>().country = relation.country.country;
                    spawnedItem.GetComponent<FillCountryFlag>().InDiplomatyUI = true;
                    spawnedItem.GetComponent<FillCountryFlag>().FillInfo();
                }
            }

            horizontalScroll.GetComponent<RectTransform>().sizeDelta = new Vector2(80 * horizontalScroll.transform.childCount, 75);
        }
        else if (data == "trade")
        {
            foreach (Relationships.Relation relation in country.GetComponent<Relationships>().relationship)
            {
                if (relation.trade)
                {
                    GameObject spawnedItem = Instantiate(countryItem, horizontalScroll.transform);
                    spawnedItem.GetComponent<FillCountryFlag>().country = relation.country.country;
                    spawnedItem.GetComponent<FillCountryFlag>().InDiplomatyUI = true;
                    spawnedItem.GetComponent<FillCountryFlag>().FillInfo();
                }
            }
            horizontalScroll.GetComponent<RectTransform>().sizeDelta = new Vector2(80 * horizontalScroll.transform.childCount, 75);
        }
        else if (data == "wars")
        {
            foreach (Relationships.Relation relation in country.GetComponent<Relationships>().relationship)
            {
                if (relation.war)
                {
                    GameObject spawnedItem = Instantiate(countryItem, horizontalScroll.transform);
                    spawnedItem.GetComponent<FillCountryFlag>().country = relation.country.country;
                    spawnedItem.GetComponent<FillCountryFlag>().InDiplomatyUI = true;
                    spawnedItem.GetComponent<FillCountryFlag>().FillInfo();
                }
            }
            horizontalScroll.GetComponent<RectTransform>().sizeDelta = new Vector2(80 * horizontalScroll.transform.childCount, 75);
        }
    }

    public void ChangeIdeology(string ideologyName)
    {
        countryManager.currentCountry.ideology = ideologyName;
        countryManager.UpdateCountryGraphics(ideologyName);
        countryManager.UpdateCountryInfo();

        UpdateUI();
    }

    public void ChangeMobilizationLaw(int id)
    {
        CountrySettings country = countryManager.currentCountry;

        ReferencesManager.Instance.ChangeMobilizationLaw(id, country);

        countryManager.UpdateCountryInfo();
        countryManager.UpdateValuesUI();
        countryManager.UpdateIncomeValuesUI();

        UpdateUI();
    }


    public void UpdateFormableNationsUI()
    {

        foreach (Transform child in formableNationsGrid.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < countryManager.currentCountry.formableNations.Length; i++)
        {
            GameObject spawnedObject = Instantiate(itemPrefab, formableNationsGrid.transform);

            spawnedObject.GetComponent<NationButton>().currentNation = countryManager.currentCountry.formableNations[i];
            spawnedObject.GetComponent<NationButton>().SetUp();
        }
    }

    public void UpdateTemplatesUI()
    {
        _templatePanel.SetActive(true);
        _templateBatalionPanel.SetActive(false);

        foreach (Transform child in _templatesUIContainer)
        {
            if (child.gameObject.GetComponent<ArmyTemplateItem_UI>())
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < ReferencesManager.Instance.army.templates.Count; i++)
        {
            GameObject newTemplateObject = Instantiate(_templatePrefab, _templatesUIContainer);

            ArmyTemplateItem_UI armyTemplateItem = newTemplateObject.GetComponent<ArmyTemplateItem_UI>();
            armyTemplateItem._index = i;
            armyTemplateItem._name = ReferencesManager.Instance.army.templates[i]._name;

            armyTemplateItem.SetUp();
        }
    }

    public void UpdateTemplateUI()
    {
        _templatePanel.SetActive(false);
        _templateBatalionPanel.SetActive(true);

        foreach (Transform child in _templateUnitsUIContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < ReferencesManager.Instance.army.templates[_currentTemplateIndex]._batalions.Count; i++)
        {
            GameObject newTemplateUnit = Instantiate(_templateUnitPrefab, _templateUnitsUIContainer);

            ArmyTemplateUnit_UI armyTemplateItem = newTemplateUnit.GetComponent<ArmyTemplateUnit_UI>();

            armyTemplateItem.batalion = ReferencesManager.Instance.army.templates[_currentTemplateIndex]._batalions[i];
            armyTemplateItem.index = i;

            armyTemplateItem.SetUp();
        }

        ArmyTemplateUnitBuy_UI[] unitBuy_UIs = FindObjectsOfType<ArmyTemplateUnitBuy_UI>();

        for (int i = 0; i < unitBuy_UIs.Length; i++)
        {
            unitBuy_UIs[i].UpdateUI();
        }
    }

    public void CreateTemplate()
    {
        Army.Template template = new Army.Template();

        string newName = "";

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            newName = "New template";
        }
        if (PlayerPrefs.GetInt("languageId") == 1)
        {
            newName = "Новый шаблон";
        }

        int templatesCount = ReferencesManager.Instance.army.templates.Count + 1;

        template._name = $"{newName} ({templatesCount})";
        template._batalions.Add(ReferencesManager.Instance.gameSettings.soldierLVL1);

        ReferencesManager.Instance.army.templates.Add(template);

        UpdateTemplatesUI();
    }

    public void AddUnitToTemplate(UnitScriptableObject unit)
    {
        if (ReferencesManager.Instance.army.templates[_currentTemplateIndex]._batalions.Count + 1 <= 10)
        {
            ReferencesManager.Instance.army.templates[_currentTemplateIndex]._batalions.Add(unit);
            UpdateTemplateUI();
        }
        else
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                WarningManager.Instance.Warn("The battalion limit has been reached");
            }
            if (PlayerPrefs.GetInt("languageId") == 1)
            {
                WarningManager.Instance.Warn("Достигнут лимит батальонов");
            }
        }
    }

    public void CreateVassalMenu()
    {
        foreach (Transform child in countriesPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (CountryScriptableObject _country in ReferencesManager.Instance.globalCountries)
        {
            if (_country._id != ReferencesManager.Instance.countryManager.currentCountry.country._id)
            {
                bool countryExists = ReferencesManager.Instance.countryManager.countries.Any(item => item.country._id == _country._id && item.exist == true);

                if (!countryExists)
                {
                    GameObject spawnedCountryItem = Instantiate(countryListItem, countriesPanel);
                    spawnedCountryItem.GetComponent<SelectCountryButton>().country_ScriptableObject = _country;
                    spawnedCountryItem.GetComponent<SelectCountryButton>().UpdateUI();

                    spawnedCountryItem.GetComponent<Button>().onClick.AddListener(spawnedCountryItem.GetComponent<SelectCountryButton>().CreateVassal);
                }
                else if (countryExists)
                {
                    GameObject spawnedCountryItem = Instantiate(countryListItem, countriesPanel);
                    spawnedCountryItem.GetComponent<SelectCountryButton>().country_ScriptableObject = _country;
                    spawnedCountryItem.GetComponent<SelectCountryButton>().UpdateUI();
                    spawnedCountryItem.GetComponent<Button>().interactable = false;

                    spawnedCountryItem.GetComponent<Button>().onClick.AddListener(spawnedCountryItem.GetComponent<SelectCountryButton>().CreateVassal);
                }
            }
        }
    }

    public void VassalGetRegions()
    {
        foreach (RegionManager province in ReferencesManager.Instance.gameSettings.provincesList)
        {
            ReferencesManager.Instance.AnnexRegion(province, newVassal);
        }

        ReferencesManager.Instance.regionUI.createVassalRegionSelectionModeButton.SetActive(false);
        ReferencesManager.Instance.gameSettings.regionSelectionMode = false;
        countryInfoPanelAdvanced.SetActive(true);
    }

    [System.Serializable]
    public class Nation
    {
        public CountryScriptableObject country;
        public List<RegionManager> regionsNeeded = new List<RegionManager>();
    }
}
