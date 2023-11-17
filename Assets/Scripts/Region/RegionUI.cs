using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RegionUI : MonoBehaviour
{
    [Header("UI References: ")]
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

    [SerializeField] GameObject settingsPanel;
    public GameObject regionsLoadingPanel;
    public Image regionsLoadingBarInner;
    public TMP_Text regionsLoadingProgressText;
    public TMP_Text regionsLoadingMainText;
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

    public Color victoryColor;
    public Color defeatColor;

    public Image defenderCountryFlag;
    public TMP_Text defenderCountryName;

    public Image winChanceBarInner;
    public TMP_Text winChanceText;

    [SerializeField] GameObject addArmyTab;
    [SerializeField] GameObject unitsShopContainer;
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
    public TMP_Text recrotsIncomesText;

    [Header("Audio/SFX")]
    public AudioClip click_01;
    public AudioClip click_02;
    public AudioClip paper_01;


    [HideInInspector] public MovePoint currentMovePoint;
    [SerializeField] List<UnitUI> unitUIs = new List<UnitUI>();

    [SerializeField] GameObject RegionInfoCanvas;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("buttonDesign"))
        {
            SetButtonsDesign(PlayerPrefs.GetInt("buttonDesign"));
        }

        statisticShowed = false;
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

        for (int i = 0; i < regionInfoCanvases.Count; i++)
        {
            ReferencesManager.Instance.mainCamera.regionInfos.Remove(regionInfoCanvases[i]);
            Destroy(ReferencesManager.Instance.mainCamera.regionInfos[i].gameObject);
        }

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
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
            unitsShopContainer.SetActive(false);
        }

        unitsShopContainer.SetActive(false);
        buildingsShopContainer.SetActive(false);

        if (ReferencesManager.Instance.regionManager.currentRegionManager.hasArmy && ReferencesManager.Instance.regionManager.currentRegionManager.currentCountry == ReferencesManager.Instance.countryManager.currentCountry)
        {
            addArmyTab.SetActive(true);
        }
        else if(!ReferencesManager.Instance.regionManager.currentRegionManager.hasArmy && ReferencesManager.Instance.regionManager.currentRegionManager.currentCountry == ReferencesManager.Instance.countryManager.currentCountry)
        {
            if (ReferencesManager.Instance.countryManager.currentCountry.money >= ReferencesManager.Instance.gameSettings.soldierLVL1.moneyCost)
            {
                if (ReferencesManager.Instance.countryManager.currentCountry.recroots >= ReferencesManager.Instance.gameSettings.soldierLVL1.recrootsCost)
                {
                    createArmyTab.SetActive(true);
                    UISoundEffect.Instance.PlayAudio(paper_01);
                }
                else
                {
                    WarningManager.Instance.Warn(ReferencesManager.Instance.gameSettings.NO_RECROOTS);
                }
            }
            else
            {
                WarningManager.Instance.Warn(ReferencesManager.Instance.gameSettings.NO_GOLD);
            }
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

    public void ToggleColliders(bool state)
    {
        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            ReferencesManager.Instance.countryManager.regions[i].GetComponent<PolygonCollider2D>().enabled = state;
        }
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
                    unitsShopContainer.SetActive(false);
                }

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
                WarningManager.Instance.Warn(ReferencesManager.Instance.gameSettings.NO_MOVEPOINTS);
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

        ToggleColliders(true);

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

    public void UpdateUnitsUI()
    {
        StartCoroutine(UpdateUnitsUI_Co());
    }

    public void CreateUnitUI(Sprite unitIcon, UnitScriptableObject unit)
    {
        UnitMovement unitMovement = ReferencesManager.Instance.regionManager.currentRegionManager.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();
        if (unitMovement.unitsHealth != null)
        {
            GameObject spawnedUIButton = Instantiate(ReferencesManager.Instance.army.unitUIPrefab, ReferencesManager.Instance.army.armyHorizontalGroup.transform);
            spawnedUIButton.GetComponent<UnitUI>().unitIcon.sprite = unitIcon;
            spawnedUIButton.GetComponent<UnitUI>().currentUnit = unit;

            if (unitMovement.unitsHealth.Count != ReferencesManager.Instance.army.maxUnits)
            {
                addArmyButton.gameObject.SetActive(true);
                ReferencesManager.Instance.army.addArmyButton.transform.SetAsLastSibling();
            }
            else
            {
                ReferencesManager.Instance.army.addArmyButton.transform.SetAsLastSibling();
                ReferencesManager.Instance.army.addArmyButton.SetActive(false);
            }
        }
    }

    public void CreateFightUnitUI(UnitScriptableObject unit, GameObject panel)
    {
        Sprite unitIcon = unit.icon;

        GameObject spawnedUnitUI = Instantiate(ReferencesManager.Instance.army.unitUnclikableUIPrefab, panel.transform);

        if (unit.type == UnitScriptableObject.Type.SOLDIER)
        {
            spawnedUnitUI.transform.SetAsFirstSibling();
        }
        if (unit.type == UnitScriptableObject.Type.ARTILERY)
        {
            spawnedUnitUI.transform.SetAsLastSibling();
        }
        if (unit.type == UnitScriptableObject.Type.TANK)
        {
            spawnedUnitUI.transform.SetAsLastSibling();
        }

        spawnedUnitUI.GetComponent<UnitUI>().unitIcon.sprite = unitIcon;
        spawnedUnitUI.GetComponent<UnitUI>().currentUnit = unit;
    }

    public void FightProceed(float winChance, RegionManager fightRegion, RaycastHit2D hit, UnitMovement unitMovement)
    {
        if (winChance >= 50) { Victory(); }

        else if (winChance < 50) { Defeat(); }
    }

    public void ConfirmResult()
    {
        ToggleColliders(true);
        if (winChance >= 50)
        {
            currentMovePoint.MoveUnit(hit, true, actionRegion.transform, false);
        }

        ReferencesManager.Instance.regionManager.SelectRegionNoHit(actionRegion);
    }


    private IEnumerator UpdateUnitsUI_Co()
    {
        armyHorizontalAnimationUI.SetActive(true);
        foreach (Transform child in ReferencesManager.Instance.army.armyHorizontalGroup.transform)
        {
            if (child.gameObject.GetComponent<UnitUI>())
            {
                Destroy(child.gameObject);
            }
        }
        yield return new WaitForSeconds(0f);
        unitUIs.Clear();
        //yield return new WaitForSeconds(0f);

        if (ReferencesManager.Instance.regionManager != null && ReferencesManager.Instance.regionManager.currentRegionManager != null && ReferencesManager.Instance.regionManager.currentRegionManager.hasArmy)
        {
            if (ReferencesManager.Instance.regionManager.currentRegionManager.transform.Find("Unit(Clone)"))
            {
                UnitMovement unitMovement = ReferencesManager.Instance.regionManager.currentRegionManager.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();

                if (unitMovement != null)
                {
                    foreach (UnitMovement.UnitHealth unit in unitMovement.unitsHealth)
                    {
                        CreateUnitUI(unit.unit.icon, unit.unit);
                    }

                    if (unitMovement.unitsHealth.Count == 0)
                    {
                        ReferencesManager.Instance.regionManager.currentRegionManager.hasArmy = false;
                        unitMovement.unitsHealth.Clear();
                        armyContainer.SetActive(false);
                        unitShop.SetActive(false);
                    }
                }

                try
                {
                    foreach (Transform child in ReferencesManager.Instance.army.armyHorizontalGroup.transform)
                    {
                        unitUIs.Add(child.GetComponent<UnitUI>());
                    }
                    unitUIs.RemoveAll(item => item == null);

                    if (unitMovement.unitsHealth.Count > 0 && unitUIs.Count > 0)
                    {
                        for (int i = 0; i < unitMovement.unitsHealth.Count; i++)
                        {
                            unitMovement.unitsHealth[i]._id = i;
                            if (unitUIs[i] != null)
                            {
                                unitUIs[i].id = unitMovement.unitsHealth[i]._id;
                            }
                        }
                    }
                }
                catch (System.Exception)
                {

                }
            }
        }

        armyHorizontalAnimationUI.SetActive(false);
        yield break;
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
        buildingsShopContainer.SetActive(false);
        ReferencesManager.Instance.diplomatyUI.diplomatyContainer.SetActive(false);
        countryInfoAdvanced.SetActive(false);
    }


    private void Victory()
    {
        resultPanel.SetActive(true);
        resultText.text = "Победа";
        resultPanelColor.GetComponent<Image>().color = victoryColor;

        annexButton.SetActive(true);
        confirmDefeatButton.SetActive(false);
        confirmFightButton.SetActive(false);
        cancelFightButton.SetActive(false);

        UpdateFightResultUI();
        CheckExistCountry();
    }

    private void Defeat()
    {
        resultPanel.SetActive(true);
        resultText.text = "Поражение";
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
            for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
            {
                for (int v = 0; v < ReferencesManager.Instance.countryManager.countries[i].GetComponent<Relationships>().relationship.Count; v++)
                {
                    if (ReferencesManager.Instance.countryManager.countries[i].GetComponent<Relationships>().relationship[v].country == actionRegion.currentCountry)
                    {
                        ReferencesManager.Instance.countryManager.countries[i].GetComponent<Relationships>().relationship[v].pact = false;
                        ReferencesManager.Instance.countryManager.countries[i].GetComponent<Relationships>().relationship[v].right = false;
                        ReferencesManager.Instance.countryManager.countries[i].GetComponent<Relationships>().relationship[v].union = false;
                        ReferencesManager.Instance.countryManager.countries[i].GetComponent<Relationships>().relationship[v].war = false;
                        ReferencesManager.Instance.countryManager.countries[i].GetComponent<Relationships>().relationship[v].vassal = false;
                        ReferencesManager.Instance.countryManager.countries[i].GetComponent<Relationships>().relationship[v].trade = false;

                        CountrySettings receiver = ReferencesManager.Instance.countryManager.countries[i].GetComponent<Relationships>().relationship[v].country;

                        for (int tradeIndex = 0; tradeIndex < ReferencesManager.Instance.diplomatyUI.globalTrades.Count; tradeIndex++)
                        {
                            if (ReferencesManager.Instance.diplomatyUI.globalTrades[i].sender == receiver && ReferencesManager.Instance.diplomatyUI.globalTrades[i].receiver == ReferencesManager.Instance.countryManager.countries[i])
                            {
                                TradeBuff trade = ReferencesManager.Instance.diplomatyUI.globalTrades[i];

                                ReferencesManager.Instance.diplomatyUI.globalTrades[i].sender.moneyTradeIncome -= trade.senderMoneyTrade;
                                ReferencesManager.Instance.diplomatyUI.globalTrades[i].sender.foodNaturalIncome -= trade.senderFoodTrade;

                                receiver.moneyTradeIncome -= trade.receiverMoneyTrade;
                                receiver.foodNaturalIncome -= trade.receiverFoodTradee;

                                ReferencesManager.Instance.diplomatyUI.globalTrades.Remove(trade);
                            }
                        }
                    }
                }
            }
        }
    }

    public void FightConfirm()
    {
        currentMovePoint.ConfirmFight();
        cancelFightButton.SetActive(false);

        FightProceed(winChance, actionRegion, hit, unitMovement);
    }

    public void CancelFight()
    {
        currentMovePoint.CancelFight();

        unitMovement._movePoints++;
        unitMovement.firstMove = true;

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

            ForceUpdateProggresbar();
        }
    }

    public void UpdateGarrisonUI()
    {
        if (ReferencesManager.Instance.regionManager.currentRegionManager.currentDefenseUnits.Count > 0)
        {
            foreach (Transform child in ReferencesManager.Instance.army.garrisonHorizontalGroup.transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < ReferencesManager.Instance.regionManager.currentRegionManager.currentDefenseUnits.Count; i++)
            {
                UnitMovement.UnitHealth unit = ReferencesManager.Instance.regionManager.currentRegionManager.currentDefenseUnits[i];

                GameObject spawnedUIButton = Instantiate(ReferencesManager.Instance.army.unitUnclikableUIPrefab, ReferencesManager.Instance.army.garrisonHorizontalGroup.transform);

                spawnedUIButton.GetComponent<UnitUI>().unitIcon.sprite = unit.unit.icon;
                spawnedUIButton.GetComponent<UnitUI>().currentUnit = unit.unit;

                if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
                {
                    spawnedUIButton.transform.SetAsFirstSibling();
                }
                else if (unit.unit.type == UnitScriptableObject.Type.ARTILERY)
                {
                    spawnedUIButton.transform.SetAsLastSibling();
                }
            }
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

    public void ScrollEffect(RectTransform rectTransform)
    {
        rectTransform.position = new Vector3(rectTransform.position.x, -rectTransform.sizeDelta.y * 1.5f, 0);
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
