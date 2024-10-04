using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Army;
using R = ReferencesManager;

public class CountryInfoAdvanced : MonoBehaviour
{
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

    [SerializeField] private Sprite[] _templatesIcons;
    [SerializeField] private TMP_InputField _templateName_Inputfield;
    [SerializeField] private Image _template_customIconHolder;

    [SerializeField] GameObject _templatePanel;
    [SerializeField] GameObject _templateBatalionPanel;

    [HideInInspector] public int _currentTemplateIndex;

    [SerializeField] Transform countriesPanel;
    [SerializeField] GameObject countryListItem;

    [HideInInspector] public CountrySettings newVassal;

    [Header("# Stability Settings: ")]
    [SerializeField] private StabilityUI_Item _stabilityPrefab;
    [SerializeField] private TMP_Text _stabilityValue;
    [SerializeField] private Transform _stabilityBuffsContainer;

    private int currentTemplateIconSelected;

    private void Awake()
    {
        regionManager = ReferencesManager.Instance.regionManager;
        countryManager = ReferencesManager.Instance.countryManager;
        regionUI = ReferencesManager.Instance.regionUI;
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

        Multiplayer.Instance.SetCountryValues(
            countryManager.currentCountry.country._id,
            countryManager.currentCountry.money,
            countryManager.currentCountry.food,
            countryManager.currentCountry.recruits);

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
        expenses[1].text = $"{Mathf.Round(countryManager.currentCountry.inflation)}%";
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

            double stability = Mathf.Round(countryManager.currentCountry.stability.value);

            _stabilityValue.text = $"{stability}%";

            if (stability >= 60)
            {
                _stabilityValue.color = Color.green;
            }
            else if (stability <= 40)
            {
                _stabilityValue.color = Color.red;
            }
            else
            {
                _stabilityValue.color = Color.yellow;
            }

            UpdateStabilityBuffs();

            UpdateDiplomatyUI(countryManager.currentCountry);
        }
    }

    private void UpdateStabilityBuffs()
    {
        foreach (Transform item in _stabilityBuffsContainer)
        {
            Destroy(item.gameObject);
        }

        foreach (Stability_buff buff in countryManager.currentCountry.stability.buffs)
        {
            if (buff != null)
            {
                GameObject prefab = Instantiate(_stabilityPrefab.gameObject, _stabilityBuffsContainer);

                StabilityUI_Item stabilityUI_Item = prefab.gameObject.GetComponent<StabilityUI_Item>();
                stabilityUI_Item._bonus = buff;

                stabilityUI_Item.SetUp();
            }
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

        if (countryManager.currentCountry.ideology == "Демократия")
        {
            countryManager.currentCountry.stability.buffs.Add(new Stability_buff("Бюрократические издержки", -5f, new List<string>() { $"date_before;{R.Instance.dateManager.currentDate[0]}-{R.Instance.dateManager.currentDate[1]}-{R.Instance.dateManager.currentDate[2] + 1}" }, null, R.Instance.sprites.Find("bureaucratic_costs")));
            countryManager.currentCountry.stability.buffs.Add(new Stability_buff("Изменение идеологии", -10f, new List<string>() { $"date_before;{R.Instance.dateManager.currentDate[0]}-{R.Instance.dateManager.currentDate[1]}-{R.Instance.dateManager.currentDate[2] + 1}" }, null, R.Instance.sprites.Find("ideology_change")));

            countryManager.currentCountry.stability.buffs.Add(new Stability_buff("Свобода слова", 10f, new List<string>() { $"not;is_ideology;{countryManager.currentCountry.country._id};d" }, new List<string>() { $"DEV_UPD_IDEOLOGY_STABILITY_BUFFS;{countryManager.currentCountry.country._id}" }, R.Instance.sprites.Find("freedom_of_speech")));
        }
        else if (countryManager.currentCountry.ideology == "Фашизм")
        {
            countryManager.currentCountry.stability.buffs.Add(new Stability_buff("Сила нации", 15f, new List<string>() { $"not;is_ideology;{countryManager.currentCountry.country._id};f" }, new List<string>() { $"DEV_UPD_IDEOLOGY_STABILITY_BUFFS;{countryManager.currentCountry.country._id}" }, R.Instance.sprites.Find("power_of_nation")));
            countryManager.currentCountry.stability.buffs.Add(new Stability_buff("Изменение идеологии", -10f, new List<string>() { $"date_before;{R.Instance.dateManager.currentDate[0]}-{R.Instance.dateManager.currentDate[1]}-{R.Instance.dateManager.currentDate[2] + 1}" }, null, R.Instance.sprites.Find("ideology_change")));
        }
        else if (countryManager.currentCountry.ideology == "Коммунизм")
        {
            countryManager.currentCountry.stability.buffs.Add(new Stability_buff("Плановая экономика", 7.5f, new List<string>() { $"not;is_ideology;{countryManager.currentCountry.country._id};c" }, new List<string>() { $"DEV_UPD_IDEOLOGY_STABILITY_BUFFS;{countryManager.currentCountry.country._id}" }, R.Instance.sprites.Find("planned_economy")));
            countryManager.currentCountry.stability.buffs.Add(new Stability_buff("Изменение идеологии", -10f, new List<string>() { $"date_before;{R.Instance.dateManager.currentDate[0]}-{R.Instance.dateManager.currentDate[1]}-{R.Instance.dateManager.currentDate[2] + 1}" }, null, R.Instance.sprites.Find("ideology_change")));
        }

        UpdateUI();
        UpdateStabilityBuffs();

        ToggleUI();
        ToggleUI();
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
            armyTemplateItem._icon = ReferencesManager.Instance.army.templates[i]._icon;

            armyTemplateItem.SetUp();
        }
    }

    public void UpdateTemplateUnits()
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

    public void UpdateTemplateUI()
    {
        _templatePanel.SetActive(false);
        _templateBatalionPanel.SetActive(true);

        _templateName_Inputfield.text = ReferencesManager.Instance.army.templates[_currentTemplateIndex]._name;
        _template_customIconHolder.sprite = ReferencesManager.Instance.army.templates[_currentTemplateIndex]._icon;
    }

    public void CreateTemplate()
    {
        Army.Template template = new Army.Template();

        string newName = ReferencesManager.Instance.languageManager.GetTranslation("NewTemplateText");

        int templatesCount = ReferencesManager.Instance.army.templates.Count + 1;

        template._name = $"{newName} ({templatesCount})";
        template._icon = ReferencesManager.Instance.gameSettings.soldierLVL1.icon;

        if (!string.IsNullOrEmpty(template._name))
        {
            template._batalions.Add(ReferencesManager.Instance.gameSettings.soldierLVL1);

            ReferencesManager.Instance.army.templates.Add(template);

            UpdateTemplatesUI();
        }
    }

    public void ChangeTemplateIcon(int increment)
    {
        currentTemplateIconSelected += increment;

        if (currentTemplateIconSelected > _templatesIcons.Length)
        {
            currentTemplateIconSelected = 0;
        }
        else if (currentTemplateIconSelected < 0)
        {
            currentTemplateIconSelected = _templatesIcons.Length; 
        }

        _template_customIconHolder.sprite = _templatesIcons[currentTemplateIconSelected];
    }


    public void UpdateCustomization()
    {
        Template template = ReferencesManager.Instance.army.templates[_currentTemplateIndex];

        template._icon = _template_customIconHolder.sprite;

        if (!string.IsNullOrEmpty(_templateName_Inputfield.text) ||
            !string.IsNullOrWhiteSpace(_templateName_Inputfield.text))
        {
            template._name = _templateName_Inputfield.text;
        }

        UpdateTemplatesUI();
    }

    public void AddUnitToTemplate(UnitScriptableObject unit)
    {
        if (ReferencesManager.Instance.army.templates[_currentTemplateIndex]._batalions.Count + 1 <= 10)
        {
            ReferencesManager.Instance.army.templates[_currentTemplateIndex]._batalions.Add(unit);
            UpdateTemplateUnits();
        }
        else
        {
            WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.BatalionLimit"));
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

                GameObject spawnedCountryItem = Instantiate(countryListItem, countriesPanel);
                spawnedCountryItem.GetComponent<SelectCountryButton>().country_ScriptableObject = _country;
                spawnedCountryItem.GetComponent<SelectCountryButton>().UpdateUI();
                spawnedCountryItem.GetComponent<Button>().interactable = !countryExists;

                spawnedCountryItem.GetComponent<Button>().onClick.AddListener(spawnedCountryItem.GetComponent<SelectCountryButton>().CreateVassal);
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
