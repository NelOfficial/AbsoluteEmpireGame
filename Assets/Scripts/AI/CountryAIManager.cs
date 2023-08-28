using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CountryAIManager : MonoBehaviour
{
    private UnitMovement attackerUnit;
    private UnitMovement defenderUnit;

    private RegionManager attackerRegion;
    private RegionManager defenderRegion;

    private MovePoint p;
    private RegionManager enemyRegion;

    public TechQueue currentTech;
    public bool researching;

    public int moneySaving;
    public int warExpenses;
    public int civilExpenses;
    public int buildingExpenses;
    public int researchingExpenses;

    public void Process(CountrySettings country)
    {
        int myRegionsBorderingWithEnemy_Count = 0;
        int myRegionsBorderingWithEnemyWithDivisions_Count = 0;

        country.UpdateCapitulation();

        moneySaving = country.money / 100 * 5; // 5%

        warExpenses = country.money / 100 * 25; // 25% in peaceful time
        civilExpenses = country.money / 100 * 70; // 70% in peaceful time
        buildingExpenses = civilExpenses / 100 * 50; // 50% for building

        if (ReferencesManager.Instance.gameSettings.developerMode) Debug.Log($"{country.country._name} Деньги: {country.money} Сберегаем {moneySaving} Военные расходы: {warExpenses} Гражданские расходы: {civilExpenses} Расходы на постройки: {buildingExpenses} Расходы на исследования: {researchingExpenses}");

        if (country.inWar)
        {
            warExpenses = country.money / 100 * 70; // 60% in war time
            civilExpenses = country.money / 100 * 30; // 20% in war time
            country.money += moneySaving;
        }

        //foreach (CountrySettings countryOther in ReferencesManager.Instance.countryManager.countries)
        //{
        //    if (countryOther.ideology == "Фашизм")
        //    {
        //        ReferencesManager.Instance.diplomatyUI.
        //            FindCountriesRelation(country, countryOther).relationship = -30;
        //        ReferencesManager.Instance.diplomatyUI.
        //            FindCountriesRelation(countryOther, country).relationship = -30;
        //    }
        //}

        List<RegionManager> dangerousBorderingRegions = new List<RegionManager>();

        foreach (RegionManager region in country.myRegions)
        {
            foreach (CountrySettings _country in GetBorderingCountiesWithRegion(region))
            {
                if (ReferencesManager.Instance.diplomatyUI.
                    FindCountriesRelation(country, _country).relationship < 0) // If bordering country not friendly to us
                {
                    if (!dangerousBorderingRegions.Contains(region))
                    {
                        dangerousBorderingRegions.Add(region);
                    }
                }
            }
        }

        if (country.countryUnits.Count > 0 && country.countryUnits.Count <= dangerousBorderingRegions.Count)
        {
            foreach (RegionManager _region in dangerousBorderingRegions)
            {
                if (!_region.hasArmy)
                {
                    CreateDivisionInRegion(_region, country);
                }
            }
        }
        else if (country.countryUnits.Count <= 0)
        {
            foreach (RegionManager _region in dangerousBorderingRegions)
            {
                if (!_region.hasArmy)
                {
                    CreateDivisionInRegion(_region, country);
                }
            }
        }
        //else if (country.countryUnits.Count > 0 && country.countryUnits.Count > dangerousBorderingRegions.Count)
        //{
        //    for (int unitIndex = 0; unitIndex < country.countryUnits.Count; unitIndex++)
        //    {
        //        foreach (RegionManager _region in dangerousBorderingRegions)
        //        {
        //            if (!isDivisionInRegion(country.countryUnits[unitIndex], _region) && !_region.hasArmy)
        //            {
        //                country.countryUnits[unitIndex].AIMoveNoHit(GetRoute(_region, country.countryUnits[unitIndex]), country.countryUnits[unitIndex].currentProvince);
        //            }
        //        }
        //    }
        //}

        List<RegionManager> warBorderingRegions = new List<RegionManager>();

        foreach (RegionManager region in country.myRegions)
        {
            foreach (CountrySettings _country in GetBorderingCountiesWithRegion(region))
            {
                if (ReferencesManager.Instance.diplomatyUI.
                    FindCountriesRelation(country, _country).war) // If bordering country at war with us
                {
                    if (!warBorderingRegions.Contains(region))
                    {
                        warBorderingRegions.Add(region);
                    }
                }
            }
        }

        if (warBorderingRegions.Count > 0)
        {
            for (int i = 0; i < warBorderingRegions.Count; i++)
            {
                RegionManager enemyRegion = GetEnemyRegionFromMy(warBorderingRegions[i]);

                if (warBorderingRegions[i].hasArmy)
                {
                    float _winChance = CountWinChance(warBorderingRegions[i], enemyRegion);
                    float currentAttackPreference = 0;

                    if (country.aiAccuracy == 0)
                    {
                        currentAttackPreference = 100;
                    }

                    if (_winChance < 50)
                    {
                        currentAttackPreference = _winChance / country.aiAccuracy;
                    }
                    else if (_winChance >= 50)
                    {
                        currentAttackPreference = _winChance * country.aiAccuracy;
                    }

                    if (currentAttackPreference >= 50)
                    {

                        //Debug.Log($"{country.country._name} хочет атаковать в {enemyRegion.name} ({enemyRegion.currentCountry.country._name}) с шансом {currentAttackPreference} ({_winChance})");
                        foreach (Transform child in warBorderingRegions[i].transform)
                        {
                            if (child.GetComponent<UnitMovement>())
                            {
                                UnitMovement division = child.GetComponent<UnitMovement>();
                                if (division._movePoints > 0)
                                {
                                    division.AIMoveNoHit(enemyRegion, warBorderingRegions[i]);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (country.countryUnits.Count < warBorderingRegions.Count)
                    {
                        CreateDivisionInRegion(warBorderingRegions[i], country);
                    }
                    if (country.countryUnits.Count >= warBorderingRegions.Count)
                    {
                        for (int unitIndex = 0; unitIndex < country.countryUnits.Count; unitIndex++)
                        {
                            if (!isDivisionFrontlineWithEnemy(country.countryUnits[unitIndex]))
                            {
                                country.countryUnits[unitIndex].AIMoveNoHit(GetRoute(warBorderingRegions[i], country.countryUnits[unitIndex]), country.countryUnits[unitIndex].currentProvince);
                            }
                        }
                    }
                }
            }
        }

        #region Buildings

        for (int k = 0; k < country.myRegions.Count; k++)
        {
            RegionManager region = country.myRegions[k];
            if (buildingExpenses >= 0)
            {
                if (country.money >= ReferencesManager.Instance.gameSettings.fabric.goldCost &&
                    buildingExpenses >= ReferencesManager.Instance.gameSettings.fabric.goldCost)
                {
                    if (region.buildings.Count + region.buildingsQueue.Count < 4)
                    {
                        region.AddBuildingToQueueForce(ReferencesManager.Instance.gameSettings.fabric, region);
                        buildingExpenses -= ReferencesManager.Instance.gameSettings.fabric.goldCost;
                    }
                    else
                    {
                        UpgradeInfrastructure(region);
                    }
                }
                if (country.money >= ReferencesManager.Instance.gameSettings.farm.goldCost &&
                    buildingExpenses >= ReferencesManager.Instance.gameSettings.farm.goldCost &&
                    country.foodNaturalIncome <= 0 || country.foodIncomeUI <= 0)
                {
                    if (region.buildings.Count + region.buildingsQueue.Count < 4)
                    {
                        region.AddBuildingToQueueForce(ReferencesManager.Instance.gameSettings.farm, region);
                        buildingExpenses -= ReferencesManager.Instance.gameSettings.farm.goldCost;
                    }
                }
                if (country.money >= ReferencesManager.Instance.gameSettings.chefarm.goldCost &&
                    buildingExpenses >= ReferencesManager.Instance.gameSettings.chefarm.goldCost &&
                    country.foodNaturalIncome <= 0 || country.foodIncomeUI <= 0)
                {
                    if (region.buildings.Count + region.buildingsQueue.Count < 4)
                    {
                        region.AddBuildingToQueueForce(ReferencesManager.Instance.gameSettings.chefarm, region);
                        buildingExpenses -= ReferencesManager.Instance.gameSettings.chefarm.goldCost;
                    }
                }

                if (country.foodIncomeUI < 0)
                {
                    if (country.money >= ReferencesManager.Instance.gameSettings.chefarm.goldCost)
                    {
                        region.buildings.Remove(ReferencesManager.Instance.gameSettings.fabric);

                        region.AddBuildingToQueueForce(ReferencesManager.Instance.gameSettings.chefarm, region);
                        buildingExpenses -= ReferencesManager.Instance.gameSettings.chefarm.goldCost;
                    }
                }
                if (country.moneyIncomeUI < 0)
                {
                    if (region.buildings.Contains(ReferencesManager.Instance.gameSettings.farm))
                    {
                        region.buildings.Remove(ReferencesManager.Instance.gameSettings.farm);
                    }
                    if (region.buildings.Contains(ReferencesManager.Instance.gameSettings.chefarm))
                    {
                        region.buildings.Remove(ReferencesManager.Instance.gameSettings.chefarm);
                    }

                    region.AddBuildingToQueueForce(ReferencesManager.Instance.gameSettings.fabric, region);
                    buildingExpenses -= ReferencesManager.Instance.gameSettings.farm.goldCost;
                }
            }
        }

        if (country.moneyIncomeUI < 150)
        {
            if (country.money < 400)
            {
                country.money += 1050;
            }
        }

        #endregion

        researchingExpenses = country.money / 100 * 90; // 90% of all country budget

        #region Research Technologies

        for (int i = 0; i < ReferencesManager.Instance.gameSettings.technologies.Length; i++)
        {
            if (CanResearch(country, ReferencesManager.Instance.gameSettings.technologies[i]))
            {
                while (!researching)
                {
                    if (!researching)
                    {
                        StartRecearch(country, ReferencesManager.Instance.gameSettings.technologies[i]);
                    }
                }
            }
        }

        #endregion

        #region SetMobilizationLaw

        if (country.recroots <= 2100)
        {
            ReferencesManager.Instance.SetRecroots(country.mobilizationLaw, country);
        }

        #endregion

        foreach (CountrySettings countryOther in ReferencesManager.Instance.countryManager.countries)
        {
            if (countryOther.country._id != country.country._id)
            {
                if (country.myRegions.Count < 1 || country.capitulated)
                {
                    country.exist = false;
                }

                countryOther.UpdateCapitulation();

                //if (country.countryUnits.Count > 0 && country.countryUnits.Count >= dangerousBorderingRegions.Count)
                //{
                //    foreach (UnitMovement division in country.countryUnits)
                //    {
                //        for (int i = 0; i < dangerousBorderingRegions.Count;)
                //        {
                //            if (!dangerousBorderingRegions[i].hasArmy)
                //            {
                //                dangerousBorderingRegions[i].GetComponent<SpriteRenderer>().color = Color.green;
                //                if (division._movePoints > 0)
                //                {
                //                    if (!isDivisionInRegion(division, dangerousBorderingRegions[i]))
                //                    {
                //                        List<CountrySettings> borderingCountries = GetBorderingCountiesWithRegion(division.currentProvince);
                //                        borderingCountries.Distinct();


                //                        if (borderingCountries.Count == 1 && borderingCountries.Contains(division.currentCountry))
                //                        {
                //                            for (int b = 0; b < borderingCountries.Count; b++)
                //                            {
                //                                Debug.Log($"{borderingCountries[b].country._nameEN}");
                //                            }
                //                            division.currentProvince.GetComponent<SpriteRenderer>().color = Color.yellow;
                //                            //division.AIMoveNoHit(GetRoute(dangerousBorderingRegions[i], division), division.currentProvince);
                //                        }
                //                    }
                //                    i++;
                //                }
                //                else
                //                {
                //                    i++;
                //                }
                //            }
                //            else
                //            {
                //                i++;
                //            }
                //        }
                //    }
                //}

                #region GoodRelations

                if (country.country._id != countryOther.country._id)
                {
                    if (country.exist && countryOther.exist)
                    {
                        if (!ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).trade && //Trade
                            !ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).war) //Go trade
                        {
                            int random = Random.Range(0, 100);
                            if (countryOther.ideology == country.ideology)
                            {
                                random += 10;
                            }
                            else if (countryOther.ideology != country.ideology)
                            {
                                random -= 10;
                            }

                            if (country.myRegions.Count > 0 && countryOther.myRegions.Count > 0)
                            {
                                if (country.myRegions.Count > countryOther.myRegions.Count)
                                {
                                    if (country.myRegions.Count / countryOther.myRegions.Count >= 4)
                                    {
                                        random -= 70;
                                    }
                                }
                                else
                                {
                                    if (countryOther.myRegions.Count / country.myRegions.Count >= 4)
                                    {
                                        random -= 70;
                                    }
                                }
                            }

                            if (countryOther.isPlayer)
                            {
                                random += 15;
                            }

                            if (countryOther.ideology == "Фашизм" && country.ideology != "Фашизм")
                            {
                                random -= Random.Range(60, 100);
                            }
                            if (country.ideology == "Фашизм" && countryOther.ideology != "Фашизм")
                            {
                                random -= Random.Range(60, 100);
                            }

                            if (countryOther.ideology == "Фашизм" && country.ideology == "Фашизм")
                            {
                                random += Random.Range(70, 100);
                            }

                            if (random >= 50)
                            {
                                SendOffer("Торговля", country, countryOther);
                            }
                        }

                        if (!ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).pact && //Pact
                            !ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).war &&
                            ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).relationship >= 35)
                        {
                            int random = Random.Range(0, 100);
                            if (countryOther.ideology == country.ideology)
                            {
                                random += Random.Range(0, 10);
                            }
                            else if (countryOther.ideology != country.ideology)
                            {
                                random -= Random.Range(10, 30);
                            }

                            if (countryOther.ideology == "Фашизм" && country.ideology != "Фашизм")
                            {
                                random -= Random.Range(60, 100);
                            }
                            if (country.ideology == "Фашизм" && countryOther.ideology != "Фашизм")
                            {
                                random -= Random.Range(60, 100);
                            }

                            if (countryOther.ideology == "Фашизм" && country.ideology == "Фашизм")
                            {
                                random += Random.Range(70, 100);
                            }

                            if (random > Random.Range(50, 90))
                            {
                                SendOffer("Пакт о ненападении", country, countryOther);
                            }
                        }

                        if (!ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).right && //Move right
                            !ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).war &&
                            ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).relationship >= 20)
                        {
                            int random = Random.Range(0, 100);
                            if (countryOther.ideology == country.ideology)
                            {
                                random += Random.Range(0, 10);
                            }
                            else if (countryOther.ideology != country.ideology)
                            {
                                random -= Random.Range(10, 30);
                            }

                            if (countryOther.ideology == "Фашизм" && country.ideology != "Фашизм")
                            {
                                random -= Random.Range(60, 100);
                            }
                            if (country.ideology == "Фашизм" && countryOther.ideology != "Фашизм")
                            {
                                random -= Random.Range(60, 100);
                            }

                            if (countryOther.ideology == "Фашизм" && country.ideology == "Фашизм")
                            {
                                random += Random.Range(70, 100);
                            }


                            if (random >= 50)
                            {
                                SendOffer("Право прохода войск", country, countryOther);
                            }
                        }

                        if (!ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).right && //Union
                            !ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).war &&
                            ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).relationship >= 10)
                        {
                            int random = Random.Range(0, 100);

                            if (countryOther.ideology == country.ideology)
                            {
                                random += Random.Range(0, 10);
                            }
                            else if (countryOther.ideology != country.ideology)
                            {
                                random -= Random.Range(10, 30);
                            }

                            if (countryOther.ideology == "Фашизм" && country.ideology != "Фашизм")
                            {
                                random -= Random.Range(60, 100);
                            }
                            if (country.ideology == "Фашизм" && countryOther.ideology != "Фашизм")
                            {
                                random -= Random.Range(60, 100);
                            }

                            if (countryOther.ideology == "Фашизм" && country.ideology == "Фашизм")
                            {
                                random += Random.Range(70, 100);
                            }

                            if (countryOther.ideology == "Коммунизм" && country.ideology == "Коммунизм")
                            {
                                random += Random.Range(70, 100);
                            }

                            if (random >= 70)
                            {
                                SendOffer("Союз", country, countryOther);
                            }
                        }
                    }
                }

                #endregion
            }
        }
        ReferencesManager.Instance.eventsContainer.UpdateEvents();
    }

    private List<CountrySettings> GetBorderingCountiesWithRegion(RegionManager region)
    {
        List<CountrySettings> countries = new List<CountrySettings>();

        foreach (Transform point in region.movePoints)
        {
            RegionManager _pointRegion = point.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>();

            if (_pointRegion.currentCountry != region.currentCountry) // If founded region is not my state
            {
                countries.Add(_pointRegion.currentCountry);
            }
        }

        countries.Distinct(); // Remove dupes

        return countries;
    }

    private RegionManager GetRoute(RegionManager regionTo, UnitMovement division)
    {
        List<float> foundedRoutesDistances = new List<float>();
        List<RegionManager> foundedRoutesProvinces = new List<RegionManager>();

        RegionManager regionToReturn = new RegionManager();

        for (int v = 0; v < division.currentProvince.movePoints.Count; v++)
        {
            RegionManager inDivisionPointRegion = division.currentProvince.movePoints[v].GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>();

            if (!inDivisionPointRegion.hasArmy)
            {
                float routeDistance = GetDistance(inDivisionPointRegion, regionTo);
                foundedRoutesDistances.Add(routeDistance);
                foundedRoutesProvinces.Add(inDivisionPointRegion);
            }
        }

        if (foundedRoutesDistances.Count > 0)
        {
            float minDistance = foundedRoutesDistances.Min();
            foreach (RegionManager province in foundedRoutesProvinces)
            {
                if (GetDistance(province, regionTo) == minDistance)
                {
                    if (!province.hasArmy)
                    {
                        if (division != null)
                        {
                            if (isRegionBorderSecondRegion(division.currentProvince, province))
                            {
                                regionToReturn = province;
                            }
                        }
                    }
                }
            }
        }

        return regionToReturn;
    }

    private bool isRegionBorderSecondRegion(RegionManager regionA, RegionManager regionB)
    {
        List<RegionManager> borderingRegions = new List<RegionManager>();

        for (int i = 0; i < regionA.movePoints.Count; i++)
        {
            borderingRegions.Add(regionA.movePoints[i].GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>());
        }

        bool result = borderingRegions.Contains(regionB);

        return result;
    }

    private void CreateDivisionInRegion(RegionManager region, CountrySettings country)
    {
        if (region.currentCountry == country)
        {
            UnitScriptableObject unit = ReferencesManager.Instance.gameSettings.soldierLVL1;
            UnitMovement division = new UnitMovement();

            if (warExpenses >= unit.moneyCost)
            {
                if (country.money >= unit.moneyCost && country.recroots >= unit.recrootsCost)
                {
                    GameObject spawnedUnit = Instantiate(ReferencesManager.Instance.army.unitPrefab, region.transform);
                    spawnedUnit.transform.localScale = new Vector3(ReferencesManager.Instance.army.unitPrefab.transform.localScale.x, ReferencesManager.Instance.army.unitPrefab.transform.localScale.y);
                    division = spawnedUnit.GetComponent<UnitMovement>();

                    spawnedUnit.GetComponent<UnitMovement>().currentCountry = country;
                    spawnedUnit.GetComponent<UnitMovement>().currentProvince = region;
                    spawnedUnit.GetComponent<UnitMovement>().UpdateInfo();

                    region.hasArmy = true;
                    country.countryUnits.Add(spawnedUnit.GetComponent<UnitMovement>());

                    AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL1, region, spawnedUnit.GetComponent<UnitMovement>());
                }

                CreateDivision(country, region, division);

                region.CheckRegionUnits(region);
            }
        }
    }

    private float CountWinChance(RegionManager attackFrom, RegionManager fightRegion)
    {
        float winChance = 0;

        float enemyDamage = 0;
        float myDamage = 0;

        int defenderForts = fightRegion.fortifications_Amount * 5 / 100;
        float attackerFortsDebuff = myDamage * defenderForts;

        if (fightRegion.hasArmy)
        {
            foreach (Transform child in fightRegion.transform)
            {
                if (child.GetComponent<UnitMovement>())
                {
                    UnitMovement division = child.GetComponent<UnitMovement>();
                    foreach (UnitMovement.UnitHealth unit in division.unitsHealth)
                    {
                        enemyDamage += unit.unit.damage;
                    }
                }
            }
        }
        else // Garrison fighting
        {
            foreach (UnitScriptableObject unit in fightRegion.currentDefenseUnits)
            {
                enemyDamage += unit.damage;
            }
        }

        foreach (Transform child in attackFrom.transform)
        {
            if (child.GetComponent<UnitMovement>())
            {
                UnitMovement division = child.GetComponent<UnitMovement>();
                foreach (UnitMovement.UnitHealth unit in division.unitsHealth)
                {
                    myDamage += unit.unit.damage;
                }
            }
        }

        myDamage = myDamage - attackerFortsDebuff;
        myDamage = Mathf.Abs(myDamage);

        if (myDamage < enemyDamage)
        {
            float difference = Mathf.Abs(enemyDamage) - Mathf.Abs(myDamage);
            difference = Mathf.Abs(difference * 100 / enemyDamage);

            if (difference >= 5 && difference <= 15)
            {
                winChance = Random.Range(47, 49);
            }

            else if (difference >= 15 && difference <= 20)
            {
                winChance = Random.Range(45, 47);
            }

            else if (difference >= 20 && difference <= 25)
            {
                winChance = Random.Range(43, 45);
            }

            else if (difference >= 25 && difference <= 30)
            {
                winChance = Random.Range(41, 43);
            }

            else if (difference >= 35 && difference <= 40)
            {
                winChance = Random.Range(39, 41);
            }

            else if (difference >= 45 && difference <= 50)
            {
                winChance = Random.Range(37, 39);
            }

            else if (difference >= 55 && difference <= 60)
            {
                winChance = Random.Range(35, 37);
            }

            else if (difference >= 65 && difference <= 70)
            {
                winChance = Random.Range(30, 37);
            }

            else if (difference >= 75 && difference <= 80)
            {
                winChance = Random.Range(10, 25);
            }

            else if (difference >= 85 && difference <= 100)
            {
                winChance = Random.Range(0, 15);
            }
        }
        else if (myDamage == enemyDamage)
        {
            winChance = 50;
        }
        else if (myDamage > enemyDamage)
        {
            float difference = Mathf.Abs(myDamage) - Mathf.Abs(enemyDamage);
            difference = Mathf.Abs(difference * 100 / myDamage);

            if (difference >= 5 && difference <= 10)
            {
                winChance = Random.Range(51, 53);
            }

            else if (difference > 10 && difference <= 15)
            {
                winChance = Random.Range(53, 56);
            }

            else if (difference > 15 && difference <= 20)
            {
                winChance = Random.Range(56, 60);
            }

            else if (difference > 25 && difference <= 30)
            {
                winChance = Random.Range(60, 64);
            }

            else if (difference > 30 && difference <= 40)
            {
                winChance = Random.Range(64, 68);
            }

            else if (difference > 40 && difference <= 50)
            {
                winChance = Random.Range(68, 74);
            }

            else if (difference > 50 && difference <= 60)
            {
                winChance = Random.Range(74, 78);
            }

            else if (difference > 60 && difference <= 70)
            {
                winChance = Random.Range(78, 82);
            }

            else if (difference > 70 && difference <= 80)
            {
                winChance = Random.Range(82, 86);
            }

            else if (difference > 80 && difference <= 90)
            {
                winChance = Random.Range(86, 90);
            }

            else if (difference > 90 && difference <= 100)
            {
                winChance = Random.Range(90, 92);
            }
        }

        return winChance;
    }

    private RegionManager GetEnemyRegionFromMy(RegionManager myRegion)
    {
        RegionManager enemyRegion = new RegionManager();

        foreach (Transform point in myRegion.movePoints)
        {
            if (ReferencesManager.Instance.diplomatyUI.
                FindCountriesRelation(point.GetComponent<MovePoint>().
                regionTo.GetComponent<RegionManager>().currentCountry, myRegion.currentCountry).war)
            {
                enemyRegion = point.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>();
            }
        }

        return enemyRegion;
    }

    private bool isDivisionFrontlineWithEnemy(UnitMovement division)
    {
        bool result = false;

        foreach (CountrySettings _country in GetBorderingCountiesWithRegion(division.currentProvince))
        {
            if (ReferencesManager.Instance.diplomatyUI.
                FindCountriesRelation(_country, division.currentCountry).war)
            {
                result = true;
            }
            else
            {
                result = false;
            }
        }

        return result;
    }

    private bool isDivisionFrontlineWithEnemyNOWARCHECK(UnitMovement division, CountrySettings enemy)
    {
        bool result = GetBorderingCountiesWithRegion(division.currentProvince).Contains(enemy);

        return result;
    }


    private bool isDivisionInRegion(UnitMovement division, RegionManager region)
    {
        bool result = division.currentProvince == region;

        return result;
    }

    public void UpgradeInfrastructure(RegionManager region)
    {
        int check = region.infrastructure_Amount + 1;

        if (buildingExpenses > 0)
        {
            if (region.currentCountry.money >= 800)
            {
                if (check <= 10)
                {
                    region.infrastructure_Amount++;
                    region.currentCountry.money -= 800;
                    region.currentCountry.moneyNaturalIncome += 8;

                    buildingExpenses -= 800;
                }
            }

            if (ReferencesManager.Instance.gameSettings.onlineGame)
            {
                Multiplayer.Instance.SetCountryValues(
                    region.currentCountry.country._id,
                    region.currentCountry.money,
                    region.currentCountry.food,
                    region.currentCountry.recroots);
            }
        }
    }

    private void CreateUnit(CountrySettings country, RegionManager region)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.CreateUnit(region._id);
        }
        else
        {
            if (region.currentCountry.country._id == country.country._id)
            {
                UnitScriptableObject unit = ReferencesManager.Instance.gameSettings.soldierLVL1;

                if (country.money >= unit.moneyCost && country.recroots >= unit.recrootsCost)
                {
                    GameObject spawnedUnit = Instantiate(ReferencesManager.Instance.army.unitPrefab, region.transform);
                    spawnedUnit.transform.localScale = new Vector3(ReferencesManager.Instance.army.unitPrefab.transform.localScale.x, ReferencesManager.Instance.army.unitPrefab.transform.localScale.y);

                    spawnedUnit.GetComponent<UnitMovement>().currentCountry = country;
                    spawnedUnit.GetComponent<UnitMovement>().currentProvince = region;
                    spawnedUnit.GetComponent<UnitMovement>().UpdateInfo();
                    region.hasArmy = true;
                    country.countryUnits.Add(spawnedUnit.GetComponent<UnitMovement>());

                    AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL1, region, spawnedUnit.GetComponent<UnitMovement>());
                }

                region.CheckRegionUnits(region);
            }
        }
    }

    private float GetDistance(RegionManager regionA, RegionManager regionB)
    {
        float distance = Vector2.Distance(regionA.transform.position, regionB.transform.position);

        if (ReferencesManager.Instance.gameSettings.developerMode) Debug.Log($"Дистанция между {regionA.name} ({regionA.currentCountry.country._name}) и {regionB.name} ({regionB.currentCountry.country._name}) равна {distance}");

        return distance;
    }

    private void CreateDivision(CountrySettings country, RegionManager region, UnitMovement addUnitToArmy)
    {
        if (region.currentCountry.country._id == country.country._id)
        {
            if (region.currentCountry.country._id == country.country._id)
            {
                int random = 0;

                if (!HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL1) &&
                    !HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL2))
                {
                    random = Random.Range(0, 25);

                    if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.artileryLVL1) ||
                        HasUnitTech(country, ReferencesManager.Instance.gameSettings.artileryLVL2))
                    {
                        random = Random.Range(25, 100);
                    }
                }
                else if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL1) ||
                    HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL1))
                {
                    random = Random.Range(100, 150);
                }
                else if (!HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL1) ||
                    !HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL1))
                {
                    if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.motoLVL1) ||
                        HasUnitTech(country, ReferencesManager.Instance.gameSettings.motoLVL2))
                    {
                        random = Random.Range(150, 200);
                    }
                }

                if (random > 0 && random < 25)
                {
                    // Infantry Only
                    for (int v = 0; v < 10; v++)
                    {
                        if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.soldierLVL2))
                        {
                            AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL2, region, addUnitToArmy);
                        }
                        else if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.soldierLVL3))
                        {
                            AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL3, region, addUnitToArmy);
                        }
                        else
                        {
                            AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL1, region, addUnitToArmy);
                        }
                    }
                }

                else if (random >= 25 && random < 100)
                {
                    // Infantry
                    for (int v = 0; v < 5; v++)
                    {
                        if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.soldierLVL2))
                        {
                            AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL2, region, addUnitToArmy);
                        }
                        else if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.soldierLVL3))
                        {
                            AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL3, region, addUnitToArmy);
                        }
                        else
                        {
                            AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL1, region, addUnitToArmy);
                        }
                    }

                    // Artilery
                    if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.artileryLVL2))
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL2, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL2, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL2, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL2, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL2, region, addUnitToArmy);
                    }
                    else if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.artileryLVL1))
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL1, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL1, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL1, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL1, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL1, region, addUnitToArmy);
                    }
                }

                else if (random >= 100 && random < 150)
                {
                    // Infantry
                    if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.soldierLVL2))
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL2, region, addUnitToArmy);
                    }
                    else if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.soldierLVL3))
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL3, region, addUnitToArmy);
                    }
                    else
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL1, region, addUnitToArmy);
                    }

                    // Artilery
                    if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.artileryLVL2))
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL2, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL2, region, addUnitToArmy);
                    }
                    else if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.artileryLVL1))
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL1, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL1, region, addUnitToArmy);
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        // Heavy
                        if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL2))
                        {
                            AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.tankLVL2, region, addUnitToArmy);
                        }
                        else if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL1))
                        {
                            AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.tankLVL1, region, addUnitToArmy);
                        }
                    }
                }
                else if (random >= 150 && random <= 200)
                {
                    // Infantry
                    if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.soldierLVL2))
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL2, region, addUnitToArmy);
                    }
                    else if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.soldierLVL3))
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL3, region, addUnitToArmy);
                    }
                    else
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL1, region, addUnitToArmy);
                    }

                    // Artilery
                    if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.artileryLVL2))
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL2, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL2, region, addUnitToArmy);
                    }
                    else if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.artileryLVL1))
                    {
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL1, region, addUnitToArmy);
                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.artileryLVL1, region, addUnitToArmy);
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        // Heavy
                        if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.motoLVL1))
                        {
                            AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.motoLVL1, region, addUnitToArmy);
                        }
                        else if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.motoLVL2))
                        {
                            AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.motoLVL2, region, addUnitToArmy);
                        }
                    }
                }
            }
        }
    }

    private void AddUnitToArmy(CountrySettings country, UnitScriptableObject unit, RegionManager region, UnitMovement unitMovement)
    {
        if (region.currentCountry.country._id == country.country._id)
        {
            if (ReferencesManager.Instance.gameSettings.onlineGame)
            {
                Multiplayer.Instance.AddUnitToArmy(unit.unitName, region._id);
            }
            else
            {
                if (unitMovement.unitsHealth.Count < 10)
                {
                    if (warExpenses >= unit.moneyCost)
                    {
                        if (country.money >= unit.moneyCost && country.recroots >= unit.recrootsCost)
                        {
                            country.money -= unit.moneyCost;
                            country.recroots -= unit.recrootsCost;
                            country.moneyNaturalIncome -= unit.moneyIncomeCost;
                            country.foodNaturalIncome -= unit.foodIncomeCost;

                            warExpenses -= unit.moneyCost;

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

                            if (ReferencesManager.Instance.gameSettings.developerMode) Debug.Log($"Добавил {unit.name} для { country.country._uiName } в регионе {region}");
                        }
                    }
                }
            }
        }
    }

    private bool HasUnitTech(CountrySettings country, UnitScriptableObject unit)
    {
        if (Researched(country, ReferencesManager.Instance.gameSettings.technologies[unit.unlockLevel]))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CanResearch(CountrySettings country, TechnologyScriptableObject tech)
    {
        bool result;
        int researchedNeeded = 0;

        if (!Researched(country, tech))
        {
            if (tech.techsNeeded.Length > 0)
            {
                for (int i = 0; i < tech.techsNeeded.Length; i++)
                {
                    if (Researched(country, tech.techsNeeded[i]))
                    {
                        researchedNeeded++;
                    }
                }

                if (country.money >= tech.moneyCost && researchingExpenses >= tech.moneyCost)
                {
                    if (tech.techsNeeded.Length == researchedNeeded)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                if (country.money >= tech.moneyCost && researchingExpenses >= tech.moneyCost)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
        }
        else
        {
            result = false;
        }

        return result;
    }

    private bool Researched(CountrySettings country, TechnologyScriptableObject tech)
    {
        return country.countryTechnologies.Contains(tech);
    }

    private void StartRecearch(CountrySettings country, TechnologyScriptableObject tech)
    {
        if (CanResearch(country, tech))
        {
            if (currentTech == null || currentTech.tech == null)
            {
                TechQueue techQueue = new TechQueue();
                techQueue.tech = tech;
                techQueue.moves = tech.moves;

                currentTech = techQueue;
                country.money -= currentTech.tech.moneyCost;
                researchingExpenses -= currentTech.tech.moneyCost;

                researching = true;
            }
        }

        Multiplayer.Instance.SetCountryValues(
            country.country._id,
            country.money,
            country.food,
            country.recroots);
    }

    public void SendOffer(string offer, CountrySettings sender, CountrySettings receiver)
    {
        if (offer == "Торговля")
        {
            int relationsRandom = Random.Range(10, 15);

            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            if (!receiver.isPlayer)
            {
                senderToReceiver.trade = true;
                receiverToSender.trade = true;

                senderToReceiver.relationship += relationsRandom;
                receiverToSender.relationship += relationsRandom;

                ReferencesManager.Instance.CalculateTradeBuff(sender, receiver);
            }
            else if (receiver.isPlayer)
            {
                SpawnEvent("Торговля", sender, receiver);
            }
        }
        else if (offer == "Пакт о ненападении")
        {
            int relationsRandom = Random.Range(10, 15);

            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            if (!receiver.isPlayer)
            {
                senderToReceiver.pact = false;
                receiverToSender.pact = false;

                senderToReceiver.relationship += relationsRandom;
                receiverToSender.relationship += relationsRandom;

            }
            else if (receiver.isPlayer)
            {
                SpawnEvent("Пакт о ненападении", sender, receiver);
            }
        }
        else if (offer == "Право прохода войск")
        {
            int relationsRandom = Random.Range(10, 15);

            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            if (!receiver.isPlayer)
            {
                senderToReceiver.right = false;
                receiverToSender.right = false;

                senderToReceiver.relationship += relationsRandom;
                receiverToSender.relationship += relationsRandom;

            }
            else if (receiver.isPlayer)
            {
                SpawnEvent("Право прохода войск", sender, receiver);
            }
        }
        else if (offer == "Объявить войну")
        {
            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            if (!receiver.isPlayer)
            {
                senderToReceiver.war = true;
                senderToReceiver.trade = false;
                senderToReceiver.right = false;
                senderToReceiver.pact = false;
                senderToReceiver.union = false;

                sender.enemy = receiver;
                receiver.enemy = sender;

                sender.inWar = true;
                receiver.inWar = true;

                receiverToSender.war = true;
                receiverToSender.trade = false;
                receiverToSender.right = false;
                receiverToSender.pact = false;
                receiverToSender.union = false;

                senderToReceiver.relationship -= 100;
                receiverToSender.relationship -= 100;
            }
            else if (receiver.isPlayer)
            {
                SpawnEvent("Объявить войну", sender, receiver);
            }
        }
    }

    private void SpawnEvent(string offer, CountrySettings sender, CountrySettings receiver)
    {
        GameObject spawned = Instantiate(ReferencesManager.Instance.regionUI.messageEvent);
        spawned.transform.SetParent(ReferencesManager.Instance.regionUI.messageReceiver);
        spawned.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        spawned.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);


        spawned.GetComponent<EventItem>().sender = sender;
        spawned.GetComponent<EventItem>().receiver = receiver;
        spawned.GetComponent<EventItem>().offer = offer;

        spawned.GetComponent<EventItem>().senderImage.sprite = sender.country.countryFlag;


        if (offer == "Торговля")
        {
            spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.tradeSprite;
        }
        else if (offer == "Объявить войну")
        {
            spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.warSprite;
        }
        else if (offer == "Пакт о ненападении")
        {
            spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.pactSprite;
        }
        else if (offer == "Право прохода войск")
        {
            spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.moveSprite;
        }
        else if (offer == "Объявить войну")
        {
            spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.warSprite;
        }
    }
}

[System.Serializable]
public class TechQueue
{
    public TechnologyScriptableObject tech;
    public int moves;
}