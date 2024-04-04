using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Photon.Pun.Demo.PunBasics;
using System.Runtime.InteropServices.WindowsRuntime;

public class RegionManager : MonoBehaviour
{
    public int _id;
    public TerrainScriptableObject regionTerrain;

    [HideInInspector] public int regionLevel;
    [HideInInspector] public int armyLevel;
    [HideInInspector] public int defenseLevel;
    
    [HideInInspector] public int regionMaxLevel;
    [HideInInspector] public int armyMaxLevel;
    [HideInInspector] public int defenseMaxLevel;

    public int population;
    [HideInInspector] public int populationGrowRate;

    [HideInInspector] public float happines;
    [HideInInspector] public float happinesGrowRate;

    [HideInInspector] public RegionManager currentRegionManager;

    [Header("Region Owner/Claims")]
    public List<CountryScriptableObject> regionClaims = new List<CountryScriptableObject>();
    public CountrySettings currentCountry;

    [Space(10f)]
    [Header("RegionArmy")]
    //[HideInInspector] public List<UnitScriptableObject> regionUnits = new List<UnitScriptableObject>();
    [HideInInspector] public List<UnitMovement.UnitHealth> currentDefenseUnits = new List<UnitMovement.UnitHealth>();
    public List<Transform> movePoints = new List<Transform>();
    [HideInInspector] public bool moveMode = false;
    public bool hasArmy;

    [Header("RegionEconomy")]
    [HideInInspector] public int goldIncome;
    [HideInInspector] public int foodIncome;
    [SerializeField] int steelAmount;
    [SerializeField] int alluminiumAmount;
    [SerializeField] int rubberAmount;
    public int OilAmount;

    [HideInInspector] public int civFactory_Amount;
    [HideInInspector] public int milFactory_Amount;
    [HideInInspector] public int infrastructure_Amount;
    [HideInInspector] public int farms_Amount;
    [HideInInspector] public int cheFarms;
    [HideInInspector] public int researchLabs;
    public int fortifications_Amount;

    public int regionScore;

    [HideInInspector] public int[] regionCostsPerLevel;
    [HideInInspector] public int[] armyCostsPerLevel;
    [HideInInspector] public int[] defenseCostsPerLevel;

    [Header("Building")]
    public List<BuildingScriptableObject> buildings = new List<BuildingScriptableObject>();
    [HideInInspector] public List<BuildingQueueItem> buildingsQueue = new List<BuildingQueueItem>();

    [HideInInspector] public Color hoverColor;
    [HideInInspector] public Color selectedColor;
    [HideInInspector] public Color defaultColor;

    public bool capital = false;
    public bool demilitarized = false;

    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool canSelect = true;

    [Header("Fleet settings")]
    public int _marineBaseLevel = 0;
    public bool _hasFleet = false;

    public bool _isCoast;

    [HideInInspector] public Fleet _currentFleet;

    private Vector3 StartPos;
    private Vector3 PosAfter;


    private void OnMouseDown()
    {
        StartPos = ReferencesManager.Instance.mainCamera.GetComponent<Camera>().WorldToViewportPoint(Input.mousePosition);
    }

