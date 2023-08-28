using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountryInfoAdvanced : MonoBehaviour
{
    private CameraMovement cameraMovement;
    private CountryManager countryManager;
    private RegionManager regionManager;
    private RegionUI regionUI;

    [SerializeField] GameObject countryInfoPanelAdvanced;

    [SerializeField] TMP_Text countryNameText;
    [SerializeField] TMP_Text countryIdeologyText;
    [SerializeField] TMP_Text countryRegionCountText;
    [SerializeField] TMP_Text countryPopulationCountText;

    [SerializeField] TMP_Text[] horizontalSenderCount;
    [SerializeField] GameObject[] horizontalSenderScrolls;

    [SerializeField] GameObject countryItem;

    [SerializeField] GameObject itemPrefab;
    [SerializeField] GameObject formableNationsGrid;

    [HideInInspector] public int currentCredit;
    [HideInInspector] public int currentCreditIncome;
    [HideInInspector] public int currentCreditMoves;
    [HideInInspector] public int l;

    [SerializeField] Image[] pieCharts;

    [SerializeField] TMP_Text[] factoriesCount;
    [SerializeField] TMP_Text[] goldIncomes;
    [SerializeField] TMP_Text[] foodIncomes;

    [SerializeField] TMP_Text[] expenses;

    [SerializeField] IdeologyFlagFill[] ideologyFlagFills;

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
        countryManager.currentCountry.moneyNaturalIncome -= 100;

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
            if (!ReferencesManager.Instance.gameSettings.spectatorMode) countryManager.currentCountry.moneyNaturalIncome += currentCreditIncome;
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

        expenses[0].text = $"-{countryManager.currentCountry.regionCosts}";
        expenses[1].text = $"{countryManager.currentCountry.inflation}%";
        expenses[2].text = $"-{countryManager.currentCountry.inflationDebuff}";
    }

    private void UpdateUI()
    {
        if (!ReferencesManager.Instance.gameSettings.spectatorMode)
        {
            countryNameText.text = countryManager.currentCountry.country._uiName;
            countryIdeologyText.text = countryManager.currentCountry.ideology;

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
        country.population += country.recroots;

        int reservedManPower = country.population / 100 * ReferencesManager.Instance.gameSettings.mobilizationPercent[id];

        country.population -= reservedManPower;
        country.recroots = reservedManPower;


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

    [System.Serializable]
    public class Nation
    {
        public CountryScriptableObject country;
        public List<RegionManager> regionsNeeded = new List<RegionManager>();
    }
}
