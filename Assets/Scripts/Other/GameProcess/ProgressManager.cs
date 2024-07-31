using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using static ResourcesMarketManager;
using System.Security.Cryptography;

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
                        if (plane.Owner.fuel > 0)
                        {
                            if (plane.Owner.fuel >= fuelToFill)
                            {
                                plane.Owner.fuel -= fuelToFill;
                                plane.fuel += fuelToFill;
                            }
                            else
                            {
                                float fuelCanFill = plane.Owner.fuel;

                                plane.Owner.fuel -= fuelCanFill;
                                plane.fuel += fuelCanFill;
                            }
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

                for (int unitIndex = 0; unitIndex < ReferencesManager.Instance.countryManager.countries[i].countryUnits.Count; unitIndex++)
                {
                    ReferencesManager.Instance.countryManager.countries[i].countryUnits.RemoveAll(item => item == null);
                    units.Add(ReferencesManager.Instance.countryManager.countries[i].countryUnits[unitIndex]);
                }

                foreach (UnitMovement division in ReferencesManager.Instance.countryManager.countries[i].countryUnits)
                {
                    foreach (UnitHealth batalion in division.unitsHealth)
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
                    UnitHealth unit = units[i].unitsHealth[unitIndex];

                    float fuelToFill = unit.unit.maxFuel - unit.fuel;

                    if (units[i].currentCountry.fuel >= fuelToFill)
                    {
                        units[i].currentCountry.fuel -= fuelToFill;

                        unit.fuel += fuelToFill;
                    }
                    else
                    {
                        float fuelCanFill = units[i].currentCountry.fuel;

                        units[i].currentCountry.fuel -= fuelCanFill;
                        unit.fuel += fuelCanFill;
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
        }
    }

    private void UpdateTechnologiesData()
    {
        TechnologyManager technologyManager = ReferencesManager.Instance.technologyManager;
        CountryManager countryManager = ReferencesManager.Instance.countryManager;
        Interpretate languageManager = ReferencesManager.Instance.languageManager;

        if (technologyManager.currentTech != null &&
    technologyManager.currentTech.tech != null)
        {
            if (technologyManager.currentTech != null && technologyManager.currentTech.tech != null)
            {
                technologyManager.currentTech.moves--;
                technologyManager.SetResearchState(true);
            }

            if (technologyManager.currentTech.moves <= 0)
            {
                WarningManager.Instance.Warn($"{languageManager.GetTranslation("Warn.technology")}: {languageManager.GetTranslation(technologyManager.currentTech.tech._name)}");

                countryManager.currentCountry.BONUS_INCOME_FUEL += technologyManager.currentTech.tech.oilBonus;
                technologyManager.SetResearchState(false);

                countryManager.currentCountry.countryTechnologies.Add(technologyManager.currentTech.tech);
                technologyManager.currentTech = null;
            }
        }
    }

    private void UpdateCountriesEconomy()
    {
        CountryManager countryManager = ReferencesManager.Instance.countryManager;
        GameSettings gameSettings = ReferencesManager.Instance.gameSettings;
        ResourcesMarketManager resourcesMarket = ReferencesManager.Instance.resourcesMarketManager;

        for (int i = 0; i < countryManager.countries.Count; i++)
        {
            countryManager.countries[i].inflationDebuff = countryManager.countries[i].moneyIncomeUI / 100 * (int)countryManager.countries[i].inflation;

            if (countryManager.countries[i].moneyIncomeUI > 0)
            {
                if (countryManager.countries[i].money / countryManager.countries[i].moneyIncomeUI >= 20)
                {
                    countryManager.countries[i].inflationDebuff = countryManager.countries[i].moneyIncomeUI / 25 * (int)countryManager.countries[i].inflation;
                }
            }
            else
            {
                countryManager.countries[i].inflationDebuff = countryManager.countries[i].moneyIncomeUI / 100 * (int)countryManager.countries[i].inflation;
            }
        }

        for (int i = 0; i < countryManager.countries.Count; i++)
        {
            CountrySettings country = countryManager.countries[i];

            country.inflationDebuff = Mathf.Abs(country.inflationDebuff);

            int goldIncome = country.civFactories * gameSettings.fabric.goldIncome + country.farms * gameSettings.farm.goldIncome + country.chemicalFarms * gameSettings.chefarm.goldIncome;
            int foodIncome = country.farms * gameSettings.farm.foodIncome + country.chemicalFarms * gameSettings.chefarm.foodIncome;

            country.moneyNaturalIncome = goldIncome;

            int marketExpenses = resourcesMarket.CountAllCustomerExpenses(country.country);
            int marketIncome = resourcesMarket.CountAllSellerIncome(country.country);

            int totalMarketIncome = marketIncome - marketExpenses;

            int market_OilSelled = resourcesMarket.CountAllSellResources(country.country, GameSettings.Resource.Oil);
            int market_OilIncome = resourcesMarket.CountAllCustomerResourceGains(country.country, GameSettings.Resource.Oil);

            int total_market_OilIncome = market_OilIncome - market_OilSelled;

            country.researchPointsIncome = country.researchLabs;

            country.foodNaturalIncome = foodIncome;
            country.foodIncomeUI = country.foodNaturalIncome + country.foodTradeIncome;

            country.oil = 0;

            foreach (RegionManager province in country.myRegions)
            {
                country.oil += province.OilAmount;
            }

            int countryOil = country.oil + total_market_OilIncome;

            country.oil = countryOil;

            country.fuelIncome = country.oil * gameSettings.fuelPerOil;

            float fuelIncome_Bonus = 0;

            if (country.BONUS_INCOME_FUEL > 0)
            {
                fuelIncome_Bonus = country.fuelIncome *
                    ((country.BONUS_INCOME_FUEL + 100) / 100);
            }

            country.fuelIncome += fuelIncome_Bonus;
            country.fuel += country.fuelIncome;

            if (country.isPlayer)
            {
                country.moneyIncomeUI = country.moneyNaturalIncome + country.moneyTradeIncome - Mathf.FloorToInt(country.inflationDebuff) - country.regionCosts + totalMarketIncome;
                country.moneyIncomeUI += (country.moneyIncomeUI / 100 * difficulty_PLAYER_BUFF);

                countryManager.UpdateIncomeValuesUI();
            }
            else
            {
                country.moneyIncomeUI = country.moneyNaturalIncome + country.moneyTradeIncome - Mathf.FloorToInt(country.inflationDebuff) - country.regionCosts + totalMarketIncome;
                country.moneyIncomeUI += country.moneyIncomeUI / 100 * difficulty_AI_BUFF;
            }

            country.money += country.moneyIncomeUI;
            country.food += country.foodIncomeUI;
            country.recroots += country.recrootsIncome;
            country.researchPoints += country.researchPointsIncome;
        }
    }

    private void UpdateTradesInfo()
    {
        DiplomatyUI diplomaty = ReferencesManager.Instance.diplomatyUI;

        foreach (TradeBuff tradeBuff in diplomaty.globalTrades)
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
        CountryManager countryManager = ReferencesManager.Instance.countryManager;

        foreach (RegionManager region in countryManager.regions)
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

        UpdateSellers();

        UpdateTradesInfo();

        UpdateCountriesEconomy();

        ReferencesManager.Instance.aiManager.OnNextMove();
        progressIndex++;

        List<RegionInfoCanvas> regionInfoCanvases = FindObjectsOfType<RegionInfoCanvas>().ToList();

        for (int i = 0; i < regionInfoCanvases.Count; i++)
        {
            ReferencesManager.Instance.mainCamera.regionInfos.Remove(regionInfoCanvases[i]);
            Destroy(regionInfoCanvases[i].gameObject);
        }

        CheckAutoSave();

        ReferencesManager.Instance.dateManager.CheckGameEvents();
        ReferencesManager.Instance.countryManager.UpdateValuesUI();
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

    //private void RPC_SetReady(string playerNickName)
    //{
    //    foreach (var player in ReferencesManager.Instance.gameSettings.multiplayer.roomPlayers)
    //    {
    //        if (player.currentNickname == playerNickName)
    //        {
    //            if (player.readyToMove)
    //            {
    //                player.readyToMove = false;
    //                multiplayer.m_ReadyPlayers--;
    //            }
    //            else
    //            {
    //                player.readyToMove = true;
    //                multiplayer.m_ReadyPlayers++;
    //            }
    //        }
    //    }
    //    ReferencesManager.Instance.gameSettings.multiplayer.UpdatePlayerListUI();
    //}

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
        var market = ReferencesManager.Instance.resourcesMarketManager;

        foreach (CountrySettings country in ReferencesManager.Instance.countryManager.countries)
        {
            country.oil = 0;

            foreach (RegionManager province in country.myRegions)
            {
                country.oil += province.OilAmount;
            }

            if (country.country._id != ReferencesManager.Instance.countryManager.currentCountry.country._id)
            {
                bool alreadyInMarket = market._sellers.Any(item => item._seller._id == country.country._id);

                for (int i = 0; i < market._sellers.Count; i++)
                {
                    SellerData sellerData = market._sellers[i];

                    if (sellerData._seller._id == country.country._id)
                    {
                        if (!country.exist || country.capitulated || country.myRegions.Count <= 0)
                        {
                            market._sellers.Remove(sellerData);
                            break;
                        }

                        sellerData._maxResAmount = country.oil;
                    }
                }

                if (!alreadyInMarket)
                {
                    if (country.oil > 0)
                    {
                        SellerData sellerData = new SellerData();
                        sellerData._seller = country.country;
                        sellerData._resource = GameSettings.Resource.Oil;
                        sellerData._currentResAmount = country.oil;
                        sellerData._maxResAmount = country.oil;
                        sellerData._cost = Random.Range(50, 250);

                        market._sellers.Add(sellerData);
                    }
                }
            }
        }

        ReferencesManager.Instance.resourcesMarketManager._sellers = ReferencesManager.Instance.resourcesMarketManager._sellers.OrderByDescending(item => item._maxResAmount).ToList();
    }
}