    private void OnMouseEnter()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (ReferencesManager.Instance.mapType != null)
            {
                if (!ReferencesManager.Instance.mapType.viewMap)
                {
                    this.GetComponent<SpriteRenderer>().color = hoverColor;
                }
            }
        }
    }

    private void OnMouseExit()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (ReferencesManager.Instance.mapType != null)
            {
                if (!ReferencesManager.Instance.mapType.viewMap)
                {
                    if (currentRegionManager != null && currentRegionManager == this.GetComponent<RegionManager>())
                    {
                        this.GetComponent<SpriteRenderer>().color = currentRegionManager.selectedColor;
                    }
                    else if (currentRegionManager == null)
                    {
                        this.GetComponent<SpriteRenderer>().color = this.GetComponent<RegionManager>().currentCountry.country.countryColor;
                    }
                    else if (currentRegionManager != null && currentRegionManager != this.GetComponent<RegionManager>())
                    {
                        this.GetComponent<SpriteRenderer>().color = this.GetComponent<RegionManager>().currentCountry.country.countryColor;
                    }
                }
            }
        }
    }

    private void OnMouseUpAsButton()
    {
        PosAfter = ReferencesManager.Instance.mainCamera.GetComponent<Camera>().WorldToViewportPoint(Input.mousePosition);

        float difference_x = (PosAfter.x - StartPos.x);
        float difference_y = (PosAfter.y - StartPos.y);

        int cameraClickOffset = ReferencesManager.Instance.mainCamera.cameraClickOffset;

        // ���� ������� �� ���������� �� 32, �� ��������� ����
        if (difference_x > -cameraClickOffset && difference_x < cameraClickOffset && difference_y > -cameraClickOffset && difference_y < cameraClickOffset)
        {
            // *���������� ����
            if (canSelect && !ReferencesManager.Instance.mainCamera.IsMouseOverUI)
            {
                if (ReferencesManager.Instance.mainCamera.Map_MoveTouch_IsAllowed)
                {
                    if (ReferencesManager.Instance.mEditor != null)
                    {
                        if (ReferencesManager.Instance.mEditor.paintMapMode)
                        {
                            PaintRegion();
                        }
                    }
                    else
                    {
                        if (!ReferencesManager.Instance.gameSettings.regionSelectionMode)
                        {
                            SelectRegion();
                        }
                    }
                    if (ReferencesManager.Instance.gameSettings.regionSelectionMode)
                    {
                        D_SelectRegion(ReferencesManager.Instance.gameSettings.provincesList, ReferencesManager.Instance.gameSettings.regionSelectionModeType); //my_country other_country;id
                    }
                }
            }
        }
    }


    public void AddClaims()
    {
        ReferencesManager.Instance.regionUI.ResetClaims();

        bool canClaim = false;

        foreach (Transform movePoint in currentRegionManager.movePoints)
        {
            if (movePoint.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>().currentCountry ==
                ReferencesManager.Instance.countryManager.currentCountry)
            {
                canClaim = true;
            }
        }

        if (canClaim)
        {
            if (!currentRegionManager.regionClaims.Contains(ReferencesManager.Instance.countryManager.currentCountry.country))
            {
                currentRegionManager.regionClaims.Add(ReferencesManager.Instance.countryManager.currentCountry.country);
            }
        }
        else
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                WarningManager.Instance.Warn("You don't border this province");
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                WarningManager.Instance.Warn("Вы не граничите с этой провинцией");
            }
        }

        for (int i = 0; i < currentRegionManager.regionClaims.Count; i++)
        {
            if (currentRegionManager.currentCountry.country._id != currentRegionManager.regionClaims[i]._id)
            {
                GameObject spawnedCountryFlag = Instantiate(ReferencesManager.Instance.regionUI.countryFlagPrefab, ReferencesManager.Instance.regionUI.regionClaimsContainer.transform);
                spawnedCountryFlag.GetComponent<FillCountryFlag>().country = currentRegionManager.regionClaims[i];
                spawnedCountryFlag.GetComponent<FillCountryFlag>().FillInfo();

                ReferencesManager.Instance.regionUI.regionClaimsContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(ReferencesManager.Instance.regionUI.countryFlagPrefab.GetComponent<RectTransform>().sizeDelta.x * ReferencesManager.Instance.regionUI.regionClaimsContainer.transform.childCount, ReferencesManager.Instance.regionUI.countryFlagPrefab.GetComponent<RectTransform>().sizeDelta.y);
            }
        }

        ReferencesManager.Instance.regionUI.UpdateBuildingUI();
        ReferencesManager.Instance.regionUI.UpdateBuildingUI();

        UpdateRegionUI();
    }

    public void Upgrade(int upgradeType)
    {
        int labelRegionLevel = currentRegionManager.regionLevel - 1;
        int labelArmyLevel = currentRegionManager.armyLevel - 1;
        int labelDefenseLevel = currentRegionManager.defenseLevel - 1;

        if (upgradeType == 0) // region
        {
            if (ReferencesManager.Instance.countryManager.currentCountry.money >= currentRegionManager.regionCostsPerLevel[labelRegionLevel])
            {
                if (currentRegionManager.regionLevel != regionMaxLevel)
                {
                    ReferencesManager.Instance.countryManager.currentCountry.money -= currentRegionManager.regionCostsPerLevel[labelRegionLevel];
                    currentRegionManager.regionLevel++;
                }
            }
        }
        else if (upgradeType == 1) // Army
        {
            if (ReferencesManager.Instance.countryManager.currentCountry.money >= currentRegionManager.armyCostsPerLevel[labelArmyLevel])
            {
                if (currentRegionManager.armyLevel != armyMaxLevel)
                {
                    ReferencesManager.Instance.countryManager.currentCountry.money -= currentRegionManager.armyCostsPerLevel[labelArmyLevel];
                    currentRegionManager.armyLevel++;
                }
            }
        }
        else if (upgradeType == 2) // Defense
        {
            if (ReferencesManager.Instance.countryManager.currentCountry.money >= currentRegionManager.defenseCostsPerLevel[labelDefenseLevel])
            {
                if (currentRegionManager.defenseLevel != defenseMaxLevel)
                {
                    ReferencesManager.Instance.countryManager.currentCountry.money -= currentRegionManager.defenseCostsPerLevel[labelDefenseLevel];
                    currentRegionManager.defenseLevel++;
                    if (currentRegionManager.defenseLevel == 2) currentRegionManager.currentDefenseUnits = ReferencesManager.Instance.gameSettings.currentDefenseUnits_SecondLevel;
                    else if (currentRegionManager.defenseLevel == 3) currentRegionManager.currentDefenseUnits = ReferencesManager.Instance.gameSettings.currentDefenseUnits_ThirdLevel;
                }
            }
        }
        ReferencesManager.Instance.countryManager.UpdateValuesUI();
        UpdateRegionUI();
    }

    public void UpdateRegionUI()
    {
        int labelRegionLevel = currentRegionManager.regionLevel - 1;
        int labelArmyLevel = currentRegionManager.armyLevel - 1;
        int labelDefenseLevel = currentRegionManager.defenseLevel - 1;

        ReferencesManager.Instance.regionUI.RegionInfrastructureAmount.text = $"{currentRegionManager.infrastructure_Amount}/10";

        if (currentRegionManager.regionLevel == regionMaxLevel)
        {
            ReferencesManager.Instance.regionUI.regionButtonUpgrade.interactable = false;
            ReferencesManager.Instance.regionUI.regionCost.text = "Макс.";
        }
        else
        {
            ReferencesManager.Instance.regionUI.regionButtonUpgrade.interactable = true;
            ReferencesManager.Instance.regionUI.regionCost.text = currentRegionManager.regionCostsPerLevel[labelRegionLevel].ToString();
        }

        if (currentRegionManager.armyLevel == armyMaxLevel)
        {
            ReferencesManager.Instance.regionUI.armyButtonUpgrade.interactable = false;
            ReferencesManager.Instance.regionUI.armyCost.text = "Макс.";
        }
        else
        {
            ReferencesManager.Instance.regionUI.armyButtonUpgrade.interactable = true;
            ReferencesManager.Instance.regionUI.armyCost.text = currentRegionManager.armyCostsPerLevel[labelArmyLevel].ToString();
        }

        if (currentRegionManager.defenseLevel == defenseMaxLevel)
        {
            ReferencesManager.Instance.regionUI.defenseButtonUpgrade.interactable = false;
            ReferencesManager.Instance.regionUI.defenseCost.text = "Макс.";
        }
        else
        {
            ReferencesManager.Instance.regionUI.defenseButtonUpgrade.interactable = true;
            ReferencesManager.Instance.regionUI.defenseCost.text = currentRegionManager.defenseCostsPerLevel[labelDefenseLevel].ToString();
        }

        if (currentRegionManager.fortifications_Amount >= 15)
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                ReferencesManager.Instance.regionUI._upgradeFortificationButton_Text.text = "Max. 15";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                ReferencesManager.Instance.regionUI._upgradeFortificationButton_Text.text = "Макс. 15";
            }

            ReferencesManager.Instance.regionUI._upgradeFortificationButton.interactable = false;
        }
        else
        {
            ReferencesManager.Instance.regionUI._upgradeFortificationButton_Text.text = $"{currentRegionManager.fortifications_Amount}/15";

            if (currentRegionManager.fortifications_Amount == 0)
            {
                ReferencesManager.Instance.regionUI._upgradeFortificationCost_Text.text = $"{400}";
            }
            else
            {
                ReferencesManager.Instance.regionUI._upgradeFortificationCost_Text.text = $"{400 * currentRegionManager.fortifications_Amount}";
            }

            ReferencesManager.Instance.regionUI._upgradeFortificationButton.interactable = true;
        }

        ReferencesManager.Instance.regionUI.regionPopulationText.text = currentRegionManager.population.ToString();
        ReferencesManager.Instance.regionUI.regionEconomyText.text = currentRegionManager.populationGrowRate.ToString();
        ReferencesManager.Instance.regionUI.civFactoryAmount.text = currentRegionManager.civFactory_Amount.ToString();
        ReferencesManager.Instance.regionUI.milFactoryAmount.text = currentRegionManager.milFactory_Amount.ToString();
        ReferencesManager.Instance.regionUI.farmsAmount.text = (currentRegionManager.farms_Amount + currentRegionManager.cheFarms).ToString();
        ReferencesManager.Instance.regionUI.infrastructureAmount.text = currentRegionManager.infrastructure_Amount.ToString();
        ReferencesManager.Instance.regionUI.fortificationsAmount.text = currentRegionManager.fortifications_Amount.ToString();
        ReferencesManager.Instance.regionUI.steelAmount.text = currentRegionManager.steelAmount.ToString();
        ReferencesManager.Instance.regionUI.alluminiumAmount.text = currentRegionManager.alluminiumAmount.ToString();
        ReferencesManager.Instance.regionUI.rubberAmount.text = currentRegionManager.rubberAmount.ToString();
        ReferencesManager.Instance.regionUI.OilAmount.text = currentRegionManager.OilAmount.ToString();
    }

    public void UpdateRegionGarrison()
    {
        if (defenseLevel == 1)
        {
            currentDefenseUnits = ReferencesManager.Instance.gameSettings.currentDefenseUnits_FirstLevel;
        }
        else if (defenseLevel == 2)
        {
            currentDefenseUnits = ReferencesManager.Instance.gameSettings.currentDefenseUnits_SecondLevel;
        }
        else if (defenseLevel == 3)
        {
            currentDefenseUnits = ReferencesManager.Instance.gameSettings.currentDefenseUnits_ThirdLevel;
        }
    }

    public void UpgradeInfrastructure()
    {
        int check = currentRegionManager.infrastructure_Amount + 1;

        if (currentRegionManager.currentCountry.money >= 200)
        {
            if (check <= 10)
            {
                currentRegionManager.infrastructure_Amount++;
                currentRegionManager.currentCountry.money -= 200;
                currentRegionManager.currentCountry.moneyNaturalIncome += 8;
            }
        }

        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
        ReferencesManager.Instance.countryManager.UpdateValuesUI();
        currentRegionManager.UpdateRegionUI();

        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.SetRegionValues(_id, population, hasArmy, goldIncome, foodIncome, civFactory_Amount,
            infrastructure_Amount, farms_Amount, cheFarms, regionScore);

            Multiplayer.Instance.SetCountryValues(
                currentRegionManager.currentCountry.country._id,
                currentRegionManager.currentCountry.money,
                currentRegionManager.currentCountry.food,
                currentRegionManager.currentCountry.recroots);
        }
    }

    public void UpgradeInfrastructureForce(RegionManager region)
    {
        int check = region.infrastructure_Amount + 1;

        if (region.currentCountry.money >= 800)
        {
            if (check <= 10)
            {
                region.infrastructure_Amount++;
                region.currentCountry.money -= 800;
                region.currentCountry.moneyNaturalIncome += 8;
            }
        }

        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
        ReferencesManager.Instance.countryManager.UpdateValuesUI();

        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.SetRegionValues(_id, population, hasArmy, goldIncome, foodIncome, civFactory_Amount,
            infrastructure_Amount, farms_Amount, cheFarms, regionScore);

            Multiplayer.Instance.SetCountryValues(
                region.currentCountry.country._id,
                region.currentCountry.money,
                region.currentCountry.food,
                region.currentCountry.recroots);
        }
    }

    public void SelectRegion()
    {
        if (canSelect)
        {
            ReferencesManager.Instance.seaRegionManager.DeselectRegions();

            Vector2 mainCamera = ReferencesManager.Instance.mainCamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mainCamera, Input.mousePosition);

            if (hit.collider)
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0);

                        if (touch.phase == TouchPhase.Began)
                        {
                            var touchPostition = touch.position;

                            bool isOverUI = touchPostition.IsPointerOverGameObject();

                            if (isOverUI)
                            {
                                return;
                            }
                        }
                    }
                }

                foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
                {
                    region.isSelected = false;
                    region.GetComponent<SpriteRenderer>().color = region.currentCountry.country.countryColor;
                    region.currentRegionManager = hit.collider.GetComponent<RegionManager>();
                }
                ReferencesManager.Instance.regionUI.CloseTabs();

                ReferencesManager.Instance.regionUI.regionInfoCountryFlag.sprite = currentRegionManager.currentCountry.country.countryFlag;
                currentRegionManager.GetComponent<SpriteRenderer>().color = currentRegionManager.selectedColor;

                if (UISoundEffect.Instance != null)
                {
                    UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.regionUI.click_01);
                }

                currentRegionManager.isSelected = true;
                ReferencesManager.Instance.regionUI.regionBarContainer.SetActive(true);
                ReferencesManager.Instance.regionUI.regionUIContainer.SetActive(true);


                if (currentRegionManager.currentCountry != ReferencesManager.Instance.countryManager.currentCountry)
                {
                    ReferencesManager.Instance.regionUI.armyButton.interactable = false;
                    ReferencesManager.Instance.regionUI.buildButton.interactable = false;
                    ReferencesManager.Instance.regionUI.defenseButton.interactable = false;
                    ReferencesManager.Instance.regionUI.regionUpgradeButton.interactable = true;

                    ReferencesManager.Instance.regionUI.regionButtonUpgrade.interactable = true;
                    ReferencesManager.Instance.regionUI.armyButtonUpgrade.interactable = false;
                    ReferencesManager.Instance.regionUI.defenseButtonUpgrade.interactable = false;
                    ReferencesManager.Instance.regionUI.moveButton.interactable = false;
                }
                else
                {
                    ReferencesManager.Instance.regionUI.armyButton.interactable = false;
                    ReferencesManager.Instance.regionUI.buildButton.interactable = true;
                    ReferencesManager.Instance.regionUI.defenseButton.interactable = !demilitarized;
                    ReferencesManager.Instance.regionUI.regionUpgradeButton.interactable = true;

                    ReferencesManager.Instance.regionUI.regionButtonUpgrade.interactable = true;
                    ReferencesManager.Instance.regionUI.armyButtonUpgrade.interactable = true;
                    ReferencesManager.Instance.regionUI.defenseButtonUpgrade.interactable = true;
                    ReferencesManager.Instance.regionUI.moveButton.interactable = true;
                }

                if (currentRegionManager.hasArmy && !currentRegionManager.demilitarized)
                {
                    try
                    {
                        Transform unitTransform = currentRegionManager.transform.Find("Unit(Clone)");

                        if (unitTransform.gameObject != null && unitTransform.gameObject.GetComponent<UnitMovement>() != null)
                        {
                            if (unitTransform.gameObject.GetComponent<UnitMovement>().currentCountry ==
                                ReferencesManager.Instance.countryManager.currentCountry)
                            {
                                ReferencesManager.Instance.regionUI.armyButton.interactable = !demilitarized;
                                ReferencesManager.Instance.regionUI.defenseButton.interactable = true;
                                ReferencesManager.Instance.regionUI.moveButton.interactable = true;
                            }
                        }
                    }
                    catch (System.NullReferenceException)
                    {
                        currentRegionManager.hasArmy = false;
                    }
                }

                ReferencesManager.Instance.regionUI.DeMoveUnitMode(false);

                ReferencesManager.Instance.regionUI.ResetClaims();

                for (int i = 0; i < regionClaims.Count; i++)
                {
                    if (currentRegionManager.currentCountry.country._id != regionClaims[i]._id)
                    {
                        GameObject spawnedCountryFlag = Instantiate(ReferencesManager.Instance.regionUI.countryFlagPrefab, ReferencesManager.Instance.regionUI.regionClaimsContainer.transform);
                        spawnedCountryFlag.GetComponent<FillCountryFlag>().country = regionClaims[i];
                        spawnedCountryFlag.GetComponent<FillCountryFlag>().FillInfo();

                        ReferencesManager.Instance.regionUI.regionClaimsContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(ReferencesManager.Instance.regionUI.countryFlagPrefab.GetComponent<RectTransform>().sizeDelta.x * ReferencesManager.Instance.regionUI.regionClaimsContainer.transform.childCount, ReferencesManager.Instance.regionUI.countryFlagPrefab.GetComponent<RectTransform>().sizeDelta.y);
                    }
                }

                ReferencesManager.Instance.regionUI.UpdateBuildingUI();
                ReferencesManager.Instance.regionUI.UpdateBuildingUI();

                UpdateRegionUI();
                CheckRegionUnits(currentRegionManager);
            }
        }
    }

    public void PaintRegion()
    {
        if (canSelect)
        {
            Vector2 mainCamera = ReferencesManager.Instance.mainCamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mainCamera, Input.mousePosition);

            if (hit.collider)
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0);

                        if (touch.phase == TouchPhase.Began)
                        {
                            var touchPostition = touch.position;

                            bool isOverUI = touchPostition.IsPointerOverGameObject();

                            if (isOverUI)
                            {
                                return;
                            }
                        }
                    }
                }

                //currentRegionManager = hit.collider.gameObject.GetComponent<RegionManager>();
                hit.collider.gameObject.GetComponent<RegionManager>().currentCountry.myRegions.Remove(hit.collider.gameObject.GetComponent<RegionManager>());

                hit.collider.gameObject.GetComponent<RegionManager>().currentCountry = ReferencesManager.Instance.mEditor.selectedCountry;
                hit.collider.gameObject.GetComponent<RegionManager>().currentCountry.myRegions.Add(hit.collider.gameObject.GetComponent<RegionManager>());
                hit.collider.transform.SetParent(hit.collider.gameObject.GetComponent<RegionManager>().currentCountry.transform);

                UpdateRegionsGraphics();

                UpdateRegions();
            }
        }
    }

    public void D_SelectRegion(List<RegionManager> list, string rules)
    {
        bool onlyMyRegions = false;
        bool onlyOtherRegions = false;
        string country_id = "";

        CountrySettings otherCountry;

        if (rules == "my_country")
        {
            onlyMyRegions = true;
        }
        else if (rules.Split(';')[0] == "other_country")
        {
            onlyOtherRegions = true;
            country_id = rules.Split(';')[1];
        }


        if (canSelect)
        {
            Vector2 mainCamera = ReferencesManager.Instance.mainCamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mainCamera, Input.mousePosition);

            if (hit.collider)
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0);

                        if (touch.phase == TouchPhase.Began)
                        {
                            var touchPostition = touch.position;

                            bool isOverUI = touchPostition.IsPointerOverGameObject();

                            if (isOverUI)
                            {
                                return;
                            }
                        }
                    }
                }

                if (onlyMyRegions)
                {
                    if (hit.collider.GetComponent<RegionManager>().currentCountry.country._id != ReferencesManager.Instance.countryManager.currentCountry.country._id)
                    {
                        return;
                    }
                }
                else if (onlyOtherRegions)
                {
                    if (hit.collider.GetComponent<RegionManager>().currentCountry.country._id != int.Parse(country_id))
                    {
                        return;
                    }
                }

                if (!list.Contains(hit.collider.GetComponent<RegionManager>()))
                {
                    list.Add(hit.collider.GetComponent<RegionManager>());

                    foreach (RegionManager province in list)
                    {
                        province.GetComponent<SpriteRenderer>().color = ReferencesManager.Instance.gameSettings.greenColor;
                    }
                }
                else
                {
                    list.Remove(hit.collider.GetComponent<RegionManager>());
                    hit.collider.GetComponent<SpriteRenderer>().color = hit.collider.GetComponent<RegionManager>().currentCountry.country.countryColor;

                    foreach (RegionManager province in list)
                    {
                        province.GetComponent<SpriteRenderer>().color = ReferencesManager.Instance.gameSettings.greenColor;
                    }
                }
            }
        }
    }

    public void SelectRegionNoHit(RegionManager selectedRegion)
    {
        try
        {
            ReferencesManager.Instance.seaRegionManager.DeselectRegions();
        }
        catch (System.Exception) { }

        ReferencesManager.Instance.regionUI.regionButtonUpgrade.interactable = true;
        ReferencesManager.Instance.regionUI.armyButtonUpgrade.interactable = true;
        ReferencesManager.Instance.regionUI.defenseButtonUpgrade.interactable = true;

        foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
        {
            region.isSelected = false;
            region.GetComponent<SpriteRenderer>().color = region.currentCountry.country.countryColor;
            region.currentRegionManager = selectedRegion;
        }

        ReferencesManager.Instance.regionUI.regionInfoCountryFlag.sprite = currentRegionManager.currentCountry.country.countryFlag;
        selectedRegion.GetComponent<SpriteRenderer>().color = currentRegionManager.selectedColor;
        selectedRegion.isSelected = true;
        ReferencesManager.Instance.regionUI.regionBarContainer.SetActive(true);

        if (currentRegionManager.currentCountry != ReferencesManager.Instance.countryManager.currentCountry)
        {
            ReferencesManager.Instance.regionUI.armyButton.interactable = false;
            ReferencesManager.Instance.regionUI.buildButton.interactable = false;
            ReferencesManager.Instance.regionUI.defenseButton.interactable = false;
            ReferencesManager.Instance.regionUI.regionUpgradeButton.interactable = false;
        }
        else
        {
            ReferencesManager.Instance.regionUI.armyButton.interactable = !demilitarized;
            ReferencesManager.Instance.regionUI.buildButton.interactable = true;
            ReferencesManager.Instance.regionUI.defenseButton.interactable = true;
            ReferencesManager.Instance.regionUI.regionUpgradeButton.interactable = true;
        }

        if (currentRegionManager.currentCountry != ReferencesManager.Instance.countryManager.currentCountry)
        {
            if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(currentRegionManager.currentCountry, ReferencesManager.Instance.countryManager.currentCountry).right)
            {
                ReferencesManager.Instance.regionUI.moveButton.interactable = true;
            }
            else
            {
                ReferencesManager.Instance.regionUI.moveButton.interactable = false;
            }
        }
        else
        {
            ReferencesManager.Instance.regionUI.moveButton.interactable = true;
        }

        ReferencesManager.Instance.regionUI.UpdateUnitsUI(true);
        UpdateRegionUI();
        ReferencesManager.Instance.regionUI.UpdateBuildingUI();
        CheckRegionUnits(currentRegionManager);
    }


    public void DestroyRegionDivision(RegionManager region)
    {
        if (region.hasArmy)
        {
            Transform unitMovement = region.transform.Find("Unit(Clone)");
            if (unitMovement != null)
            {
                Destroy(unitMovement.gameObject);
            }
            else
            {
                CheckRegionUnits(region);
            }
        }
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.SetRegionValues(_id, population, hasArmy, goldIncome, foodIncome, civFactory_Amount,
            infrastructure_Amount, farms_Amount, cheFarms, regionScore);
        }
    }


    public void CheckRegionUnits(RegionManager region)
    {
        if (region != null)
        {
            Transform unitTransform = region.transform.Find("Unit(Clone)");

            UnitMovement unitMovement;

            if (unitTransform != null)
            {
                unitMovement = unitTransform.GetComponent<UnitMovement>();

                if (unitMovement != null && unitMovement.unitsHealth.Count > 0)
                {
                    region.hasArmy = true;
                }
                else if (unitMovement != null && unitMovement.unitsHealth.Count <= 0)
                {
                    region.hasArmy = false;
                    unitTransform.GetComponent<UnitMovement>().currentCountry.countryUnits.Remove(unitTransform.GetComponent<UnitMovement>());

                    Destroy(unitTransform.gameObject);
                }
            }
            else
            {
                region.hasArmy = false;
            }

            if (unitTransform != null)
            {
                if (unitTransform.GetComponent<UnitMovement>())
                {
                    unitMovement = unitTransform.GetComponent<UnitMovement>();
                    if (unitMovement.unitsHealth.Count <= 0)
                    {
                        region.hasArmy = false;
                        unitTransform.GetComponent<UnitMovement>().currentCountry.countryUnits.Remove(unitTransform.GetComponent<UnitMovement>());
                        Destroy(unitTransform.gameObject);
                    }
                }
                else
                {
                    Destroy(unitTransform.gameObject);
                }
            }
        }

        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.SetRegionValues(_id, population, hasArmy, goldIncome, foodIncome, civFactory_Amount,
                infrastructure_Amount, farms_Amount, cheFarms, regionScore);
        }
    }

    public void DeselectRegions()
    {
        foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
        {
            region.isSelected = false;
            region.GetComponent<SpriteRenderer>().color = region.currentCountry.country.countryColor;
            region.currentRegionManager = null;
        }
        ReferencesManager.Instance.regionUI.createArmyTab.SetActive(false);

        Animator regionUIContainerAnimator = ReferencesManager.Instance.regionUI.regionUIContainer.GetComponent<Animator>();
        if(regionUIContainerAnimator != null) regionUIContainerAnimator.Play("closePanel");
        ReferencesManager.Instance.regionUI.statisticShowed = false;

    }

    private void UpdateRegionsGraphics()
    {
        this.GetComponent<SpriteRenderer>().color = this.currentCountry.country.countryColor;
    }

    public void UpdateRegions()
    {
        foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
        {
            region.isSelected = false;
            region.GetComponent<SpriteRenderer>().color = region.currentCountry.country.countryColor;

            region.selectedColor.r = region.currentCountry.country.countryColor.r + 0.2f;
            region.selectedColor.g = region.currentCountry.country.countryColor.g + 0.2f;
            region.selectedColor.b = region.currentCountry.country.countryColor.b + 0.2f;
            region.selectedColor.a = 0.5f;

            region.hoverColor.r = region.currentCountry.country.countryColor.r + 0.3f;
            region.hoverColor.g = region.currentCountry.country.countryColor.g + 0.3f;
            region.hoverColor.b = region.currentCountry.country.countryColor.b + 0.3f;
            region.hoverColor.a = 0.5f;
        }
    }

    public void AddBuildingToQueue(BuildingScriptableObject building)
    {
        int buildSpeed = 1;

        if (currentRegionManager.infrastructure_Amount < 4) buildSpeed = 1;
        else if (currentRegionManager.infrastructure_Amount == 4) buildSpeed = 2;
        else if (currentRegionManager.infrastructure_Amount == 6) buildSpeed = 3;
        else if (currentRegionManager.infrastructure_Amount == 8) buildSpeed = 4;
        else if (currentRegionManager.infrastructure_Amount == 10) buildSpeed = 5;

        if (ReferencesManager.Instance.countryManager.currentCountry.money >= building.goldCost)
        {
            if (currentRegionManager.buildings.Count + currentRegionManager.buildingsQueue.Count >= 4)
            {
                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    WarningManager.Instance.Warn("There are no free slots");
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    WarningManager.Instance.Warn("Нет доступных слотов");
                }
            }
            else
            {
                if (building.buildType == BuildingScriptableObject.BuildType.Dockyard)
                {
                    if (!currentRegionManager._isCoast)
                    {
                        if (PlayerPrefs.GetInt("languageId") == 0)
                        {
                            WarningManager.Instance.Warn("This province has no coast");
                        }
                        else if (PlayerPrefs.GetInt("languageId") == 1)
                        {
                            WarningManager.Instance.Warn("У провинции нет побережья");
                        }

                        return;
                    }
                }

                BuildingQueueItem buildingQueueItem = new BuildingQueueItem();
                buildingQueueItem.building = building;
                buildingQueueItem.movesLasts = building.moves;
                buildingQueueItem.region = currentRegionManager;

                ReferencesManager.Instance.countryManager.currentCountry.money -= building.goldCost;

                if ((buildingQueueItem.movesLasts - buildSpeed) <= 0)
                {
                    BuildBuilding(buildingQueueItem.building, buildingQueueItem.region, true);
                }
                else
                {
                    currentRegionManager.buildingsQueue.Add(buildingQueueItem);
                }

                Multiplayer.Instance.SetCountryValues(
                    currentRegionManager.currentCountry.country._id,
                    currentRegionManager.currentCountry.money,
                    currentRegionManager.currentCountry.food,
                    currentRegionManager.currentCountry.recroots);

            }
        }
        ReferencesManager.Instance.regionUI.UpdateBuildingUI();
        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
        ReferencesManager.Instance.countryManager.UpdateValuesUI();
    }

    public void Upgrade(string data)
    {
        string _upgrade = data.Split(';')[0];
        int goldCost = 400 * currentRegionManager.fortifications_Amount;

        if (goldCost == 0)
        {
            goldCost = 400;
        }

        if (ReferencesManager.Instance.countryManager.currentCountry.money >= goldCost)
        {
            if (currentRegionManager.fortifications_Amount + 1 <= 15)
            {
                ReferencesManager.Instance.countryManager.currentCountry.money -= goldCost;
                currentRegionManager.fortifications_Amount++;
            }
        }
        else
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                WarningManager.Instance.Warn("Not enough money");
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                WarningManager.Instance.Warn("Недостаточно золота");
            }
        }

        currentRegionManager.UpdateRegionUI();
        ReferencesManager.Instance.regionUI.UpdateBuildingUI();
        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
        ReferencesManager.Instance.countryManager.UpdateValuesUI();
    }


    public void AddBuildingToQueueForce(BuildingScriptableObject building, RegionManager region)
    {
        int buildSpeed = 1;

        if (region.infrastructure_Amount < 4) buildSpeed = 1;
        else if (region.infrastructure_Amount == 4) buildSpeed = 2;
        else if (region.infrastructure_Amount == 6) buildSpeed = 3;
        else if (region.infrastructure_Amount == 8) buildSpeed = 4;
        else if (region.infrastructure_Amount == 10) buildSpeed = 5;

        if (region.buildings.Count + region.buildingsQueue.Count < 4)
        {
            BuildingQueueItem buildingQueueItem = new BuildingQueueItem();
            buildingQueueItem.building = building;
            buildingQueueItem.movesLasts = building.moves;
            buildingQueueItem.region = region;


            region.buildingsQueue.Add(buildingQueueItem);
            region.currentCountry.money -= building.goldCost;

            if (ReferencesManager.Instance.gameSettings.onlineGame)
            {
                Multiplayer.Instance.SetCountryValues(
                    buildingQueueItem.region.currentCountry.country._id,
                    buildingQueueItem.region.currentCountry.money,
                    buildingQueueItem.region.currentCountry.food,
                    buildingQueueItem.region.currentCountry.recroots);
            }

            if ((buildingQueueItem.movesLasts -= buildSpeed) <= 0)
            {
                BuildBuilding(buildingQueueItem.building, region, true);
            }
        }
    }

    public void CheckBuildedBuildignsUI(RegionManager region)
    {
        region.civFactory_Amount = 0;
        region.milFactory_Amount = 0;
        region.farms_Amount = 0;
        region.cheFarms = 0;

        for (int i = 0; i < region.buildings.Count; i++)
        {
            BuildingScriptableObject building = region.buildings[i];

            if (building._name == "CFR")
            {
                region.civFactory_Amount++;
            }
            else if (building._name == "MFR")
            {
                region.milFactory_Amount++;
            }
            else if (building._name == "INF")
            {
                region.infrastructure_Amount++;
            }
            else if (building._name == "FOR")
            {
                region.fortifications_Amount++;
            }
            else if (building._name == "FAR")
            {
                region.farms_Amount++;
            }
            else if (building._name == "CHF")
            {
                region.cheFarms++;
            }
            else if (building._name == "REL")
            {
                region.researchLabs++;
            }
        }
    }

    public void BuildBuilding(BuildingScriptableObject building, RegionManager region, bool changeValues)
    {
        region.buildings.Add(building);

        if (changeValues)
        {
            region.currentCountry.moneyNaturalIncome += building.goldIncome;
            region.currentCountry.foodNaturalIncome += building.foodIncome;
            region.currentCountry.recrootsIncome += building.recrootsIncome;
            region.currentCountry.researchPointsIncome += building.researchPointsIncome;
        }

        if (building == ReferencesManager.Instance.gameSettings.fabric)
        {
            region.currentCountry.civFactories++;
        }
        if (building == ReferencesManager.Instance.gameSettings.farm)
        {
            region.currentCountry.farms++;
        }
        if (building == ReferencesManager.Instance.gameSettings.chefarm)
        {
            region.currentCountry.chemicalFarms++;
        }
        if (building == ReferencesManager.Instance.gameSettings.researchLab)
        {
            region.currentCountry.researchLabs++;
        }

        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.SetRegionValues(_id, population, hasArmy, goldIncome, foodIncome, civFactory_Amount,
                infrastructure_Amount, farms_Amount, cheFarms, regionScore);
        }

        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
        ReferencesManager.Instance.countryManager.UpdateValuesUI();
    }

    [System.Serializable]
    public class BuildingQueueItem
    {
        public BuildingScriptableObject building;
        public RegionManager region;
        public float movesLasts;
    }
}
