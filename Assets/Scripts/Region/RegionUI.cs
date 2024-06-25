using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class RegionUI : MonoBehaviour
{
    [Header("UI References: ")]
    public Button fightCloseOverlay;
    public GameObject blockPanel;
    public GameObject countryInfoPanel;
    public GameObject pauseButton;
    public GameObject nextMoveButton;
    public GameObject photoModeOffPanel;
    public GameObject regionUIContainer;
    public GameObject regionClaimsContainer;
    public GameObject countryFlagPrefab;
    public GameObject countryInfoAdvanced;
    [SerializeField] GameObject armyHorizontalAnimationUI;

    public Button _upgradeFortificationButton;
    public TMP_Text _upgradeFortificationButton_Text;
    public TMP_Text _upgradeFortificationCost_Text;

    public Button _upgradeMarineBaseButton;
    public TMP_Text _upgradeMarineBaseButton_Text;
    public TMP_Text _upgradeMarineBaseCost_Text;

    public Button _upgradeAirBaseButton;
    public TMP_Text _upgradeAirBaseButton_Text;
    public TMP_Text _upgradeAirBaseCost_Text;

    [SerializeField] GameObject settingsPanel;
    public GameObject[] tabs;
    [SerializeField] GameObject[] unitShopTabs;
    public GameObject[] verifyPanels;

    public TMP_Text regionPopulationText;
    public TMP_Text regionEconomyText;
    public TMP_Text civFactoryAmount;
    public TMP_Text farmsAmount;
    public TMP_Text milFactoryAmount;
    public TMP_Text infrastructureAmount;
    public TMP_Text fortificationsAmount;
    public TMP_Text steelAmount;
    public TMP_Text alluminiumAmount;
    public TMP_Text rubberAmount;
    public TMP_Text OilAmount;
    public TMP_Text RegionInfrastructureAmount;

    public GameObject createArmyTab;
    public GameObject fightPanelContainer;
    public GameObject fightPanelAttackerHorizontalGroup;
    public GameObject fightPanelDefenderHorizontalGroup;

    public Button regionUpgradeButton;
    public Button buildButton;
    public Button armyButton;
    public Button defenseButton;
    public Button addArmyButton;
    public Button aviationButton;

    public Button regionButtonUpgrade;
    public Button armyButtonUpgrade;
    public Button defenseButtonUpgrade;
    public Button moveButton;

    [HideInInspector] public bool statisticShowed;

    [Header("Diplomaty")]
    public Sprite tradeSprite;
    public Sprite moveSprite;
    public Sprite warSprite;
    public Sprite unionSprite;
    public Sprite pactSprite;
    public Sprite AntipactSprite;

    public Image regionInfoCountryFlag;

    public GameObject resultPanel;
    public GameObject resultPanelColor;
    public TMP_Text resultText;

    public GameObject annexButton;
    public GameObject confirmDefeatButton;
    public GameObject confirmFightButton;
    public GameObject cancelFightButton;
    public Transform messageReceiver;
    public GameObject messageEvent;

    [HideInInspector] public float winChance;
    [HideInInspector] public RaycastHit2D hit;
    [HideInInspector] public UnitMovement unitMovement;
    [HideInInspector] public RegionManager actionRegion;
    [HideInInspector] public Transform _annexedRegion;

    [Header("Fight settings")]
    public Color victoryColor;
    public Color defeatColor;

    public Image defenderCountryFlag;
    public TMP_Text defenderCountryName;

    public Image winChanceBarInner;
    public TMP_Text winChanceText;

    public Image _currentTerrainThumbnail;

    [SerializeField] GameObject addFleetTab;
    [SerializeField] GameObject createFleetTab;
    [SerializeField] GameObject addArmyTab;
    [SerializeField] GameObject unitsShopContainer;
    [SerializeField] GameObject navyUnitsShopContainer;
    [SerializeField] GameObject buildingsShopContainer;
    public GameObject barContent;

    private UnitShopCheck[] unitShopCheckButtons;

    public TMP_Text regionCost;
    public TMP_Text armyCost;
    public TMP_Text defenseCost;

    public GameObject regionBarContainer;
    public GameObject armyContainer;
    public GameObject unitShop;

    private List<Transform> movePoints = new List<Transform>();

    public Sprite[] buttonDesigns;

    public UI_Button[] buttons;

    [Header("Building")]
    public Transform buildingUIContent;
    public GameObject buildingUIPrefab;
    public GameObject buildingQueueUIPrefab;
    [SerializeField] BuildingQueueItem[] buildingQueueList;

    public TMP_Text goldIncomesText;
    public TMP_Text foodIncomesText;
    public TMP_Text pointsIncomesText;

    [Header("Audio/SFX")]
    public AudioClip click_01;
    public AudioClip click_02;
    public AudioClip paper_01;


    [HideInInspector] public MovePoint currentMovePoint;
    [SerializeField] List<UnitUI> unitUIs = new List<UnitUI>();

    [SerializeField] GameObject RegionInfoCanvas;

    public GameObject cancelRegionSelectionModeButton;
    public GameObject createVassalRegionSelectionModeButton;

    [SerializeField] TMP_Text _marchEventText;
    public GameObject _autoSaveMessage;

    [SerializeField] private NavyAddButton[] navyAddButtons;


    private void Awake()
    {
        if (PlayerPrefs.HasKey("buttonDesign"))
        {
            SetButtonsDesign(PlayerPrefs.GetInt("buttonDesign"));
        }

        statisticShowed = false;

        //_marchEventText.gameObject.SetActive(ReferencesManager.Instance.gameSettings.marchEvent.value);
    }

    public void OpenTab(GameObject tab)
    {
        CloseAllUI();

        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
        }

        tab.SetActive(true);

        List<RegionInfoCanvas> regionInfoCanvases = new List<RegionInfoCanvas>();
        regionInfoCanvases = FindObjectsOfType<RegionInfoCanvas>().ToList();

        if (regionInfoCanvases.Count > 0)
        {
            for (int i = 0; i < regionInfoCanvases.Count; i++)
            {
                Destroy(ReferencesManager.Instance.mainCamera.regionInfos[i].gameObject);
            }
        }

        ReferencesManager.Instance.mainCamera.regionInfos.Distinct();

        if (tab.name == "BuildingContainer")
        {
            foreach (RegionManager region in ReferencesManager.Instance.countryManager.currentCountry.myRegions)
            {
                GameObject spawnedRegionCanvas = Instantiate(RegionInfoCanvas, region.transform);
                spawnedRegionCanvas.transform.localScale = Vector3.one;
                spawnedRegionCanvas.GetComponent<RegionInfoCanvas>().UpdateSize();

                spawnedRegionCanvas.GetComponent<RegionInfoCanvas>().UpdateUI(region);
                ReferencesManager.Instance.mainCamera.regionInfos.Add(spawnedRegionCanvas.GetComponent<RegionInfoCanvas>());
            }
        }
    }

    public void CloseTabs()
    {
        CloseAllUI();

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[3].activeSelf)
            {
                tabs[3].SetActive(true);
            }
            tabs[0].SetActive(false);
            tabs[1].SetActive(false);
            tabs[2].SetActive(false);
        }
    }

    public void OpenArmyTab()
    {
        if (!ReferencesManager.Instance.regionManager.currentRegionManager.demilitarized)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].SetActive(false);
            }

            unitsShopContainer.SetActive(false);
            navyUnitsShopContainer.SetActive(false);

            buildingsShopContainer.SetActive(false);

            if (ReferencesManager.Instance.regionManager.currentRegionManager.hasArmy && ReferencesManager.Instance.regionManager.currentRegionManager.currentCountry == ReferencesManager.Instance.countryManager.currentCountry)
            {
                addArmyTab.SetActive(true);
            }
            else if (!ReferencesManager.Instance.regionManager.currentRegionManager.hasArmy && ReferencesManager.Instance.regionManager.currentRegionManager.currentCountry == ReferencesManager.Instance.countryManager.currentCountry)
            {
                if (ReferencesManager.Instance.countryManager.currentCountry.money >= ReferencesManager.Instance.gameSettings.soldierLVL1.moneyCost)
                {
                    if (ReferencesManager.Instance.countryManager.currentCountry.food >= ReferencesManager.Instance.gameSettings.soldierLVL1.foodCost)
                    {
                        if (ReferencesManager.Instance.countryManager.currentCountry.recroots >= ReferencesManager.Instance.gameSettings.soldierLVL1.recrootsCost)
                        {
                            createArmyTab.SetActive(true);

                            foreach (Transform container in createArmyTab.transform)
                            {
                                container.gameObject.SetActive(true);
                            }

                            UISoundEffect.Instance.PlayAudio(paper_01);
                        }
                        else
                        {
                            WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.NoRecruits"));
                        }
                    }
                    else
                    {
                        WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.NoFood"));

                    }
                }
                else
                {
                    WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.NoMoney"));
                }
            }
        }
        else
        {
            WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.Demil"));
        }
    }

    public void OpenFleetTab()
    {
        if (!ReferencesManager.Instance.regionManager.currentRegionManager.demilitarized)
        {
            CloseAllUI();

            if (ReferencesManager.Instance.regionManager.currentRegionManager._marineBaseLevel > 0)
            {
                if (ReferencesManager.Instance.regionManager.currentRegionManager._hasFleet &&
    ReferencesManager.Instance.regionManager.currentRegionManager.currentCountry ==
    ReferencesManager.Instance.countryManager.currentCountry)
                {
                    addFleetTab.SetActive(true);
                }
                else if (!ReferencesManager.Instance.regionManager.currentRegionManager._hasFleet &&
                    ReferencesManager.Instance.regionManager.currentRegionManager.currentCountry ==
                    ReferencesManager.Instance.countryManager.currentCountry)
                {
                    if (ReferencesManager.Instance.countryManager.currentCountry._fleet.Count > 0)
                    {
                        createFleetTab.SetActive(true);

                        foreach (Transform container in createFleetTab.transform)
                        {
                            container.gameObject.SetActive(true);
                        }

                        UISoundEffect.Instance.PlayAudio(paper_01);
                    }
                    else
                    {
                        WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.NoShips"));
                    }
                }

                for (int i = 0; i < navyAddButtons.Length; i++)
                {
                    navyAddButtons[i].SetUp();
                }

                ReferencesManager.Instance.fleetManager.UpdateFleetUI();
            }
            else
            {
                WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.NoMarineBase"));
            }
        }
    }

    public void UpdateTenplatesTab(Transform templatesContainer)
    {
        foreach (Transform child in templatesContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < ReferencesManager.Instance.army.templates.Count; i++)
        {
            GameObject newTemplateObject = Instantiate(ReferencesManager.Instance.countryInfo._templatePrefab, templatesContainer);

            newTemplateObject.GetComponent<ArmyTemplateItem_UI>()._index = i;
            newTemplateObject.GetComponent<ArmyTemplateItem_UI>()._name = ReferencesManager.Instance.army.templates[i]._name;
            newTemplateObject.GetComponent<ArmyTemplateItem_UI>().SetUp();

            newTemplateObject.GetComponent<Button>().onClick.AddListener(newTemplateObject.GetComponent<ArmyTemplateItem_UI>().BuyTemplate);
        }
    }

    public void ChangeUnitShopTab(GameObject tab)
    {
        for (int i = 0; i < unitShopTabs.Length; i++)
        {
            unitShopTabs[i].SetActive(false);
            tab.SetActive(true);
        }

        ReferencesManager.Instance.army.CheckUnitTech();
        UISoundEffect.Instance.PlayAudio(paper_01);
    }

    public void MoveUnitMode()
    {
        if (ReferencesManager.Instance.regionManager.currentRegionManager.hasArmy)
        {
            UnitMovement unitMovement = ReferencesManager.Instance.regionManager.currentRegionManager.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();

            if (unitMovement._movePoints > 0)
            {
                ReferencesManager.Instance.regionManager.moveMode = true;

                for (int i = 0; i < tabs.Length; i++)
                {
                    tabs[i].SetActive(false);
                }
                unitsShopContainer.SetActive(false);
                navyUnitsShopContainer.SetActive(false);

                barContent.SetActive(false);

                UnitMovement[] unitsMovements = FindObjectsOfType<UnitMovement>();

                for (int i = 0; i < unitsMovements.Length; i++)
                {
                    unitsMovements[i].isSelected = false;
                }

                foreach (Transform movePoint in ReferencesManager.Instance.regionManager.currentRegionManager.movePoints)
                {
                    movePoint.GetComponent<PolygonCollider2D>().enabled = true;
                    movePoint.GetComponent<MovePoint>().attackerUnit = unitMovement;
                }

                unitMovement.isSelected = true;
                unitMovement.ShowClosestsPoints();

                UISoundEffect.Instance.PlayAudio(click_01);
            }
            else
            {
                WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.NoMovePoints"));
            }
        }
    }

    public void DeMoveUnitMode(bool playSound)
    {
        MovePoint[] localMovePoints = FindObjectsOfType<MovePoint>();

        movePoints.Clear();
        foreach (MovePoint movePoint in localMovePoints)
        {
            movePoints.Add(movePoint.transform);
        }

        for (int i = 0; i < movePoints.Count; i++)
        {
            movePoints[i].gameObject.GetComponent<PolygonCollider2D>().enabled = false;
            Destroy(movePoints[i].gameObject.GetComponent<SpriteRenderer>());
        }

        if (playSound)
        {
            UISoundEffect.Instance.PlayAudio(click_01);
        }
    }

    public void ExecuteLockState()
    {
        unitShopCheckButtons = FindObjectsOfType<UnitShopCheck>();

        for (int i = 0; i < unitShopCheckButtons.Length; i++)
        {
            unitShopCheckButtons[i].CheckLockState();
        }
    }

    public void UpdateUnitsUI(bool checkUnits)
    {
        armyHorizontalAnimationUI.SetActive(true);

        foreach (Transform child in ReferencesManager.Instance.army.armyHorizontalGroup.transform)
        {
            if (child.gameObject.GetComponent<UnitUI>())
            {
                Destroy(child.gameObject);
            }
        }

        unitUIs.Clear();

        UnitMovement division = ReferencesManager.Instance.regionManager.currentRegionManager.GetDivision(ReferencesManager.Instance.regionManager.currentRegionManager);

        if (division != null)
        {
            foreach (UnitMovement.UnitHealth unit in division.unitsHealth)
            {
                GameObject spawnedUIButton = Instantiate(ReferencesManager.Instance.army.unitUIPrefab, ReferencesManager.Instance.army.armyHorizontalGroup.transform);
                spawnedUIButton.GetComponent<UnitUI>().unitIcon.sprite = unit.unit.icon;
                spawnedUIButton.GetComponent<UnitUI>().currentUnit = unit.unit;
                spawnedUIButton.GetComponent<UnitUI>().division = division;
                spawnedUIButton.GetComponent<UnitUI>().id = unit._id;
                spawnedUIButton.GetComponent<UnitUI>().UpdateUI();

                if (division.unitsHealth.Count >= ReferencesManager.Instance.army.maxUnits)
                {
                    addArmyButton.gameObject.SetActive(false);
                }
                else
                {
                    ReferencesManager.Instance.army.addArmyButton.SetActive(true);
                }

                ReferencesManager.Instance.army.addArmyButton.transform.SetAsLastSibling();
            }

            if (division.unitsHealth.Count <= 0 && checkUnits)
            {
                ReferencesManager.Instance.regionManager.currentRegionManager.hasArmy = false;
                division.unitsHealth.Clear();
                armyContainer.SetActive(false);
                unitShop.SetActive(false);
                Destroy(division.gameObject);
            }
        }

        armyHorizontalAnimationUI.SetActive(false);
    }

    public void UpdateDivisionUnitsIDs(UnitMovement division)
    {
        foreach (UnitMovement.UnitHealth batalion in division.unitsHealth)
        {
            batalion._id = Random.Range(1, 9999);
        }
    }

    public void UpdateFightUnitsUI(Transform panel, UnitMovement division, RegionManager fightRegion)
    {
        if (division == null)
        {
            fightRegion.currentDefenseUnits = ReferencesManager.Instance.gameSettings.currentDefenseUnits_FirstLevel;

            foreach (UnitMovement.UnitHealth batalion in fightRegion.currentDefenseUnits)
            {
                GameObject spawnedUIButton = Instantiate(ReferencesManager.Instance.army.unitUIPrefab, panel);
                spawnedUIButton.GetComponent<UnitUI>().unitIcon.sprite = batalion.unit.icon;
                spawnedUIButton.GetComponent<UnitUI>().currentUnit = batalion.unit;
                spawnedUIButton.GetComponent<UnitUI>().division = null;
                spawnedUIButton.GetComponent<UnitUI>().id = batalion._id;
                spawnedUIButton.GetComponent<UnitUI>().UpdateUI();
                spawnedUIButton.GetComponent<Button>().interactable = false;
            }
        }
        else
        {
            if (division.unitsHealth.Count > 0)
            {
                foreach (UnitMovement.UnitHealth batalion in division.unitsHealth)
                {
                    GameObject spawnedUIButton = Instantiate(ReferencesManager.Instance.army.unitUIPrefab, panel);
                    spawnedUIButton.GetComponent<UnitUI>().unitIcon.sprite = batalion.unit.icon;
                    spawnedUIButton.GetComponent<UnitUI>().currentUnit = batalion.unit;
                    spawnedUIButton.GetComponent<UnitUI>().division = division;
                    spawnedUIButton.GetComponent<UnitUI>().id = batalion._id;
                    spawnedUIButton.GetComponent<UnitUI>().UpdateUI();
                    spawnedUIButton.GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    public void FightProceed(float winChance, RegionManager fightRegion)
    {
        if (winChance >= 50)
        {
            Victory();

            UnitMovement loserDivision = ReferencesManager.Instance.gameSettings.currentBattle.defenderDivision;
            UnitMovement djarahov = ReferencesManager.Instance.gameSettings.currentBattle.attackerDivision;

            if (loserDivision != null)
            {
                loserDivision.Retreat(loserDivision);
            }

            RegionManager _actionRegion = ReferencesManager.Instance.gameSettings.currentBattle.fightRegion;



            djarahov.AIMoveNoHit(_actionRegion, djarahov);
        }
        else if (winChance < 50)
        {
            Defeat();

            UnitMovement attackerDivision = ReferencesManager.Instance.gameSettings.currentBattle.attackerDivision;

            attackerDivision.firstMove = false;
            attackerDivision._movePoints--;
            attackerDivision.UpdateInfo();
        }
    }

    public void ConfirmResult()
    {
        if (winChance >= 50)
        {
            ReferencesManager.Instance.regionManager.SelectRegionNoHit(actionRegion);
        }
        fightCloseOverlay.interactable = true;
    }

    public void CloseAllUI()
    {
        List<RegionInfoCanvas> regionInfoCanvases = new List<RegionInfoCanvas>();
        regionInfoCanvases = FindObjectsOfType<RegionInfoCanvas>().ToList();

        for (int i = 0; i < regionInfoCanvases.Count; i++)
        {
            ReferencesManager.Instance.mainCamera.regionInfos.Remove(regionInfoCanvases[i]);
            Destroy(regionInfoCanvases[i].gameObject);
        }

        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
        }

        createArmyTab.SetActive(false);
        settingsPanel.SetActive(false);
        unitsShopContainer.SetActive(false);
        navyUnitsShopContainer.SetActive(false);
        createFleetTab.SetActive(false);
        buildingsShopContainer.SetActive(false);
        ReferencesManager.Instance.diplomatyUI.diplomatyContainer.SetActive(false);
        countryInfoAdvanced.SetActive(false);
    }


    private void Victory()
    {
        resultPanel.SetActive(true);
        resultText.text = ReferencesManager.Instance.languageManager.GetTranslation("FightUI.Victory");
        resultPanelColor.GetComponent<Image>().color = victoryColor;

        annexButton.SetActive(true);
        confirmDefeatButton.SetActive(false);
        confirmFightButton.SetActive(false);
        cancelFightButton.SetActive(false);

        UpdateFightResultUI();
        //CheckExistCountry();
    }

    private void Defeat()
    {
        resultPanel.SetActive(true);
        resultText.text = ReferencesManager.Instance.languageManager.GetTranslation("FightUI.Defeat");
        resultPanelColor.GetComponent<Image>().color = defeatColor;

        annexButton.SetActive(false);
        confirmDefeatButton.SetActive(true);
        confirmFightButton.SetActive(false);
        cancelFightButton.SetActive(false);

        UpdateFightResultUI();
    }


    private void UpdateFightResultUI()
    {
        for (int i = 0; i < ReferencesManager.Instance.army.attackerArmyLosses.Length; i++)
        {
            TMP_Text text = ReferencesManager.Instance.army.attackerArmyLosses[i];
            text.text = $"-{ReferencesManager.Instance.army.attackerArmyLossesValue[i]}";
        }

        for (int i = 0; i < ReferencesManager.Instance.army.defenderArmyLosses.Length; i++)
        {
            TMP_Text text = ReferencesManager.Instance.army.defenderArmyLosses[i];
            text.text = $"-{ReferencesManager.Instance.army.defenderArmyLossesValue[i]}";
        }

        for (int i = 0; i < ReferencesManager.Instance.army.attackerEconomy.Length; i++)
        {
            TMP_Text text = ReferencesManager.Instance.army.attackerEconomy[i];
            text.text = $"+{ReferencesManager.Instance.army.attackerEconomyValue[i]}";
        }

        for (int i = 0; i < ReferencesManager.Instance.army.defenderEconomy.Length; i++)
        {
            TMP_Text text = ReferencesManager.Instance.army.defenderEconomy[i];
            text.text = $"+{ReferencesManager.Instance.army.defenderEconomyValue[i]}";
        }
    }


    private void CheckExistCountry()
    {
        if (actionRegion.currentCountry.myRegions.Count <= 1) // Country Capitulated
        {
            foreach (CountrySettings country in ReferencesManager.Instance.countryManager.countries)
            {
                Relationships.Relation relation = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(country, actionRegion.currentCountry);

                relation.relationship = 0;
                relation.right = false;
                relation.union = false;
                relation.pact = false;
                relation.war = false;
                relation.trade = false;
                relation.vassal = false;

                for (int i = 0; i < ReferencesManager.Instance.diplomatyUI.globalTrades.Count; i++)
                {
                    if (ReferencesManager.Instance.diplomatyUI.globalTrades[i].sender == actionRegion.currentCountry && ReferencesManager.Instance.diplomatyUI.globalTrades[i].receiver == country)
                    {
                        TradeBuff trade = ReferencesManager.Instance.diplomatyUI.globalTrades[i];

                        actionRegion.currentCountry.moneyTradeIncome -= trade.senderMoneyTrade;
                        actionRegion.currentCountry.foodTradeIncome -= trade.senderFoodTrade;

                        relation.country.moneyTradeIncome -= trade.receiverMoneyTrade;
                        relation.country.foodTradeIncome -= trade.receiverFoodTradee;

                        ReferencesManager.Instance.diplomatyUI.globalTrades.Remove(trade);
                    }
                }
            }
        }
    }

    public void FightConfirm()
    {
        currentMovePoint.ConfirmFight();
        cancelFightButton.SetActive(false);
        fightCloseOverlay.interactable = false;

        FightProceed(winChance, actionRegion);
    }

    public void CancelFight()
    {
        currentMovePoint.CancelFight();

        unitMovement.UpdateInfo();

        DeMoveUnitMode(true);
    }

    public void UpdateBuildingUI()
    {
        ReferencesManager.Instance.regionManager.currentRegionManager.goldIncome = 0;
        ReferencesManager.Instance.regionManager.currentRegionManager.foodIncome = 0;

        foreach (Transform child in buildingUIContent)
        {
            Destroy(child.gameObject);
        }

        if (ReferencesManager.Instance.regionManager.currentRegionManager != null)
        {
            foreach (RegionManager.BuildingQueueItem buildingQueueItem in ReferencesManager.Instance.regionManager.currentRegionManager.buildingsQueue)
            {
                GameObject spawnedQueuePrefab = Instantiate(buildingQueueUIPrefab, buildingUIContent);

                spawnedQueuePrefab.GetComponent<BuildingQueueItem>().buildingQueueItem = buildingQueueItem;

                if (buildingQueueItem.building.buildType == BuildingScriptableObject.BuildType.GoldProducer)
                {
                    spawnedQueuePrefab.transform.SetAsFirstSibling();
                }
                else if (buildingQueueItem.building.buildType == BuildingScriptableObject.BuildType.FoodProducer)
                {
                    spawnedQueuePrefab.transform.SetAsLastSibling();
                }
                else if (buildingQueueItem.building.buildType == BuildingScriptableObject.BuildType.RecrootsProducer)
                {
                    spawnedQueuePrefab.transform.SetAsLastSibling();
                }

                spawnedQueuePrefab.GetComponent<Image>().sprite = buildingQueueItem.building.icon;
            }

            foreach (BuildingScriptableObject building in ReferencesManager.Instance.regionManager.currentRegionManager.buildings)
            {
                GameObject spawnedPrefab = Instantiate(buildingUIPrefab, buildingUIContent);

                spawnedPrefab.GetComponent<BuildingButton>().currentBuilding = building;

                if (building.buildType == BuildingScriptableObject.BuildType.GoldProducer)
                {
                    spawnedPrefab.transform.SetAsFirstSibling();
                }
                else if (building.buildType == BuildingScriptableObject.BuildType.FoodProducer)
                {
                    spawnedPrefab.transform.SetAsLastSibling();
                }
                else if (building.buildType == BuildingScriptableObject.BuildType.RecrootsProducer)
                {
                    spawnedPrefab.transform.SetAsLastSibling();
                }

                spawnedPrefab.GetComponent<Image>().sprite = building.icon;

                ReferencesManager.Instance.regionManager.currentRegionManager.goldIncome += building.goldIncome;
                ReferencesManager.Instance.regionManager.currentRegionManager.foodIncome += building.foodIncome;
            }

            goldIncomesText.text = $"+{ReferencesManager.Instance.regionManager.currentRegionManager.goldIncome}";
            foodIncomesText.text = $"+{ReferencesManager.Instance.regionManager.currentRegionManager.foodIncome}";
            pointsIncomesText.text = $"+{ReferencesManager.Instance.regionManager.currentRegionManager.researchLabs}";

            ForceUpdateProggresbar();
        }
    }

    public void TogglePhotoMode(bool state)
    {
        CloseAllUI();
        photoModeOffPanel.SetActive(state);
    }

    public void ToggleRegionStatistic()
    {
        statisticShowed = !statisticShowed;

        Animator regionUIContainerAnimator = regionUIContainer.GetComponent<Animator>();

        if (statisticShowed) // close
        {
            regionUIContainerAnimator.Play("closePanel");
        }
        else if (!statisticShowed) // open
        {
            regionUIContainerAnimator.Play("openPanel");
        }
    }

    public void SetButtonsDesign(int data)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].targetImage.sprite = buttonDesigns[data];
        }
    }

    public void ResetClaims()
    {
        foreach (Transform child in regionClaimsContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ForceUpdateProggresbar()
    {
        foreach (Transform _buildingUIItem in buildingUIContent)
        {
            BuildingQueueItem buildingQueueItem = _buildingUIItem.GetComponent<BuildingQueueItem>();

            if (buildingQueueItem != null)
            {
                buildingQueueItem.UpdateUI();
            }

            /*
            ⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
            ⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⠛⠛⠉⠉⠉⠀⠀⠀⠉⠉⠉⠛⠛⠛⠿⣿⣿⣿⣿⣿
            ⣿⣿⣿⣿⠿⠋⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠛⢿⣿⣿
            ⣿⣿⡆⢀⣤⡄⠀⠀⠀⠀⠀⠀⠀⠀⣠⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡄⣿⣿
            ⣿⣿⣇⡀⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡼⠟⢫⣿⣿
            ⣿⣿⣿⣿⣿⣶⣄⣀⣀⣀⠀⣀⣠⣤⡤⠤⠀⠀⠀⠀⠀⠀⠀⡴⠊⠁⠀⣼⣿⣿
            ⣿⣿⣿⡇⠀⠉⠙⠻⠿⠿⠿⣿⣿⡀⠀⠀⠀⠀⠀⠀⠀⢀⡴⠁⠀⠀⢠⣿⣿⣿
            ⣿⣿⣿⠁⠀⠀⠀⠀⠀⢀⣾⣿⣿⣿⣶⣤⣀⡀⠀⠀⠶⣿⣷⣶⣶⣿⣿⣿⣿⣿
            ⣿⣿⣿⣄⣀⣀⣀⣤⣶⣿⣿⣿⣿⣿⣿⣿⣿⣷⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿
            ⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
            ⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
            ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
            ⠀⠀⣀⣴⣴⡾⠿⠶⣶⣶⣶⣦⣤⠀⠀⠀⢠⣤⣴⣶⣶⣶⠶⢶⣶⣴⣄⠀⠀⠀
            ⠀⠀⠑⠋⠁⢀⣐⣒⣒⣒⣒⣢⡄⠀⠀⠀⠀⣀⢶⣶⣶⣶⣶⣀⠀⠉⠓⠁⠀⠀
            ⠀⠀⠀⠀⠶⢭⣸⣿⣿⣟⣹⡁⠏⠀⠀⠀⠘⠇⣹⣙⣿⣿⣿⣨⠷⠄⠀⠀⠀⠀
            ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
            ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⠀⠀⠀
            ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⠋⠀⠀⠀⠀
            ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡴⠀⣰⠃⠀⠀⠀⠀⠀
            ⠀⠀⠀⠀⠀⠀⠀⠀⣦⢤⣤⣤⠤⠤⠤⠤⠴⠒⠒⠋⠁⠀⠀⡇⠀⠀⠀⠀⠀⠀
            ⠀⠀⠀⠀⠀⠀⠀⠀⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠇⠀⠀⠀⠀⠀⠀
            ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
            ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀ 
            */
        }
    }
}
