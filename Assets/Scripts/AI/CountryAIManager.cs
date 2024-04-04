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

    private DiplomatyUI diplomatyUI;

    private void Awake()
    {
        diplomatyUI = FindObjectOfType<DiplomatyUI>();
    }

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

                    //if (country.aiAccuracy == 0)
                    //{
                    //    currentAttackPreference = 100;
                    //}

                    //if (_winChance < 50)
                    //{
                    //    currentAttackPreference = _winChance / country.aiAccuracy;
                    //}
                    //else if (_winChance >= 50)
                    //{
                    //    currentAttackPreference = _winChance * country.aiAccuracy;
                    //}

                    currentAttackPreference = 100;

                    //if (!enemyRegion.currentCountry.isPlayer && enemyRegion.currentCountry.country._tag != "POL")
                    //{
                    //    currentAttackPreference += 0.25f;
                    //}

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

                if (country.money >= ReferencesManager.Instance.gameSettings.researchLab.goldCost &&
                    buildingExpenses >= ReferencesManager.Instance.gameSettings.researchLab.goldCost &&
                    country.researchPointsIncome <= 0)
                {
                    if (region.buildings.Count + region.buildingsQueue.Count < 4)
                    {
                        region.AddBuildingToQueueForce(ReferencesManager.Instance.gameSettings.researchLab, region);
                        buildingExpenses -= ReferencesManager.Instance.gameSettings.researchLab.goldCost;
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

        if (!researching)
        {
            moneySaving += researchingExpenses;
        }

        #region SetMobilizationLaw

        if (country.myRegions.Count > 0 && ReferencesManager.Instance.dateManager.currentDate[0] > 1)
        {
            float preferedMP = country.myRegions.Count * Random.Range(700, 4500);

            preferedMP *= country.aiAccuracy;

            if (country.ideology == "Коммунизм" || country.ideology == "Фашизм")
            {
                preferedMP *= Random.Range(2, 5);
            }
            else if (country.ideology == "Демократия")
            {
                preferedMP /= 2;
            }

            if (country.recroots <= preferedMP)
            {
                if (!country.inWar)
                {
                    if (!country.mobilasing)
                    {
                        if (country.mobilizationLaw + 1 < ReferencesManager.Instance.gameSettings.mobilizationPercent.Length)
                        {
                            ReferencesManager.Instance.SetRecroots(country.mobilizationLaw + 1, country);
                            //Debug.Log($"{country.country._name} начинает мобилизацию, устанавливая закон {country.mobilizationLaw} | {country.recroots} людей => {preferedMP} людей");
                        }
                    }
                }
                else
                {
                    if (country.mobilizationLaw + 1 < ReferencesManager.Instance.gameSettings.mobilizationPercent.Length)
                    {
                        ReferencesManager.Instance.SetRecroots(country.mobilizationLaw + 1, country);
                        //Debug.Log($"{country.country._name} начинает мобилизацию, устанавливая закон {country.mobilizationLaw} | {country.recroots} людей => {preferedMP} людей");
                    }
                }
            }
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

                            if (countryOther.ideology == "Коммунизм" && country.ideology == "Коммунизм")
                            {
                                random += Random.Range(70, 100);
                            }

                            if (random >= 85)
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
        try
        {

            foreach (Transform point in region.movePoints)
            {
                RegionManager _pointRegion = point.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>();

                if (_pointRegion.currentCountry != region.currentCountry) // If founded region is not my state
                {
                    countries.Add(_pointRegion.currentCountry);
                }
            }

            countries.Distinct(); // Remove dupes

        }
        catch (System.Exception) { }

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
        UnitMovement.BattleInfo battle = new UnitMovement.BattleInfo();

        battle.defenderForts = fightRegion.fortifications_Amount;

        try
        {
            if (defenderUnit.Encircled(defenderUnit.currentProvince))
            {
                battle.defender_BONUS_ATTACK -= 50;
                battle.defender_BONUS_DEFENCE -= 50;
            }
        }
        catch (System.Exception) { }

        try
        {
            if (attackerUnit.Encircled(attackerUnit.currentProvince))
            {
                battle.attacker_BONUS_ATTACK -= 50;
                battle.attacker_BONUS_DEFENCE -= 50;
            }
        }
        catch (System.Exception) { }

        battle.defender_BONUS_ATTACK = 100;
        battle.defender_BONUS_DEFENCE = 100;

        battle.attacker_BONUS_ATTACK = 100;
        battle.attacker_BONUS_DEFENCE = 100;

        if (attackFrom.hasArmy)
        {
            try
            {
                attackerUnit = attackFrom.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();
            }
            catch (System.Exception)
            {
                attackFrom.hasArmy = false;
            }
        }

        int winChance = 0;
        int offset = 0;

        #region Defender info

        if (fightRegion.hasArmy)
        {
            try
            {
                List<float> _armors = new List<float>();
                UnitMovement fightRegionUnitMovement = fightRegion.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();

                if (fightRegionUnitMovement != null)
                {
                    battle.defenderDivision = fightRegionUnitMovement;
                    battle.enemyUnits = battle.defenderDivision.unitsHealth;
                    battle.fightRegion = fightRegion;

                    foreach (UnitMovement.UnitHealth unit in battle.defenderDivision.unitsHealth)
                    {
                        _armors.Add(unit.unit.armor);

                        battle.defenderSoftAttack += unit.unit.softAttack;
                        battle.defenderHardAttack += unit.unit.hardAttack;
                        battle.defenderDefense += unit.unit.defense;
                        battle.defenderArmor += unit.unit.armor;
                        battle.defenderArmorPiercing += unit.unit.armorPiercing;
                        battle.defenderHardness += unit.unit.hardness;
                    }

                    if (_armors.Count > 0)
                    {
                        float _maxArmor = _armors.Max();
                        float _midArmor = battle.defenderArmor / battle.defenderDivision.unitsHealth.Count;

                        battle.defenderHardness = battle.defenderHardness / battle.defenderDivision.unitsHealth.Count;
                        battle.defenderArmor = 0.4f * _maxArmor + 0.6f * _midArmor;
                    }
                }
            }
            catch (System.NullReferenceException)
            {
                fightRegion.hasArmy = false;
            }
        }
        else if (!fightRegion.hasArmy)
        {
            List<float> defenderArmors = new List<float>();

            battle.defenderDivision = null;
            battle.fightRegion = fightRegion;
            battle.enemyUnits = battle.fightRegion.currentDefenseUnits;

            if (fightRegion.currentDefenseUnits.Count > 0)
            {
                try
                {
                    foreach (UnitMovement.UnitHealth unit in fightRegion.currentDefenseUnits)
                    {
                        defenderArmors.Add(unit.unit.armor);

                        battle.defenderSoftAttack += unit.unit.softAttack;
                        battle.defenderHardAttack += unit.unit.hardAttack;
                        battle.defenderDefense += unit.unit.defense;
                        battle.defenderArmor += unit.unit.armor;
                        battle.defenderArmorPiercing += unit.unit.armorPiercing;
                        battle.defenderHardness += unit.unit.hardness;
                    }

                    if (defenderArmors.Count > 0)
                    {
                        float maxArmor = defenderArmors.Max();
                        float midArmor = battle.defenderArmor / battle.enemyUnits.Count;

                        battle.defenderHardness = battle.defenderHardness / battle.enemyUnits.Count;
                        battle.defenderArmor = 0.4f * maxArmor + 0.6f * midArmor;
                    }
                }
                catch (System.Exception) {}
            }
        }

        #endregion

        #region Attacker info

        List<float> attackerArmors = new List<float>();

        battle.attackerDivision = attackerUnit;
        battle.myUnits = battle.attackerDivision.unitsHealth;
        battle.fightRegion = fightRegion;

        foreach (UnitMovement.UnitHealth unit in attackerUnit.unitsHealth)
        {
            attackerArmors.Add(unit.unit.armor);

            battle.attackerSoftAttack += unit.unit.softAttack;
            battle.attackerHardAttack += unit.unit.hardAttack;
            battle.attackerDefense += unit.unit.defense;
            battle.attackerArmor += unit.unit.armor;
            battle.attackerArmorPiercing += unit.unit.armorPiercing;
            battle.attackerHardness += unit.unit.hardness;
        }

        float attackerMaxArmor = 0;
        try
        {
            attackerMaxArmor = attackerArmors.Max();
        }
        catch (System.Exception)
        {
        }

        float attackerMidArmor = battle.attackerArmor / battle.attackerDivision.unitsHealth.Count;

        battle.attackerHardness = battle.attackerHardness / battle.attackerDivision.unitsHealth.Count;
        battle.attackerArmor = 0.4f * attackerMaxArmor + 0.6f * attackerMidArmor;

        #endregion

        if (battle.defenderForts > 0)
        {
            float fortsDebuff = battle.defenderForts * ReferencesManager.Instance.gameSettings.fortDebuff;
            battle.defender_BONUS_DEFENCE += fortsDebuff;
            battle.attacker_BONUS_ATTACK -= fortsDebuff;
        }

        if (battle.attackerArmorPiercing < battle.defenderArmor)
        {
            battle.attacker_BONUS_ATTACK -= 50;
        }

        if (battle.defender_BONUS_ATTACK <= 0) battle.defender_BONUS_ATTACK = 5;
        if (battle.defender_BONUS_DEFENCE <= 0) battle.defender_BONUS_DEFENCE = 5;
        if (battle.attacker_BONUS_ATTACK <= 0) battle.attacker_BONUS_ATTACK = 5;
        if (battle.attacker_BONUS_DEFENCE <= 0) battle.attacker_BONUS_DEFENCE = 5;

        #region Buffs / Final Countings

        float buffs_attackerSoftAttack = battle.attackerSoftAttack * (battle.attacker_BONUS_ATTACK / 100f);
        float buffs_attackerHardAttack = battle.attackerHardAttack * (battle.attacker_BONUS_ATTACK / 100f);

        float buffs_defenderSoftAttack = battle.defenderSoftAttack * (battle.defender_BONUS_ATTACK / 100f);
        float buffs_defenderHardAttack = battle.defenderHardAttack * (battle.defender_BONUS_ATTACK / 100f);

        float defender_receive_SoftAttack = 100 - battle.defenderHardness;
        float defender_receive_HardAttack = battle.defenderHardness;

        float attacker_receive_SoftAttack = 100 - battle.attackerHardness;
        float attacker_receive_HardAttack = battle.attackerHardness;

        float finalAttackerSoftAttack = buffs_attackerSoftAttack * (defender_receive_SoftAttack / 100);
        float finalAttackerHardAttack = buffs_attackerHardAttack * (defender_receive_HardAttack / 100);

        float finalDefenderSoftAttack = buffs_defenderSoftAttack * (attacker_receive_SoftAttack / 100);
        float finalDefenderHardAttack = buffs_defenderHardAttack * (attacker_receive_HardAttack / 100);
        float finalDefenderDefence = battle.defenderDefense * (battle.defender_BONUS_DEFENCE / 100f);

        battle.attackerSoftAttack = finalAttackerSoftAttack;
        battle.attackerHardAttack = finalAttackerHardAttack;

        battle.defenderDefense = finalDefenderDefence;
        battle.defenderSoftAttack = finalDefenderSoftAttack;
        battle.defenderHardAttack = finalDefenderHardAttack;

        battle.attackerStrength = battle.attackerSoftAttack + battle.attackerHardAttack;
        battle.defenderStrength = battle.defenderSoftAttack + battle.defenderHardAttack;

        #endregion

        if (battle.attackerStrength < battle.defenderStrength)
        {
            float difference = Mathf.Abs(battle.defenderStrength) - Mathf.Abs(battle.attackerStrength);
            difference = Mathf.Abs(difference * 100 / battle.defenderStrength);

            if (difference >= 1 && difference <= 15)
            {
                winChance = Random.Range(47, 49);

                offset = 85;
            }

            else if (difference >= 15 && difference <= 20)
            {
                winChance = Random.Range(45, 47);

                offset = 75;
            }

            else if (difference >= 20 && difference <= 25)
            {
                winChance = Random.Range(43, 45);

                offset = 65;
            }

            else if (difference >= 25 && difference <= 30)
            {
                winChance = Random.Range(41, 43);

                offset = 55;
            }

            else if (difference >= 35 && difference <= 40)
            {
                winChance = Random.Range(39, 41);

                offset = 45;
            }

            else if (difference >= 45 && difference <= 50)
            {
                winChance = Random.Range(37, 39);

                offset = 35;
            }

            else if (difference >= 55 && difference <= 60)
            {
                winChance = Random.Range(35, 37);

                offset = 25;
            }

            else if (difference >= 65 && difference <= 70)
            {
                winChance = Random.Range(30, 37);

                offset = 20;
            }

            else if (difference >= 75 && difference <= 80)
            {
                winChance = Random.Range(10, 25);

                offset = 10;
            }

            else if (difference >= 85 && difference <= 100)
            {
                winChance = Random.Range(0, 15);

                offset = 0;
            }
        }
        else if (battle.defenderStrength == battle.attackerStrength)
        {
            winChance = 50;
        }
        else if (battle.attackerStrength > battle.defenderStrength)
        {
            float difference = Mathf.Abs(battle.attackerStrength) - Mathf.Abs(battle.defenderStrength);
            difference = Mathf.Abs(difference * 100 / battle.attackerStrength);

            if (difference >= 1 && difference <= 10)
            {
                winChance = Random.Range(51, 53);

                offset = 85;
            }

            else if (difference > 10 && difference <= 15)
            {
                winChance = Random.Range(53, 56);

                offset = 75;
            }

            else if (difference > 15 && difference <= 20)
            {
                winChance = Random.Range(56, 60);

                offset = 65;
            }

            else if (difference > 25 && difference <= 30)
            {
                winChance = Random.Range(60, 64);

                offset = 55;
            }

            else if (difference > 30 && difference <= 40)
            {
                winChance = Random.Range(64, 68);

                offset = 45;
            }

            else if (difference > 40 && difference <= 50)
            {
                winChance = Random.Range(68, 74);


                offset = 35;
            }

            else if (difference > 50 && difference <= 60)
            {
                winChance = Random.Range(74, 78);


                offset = 25;
            }

            else if (difference > 60 && difference <= 70)
            {
                winChance = Random.Range(78, 82);


                offset = 15;
            }

            else if (difference > 70 && difference <= 80)
            {
                winChance = Random.Range(82, 86);


                offset = 5;
            }

            else if (difference > 80 && difference <= 90)
            {
                winChance = Random.Range(86, 90);


                offset = 0;
            }

            else if (difference > 90 && difference <= 100)
            {
                winChance = Random.Range(90, 92);


                offset = 0;
            }
        }

        return winChance;
    }

    private RegionManager GetEnemyRegionFromMy(RegionManager myRegion)
    {
        RegionManager enemyRegion = new RegionManager();


        foreach (Transform point in myRegion.movePoints)
        {
            CountrySettings who = point.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>().currentCountry;

            if (diplomatyUI.
                FindCountriesRelation(who, myRegion.currentCountry).war)
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
                    HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL2))
                {
                    random = Random.Range(101, 150);
                }
                else if (!HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL1) &&
                    !HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL2))
                {
                    if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.motoLVL1) ||
                        HasUnitTech(country, ReferencesManager.Instance.gameSettings.motoLVL2))
                    {
                        random = Random.Range(151, 200);
                    }
                }

                RemoveUnitFromArmy(country, 0, region, addUnitToArmy);

                if (random > 0 && random <= 25)
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

                else if (random > 25 && random <= 100)
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

                else if (random > 100 && random <= 150)
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
                else if (random > 150 && random <= 200)
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
                        if (country.money >= unit.moneyCost &&
                            country.recroots >= unit.recrootsCost &&
                            country.food >= unit.foodCost)
                        {
                            country.money -= unit.moneyCost;
                            country.food -= unit.foodCost;
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

    private void RemoveUnitFromArmy(CountrySettings country, int index, RegionManager region, UnitMovement unitMovement)
    {
        if (region.currentCountry.country._id == country.country._id)
        {
            if (unitMovement.unitsHealth.Count > 0)
            {
                unitMovement.unitsHealth.Remove(unitMovement.unitsHealth[index]);

                ReferencesManager.Instance.countryManager.UpdateValuesUI();
                ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();

                //if (unitMovement.unitsHealth.Count <= 0)
                //{
                //    ReferencesManager.Instance.regionManager.currentRegionManager.DeselectRegions();
                //    unitMovement.currentCountry.countryUnits.Remove(unitMovement);
                //    Destroy(unitMovement.gameObject);
                //}
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

                if (country.money >= tech.moneyCost && researchingExpenses >= tech.moneyCost && country.researchPoints >= tech.researchPointsCost)
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
                if (country.money >= tech.moneyCost && researchingExpenses >= tech.moneyCost && country.researchPoints >= tech.researchPointsCost)
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
                country.researchPoints -= currentTech.tech.researchPointsCost;
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
                senderToReceiver.pact = true;
                receiverToSender.pact = true;

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
                senderToReceiver.right = true;
                receiverToSender.right = true;

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
        else if (offer == "Союз")
        {
            int relationsRandom = Random.Range(40, 70);

            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            if (!receiver.isPlayer)
            {
                senderToReceiver.union = true;
                receiverToSender.union = true;

                senderToReceiver.relationship += relationsRandom;
                receiverToSender.relationship += relationsRandom;

            }
            else if (receiver.isPlayer)
            {
                SpawnEvent("Союз", sender, receiver);
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