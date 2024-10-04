using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Army : MonoBehaviour
{
    public GameObject encircleAnimation;

    public GameObject unitPrefab;
    public GameObject unitUIPrefab;
    public GameObject addArmyButton;
    public GameObject armyHorizontalGroup;
    public GameObject garrisonHorizontalGroup;

    public Image infantryRatio;
    public Image artileryRatio;
    public Image heavyRatio;

    public TMP_Text[] attackerArmy;
    public TMP_Text[] defenderArmy;

    public Sprite chatSprite;
    public Sprite pointSprite;
    public Sprite attackSprite;

    public TMP_Text[] attackerArmyLosses; // 0 - Infantry; 1 - Artilery; 2 - Heavy; 3 - Cavalry
    public TMP_Text[] attackerEconomy; // 0 - MoneyIncome; 1 - FoodIncome; 2 - Population; 3 - CivFactory; 4 - Farm

    public TMP_Text[] defenderArmyLosses; // 0 - Infantry; 1 - Artilery; 2 - Heavy
    public TMP_Text[] defenderEconomy; // 0 - MoneyIncome; 1 - FoodIncome; 2 - Population; 3 - CivFactory; 4 - Farm

    public TMP_Text[] defenderBonus; // 0 - attack buff; 1 - defence buff;
    public TMP_Text[] attackerBonus; // 0 - attack buff; 1 - defence buff;

    public int[] attackerArmyLossesValue = new int[3]; // 0 - Infantry; 1 - Artilery; 2 - Heavy
    [HideInInspector] public int[] attackerEconomyValue; // 0 - MoneyIncome; 1 - FoodIncome; 2 - Population; 3 - CivFactory; 4 - Farm

    public int[] defenderArmyLossesValue = new int[3]; // 0 - Infantry; 1 - Artilery; 2 - Heavy
    [HideInInspector] public int[] defenderEconomyValue; // 0 - MoneyIncome; 1 - FoodIncome; 2 - Population; 3 - CivFactory; 4 - Farm

    [HideInInspector] public UnitMovement[] unitMovements;
    [HideInInspector] public List<GameObject> nearestPoints = new List<GameObject>();

    [HideInInspector] public int maxUnits = 10;

    private Transform unitTransform;
    private UnitMovement unitMovement;

    private List<UnitUI> unitUIs = new List<UnitUI>();

    public List<Template> templates = new List<Template>();

    [SerializeField] private TMP_Text _healHP_description;

    // Temp values
    private int countOfBatalions = 0;
    private float recruits = 0;
    private float money = 0;
    private float food = 0;

    private void Start()
    {
        unitMovements = FindObjectsOfType<UnitMovement>();

        MovePoint[] movePoints = FindObjectsOfType<MovePoint>();
        foreach (MovePoint movePoint in movePoints)
        {
            movePoint.GetComponent<PolygonCollider2D>().enabled = false;
        }
    }

    public void CreateUnit()
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.CreateUnit(ReferencesManager.Instance.regionManager.currentRegionManager._id);
        }
        else
        {
            TryCreateUnitInRegion(ReferencesManager.Instance.regionManager.currentRegionManager);
        }

        ReferencesManager.Instance.regionManager.currentRegionManager.UpdateRegionUI();
        ReferencesManager.Instance.regionManager.CheckRegionUnits(ReferencesManager.Instance.regionManager.currentRegionManager);
    }

    public void CreateUnitInRegion(RegionManager region)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.CreateUnit(region._id);
        }
        else
        {
            InstantiateUnit(unitPrefab, region.transform);
        }

        region.CheckRegionUnits(region);
    }

    private void TryCreateUnitInRegion(RegionManager region)
    {
        UnitScriptableObject unit = ReferencesManager.Instance.gameSettings.soldierLVL1;
        var country = ReferencesManager.Instance.countryManager.currentCountry;

        if (country.money >= unit.moneyCost && country.recruits >= unit.recrootsCost && country.food >= unit.foodCost)
        {
            InstantiateUnit(unitPrefab, region.transform);
            AddUnitToArmy(unit);
            ReferencesManager.Instance.regionManager.CheckRegionUnits(region);
        }
        else
        {
            region.hasArmy = false;
        }
    }

    private void InstantiateUnit(GameObject prefab, Transform parent)
    {
        GameObject spawnedUnit = Instantiate(prefab, parent);
        spawnedUnit.transform.localScale = prefab.transform.localScale;

        var region = parent.GetComponent<RegionManager>();
        region.hasArmy = true;

        var movement = spawnedUnit.GetComponent<UnitMovement>();
        movement.currentCountry = region.currentCountry;
        movement.currentProvince = region;
        movement.UpdateInfo();
        region.currentCountry.countryUnits.Add(movement);
    }

    public void AddUnitToArmy(UnitScriptableObject unit)
    {
        ProcessUnitAddition(unit);
    }

    private void ProcessUnitAddition(UnitScriptableObject unit)
    {
        unitTransform = GetUnitTransform(ReferencesManager.Instance.regionManager.currentRegionManager.transform);

        if (unitTransform != null)
        {
            unitMovement = unitTransform.GetComponent<UnitMovement>();

            if (unitMovement.unitsHealth.Count < maxUnits)
            {
                var country = ReferencesManager.Instance.countryManager.currentCountry;
                if (HasSufficientResources(country, unit))
                {
                    DeductResources(country, unit);

                    if (ReferencesManager.Instance.gameSettings.onlineGame)
                    {
                        Multiplayer.Instance.AddUnitToArmy(unit.unitName, ReferencesManager.Instance.regionManager.currentRegionManager._id);
                    }
                    else
                    {
                        AddUnitHealth(unitMovement, unit);
                    }

                    UpdateAllUIs(false);
                }
                else
                {
                    DisplayResourceWarning(country, unit);
                }
            }
            else
            {
                WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.ArmyLimit"));
            }
        }
    }

    private Transform GetUnitTransform(Transform parent)
    {
        Transform transform = null;

        foreach (Transform child in parent)
        {
            if (child.name == "Unit(Clone)")
            {
                transform = child;
            }
        }

        return transform;
    }

    private bool HasSufficientResources(CountrySettings country, UnitScriptableObject unit)
    {
        return country.money >= unit.moneyCost && country.recruits >= unit.recrootsCost && country.food >= unit.foodCost;
    }

    private void DeductResources(CountrySettings country, UnitScriptableObject unit)
    {
        country.money -= unit.moneyCost;
        country.food -= unit.foodCost;
        country.recruits -= unit.recrootsCost;
        country.foodNaturalIncome -= unit.foodIncomeCost;
        country.moneyNaturalIncome -= unit.moneyIncomeCost;
    }

    private void AddUnitHealth(UnitMovement unitMovement, UnitScriptableObject unit)
    {
        var newUnitHealth = new UnitHealth
        {
            unit = unit,
            health = unit.health,
            _id = unitMovement.unitsHealth.Count > 0 ? unitMovement.unitsHealth[^1]._id + 1 : 0
        };

        unitMovement.unitsHealth.Add(newUnitHealth);
    }

    private void DisplayResourceWarning(CountrySettings country, UnitScriptableObject unit)
    {
        if (country.money < unit.moneyCost)
        {
            WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.NoMoney"));
        }
        else if (country.recruits < unit.recrootsCost)
        {
            WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.NoRecruits"));
        }
    }

    public void AddUnitToArmyNoUI(UnitScriptableObject unit)
    {
        ProcessUnitAddition(unit);
    }

    public void UpdateDivisionUI()
    {
        if (ReferencesManager.Instance.regionManager.currentRegionManager != null)
        {
            ReferencesManager.Instance.regionManager.CheckRegionUnits(ReferencesManager.Instance.regionManager.currentRegionManager);
        }

        ReferencesManager.Instance.regionUI.UpdateUnitsUI(true);
    }

    public void RemoveUnitFromDivision(UnitHealth batalion, UnitMovement division, bool checkDivisionOnDestroy)
    {
        division.unitsHealth.Remove(batalion);

        var country = division.currentCountry;
        country.recruits += Mathf.CeilToInt(batalion.unit.recrootsCost * 0.7f);
        country.moneyNaturalIncome += batalion.unit.moneyIncomeCost;
        country.foodNaturalIncome += batalion.unit.foodIncomeCost;

        ReferencesManager.Instance.regionUI.UpdateUnitsUI(false);
        ReferencesManager.Instance.countryManager.UpdateValuesUI();
        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
    }

    public void CreateUnit_NoCheck(RegionManager region)
    {
        InstantiateUnit(unitPrefab, region.transform);
    }

    public void AddUnitToArmy_NoCheck(UnitScriptableObject unit, RegionManager province)
    {
        unitTransform = GetUnitTransform(province.transform);

        if (unitTransform != null)
        {
            unitMovement = unitTransform.GetComponent<UnitMovement>();

            if (unitMovement.unitsHealth.Count < maxUnits)
            {
                AddUnitHealth(unitMovement, unit);
                UpdateAllUIs(false);
            }
        }
    }

    public void AddUnitToArmy_Save(UnitScriptableObject unit, UnitMovement division)
    {
        if (division != null)
        {
            if (division.unitsHealth.Count < maxUnits)
            {
                AddUnitHealth(division, unit);
            }
        }
    }

    public void ResetUI()
    {
        var childCount = armyHorizontalGroup.transform.childCount;
        for (int i = 1; i < childCount; i++)
        {
            Destroy(armyHorizontalGroup.transform.GetChild(i).gameObject);
        }
    }

    private void UpdateAllUIs(bool showWarning)
    {
        ReferencesManager.Instance.countryManager.UpdateValuesUI();
        UpdateDivisionUI();
        ReferencesManager.Instance.regionManager.UpdateRegionUI();
    }

    public void ReCreateArmyUI()
    {
        for (int i = 1; i < armyHorizontalGroup.transform.childCount; i++)
        {
            Destroy(armyHorizontalGroup.transform.GetChild(i).gameObject);
        }

        var region = ReferencesManager.Instance.regionManager.currentRegionManager;
        for (int i = 0; i < region.transform.childCount; i++)
        {
            if (region.transform.GetChild(i).name == "Unit(Clone)")
            {
                CreateArmyUI(region.transform.GetChild(i).GetComponent<UnitMovement>());
            }
        }
    }

    private void CreateArmyUI(UnitMovement movement)
    {
        for (int i = 0; i < movement.unitsHealth.Count; i++)
        {
            GameObject button = Instantiate(unitUIPrefab, armyHorizontalGroup.transform);
            var unitUI = button.GetComponent<UnitUI>();
            unitUI.SetUnit(movement.unitsHealth[i].unit, movement, movement.unitsHealth[i]);

            var unitButton = button.GetComponent<Button>();
        }
    }

    public void CheckUnitTech()
    {
        UnitShopCheck[] unitShopChecks = FindObjectsOfType<UnitShopCheck>();

        for (int i = 0; i < unitShopChecks.Length; i++)
        {
            unitShopChecks[i].GetComponent<Button>().interactable = false;
            if (unitShopChecks[i].currentUnit == ReferencesManager.Instance.gameSettings.soldierLVL1)
            {
                unitShopChecks[i].GetComponent<Button>().interactable = true;
            }
        }

        if (ReferencesManager.Instance.countryManager.currentCountry.countryTechnologies.Count > 0)
        {
            for (int i = 0; i < unitShopChecks.Length; i++)
            {
                foreach (TechnologyScriptableObject technology in ReferencesManager.Instance.countryManager.currentCountry.countryTechnologies)
                {
                    if (unitShopChecks[i].currentUnit != ReferencesManager.Instance.gameSettings.soldierLVL1)
                    {
                        if (ReferencesManager.Instance.technologyManager.HasUnitTech(ReferencesManager.Instance.countryManager.currentCountry, unitShopChecks[i].currentUnit))
                        {
                            unitShopChecks[i].GetComponent<Button>().interactable = true;
                        }
                    }
                }
            }
        }
    }

    private void UpdateHealDivisionUI(int countOfBatalions, float recruits, float money, float food)
    {
        recruits = Mathf.FloorToInt(recruits);
        money = Mathf.FloorToInt(money);
        food = Mathf.FloorToInt(food);

        _healHP_description.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("RegionUI.Division.HealText")} {recruits} {ReferencesManager.Instance.languageManager.GetTranslation("RegionUI.RecruitsText")}, {money} {ReferencesManager.Instance.languageManager.GetTranslation("RegionUI.GoldText")}, {food} {ReferencesManager.Instance.languageManager.GetTranslation("RegionUI.FoodText")}";
    }

    public void CountResourcesToHeal()
    {
        recruits = 0;
        food = 0;
        money = 0;
        countOfBatalions = 0;

        UnitMovement division = null;

        if (ReferencesManager.Instance.regionManager.currentRegionManager != null)
        {
            division = ReferencesManager.Instance.regionManager.GetDivision(ReferencesManager.Instance.regionManager.currentRegionManager);
        }
        else if (ReferencesManager.Instance.seaRegionManager._currentSeaRegion != null)
        {
            division = ReferencesManager.Instance.seaRegionManager._currentSeaRegion._division;
        }

        foreach (UnitHealth batalion in division.unitsHealth)
        {
            if (batalion.health < batalion.unit.health)
            {
                countOfBatalions++;

                recruits += batalion.unit.recrootsCost - (batalion.health * batalion.unit.recrootsCost / batalion.unit.health);
                food += batalion.unit.foodCost - (batalion.health * batalion.unit.foodCost / batalion.unit.health);
                money += batalion.unit.moneyCost - (batalion.health * batalion.unit.moneyCost / batalion.unit.health);
            }
        }

        UpdateHealDivisionUI(countOfBatalions, recruits, money, food);
    }

    public void HealDivision()
    {
        if (ReferencesManager.Instance.countryManager.currentCountry.recruits >= recruits &&
            ReferencesManager.Instance.countryManager.currentCountry.food >= food &&
            ReferencesManager.Instance.countryManager.currentCountry.money >= money)
        {
            ReferencesManager.Instance.countryManager.currentCountry.recruits -= Mathf.FloorToInt(recruits);
            ReferencesManager.Instance.countryManager.currentCountry.food -= Mathf.FloorToInt(food);
            ReferencesManager.Instance.countryManager.currentCountry.money -= Mathf.FloorToInt(money);

            UnitMovement division = null;

            if (ReferencesManager.Instance.regionManager.currentRegionManager != null)
            {
                division = ReferencesManager.Instance.regionManager.currentRegionManager.GetDivision(ReferencesManager.Instance.regionManager.currentRegionManager);
            }
            else if (ReferencesManager.Instance.seaRegionManager._currentSeaRegion != null)
            {
                division = ReferencesManager.Instance.seaRegionManager._currentSeaRegion._division;
            }

            foreach (UnitHealth batalion in division.unitsHealth)
            {
                if (batalion.health < batalion.unit.health)
                {
                    batalion.health = batalion.unit.health;
                }
            }
        }
        else
        {
            WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.NotEnoughtResources"));
        }

        UpdateDivisionUI();
        ReferencesManager.Instance.countryManager.UpdateValuesUI();
        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
    }

    public void DisbandDivision()
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.DisbandDivision(
                ReferencesManager.Instance.regionManager.currentRegionManager._id,
                ReferencesManager.Instance.countryManager.currentCountry.country._id);
        }
        else
        {
            UnitMovement division = null;

            if (ReferencesManager.Instance.regionManager.currentRegionManager != null)
            {
                division = ReferencesManager.Instance.regionManager.GetDivision(ReferencesManager.Instance.regionManager.currentRegionManager);
            }
            else if (ReferencesManager.Instance.seaRegionManager._currentSeaRegion != null)
            {
                division = ReferencesManager.Instance.seaRegionManager._currentSeaRegion._division;
            }

            for (int i = 0; i < division.unitsHealth.Count; i++)
            {
                ReferencesManager.Instance.countryManager.currentCountry.recruits += Mathf.CeilToInt(division.unitsHealth[i].unit.recrootsCost * 0.7f);
                ReferencesManager.Instance.countryManager.currentCountry.moneyNaturalIncome += division.unitsHealth[i].unit.moneyIncomeCost;
                ReferencesManager.Instance.countryManager.currentCountry.foodNaturalIncome += division.unitsHealth[i].unit.foodIncomeCost;
            }
            division.unitsHealth.Clear();

            if (ReferencesManager.Instance.regionManager.currentRegionManager != null)
            {
                ReferencesManager.Instance.regionManager.CheckRegionUnits(ReferencesManager.Instance.regionManager.currentRegionManager);
            }

            Destroy(division.gameObject);
        }

        ReferencesManager.Instance.regionUI.CloseAllUI();

        ReferencesManager.Instance.countryManager.UpdateValuesUI();
    }

    [System.Serializable]
    public class Template
    {
        public string _name;
        public Sprite _icon;

        public List<UnitScriptableObject> _batalions = new List<UnitScriptableObject>();
    }
}
