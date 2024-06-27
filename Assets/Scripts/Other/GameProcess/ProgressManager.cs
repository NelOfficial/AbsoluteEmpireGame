using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class ProgressManager : MonoBehaviour
{
    public Multiplayer multiplayer;

    public GameObject countryMovePanel;
    public Image countryMoveImage;
    public TMP_Text countryMoveName;

    [HideInInspector] public int progressIndex = 0;

    int difficulty_AI_BUFF = 0;
    int difficulty_PLAYER_BUFF = 0;

    private void Start()
    {
        UpdateResources();
        UpdateSellers();


        if (ReferencesManager.Instance.gameSettings.difficultyValue.value == "EASY")
        {
            difficulty_AI_BUFF = -15;
            difficulty_PLAYER_BUFF = 15;
        }
        else if (ReferencesManager.Instance.gameSettings.difficultyValue.value == "HARD")
        {
            difficulty_AI_BUFF = 20;
            difficulty_PLAYER_BUFF = -20;
        }
        else if (ReferencesManager.Instance.gameSettings.difficultyValue.value == "INSANE")
        {
            difficulty_AI_BUFF = 40;
            difficulty_PLAYER_BUFF = -40;
        }
        else if (ReferencesManager.Instance.gameSettings.difficultyValue.value == "HARDCORE")
        {
            difficulty_AI_BUFF = 75;
            difficulty_PLAYER_BUFF = -75;
        }
    }

    private void DateProgress()
    {
        ReferencesManager.Instance.dateManager.currentDate[0] += 10;

        ReferencesManager.Instance.dateManager.UpdateUI();
        ReferencesManager.Instance.dateManager.CheckGameEvents();
    }

    private void UpdateAviationData()
    {
        Aviation_Storage[] aviation_Storages = FindObjectsOfType<Aviation_Storage>();

        for (int i = 0; i < aviation_Storages.Length; i++)
        {
            foreach (var plane in aviation_Storages[i].planes)
            {
                if (plane.fuel < plane.AirPlane.fuelMax)
                {
                    float fuelToFill = plane.AirPlane.fuelMax - plane.fuel;

                    if (plane.Owner != null)
                    {
                        if (plane.Owner.fuel >= fuelToFill)
                        {
                            plane.Owner.fuel -= fuelToFill;
                            plane.fuel += fuelToFill;
                        }
                    }
                }
            }
        }
    }

    private void UpdateUnitsData()
    {
        List<UnitMovement> units = new List<UnitMovement>();

        try
        {
            for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
            {
                ReferencesManager.Instance.countryManager.countries[i].armyPersonnel = 0;

                UpdateProduction(ReferencesManager.Instance.countryManager.countries[i]);

                //foreach (RegionManager province in ReferencesManager.Instance.countryManager.countries[i].myRegions)
                //{
                //    ReferencesManager.Instance.countryManager.countries[i].oil += province.OilAmount;
                //}

                for (int unitIndex = 0; unitIndex < ReferencesManager.Instance.countryManager.countries[i].countryUnits.Count; unitIndex++)
                {
                    ReferencesManager.Instance.countryManager.countries[i].countryUnits.RemoveAll(item => item == null);
                    units.Add(ReferencesManager.Instance.countryManager.countries[i].countryUnits[unitIndex]);
                }

                foreach (UnitMovement division in ReferencesManager.Instance.countryManager.countries[i].countryUnits)
                {
                    foreach (UnitMovement.UnitHealth batalion in division.unitsHealth)
                    {
                        ReferencesManager.Instance.countryManager.countries[i].armyPersonnel += batalion.unit.recrootsCost;
                    }
                }

                ReferencesManager.Instance.countryManager.countries[i].manpower =
                    ReferencesManager.Instance.countryManager.countries[i].recroots +
                    ReferencesManager.Instance.countryManager.countries[i].armyPersonnel;

                if (ReferencesManager.Instance.countryManager.countries[i].mobilasing == true)
                {
                    if (ReferencesManager.Instance.countryManager.countries[i].manpower >=
                    ReferencesManager.Instance.countryManager.countries[i].recruitsLimit)
                    {
                        ReferencesManager.Instance.countryManager.countries[i].recrootsIncome = 0;
                        ReferencesManager.Instance.countryManager.countries[i].mobilasing = false;
                    }
                }
                else
                {
                    if (ReferencesManager.Instance.countryManager.countries[i].manpower <=
                    ReferencesManager.Instance.countryManager.countries[i].recruitsLimit)
                    {
                        ReferencesManager.Instance.countryManager.countries[i].recrootsIncome = 0;
                        ReferencesManager.Instance.countryManager.countries[i].deMobilasing = false;
                    }
                }
            }
        }
        catch (System.Exception) { }

        bool hasFuel = false;

        for (int i = 0; i < units.Count; i++)
        {
            if (!units[i].currentCountry.exist || units[i].currentCountry.myRegions.Count <= 0)
            {
                Destroy(units[i].gameObject);
            }
            else
            {
                units[i].currentProvince = units[i].transform.parent.GetComponent<RegionManager>();

                int motorized = 0;
                units[i]._movePoints = 1;
                units[i].firstMove = true;

                for (int unitIndex = 0; unitIndex < units[i].unitsHealth.Count; unitIndex++)
                {
                    UnitMovement.UnitHealth unit = units[i].unitsHealth[unitIndex];

                    float fuelToFill = unit.unit.maxFuel - unit.fuel;

                    if (units[i].currentCountry.fuel > fuelToFill)
                    {
                        units[i].currentCountry.fuel -= fuelToFill;

                        unit.fuel += fuelToFill;
                    }

                    if (unit.fuel >= 50 || unit.unit.maxFuel <= 0)
                    {
                        hasFuel = true;
                    }

                    if (unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED ||
                        unit.unit.type == UnitScriptableObject.Type.TANK ||
                        unit.unit.type == UnitScriptableObject.Type.CAVALRY)
                    {
                        motorized++;
                    }
                }

                if (motorized >= 6 && hasFuel)
                {
                    units[i]._movePoints = 2;
                }

                units[i].UpdateInfo();
            }

            //if (ReferencesManager.Instance.diplomatyUI.
            //    FindCountriesRelation(units[i].currentProvince.currentCountry, units[i].currentCountry).war)
            //{
            //    ReferencesManager.Instance.AnnexRegion(units[i].currentProvince, units[i].currentCountry);
            //}
        }
    }

    private void UpdateTechnologiesData()
    {
        if (ReferencesManager.Instance.technologyManager.currentTech != null &&
    ReferencesManager.Instance.technologyManager.currentTech.tech != null)
        {
            if (ReferencesManager.Instance.technologyManager.currentTech != null && ReferencesManager.Instance.technologyManager.currentTech.tech != null)
            {
                ReferencesManager.Instance.technologyManager.currentTech.moves--;
                ReferencesManager.Instance.technologyManager.SetResearchState(true);
            }

            if (ReferencesManager.Instance.technologyManager.currentTech.moves <= 0)
            {
                WarningManager.Instance.Warn($"{ReferencesManager.Instance.languageManager.GetTranslation("Warn.technology")}: {ReferencesManager.Instance.languageManager.GetTranslation(ReferencesManager.Instance.technologyManager.currentTech.tech._name)}");

                ReferencesManager.Instance.countryManager.currentCountry.BONUS_INCOME_FUEL += ReferencesManager.Instance.technologyManager.currentTech.tech.oilBonus;
                ReferencesManager.Instance.technologyManager.SetResearchState(false);

                ReferencesManager.Instance.countryManager.currentCountry.countryTechnologies.Add(ReferencesManager.Instance.technologyManager.currentTech.tech);
                ReferencesManager.Instance.technologyManager.currentTech = null;
            }
        }
    }

    private void UpdateCountriesEconomy()
    {
        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            ReferencesManager.Instance.countryManager.countries[i].inflationDebuff = ReferencesManager.Instance.countryManager.countries[i].moneyIncomeUI / 100 * (int)ReferencesManager.Instance.countryManager.countries[i].inflation;

            if (ReferencesManager.Instance.countryManager.countries[i].moneyIncomeUI > 0)
            {
                if (ReferencesManager.Instance.countryManager.countries[i].money / ReferencesManager.Instance.countryManager.countries[i].moneyIncomeUI >= 20)
                {
                    ReferencesManager.Instance.countryManager.countries[i].inflationDebuff = ReferencesManager.Instance.countryManager.countries[i].moneyIncomeUI / 25 * (int)ReferencesManager.Instance.countryManager.countries[i].inflation;
                }
            }
            else
            {
                ReferencesManager.Instance.countryManager.countries[i].inflationDebuff = ReferencesManager.Instance.countryManager.countries[i].moneyIncomeUI / 100 * (int)ReferencesManager.Instance.countryManager.countries[i].inflation;
            }
        }

        ReferencesManager.Instance.countryInfo.currentCreditMoves--;
        ReferencesManager.Instance.countryInfo.CheckCredit();
        ReferencesManager.Instance.countryManager.currentCountry.money -= ReferencesManager.Instance.countryInfo.currentCreditIncome;

        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            CountrySettings country = ReferencesManager.Instance.countryManager.countries[i];

            country.inflationDebuff = Mathf.Abs(country.inflationDebuff);

            int goldIncome = country.civFactories * ReferencesManager.Instance.gameSettings.fabric.goldIncome + country.farms * ReferencesManager.Instance.gameSettings.farm.goldIncome + country.chemicalFarms * ReferencesManager.Instance.gameSettings.chefarm.goldIncome;
            int foodIncome = country.farms * ReferencesManager.Instance.gameSettings.farm.foodIncome + country.chemicalFarms * ReferencesManager.Instance.gameSettings.chefarm.foodIncome;

            country.moneyNaturalIncome = goldIncome;

            int marketExpenses = ReferencesManager.Instance.resourcesMarketManager.CountAllCustomerExpenses(country.country);
            int marketIncome = ReferencesManager.Instance.resourcesMarketManager.CountAllSellerIncome(country.country);

            int totalMarketIncome = marketIncome - marketExpenses;

            int market_OilSelled = ReferencesManager.Instance.resourcesMarketManager.CountAllSellResources(country.country, GameSettings.Resource.Oil);
            int market_OilIncome = ReferencesManager.Instance.resourcesMarketManager.CountAllCustomerResourceGains(country.country, GameSettings.Resource.Oil);

            int total_market_OilIncome = market_OilIncome - market_OilSelled;

            country.researchPointsIncome = country.researchLabs;

            if (country.isPlayer)
            {
                country.moneyIncomeUI = country.moneyNaturalIncome + country.moneyTradeIncome - Mathf.FloorToInt(country.inflationDebuff) - country.regionCosts + totalMarketIncome;
                country.moneyIncomeUI += (country.moneyIncomeUI / 100 * difficulty_PLAYER_BUFF);
            }
            else
            {
                country.moneyIncomeUI = country.moneyNaturalIncome + country.moneyTradeIncome - Mathf.FloorToInt(country.inflationDebuff) - country.regionCosts + totalMarketIncome;
                country.moneyIncomeUI += country.moneyIncomeUI / 100 * difficulty_AI_BUFF;
            }

            country.foodNaturalIncome = foodIncome;
            country.foodIncomeUI = country.foodNaturalIncome + country.foodTradeIncome;

            country.money += country.moneyIncomeUI;
            country.food += country.foodIncomeUI;
            country.recroots += country.recrootsIncome;
            country.researchPoints += country.researchPointsIncome;

            country.oil = 0;

            foreach (RegionManager province in country.myRegions)
            {
                country.oil += province.OilAmount;
            }

            int countryOil = country.oil + total_market_OilIncome;

            country.oil = countryOil;

            country.fuelIncome = country.oil * ReferencesManager.Instance.gameSettings.fuelPerOil;

            float fuelIncome_Bonus = 0;

            if (country.BONUS_INCOME_FUEL > 0)
            {
                fuelIncome_Bonus = country.fuelIncome *
                    ((country.BONUS_INCOME_FUEL + 100) / 100);
            }

            country.fuelIncome += fuelIncome_Bonus;
            country.fuel += country.fuelIncome;
        }

        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
        ReferencesManager.Instance.countryManager.UpdateValuesUI();
    }

    private void UpdateTradesInfo()
    {
        foreach (TradeBuff tradeBuff in ReferencesManager.Instance.gameSettings.diplomatyUI.globalTrades)
        {
            if (tradeBuff.sender.moneyNaturalIncome > 0 && tradeBuff.sender.foodNaturalIncome > 0)
            {
                tradeBuff.senderMoneyTrade = Mathf.Abs(tradeBuff.sender.moneyNaturalIncome / 100 * 2);
                tradeBuff.senderFoodTrade = Mathf.Abs(tradeBuff.sender.foodNaturalIncome / 100 * 2);
            }

            if (tradeBuff.receiver.moneyNaturalIncome > 0 && tradeBuff.receiver.foodNaturalIncome > 0)
            {
                tradeBuff.receiverMoneyTrade = Mathf.Abs(tradeBuff.receiver.moneyNaturalIncome / 100 * 2);
                tradeBuff.receiverFoodTradee = Mathf.Abs(tradeBuff.receiver.foodNaturalIncome / 100 * 2);
            }

            tradeBuff.sender.moneyTradeIncome = tradeBuff.receiverMoneyTrade;
            tradeBuff.sender.foodTradeIncome = tradeBuff.receiverFoodTradee;

            tradeBuff.receiver.moneyTradeIncome = tradeBuff.senderFoodTrade;
            tradeBuff.receiver.foodTradeIncome = tradeBuff.senderFoodTrade;
        }

    }

    private void UpdateRegionsInfo()
    {
        foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
        {
            for (int _i = 0; _i < region.buildingsQueue.Count; _i++)
            {
                int buildSpeed = 1;

                if (region.infrastructure_Amount < 4) buildSpeed = 1;
                else if (region.infrastructure_Amount == 4) buildSpeed = 2;
                else if (region.infrastructure_Amount == 6) buildSpeed = 3;
                else if (region.infrastructure_Amount == 8) buildSpeed = 4;
                else if (region.infrastructure_Amount == 10) buildSpeed = 5;

                if (region.currentCountry.isPlayer) buildSpeed += buildSpeed / 100 * difficulty_PLAYER_BUFF;
                else buildSpeed += buildSpeed / 100 * difficulty_AI_BUFF;

                region.buildingsQueue[_i].movesLasts -= buildSpeed;
            }

            region.CheckRegionUnits(region);
            region.CheckBuildedBuildignsUI(region);

            CheckQueue(region);

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

    public void NextProgress()
    {
        DateProgress();

        UpdateRegionsInfo();

        UpdateUnitsData();

        UpdateAviationData();

        ReferencesManager.Instance.regionManager.DeselectRegions();

        if (!ReferencesManager.Instance.gameSettings.spectatorMode)
        {
            ReferencesManager.Instance.countryManager.currentCountry.UpdateCapitulation();
        }

        UpdateTechnologiesData();



        ReferencesManager.Instance.aiManager.OnNextMove();
        progressIndex++;

        UpdateTradesInfo();

        UpdateCountriesEconomy();

        List<RegionInfoCanvas> regionInfoCanvases = FindObjectsOfType<RegionInfoCanvas>().ToList();

        for (int i = 0; i < regionInfoCanvases.Count; i++)
        {
            ReferencesManager.Instance.mainCamera.regionInfos.Remove(regionInfoCanvases[i]);
            Destroy(regionInfoCanvases[i].gameObject);
        }

        CheckAutoSave();
    }

    private void CheckAutoSave()
    {
        if (progressIndex % ReferencesManager.Instance.settings._autosaveMovesSlider.value == 0)
        {
            StartCoroutine(AutoSave_Co());
        }
    }

    private IEnumerator AutoSave_Co()
    {
        ReferencesManager.Instance.regionUI._autoSaveMessage.SetActive(true);

        yield return new WaitForSeconds(0.3f);
        ReferencesManager.Instance.saveManager.Save();

        ReferencesManager.Instance.regionUI._autoSaveMessage.SetActive(false);

        yield break;
    }

    private void RPC_SetReady(string playerNickName)
    {
        foreach (var player in ReferencesManager.Instance.gameSettings.multiplayer.roomPlayers)
        {
            if (player.currentNickname == playerNickName)
            {
                if (player.readyToMove)
                {
                    player.readyToMove = false;
                    multiplayer.m_ReadyPlayers--;
                }
                else
                {
                    player.readyToMove = true;
                    multiplayer.m_ReadyPlayers++;
                }
            }
        }
        ReferencesManager.Instance.gameSettings.multiplayer.UpdatePlayerListUI();
    }

    private void CheckQueue(RegionManager region)
    {
        for (int _i = 0; _i < region.buildingsQueue.Count; ++_i)
        {
            if (region.buildingsQueue[_i].movesLasts <= 0)
            {
                region.BuildBuilding(region.buildingsQueue[_i].building, region.buildingsQueue[_i].region, true);
                region.buildingsQueue.Remove(region.buildingsQueue[_i]);

                Multiplayer.Instance.SetRegionValues(region._id, region.population, region.hasArmy, region.goldIncome, region.foodIncome,
                    region.civFactory_Amount, region.infrastructure_Amount, region.farms_Amount, region.cheFarms, region.regionScore);
            }
        }
    }

    private void UpdateProduction(CountrySettings country)
    {
        for (int i = 0; i < country._prodQueue.Count; i++)
        {
            ProductionManager.ProductionQueue productionItemQueue = country._prodQueue[i];

            productionItemQueue._currentProgress += productionItemQueue._currentFactories * ReferencesManager.Instance.gameSettings.dockyardProduction;

            if (productionItemQueue._currentProgress >= productionItemQueue._equipment._productionCost)
            {
                WarningManager.Instance.Warn($"{ReferencesManager.Instance.languageManager.GetTranslation("Warn.Produced")}: {productionItemQueue._equipment._name}");

                productionItemQueue._owner._fleet.Add(productionItemQueue._equipment);
                country._prodQueue.Remove(productionItemQueue);
            }


            int factoriesNeeded = productionItemQueue._equipment._maxFactories - productionItemQueue._currentFactories;
            int freeDockyards = ReferencesManager.Instance.productionManager.GetFreeDockyards(country);

            int k = factoriesNeeded - freeDockyards;

            if (k < productionItemQueue._equipment._maxFactories && k > 0)
            {
                productionItemQueue._currentFactories += freeDockyards;
            }
            else if (k >= productionItemQueue._equipment._maxFactories && freeDockyards > 0)
            {
                productionItemQueue._currentFactories = productionItemQueue._equipment._maxFactories;
            }
        }
    }

    private void UpdateResources()
    {
        foreach (CountrySettings country in ReferencesManager.Instance.countryManager.countries)
        {
            foreach (RegionManager province in country.myRegions)
            {
                country.oil += province.OilAmount;
            }
        }
    }

    private void UpdateSellers()
    {
        foreach (CountrySettings country in ReferencesManager.Instance.countryManager.countries)
        {
            if (country.country._id != ReferencesManager.Instance.countryManager.currentCountry.country._id)
            {
                if (country.oil > 0)
                {
                    ResourcesMarketManager.SellerData sellerData = new ResourcesMarketManager.SellerData();
                    sellerData._seller = country.country;
                    sellerData._resource = GameSettings.Resource.Oil;
                    sellerData._currentResAmount = country.oil;
                    sellerData._maxResAmount = country.oil;
                    sellerData._cost = Random.Range(50, 250);

                    ReferencesManager.Instance.resourcesMarketManager._sellers.Add(sellerData);
                }
            }
        }

        ReferencesManager.Instance.resourcesMarketManager._sellers = ReferencesManager.Instance.resourcesMarketManager._sellers.OrderByDescending(item => item._currentResAmount).ToList();

    }
}
