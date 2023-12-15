using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Army : MonoBehaviour
{
    public GameObject unitPrefab;

    public GameObject unitUIPrefab;
    public GameObject unitUnclikableUIPrefab;
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

    public TMP_Text[] attackerArmyLosses; // 0 - Infantry; 1 - Artilery; 2 - Heavy
    public TMP_Text[] attackerEconomy; // 0 - MoneyIncome; 1 - FoodIncome; 2 - Population; 3 - CivFactory; 4 - Farm

    public TMP_Text[] defenderArmyLosses; // 0 - Infantry; 1 - Artilery; 2 - Heavy
    public TMP_Text[] defenderEconomy; // 0 - MoneyIncome; 1 - FoodIncome; 2 - Population; 3 - CivFactory; 4 - Farm

    public TMP_Text[] defenderBonus; // 0 - attack buff; 1 - defence buff;
    public TMP_Text[] attackerBonus; // 0 - attack buff; 1 - defence buff;

    [HideInInspector] public int[] attackerArmyLossesValue; // 0 - Infantry; 1 - Artilery; 2 - Heavy
    [HideInInspector] public int[] attackerEconomyValue; // 0 - MoneyIncome; 1 - FoodIncome; 2 - Population; 3 - CivFactory; 4 - Farm

    [HideInInspector] public int[] defenderArmyLossesValue; // 0 - Infantry; 1 - Artilery; 2 - Heavy
    [HideInInspector] public int[] defenderEconomyValue; // 0 - MoneyIncome; 1 - FoodIncome; 2 - Population; 3 - CivFactory; 4 - Farm

    [HideInInspector] public UnitMovement[] unitMovements;

    [HideInInspector] public List<GameObject> nearestPoints = new List<GameObject>();

    [HideInInspector] public int maxUnits = 10;

    private Transform unitTransform;
    private UnitMovement unitMovement;

    private List<UnitUI> unitUIs;

    public List<Template> templates = new List<Template>();

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
            UnitScriptableObject unit = ReferencesManager.Instance.gameSettings.soldierLVL1;

            if (ReferencesManager.Instance.countryManager.currentCountry.money >= unit.moneyCost && ReferencesManager.Instance.countryManager.currentCountry.recroots >= unit.recrootsCost)
            {
                GameObject spawnedUnit = Instantiate(unitPrefab, ReferencesManager.Instance.regionManager.currentRegionManager.transform);
                spawnedUnit.transform.localScale = new Vector3(unitPrefab.transform.localScale.x, unitPrefab.transform.localScale.y);

                ReferencesManager.Instance.regionManager.currentRegionManager.hasArmy = true;

                spawnedUnit.GetComponent<UnitMovement>().currentCountry = ReferencesManager.Instance.countryManager.currentCountry;
                spawnedUnit.GetComponent<UnitMovement>().currentProvince = ReferencesManager.Instance.regionManager.currentRegionManager;
                spawnedUnit.GetComponent<UnitMovement>().UpdateInfo();
                ReferencesManager.Instance.countryManager.currentCountry.countryUnits.Add(spawnedUnit.GetComponent<UnitMovement>());

                AddUnitToArmy(unit);

                ReferencesManager.Instance.regionManager.CheckRegionUnits(ReferencesManager.Instance.regionManager.currentRegionManager);
            }
            else
            {
                ReferencesManager.Instance.regionManager.currentRegionManager.hasArmy = false;
            }
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
            GameObject spawnedUnit = Instantiate(unitPrefab, region.transform);
            spawnedUnit.transform.localScale = new Vector3(unitPrefab.transform.localScale.x, unitPrefab.transform.localScale.y);

            region.hasArmy = true;

            spawnedUnit.GetComponent<UnitMovement>().currentCountry = region.currentCountry;
            spawnedUnit.GetComponent<UnitMovement>().currentProvince = region;
            spawnedUnit.GetComponent<UnitMovement>().UpdateInfo();
            region.currentCountry.countryUnits.Add(spawnedUnit.GetComponent<UnitMovement>());
        }

        region.CheckRegionUnits(region);
    }


    public void AddUnitToArmy(UnitScriptableObject unit)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.AddUnitToArmy(unit.unitName, ReferencesManager.Instance.regionManager.currentRegionManager._id);
        }
        else
        {
            foreach (Transform child in ReferencesManager.Instance.regionManager.currentRegionManager.transform)
            {
                if (child.name == "Unit(Clone)")
                {
                    unitTransform = child;
                }
            }

            if (unitTransform != null)
            {
                unitMovement = unitTransform.GetComponent<UnitMovement>();

                if (unitMovement.unitsHealth.Count != 10)
                {
                    if (ReferencesManager.Instance.countryManager.currentCountry.money >= unit.moneyCost && ReferencesManager.Instance.countryManager.currentCountry.recroots >= unit.recrootsCost)
                    {
                        ReferencesManager.Instance.countryManager.currentCountry.money -= unit.moneyCost;
                        ReferencesManager.Instance.countryManager.currentCountry.recroots -= unit.recrootsCost;
                        ReferencesManager.Instance.countryManager.currentCountry.foodNaturalIncome -= unit.foodIncomeCost;
                        ReferencesManager.Instance.countryManager.currentCountry.moneyNaturalIncome -= unit.moneyIncomeCost;

                        if (unitTransform != null)
                        {
                            unitMovement = unitTransform.GetComponent<UnitMovement>();
                            UnitMovement.UnitHealth newUnitHealth = new UnitMovement.UnitHealth();
                            newUnitHealth.unit = unit;
                            newUnitHealth.health = unit.health;
                            if (unitMovement.unitsHealth.Count > 0)
                            {
                                newUnitHealth._id = unitMovement.unitsHealth[unitMovement.unitsHealth.Count - 1]._id + 1;
                            }
                            else
                            {
                                newUnitHealth._id = 0;
                            }

                            unitMovement.unitsHealth.Add(newUnitHealth);
                        }
                        ReferencesManager.Instance.regionManager.CheckRegionUnits(ReferencesManager.Instance.regionManager.currentRegionManager);

                        ReferencesManager.Instance.regionUI.UpdateUnitsUI();

                    }

                    else if (ReferencesManager.Instance.countryManager.currentCountry.money < unit.moneyCost)
                    {
                        WarningManager.Instance.Warn("Нет золота.");
                    }

                    else if (ReferencesManager.Instance.countryManager.currentCountry.recroots < unit.recrootsCost)
                    {
                        WarningManager.Instance.Warn("Нет рекрутов.");
                    }

                    ReferencesManager.Instance.regionManager.currentRegionManager.UpdateRegionUI();
                    ReferencesManager.Instance.countryManager.UpdateValuesUI();
                    ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();

                }
                else
                {
                    WarningManager.Instance.Warn("В шаблоне дивизии может быть только 10 юнитов.");
                }
            }
        }
    }

    public void CreateUnit_NoCheck(RegionManager region)
    {
        GameObject spawnedUnit = Instantiate(unitPrefab, region.transform);
        spawnedUnit.transform.localScale = new Vector3(unitPrefab.transform.localScale.x, unitPrefab.transform.localScale.y);

        region.hasArmy = true;

        spawnedUnit.GetComponent<UnitMovement>().currentCountry = region.currentCountry;
        spawnedUnit.GetComponent<UnitMovement>().currentProvince = region;
        spawnedUnit.GetComponent<UnitMovement>().UpdateInfo();
        region.currentCountry.countryUnits.Add(spawnedUnit.GetComponent<UnitMovement>());
    }

    public void AddUnitToArmy_NoCheck(UnitScriptableObject unit, RegionManager region)
    {
        unitTransform = region.transform.Find("Unit(Clone)");

        if (unitTransform != null)
        {
            unitMovement = unitTransform.GetComponent<UnitMovement>();

            if (unitMovement.unitsHealth.Count != 10)
            {
                if (unitTransform != null)
                {
                    unitMovement = unitTransform.GetComponent<UnitMovement>();

                    UnitMovement.UnitHealth newUnitHealth = new UnitMovement.UnitHealth();
                    newUnitHealth.unit = unit;
                    newUnitHealth.health = unit.health;
                    if (unitMovement.unitsHealth.Count > 0)
                    {
                        newUnitHealth._id = unitMovement.unitsHealth[unitMovement.unitsHealth.Count - 1]._id + 1;
                    }
                    else
                    {
                        newUnitHealth._id = 0;
                    }

                    unitMovement.unitsHealth.Add(newUnitHealth);
                }
                region.CheckRegionUnits(region);
                if (ReferencesManager.Instance.regionUI != null)
                {
                    ReferencesManager.Instance.regionUI.UpdateUnitsUI();
                }

                ReferencesManager.Instance.countryManager.UpdateValuesUI();
                ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
            }
            else
            {
                WarningManager.Instance.Warn("В шаблоне дивизии может быть только 10 юнитов.");
            }
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
                        if (HasUnitTech(ReferencesManager.Instance.countryManager.currentCountry, unitShopChecks[i].currentUnit))
                        {
                            unitShopChecks[i].GetComponent<Button>().interactable = true;
                        }
                    }
                }
            }
        }
    }

    private bool HasTech(CountrySettings country, TechnologyScriptableObject tech)
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

    private bool HasUnitTech(CountrySettings country, UnitScriptableObject unit)
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

    public void UpdateUIs()
    {
        Transform unitTransform = ReferencesManager.Instance.regionManager.currentRegionManager.transform.Find("Unit(Clone)");
        UnitMovement unitMovement;

        unitMovement = unitTransform.GetComponent<UnitMovement>();

        unitUIs.Clear();

        foreach (Transform child in armyHorizontalGroup.transform)
        {
            if (child.GetComponent<UnitUI>())
            {
                unitUIs.Add(child.GetComponent<UnitUI>());
            }
        }
        for (int i = 0; i < unitMovement.unitsHealth.Count; i++)
        {
            unitUIs[i].id = i;
        }
    }

    [System.Serializable]
    public class Template
    {
        public string _name;

        public List<UnitScriptableObject> _batalions = new List<UnitScriptableObject>();
    }
}
