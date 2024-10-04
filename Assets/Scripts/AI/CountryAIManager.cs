using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Guild;

public class CountryAIManager : MonoBehaviour
{
    private UnitMovement attackerUnit;
    private UnitMovement defenderUnit;

    public TechQueue currentTech;
    public bool researching;

    public int moneySaving;
    public int warExpenses;
    public int civilExpenses;
    public int buildingExpenses;
    public int researchingExpenses;

    private DiplomatyUI diplomatyUI;

    // Rfghhbc rgb rsv3:4';*4;4:4';7dvtv dzeg rx = 228
    float mult = 1.0f;

    private void Awake()
    {
        diplomatyUI = ReferencesManager.Instance.diplomatyUI;
        switch (ReferencesManager.Instance.gameSettings.difficultyValue.value)
        {
            case "EASY":
                mult = 0.5f;
                break;
            case "HARD":
                mult = 1.3f;
                break;
            case "INSANE":
                mult = 1.5f;
                break;
            case "HARDCORE":
                mult = 1.75f;
                break;
        }
    }

    public void Process(CountrySettings country, int mode)
    {
        if (country.myRegions.Count > 0)
        {
            ReferencesManager.Instance.countryManager.botCountry = country;

            moneySaving = country.money / 100 * 35; // 35% на сохранение
            warExpenses = country.money / 100 * 15; // 15% на военные расходы
            civilExpenses = country.money / 100 * 50; // 50% на стройку
            buildingExpenses = civilExpenses;

            if (country.enemies.Count > 0)
            {
                country.inWar = true;
            }
            else
            {
                country.inWar = false;
            }

            if (country.inWar)
            {
                switch (country.ideology)
                {
                    case "Демократия":
                        moneySaving = country.money / 100 * 5;
                        warExpenses = country.money / 100 * 85;
                        civilExpenses = country.money / 100 * 10;
                        break;
                    case "Монархия":
                        warExpenses = country.money / 100 * 90;
                        civilExpenses = country.money / 100 * 10;
                        break;
                    case "Неопределено":
                        warExpenses = country.money / 100 * 90;
                        civilExpenses = country.money / 100 * 10;
                        break;
                    case "Коммунизм":
                        moneySaving = country.money / 100 * 20;
                        warExpenses = country.money / 100 * 75;
                        civilExpenses = country.money / 100 * 5;
                        break;
                    case "Фашизм":
                        moneySaving = country.money / 100 * 20;
                        warExpenses = country.money / 100 * 75;
                        civilExpenses = country.money / 100 * 5;
                        break;
                }
            }

            // самые опасные регионы (на границе с странами-противниками)
            List<RegionManager> warBorderingRegions = new(20);
            // регионы 2го плана (соседние с опасными)
            List<RegionManager> dangerousBorderingRegions = new(33);
            // регионы 3го плана (обычно границы с мирными странами)
            List<RegionManager> calmBorderingRegions = new(50);
            //
            //List<Decisions_ScriptableObj> decs = new(0);

            if (ReferencesManager.Instance.gameSettings.onlineGame)
            {
                foreach (RegionManager region in country.myRegions)
                {
                    ProcessRegions(region, country);
                }
            }
            else
            {
                Thread thread = new(() =>
                {
                    Parallel.ForEach(country.myRegions, reg =>
                    {
                        RegionManager region = reg;

                        ProcessRegions(region, country);
                    });
                });

                thread.Start();
                thread.Join();
            }

            ProcessTechnologies(country);

            //decs = DecisionsManager.Enable(country);
            //for (int i = 0; i < decs.Count; i++)
            //{
            //    Decision_obj.Execute(country, decs[i]);
            //}

            #region aviation

            bool hasTech = country.countryTechnologies.Any(item => item._type == TechnologyScriptableObject.TechType.AirPlane);

            if (country.inWar && hasTech)
            {
                foreach (RegionManager reg in country.myRegions)
                {
                    bool check = false;
                    foreach (RegionManager reg1 in GetNeiboursOfRegion(reg))
                    {
                        if (reg1.currentCountry != country && ReferencesManager.Instance.diplomatyUI.
                            FindCountriesRelation(country, reg1.currentCountry).war)
                        {
                            check = true;
                            break;
                        }
                    }
                    if (!check) { continue; }

                    RegionManager max_reg = null;
                    float max_distance = 0f;
                    foreach (RegionManager reg1 in GetNeiboursOfRegion(reg))
                    {
                        if (reg1.currentCountry == country)
                        {
                            if (Vector2.Distance(reg.transform.position, reg1.transform.position) > max_distance)
                            {
                                max_distance = Vector2.Distance(reg.transform.position, reg1.transform.position);
                                max_reg = reg1;

                                UpgradeAirBase(max_reg);
                                BuyAirplanes(max_reg);
                            }
                        }
                    }
                }
            }

            foreach (RegionManager reg in country.myRegions)
            {
                if (reg._airBaseLevel > 0)
                {
                    Aviation_Storage storage = reg.gameObject.GetComponent<Aviation_Storage>();
                    if (storage.planes.Count > 0)
                    {
                        foreach (Aviation_Cell cell in storage.planes)
                        {
                            if (cell.AirPlane.type == Aviation_ScriptableObj.Type.bomber)
                            {
                                List<RegionManager> regs = new(3);
                                foreach (GameObject obj in FindObjectsWithComponent<RegionManager>(new Vector2(reg.transform.position.x, reg.transform.position.y), 0.00375f * cell.AirPlane.distance))
                                {
                                    if (obj.GetComponent<RegionManager>().currentCountry != country && ReferencesManager.Instance.diplomatyUI.
                                    FindCountriesRelation(country, obj.GetComponent<RegionManager>().currentCountry).war && new System.Random().Next(0, 10) == 0)
                                    {
                                        if (regs.Count < 3 && obj.GetComponent<RegionManager>().buildings.Count > 0)
                                        {
                                            regs.Add(obj.GetComponent<RegionManager>());
                                        }
                                    }
                                }
                                attack(cell, regs, 1, reg);
                            }
                            else if (cell.AirPlane.type == Aviation_ScriptableObj.Type.stormtrooper)
                            {
                                foreach (GameObject obj in FindObjectsWithComponent<RegionManager>(new Vector2(reg.transform.position.x, reg.transform.position.y), 0.00375f * cell.AirPlane.distance))
                                {
                                    if (obj.GetComponent<RegionManager>().currentCountry != country && ReferencesManager.Instance.diplomatyUI.
                                    FindCountriesRelation(country, obj.GetComponent<RegionManager>().currentCountry).war && obj.GetComponent<RegionManager>().hasArmy)
                                    {
                                        attack(cell, new List<RegionManager>(1) { obj.GetComponent<RegionManager>() }, 0, reg);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            List<GameObject> FindObjectsWithComponent<T>(Vector2 center, float searchRadius) where T : Component
            {
                // Найти все коллайдеры в радиусе
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, searchRadius);

                List<GameObject> foundObjects = new List<GameObject>();

                // Проверяем каждый найденный коллайдер
                foreach (Collider2D collider in hitColliders)
                {
                    // Проверяем, содержит ли объект нужный компонент
                    T component = collider.GetComponent<T>();
                    if (component != null)
                    {
                        foundObjects.Add(collider.gameObject);
                    }
                }

                return foundObjects;
            }
            #endregion

            if (country.myRegions.Count >= 3)
            {
                foreach (RegionManager region in country.myRegions)
                {
                    foreach (CountrySettings _country in GetBorderingCountiesWithRegion(region))
                    {
                        if (ReferencesManager.Instance.diplomatyUI.
                            FindCountriesRelation(country, _country).war) // Если граничащая страна воюет с нами
                        {
                            if (!warBorderingRegions.Contains(region))
                            {
                                warBorderingRegions.Add(region);
                                if (ReferencesManager.Instance.gameSettings.difficultyValue.value
                                    == "HARD" ||
                                    ReferencesManager.Instance.gameSettings.difficultyValue.value
                                    == "INSANE" ||
                                    ReferencesManager.Instance.gameSettings.difficultyValue.value
                                    == "HARDCORE")
                                {
                                    foreach (RegionManager reg in GetNeiboursOfRegion(region))
                                    {
                                        if (reg.currentCountry == country && !reg._isCoast)
                                        {
                                            dangerousBorderingRegions.Add(reg);
                                            if (ReferencesManager.Instance.gameSettings.difficultyValue.value == "HARDCORE" ||
                                                ReferencesManager.Instance.gameSettings.difficultyValue.value == "INSANE")
                                            {
                                                foreach (RegionManager reg1 in GetNeiboursOfRegion(reg))
                                                {
                                                    if (reg1.currentCountry == country && !reg._isCoast)
                                                    {
                                                        calmBorderingRegions.Add(reg1);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (!ReferencesManager.Instance.diplomatyUI.
                            FindCountriesRelation(country, _country).union && !ReferencesManager.Instance.diplomatyUI.
                            FindCountriesRelation(country, _country).pact) // иначе просто добавляем границу в регионы 3 плана (если нет пакта или альянса)
                        {
                            foreach (RegionManager reg in GetNeiboursOfRegion(region))
                            {
                                if (reg.currentCountry != region.currentCountry)
                                {
                                    if (reg.hasArmy)
                                    {
                                        dangerousBorderingRegions.Add(region);
                                    }

                                    else
                                    {
                                        calmBorderingRegions.Add(region);
                                    }
                                }
                            }
                        }
                        if (region._isCoast && country.money >= 40000 && country.recruits >= 20000)
                        {
                            calmBorderingRegions.Add(region);
                        }
                    }
                }
            }
            else
            {
                calmBorderingRegions = country.myRegions;
            }

            foreach (CountrySettings countryOther in ReferencesManager.Instance.countryManager.countries)
            {

                Relationships.Relation relations = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country);
                if (countryOther.country._id != country.country._id)
                {
                    if (country.myRegions.Count < 1 || country.capitulated)
                    {
                        country.exist = false;
                    }

                    //countryOther.UpdateCapitulation();

                    if (country.country._id != countryOther.country._id)
                    {
                        if (country.exist && countryOther.exist)
                        {
                            if (!relations.trade && //Trade
                                !relations.war) //Go trade
                            {
                                int random = (new System.Random()).Next(0, 100);
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
                                    random -= (new System.Random()).Next(60, 100);
                                }
                                if (country.ideology == "Фашизм" && countryOther.ideology != "Фашизм")
                                {
                                    random -= (new System.Random()).Next(60, 100);
                                }
                                if (countryOther.ideology == "Фашизм" && country.ideology == "Фашизм")
                                {
                                    random += (new System.Random()).Next(70, 100);
                                }

                                if (random >= 50)
                                {
                                    SendOffer("Торговля", country, countryOther);
                                }
                            }

                            if (!relations.pact && //Pact
                                !relations.war &&
                                relations.relationship >= 35)
                            {
                                int random = (new System.Random()).Next(0, 100);
                                if (countryOther.ideology == country.ideology)
                                {
                                    random += (new System.Random()).Next(0, 10);
                                }
                                else if (countryOther.ideology != country.ideology)
                                {
                                    random -= (new System.Random()).Next(10, 30);
                                }

                                if (countryOther.ideology == "Фашизм" && country.ideology != "Фашизм")
                                {
                                    random -= (new System.Random()).Next(60, 100);
                                }
                                if (country.ideology == "Фашизм" && countryOther.ideology != "Фашизм")
                                {
                                    random -= (new System.Random()).Next(60, 100);
                                }

                                if (countryOther.ideology == "Фашизм" && country.ideology == "Фашизм")
                                {
                                    random += (new System.Random()).Next(70, 100);
                                }

                                if (random > (new System.Random()).Next(50, 90))
                                {
                                    SendOffer("Пакт о ненападении", country, countryOther);
                                }
                            }

                            if (!relations.right && //Move right
                                !relations.war &&
                                relations.relationship >= 20)
                            {
                                int random = (new System.Random()).Next(0, 100);
                                if (countryOther.ideology == country.ideology)
                                {
                                    random += (new System.Random()).Next(0, 10);
                                }
                                else if (countryOther.ideology != country.ideology)
                                {
                                    random -= (new System.Random()).Next(10, 30);
                                }

                                if (countryOther.ideology == "Фашизм" && country.ideology != "Фашизм")
                                {
                                    random -= (new System.Random()).Next(60, 100);
                                }
                                if (country.ideology == "Фашизм" && countryOther.ideology != "Фашизм")
                                {
                                    random -= (new System.Random()).Next(60, 100);
                                }

                                if (countryOther.ideology == "Фашизм" && country.ideology == "Фашизм")
                                {
                                    random += (new System.Random()).Next(70, 100);
                                }


                                if (random >= 50)
                                {
                                    SendOffer("Право прохода войск", country, countryOther);
                                }
                            }

                            if (!relations.right && //Union
                                !relations.war &&
                                relations.relationship >= 20)
                            {
                                int random = (new System.Random()).Next(0, 100);

                                if (countryOther.ideology == country.ideology)
                                {
                                    random += (new System.Random()).Next(0, 10);
                                }
                                else if (countryOther.ideology != country.ideology)
                                {
                                    random -= (new System.Random()).Next(10, 30);
                                }

                                if (countryOther.ideology == "Фашизм" && country.ideology != "Фашизм")
                                {
                                    random -= (new System.Random()).Next(60, 100);
                                }
                                else if (country.ideology == "Фашизм" && countryOther.ideology != "Фашизм")
                                {
                                    random -= (new System.Random()).Next(60, 100);
                                }

                                else if (countryOther.ideology == "Фашизм" && country.ideology == "Фашизм")
                                {
                                    random += (new System.Random()).Next(70, 100);
                                }

                                else if (countryOther.ideology == "Коммунизм" && country.ideology == "Коммунизм")
                                {
                                    random += (new System.Random()).Next(70, 100);
                                }

                                else if (random >= 85)
                                {
                                    SendOffer("Союз", country, countryOther);
                                }
                            }
                        }
                    }

                    if (mode == 1)
                    {
                        int random = (new System.Random()).Next(0, 100);

                        if (random >= 99 && ReferencesManager.Instance.aiManager.progressManager.progressIndex >= 10)
                        {

                            if (countryOther.myRegions.Count > 0)
                            {
                                if (!relations.war)
                                {
                                    if (isCountriesAreNeibours(country, countryOther))
                                    {
                                        if (country.myRegions.Count / countryOther.myRegions.Count >= 2f / mult / country.aiAccuracy)
                                        {
                                            if (relations.pact)
                                            {
                                                SendOffer("Расторгнуть пакт о ненападении", country, countryOther);
                                            }
                                            else
                                            {
                                                SendOffer("Объявить войну", country, countryOther);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }

            for (int i = 0; i < warBorderingRegions.Count; i++)
            {
                RegionManager enemyRegion = GetEnemyRegionFromMy(warBorderingRegions[i]);

                if (warBorderingRegions[i].hasArmy)
                {
                    float currentAttackPreference = 100;
                    //currentAttackPreference = CountWinChance(warBorderingRegions[i], enemyRegion);

                    if (currentAttackPreference >= 10f * mult)
                    {
                        //Debug.Log($"{country.country._name} хочет атаковать в {enemyRegion.name} ({enemyRegion.currentCountry.country._name}) с шансом {currentAttackPreference} ({_winChance})");
                        foreach (Transform child in warBorderingRegions[i].transform)
                        {
                            if (child.GetComponent<UnitMovement>())
                            {
                                UnitMovement division = child.GetComponent<UnitMovement>();
                                if (division._movePoints > 0)
                                {
                                    division.AIMoveNoHit(enemyRegion, division);
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
                                country.countryUnits[unitIndex].AIMoveNoHit(GetRoute(warBorderingRegions[i], country.countryUnits[unitIndex]), country.countryUnits[unitIndex]);
                            }
                        }
                    }
                }
            }

            // тут крч движение армий ближе к границе
            foreach (RegionManager region in country.myRegions)
            {
                if (region.hasArmy)
                {
                    UnitMovement division = region.GetDivision(region);
                    if (division != null && division.currentCountry == country && division.currentProvince != null)
                    {
                        if (warBorderingRegions.Contains(region))
                        {
                            continue;
                        }
                        if (dangerousBorderingRegions.Contains(region))
                        {
                            foreach (RegionManager neighbour_region in GetNeiboursOfRegion(region))
                            {
                                if (warBorderingRegions.Contains(neighbour_region))
                                {
                                    division.AIMoveNoHit(neighbour_region, division);
                                    break;
                                }
                            }
                            continue;
                        }
                        if (Random.value > 0.95f && !country.inWar)
                        {
                            DisbandDivision(division);
                        }
                        else
                        {
                            if (division._movePoints > 0)
                            {
                                if (division.moveto == null)
                                {
                                    if (Random.value > 0.5)
                                    {
                                        if (warBorderingRegions.Count > 0)
                                        {
                                            division.moveto = warBorderingRegions[0];
                                            float minvalue = Vector2.Distance(warBorderingRegions[0].transform.position, region.transform.position);
                                            for (int i = 0; i < warBorderingRegions.Count; i++)
                                            {
                                                if (Vector2.Distance(warBorderingRegions[i].transform.position, region.transform.position) < minvalue)
                                                {
                                                    minvalue = Vector2.Distance(warBorderingRegions[i].transform.position, region.transform.position);
                                                    division.moveto = warBorderingRegions[i];
                                                }
                                            }
                                        }
                                        else if (dangerousBorderingRegions.Count > 0)
                                        {
                                            division.moveto = dangerousBorderingRegions[0];
                                            float minvalue = Vector2.Distance(dangerousBorderingRegions[0].transform.position, region.transform.position);
                                            for (int i = 0; i < dangerousBorderingRegions.Count; i++)
                                            {
                                                if (Vector2.Distance(dangerousBorderingRegions[i].transform.position, region.transform.position) < minvalue)
                                                {
                                                    minvalue = Vector2.Distance(dangerousBorderingRegions[i].transform.position, region.transform.position);
                                                    division.moveto = dangerousBorderingRegions[i];
                                                }
                                            }
                                        }
                                    }
                                }
                                if (division.moveto != null)
                                {
                                    if (division.moveto == division.currentProvince)
                                    {
                                        division.moveto = null;
                                    }
                                    else
                                    {
                                        RegionManager min2 = region;
                                        float minvalue2 = Vector2.Distance(division.moveto.transform.position, region.transform.position);
                                        foreach (RegionManager reg in GetNeiboursOfRegion(region))
                                        {
                                            if (!reg.hasArmy && reg.currentCountry == country && Vector2.Distance(reg.transform.position, division.moveto.transform.position) < minvalue2)
                                            {
                                                minvalue2 = Vector2.Distance(reg.transform.position, division.moveto.transform.position);
                                                min2 = reg;
                                            }
                                        }
                                        region.GetDivision(region).AIMoveNoHit(min2, region.GetDivision(region));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ((country.countryUnits.Count > 0 && country.countryUnits.Count <= dangerousBorderingRegions.Count) || country.countryUnits.Count <= 0)
            {
                foreach (RegionManager _region in dangerousBorderingRegions)
                {
                    _region.CheckRegionUnits(_region);

                    if (!_region.hasArmy)
                    {
                        CreateDivisionInRegion(_region, country);
                    }
                    else
                    {
                        UnitMovement division = _region.GetDivision(_region);
                        if (division.currentCountry != _region.currentCountry)
                        {
                            Relationships.Relation relation = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(country, division.currentCountry);

                            if (relation.war)
                            {
                                _region.DestroyRegionDivision(_region);
                            }
                            else if(!relation.right && !relation.war)
                            {
                                division.Retreat(division);
                            }
                        }
                    }
                }
            }
            foreach (RegionManager _region in calmBorderingRegions)
            {
                if (new System.Random().Next(1, (int)(100 / mult / country.aiAccuracy)) == 1)
                {
                    CreateDivisionInRegion(_region, country);
                }
            }

            Parallel.ForEach(country.guilds, guild =>
            {
                foreach (Guild.Offer offer in guild._offers)
                {
                    if (!offer.Voted(country))
                    {
                        float base_value = 50f;

                        CountrySettings other_country;
                        switch (offer.action)
                        {
                            case Guild.Action.Kick:

                                other_country = ((Guild.Country)offer.arg).country;

                                base_value -= 15f;

                                if (other_country.ideology != country.ideology)
                                {
                                    base_value += 10f;
                                }
                                else
                                {
                                    base_value -= 5f;
                                }

                                if (other_country.ideology != guild._ideology)
                                {
                                    base_value += 5f;
                                }

                                if (((Guild.Country)offer.arg).role == Guild.Role.Owner)
                                {
                                    base_value -= 10f;
                                }

                                if (guild._type == Guild.GuildType.Alliance)
                                {
                                    base_value -= 5f;
                                }
                                break;
                            case Guild.Action.Invite:
                                other_country = (CountrySettings)offer.arg;

                                if (other_country.ideology != country.ideology)
                                {
                                    base_value -= 10f;
                                }
                                else
                                {
                                    base_value += 5f;
                                }

                                if (guild._type == Guild.GuildType.Alliance)
                                {
                                    base_value -= 5f;
                                }
                                else
                                {
                                    base_value += 10f;
                                }
                                break;
                            case Guild.Action.Join:
                                other_country = (CountrySettings)offer.arg;

                                if (other_country.ideology != country.ideology)
                                {
                                    base_value -= 10f;
                                }
                                else
                                {
                                    base_value += 5f;
                                }

                                if (guild._type == Guild.GuildType.Alliance)
                                {
                                    base_value -= 5f;
                                }
                                else
                                {
                                    base_value += 10f;
                                }
                                break;
                            case Guild.Action.AskGold:
                                other_country = offer.starter;
                                base_value -= (int)offer.arg / (float)guild._storage.gold * 50f;

                                if (other_country.ideology != country.ideology)
                                {
                                    base_value -= 5f;
                                }
                                else
                                {
                                    base_value += 10f;
                                }

                                if (guild._type == Guild.GuildType.Alliance)
                                {
                                    base_value += 25f;
                                }

                                if (other_country.inWar)
                                {
                                    base_value += 10f;
                                }
                                break;
                            case Guild.Action.AskFood:
                                other_country = offer.starter;
                                base_value -= (int)offer.arg / (float)guild._storage.food * 15f;

                                if (other_country.ideology != country.ideology)
                                {
                                    base_value -= 5f;
                                }
                                else
                                {
                                    base_value += 10f;
                                }

                                if (guild._type == Guild.GuildType.Alliance)
                                {
                                    base_value += 25f;
                                }

                                if (other_country.inWar)
                                {
                                    base_value += 5f;
                                }
                                break;
                            case Guild.Action.AskRecruits:
                                other_country = offer.starter;
                                base_value -= (int)offer.arg / (float)guild._storage.recruits * 40f;

                                if (other_country.ideology != country.ideology)
                                {
                                    base_value -= 5f;
                                }
                                else
                                {
                                    base_value += 10f;
                                }

                                if (guild._type == Guild.GuildType.Alliance)
                                {
                                    base_value += 25f;
                                }
                                else
                                {
                                    base_value -= 10f;
                                }

                                if (other_country.inWar)
                                {
                                    base_value += 10f;
                                }
                                break;
                            case Guild.Action.AskFuel:
                                other_country = offer.starter;
                                base_value -= (int)offer.arg / (float)guild._storage.fuel * 25f;

                                if (other_country.ideology != country.ideology)
                                {
                                    base_value -= 5f;
                                }
                                else
                                {
                                    base_value += 10f;
                                }

                                if (guild._type == Guild.GuildType.Alliance)
                                {
                                    base_value += 25f;
                                }

                                if (other_country.inWar)
                                {
                                    base_value += 10f;
                                }
                                break;
                            case Guild.Action.Attack:
                                if (offer.arg is CountrySettings)
                                {
                                    other_country = (CountrySettings)offer.arg;

                                    Relationships.Relation relations = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(country, other_country);

                                    if (relations.war)
                                    {
                                        base_value += 1000f;
                                    }
                                    if (relations.union)
                                    {
                                        base_value -= 25f;
                                    }
                                    if (relations.right)
                                    {
                                        base_value -= 10f;
                                    }
                                    if (relations.pact)
                                    {
                                        base_value -= 5f;
                                    }
                                    if (other_country.ideology != country.ideology)
                                    {
                                        base_value += 10f;
                                    }
                                }
                                break;
                            case Guild.Action.Peace:
                                if (offer.arg is CountrySettings)
                                {
                                    other_country = (CountrySettings)offer.arg;

                                    base_value -= 10f;
                                    if (other_country.ideology != country.ideology)
                                    {
                                        base_value -= 10f;
                                    }
                                }
                                break;
                            case Guild.Action.Promote:
                                Country _countryToPromote;

                                if (offer.arg is Guild.Country)
                                {
                                    _countryToPromote = (Country)offer.arg;

                                    if (_countryToPromote.country.ideology == country.ideology)
                                    {
                                        base_value += 10f;
                                    }

                                    base_value += ((_countryToPromote.country.myRegions.Count / 25f) - 1) * 20f;
                                    if (_countryToPromote.country.myRegions.Count < country.myRegions.Count)
                                    {
                                        base_value -= _countryToPromote.country.myRegions.Count / (float)country.myRegions.Count * 2f;
                                    }
                                }

                                break;
                            case Guild.Action.Demote:
                                Country _countryToDemote;

                                if (offer.arg is Guild.Country)
                                {
                                    _countryToDemote = (Country)offer.arg;

                                    if (_countryToDemote.country.ideology != country.ideology)
                                    {
                                        base_value += 10f;
                                    }

                                    if (_countryToDemote.country.myRegions.Count < country.myRegions.Count / 1.5f)
                                    {
                                        base_value -= 1000f;
                                    }
                                }

                                break;
                            default:
                                break;
                        } // расчёт

                        base_value *= (new System.Random().Next(8, 12) / 10f);

                        if (base_value > 50f)
                        {
                            offer.agree.Add(guild.GetCountry(country));
                        }
                        else
                        {
                            offer.disagree.Add(guild.GetCountry(country));
                        }
                    }
                }
                Guild.Country cou = guild.GetCountry(country);
                // просит повышение
                if (cou.role != Guild.Role.Owner && cou.role != Guild.Role.Puppet)
                {
                    if (cou.country.myRegions.Count > guild.CountSize() / guild._countries.Count && new System.Random().Next(0, 15) == 0)
                    {
                        guild._offers.Add(new Guild.Offer(guild, country, cou, Guild.Action.Promote));
                    }
                }
                if (country.inWar)
                {
                    foreach (CountrySettings countryOther in ReferencesManager.Instance.countryManager.countries)
                    {
                        if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(countryOther, country).war)
                        {
                            if (new System.Random().Next(0, 7) == 0)
                            {
                                guild._offers.Add(new Guild.Offer(guild, country, countryOther, Guild.Action.Attack));
                            }
                        }

                    }
                }
                if (country.money < 5000 && new System.Random().Next(0, 3) == 0 && guild._storage.gold > 500)
                {
                    float percent = 0.25f;
                    if (country.inWar)
                    {
                        percent += 0.15f;
                    }
                    if (guild._type == Guild.GuildType.Alliance)
                    {
                        percent += 0.20f;
                    }
                    int toAsk = Mathf.CeilToInt(guild._storage.gold * percent / 100 * new System.Random().Next(8, 10) / 10) * 100;
                    if (toAsk > 100)
                    {
                        guild._offers.Add(new Guild.Offer(guild, country, toAsk, Guild.Action.AskGold));
                    }
                }
                if (country.food < 500 && new System.Random().Next(0, 2) == 0 && guild._storage.food > 100)
                {
                    float percent = 0.20f;
                    if (country.inWar)
                    {
                        percent += 0.10f;
                    }
                    int toAsk = Mathf.CeilToInt(guild._storage.food * percent / 50 * new System.Random().Next(8, 10) / 10) * 50;
                    if (toAsk > 50)
                    {
                        guild._offers.Add(new Guild.Offer(guild, country, toAsk, Guild.Action.AskFood));
                    }
                }
                if (country.recruits < 10000 && new System.Random().Next(0, 5) == 0 && guild._storage.recruits > 500)
                {
                    float percent = 0.20f;
                    if (country.inWar)
                    {
                        percent += 0.05f;
                    }
                    if (guild._type == Guild.GuildType.Alliance)
                    {
                        percent += 0.15f;
                    }
                    int toAsk = Mathf.CeilToInt(guild._storage.recruits * percent / 100 * new System.Random().Next(8, 10) / 10) * 100;
                    if (toAsk > 250)
                    {
                        guild._offers.Add(new Guild.Offer(guild, country, toAsk, Guild.Action.AskRecruits));
                    }
                }
                if (country.fuel < 20000 && new System.Random().Next(0, 10) == 0 && guild._storage.fuel > 1000)
                {
                    float percent = 0.10f;
                    if (country.inWar)
                    {
                        percent += 0.05f;
                    }
                    if (guild._type == Guild.GuildType.Alliance)
                    {
                        percent += 0.10f;
                    }
                    int toAsk = Mathf.CeilToInt(guild._storage.fuel * percent / 100 * new System.Random().Next(8, 10) / 10) * 100;
                    if (toAsk > 1000)
                    {
                        guild._offers.Add(new Guild.Offer(guild, country, toAsk, Guild.Action.AskFuel));
                    }
                }

                if (country.money > 15000)
                {
                    int count = (int)Mathf.Round(country.money / 50f);

                    country.money -= count;
                    guild._storage.gold += count;
                }
                if (country.food > 2500)
                {
                    int count = (int)Mathf.Round(country.food / 10f);

                    country.food -= count;
                    guild._storage.food += count;
                }
                if (country.recruits > 50000)
                {
                    int count = (int)Mathf.Round(country.recruits / 100f);

                    country.recruits -= count;
                    guild._storage.recruits += count;
                }
                if (country.fuel > 15000)
                {
                    int count = (int)Mathf.Round(country.fuel / 100f);

                    country.fuel -= count;
                    guild._storage.fuel += count;
                }
            });

            for (int i = 0; i < country.guilds.Count; i++)
            {
                for (int j = 0; j < country.guilds[i]._offers.Count; j++)
                {
                    country.guilds[i]._offers[j].Execute();
                }
            }

            country.UpdateCapitulation();
            ReferencesManager.Instance.eventsContainer.UpdateEvents();
        }
        else
        {
            country.UpdateCapitulation();
        }
    }

    private void ProcessRegions(RegionManager region, CountrySettings country)
    {
        if (buildingExpenses >= 0)
        {
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
            if (country.money >= ReferencesManager.Instance.gameSettings.fabric.goldCost &&
                buildingExpenses >= ReferencesManager.Instance.gameSettings.fabric.goldCost)
            {
                if (region.buildings.Count + region.buildingsQueue.Count + 1 <= 4)
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
                if (region.buildings.Count + region.buildingsQueue.Count + 1 <= 4)
                {
                    region.AddBuildingToQueueForce(ReferencesManager.Instance.gameSettings.farm, region);
                    buildingExpenses -= ReferencesManager.Instance.gameSettings.farm.goldCost;
                }
            }

            if (country.money >= ReferencesManager.Instance.gameSettings.chefarm.goldCost &&
            buildingExpenses >= ReferencesManager.Instance.gameSettings.chefarm.goldCost &&
                country.foodNaturalIncome <= 0 || country.foodIncomeUI <= 0)
            {
                if (region.buildings.Count + region.buildingsQueue.Count + 1 <= 4)
                {
                    region.AddBuildingToQueueForce(ReferencesManager.Instance.gameSettings.chefarm, region);
                    buildingExpenses -= ReferencesManager.Instance.gameSettings.chefarm.goldCost;
                }
            }

            if (country.money >= ReferencesManager.Instance.gameSettings.researchLab.goldCost &&
                buildingExpenses >= ReferencesManager.Instance.gameSettings.researchLab.goldCost &&
                country.researchPointsIncome <= 0)
            {
                if (region.buildings.Count + region.buildingsQueue.Count + 1 <= 4)
                {
                    region.AddBuildingToQueueForce(ReferencesManager.Instance.gameSettings.researchLab, region);
                    buildingExpenses -= ReferencesManager.Instance.gameSettings.researchLab.goldCost;
                }
            }
        }
    }

    private void ProcessTechnologies(CountrySettings country)
    {
        researchingExpenses = country.money / 100 * 90; // 90% of all country budget

        for (int i = 0; i < ReferencesManager.Instance.gameSettings.technologies.Length; i++)
        {
            TechnologyScriptableObject tech = ReferencesManager.Instance.gameSettings.technologies[i];

            if (country.countryTechnologies.Any(item => item._type == TechnologyScriptableObject.TechType.Heavy))
            {
                if (country.moneyNaturalIncome >= new System.Random().Next(4000, 6000))
                {
                    if (country.researchPointsIncome >= new System.Random().Next(5, 15))
                    {
                        if (tech._type == TechnologyScriptableObject.TechType.Heavy &&
                        !Researched(country, tech))
                        {
                            if (CanResearch(country, tech))
                            {
                                while (!researching)
                                {
                                    if (!researching)
                                    {
                                        StartRecearch(country, tech);
                                    }
                                }
                            }
                        }
                        else if (tech._type == TechnologyScriptableObject.TechType.Aviation ||
                        tech._type == TechnologyScriptableObject.TechType.AirPlane &&
                        !Researched(country, tech))
                        {
                            if (CanResearch(country, tech))
                            {
                                while (!researching)
                                {
                                    if (!researching)
                                    {
                                        StartRecearch(country, tech);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        if (!researching)
        {
            moneySaving += researchingExpenses;
        }


        if (country.myRegions.Count > 0)
        {
            int recruits_needs = country.myRegions.Count * new System.Random().Next(150, 300);
            if (country.inWar)
            {
                recruits_needs *= 5;
            }
            if (country.recruits < recruits_needs / 2)
            {
                country.mobilizationLaw = Mathf.Clamp(country.mobilizationLaw + new System.Random().Next(1, 3), 0, ReferencesManager.Instance.gameSettings.mobilizationPercent.Length - 1);
                ReferencesManager.Instance.SetRecroots(country.mobilizationLaw, country);
            }
            else if (country.recruits < recruits_needs)
            {
                country.mobilizationLaw = Mathf.Clamp(country.mobilizationLaw + new System.Random().Next(2), 0, ReferencesManager.Instance.gameSettings.mobilizationPercent.Length - 1);
                ReferencesManager.Instance.SetRecroots(country.mobilizationLaw, country);
            }
            else if (country.recruits > recruits_needs && country.recruitsIncome > 0)
            {
                country.mobilizationLaw = Mathf.Clamp(country.mobilizationLaw - 2, 0, ReferencesManager.Instance.gameSettings.mobilizationPercent.Length - 1);
                ReferencesManager.Instance.SetRecroots(country.mobilizationLaw, country);
            }
        }
    }

    private void attack(Aviation_Cell plane, List<RegionManager> regs, int mode, RegionManager regg)
    {
        if (plane.fuel >= plane.AirPlane.fuelperattack)
        {
            if (mode == 0)
            {
                float damage = (float)plane.AirPlane.army_damage;

                foreach (RegionManager reg in regs)
                {
                    if (reg.hasArmy)
                    {
                        while (damage != 0f)
                        {
                            UnitHealth unit = reg.GetDivision(reg).unitsHealth[0];

                            float unithp = unit.health * (1 + (ArmorBreak(unit.unit.armor, plane.AirPlane.armorBreak) / 100)) / (plane.hp / plane.AirPlane.maxhp);

                            if (damage >= unithp)
                            {
                                damage -= unithp;

                                reg.GetDivision(reg).unitsHealth.Remove(unit);

                                plane.hp -= unit.unit.aviationAttack / (1 + (plane.AirPlane.armor / 100));
                            }
                            else
                            {
                                unit.health -= damage * (1 + (ArmorBreak(unit.unit.armor, plane.AirPlane.armorBreak) / 100)) / (plane.hp / plane.AirPlane.maxhp);

                                damage = 0;

                                plane.hp -= unit.unit.aviationAttack / (1 + (plane.AirPlane.armor / 100));
                            }

                            reg.CheckRegionUnits(reg);

                            if (reg.GetDivision(reg).unitsHealth.Count <= 0 || !reg.GetDivision(reg))
                            {
                                reg.hasArmy = false;
                                DisbandDivision(reg.GetDivision(reg));

                                damage = 0;
                            }
                        }
                    }
                }

                ReferencesManager.Instance.aviationManager.regions = regs.ToArray();
                ReferencesManager.Instance.aviationManager.StartExplosionCoroutine(0f);

                plane.fuel -= plane.AirPlane.fuelperattack;


                if (plane.hp <= 0)
                {
                    Aviation_Storage airport = regg.GetComponent<Aviation_Storage>();

                    for (int i = 0; i < airport.planes.Count; i++)
                    {
                        if (airport.planes[i].AirPlane == plane.AirPlane && airport.planes[i].hp <= 0)
                        {
                            airport.planes.Remove(airport.planes[i]);
                        }
                    }
                }

                float ArmorBreak(float armor, float armorbreak)
                {
                    if (armorbreak >= armor)
                    {
                        return 0f;
                    }
                    return armor - armorbreak;
                }
            }

            else if (mode == 1)
            {
                float damage = (float)plane.AirPlane.builds_damage;

                foreach (RegionManager reg in regs)
                {
                    while (reg.buildings.Count != 0)
                    {
                        RegionManager.BuildingQueueItem item = new RegionManager.BuildingQueueItem();
                        item.building = reg.buildings[0];
                        item.region = reg;
                        item.movesLasts = reg.buildings[0].moves * (damage / 100);
                        reg.buildingsQueue.Add(item);
                        reg.buildings.Remove(reg.buildings[0]);
                    }

                    for (int i = 0; i < reg.buildingsQueue.Count; i++)
                    {
                        RegionManager.BuildingQueueItem queueItem = reg.buildingsQueue[i];

                        queueItem.movesLasts -= queueItem.building.moves * (damage / 100);

                        if (queueItem.movesLasts <= 0)
                        {
                            reg.buildingsQueue.Remove(queueItem);
                        }
                    }

                    ReferencesManager.Instance.aviationManager.regions = regs.ToArray();
                    ReferencesManager.Instance.aviationManager.StartExplosionCoroutine(0f);
                }

                plane.fuel -= plane.AirPlane.fuelperattack;


                if (plane.hp <= 0)
                {
                    Aviation_Storage airport = regg.GetComponent<Aviation_Storage>();

                    for (int i = 0; i < airport.planes.Count; i++)
                    {
                        if (airport.planes[i].AirPlane == plane.AirPlane && airport.planes[i].hp <= 0)
                        {
                            airport.planes.Remove(airport.planes[i]);
                        }
                    }
                }
            }
        }
    }

    public List<RegionManager> GetNeiboursOfRegion(RegionManager region)
    {
        List<RegionManager> regions = new List<RegionManager>(10);

        foreach (Transform movePoint in region.movePoints)
        {
            MovePoint movePointComponent = movePoint.GetComponent<MovePoint>();
            if (movePointComponent != null)
            {
                try
                    {
                    RegionManager _region = movePointComponent.regionTo.GetComponent<RegionManager>();
                    if (_region != null)
                    {
                        regions.Add(_region);
                    }
                }
                catch (System.Exception)
                {
                    Debug.LogError($"Error: RegionTo is null {region.gameObject.name}/{movePointComponent.gameObject.name}");
                }
            }
            else
            {
                Debug.Log($"{movePoint.parent.name} -> {movePoint.name}");
            }
        }

        return regions;
    }

    private void BuyAirplanes(RegionManager region)
    {
        if (region._airBaseLevel > 0)
        {
            byte highestFighter = 0;
            byte highestStormFighter = 0;
            byte highestBomber = 0;

            foreach (Aviation_ScriptableObj plane in ReferencesManager.Instance.gameSettings._planes)
            {
                if (CheckPlanePurchase(region.currentCountry, region, plane))
                {
                    switch (plane.type)
                    {
                        case Aviation_ScriptableObj.Type.fighter:
                            highestFighter = (byte)plane.level;
                            break;
                        case Aviation_ScriptableObj.Type.stormtrooper:
                            highestStormFighter = (byte)plane.level;
                            break;
                        case Aviation_ScriptableObj.Type.bomber:
                            highestBomber = (byte)plane.level;
                            break;
                    }
                }
            }

            foreach (Aviation_ScriptableObj plane in ReferencesManager.Instance.gameSettings._planes)
            {
                if (CheckPlanePurchase(region.currentCountry, region, plane))
                {
                    if (plane.type == Aviation_ScriptableObj.Type.fighter)
                    {
                        if (plane.level == highestFighter)
                        {
                            BuyPlane(region, region.currentCountry, plane);
                        }
                    }
                    else if (plane.type == Aviation_ScriptableObj.Type.stormtrooper)
                    {
                        if (plane.level == highestStormFighter)
                        {
                            BuyPlane(region, region.currentCountry, plane);
                        }
                    }
                    else if (plane.type == Aviation_ScriptableObj.Type.bomber)
                    {
                        if (plane.level == highestBomber)
                        {
                            BuyPlane(region, region.currentCountry, plane);
                        }
                    }
                }
            }
        }
    }

    private void BuyPlane(RegionManager region, CountrySettings country, Aviation_ScriptableObj plane)
    {
        Aviation_Storage airBase = region.GetComponent<Aviation_Storage>();

        if (country.money >= plane.price && country.recruits >= plane.recruitsCost)
        {
            if (airBase.planes.Count + 1 <= region._airBaseLevel)
            {
                Aviation_Cell air_cell = new Aviation_Cell(plane, country);
                airBase.planes.Add(air_cell);
            }
        }
    }

    private bool CheckPlanePurchase(CountrySettings country, RegionManager region, Aviation_ScriptableObj plane)
    {
        bool result = false;

        if (country.money >= plane.price && country.recruits >= plane.recruitsCost)
        {
            if (country.countryTechnologies.Contains(plane._tech))
            {
                result = true;
            }
        }

        return result;
    }

    private List<CountrySettings> GetBorderingCountiesWithRegion(RegionManager region)
    {
        List<CountrySettings> countries = new(3);
        try
        {
            for (int i = 0; i < region.movePoints.Count; i++)
            {
                Transform point = region.movePoints[i];
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
        RegionManager regionToReturn = null;

        if (!division.inSea)
        {
            List<float> foundedRoutesDistances = new List<float>(10);
            List<RegionManager> foundedRoutesProvinces = new List<RegionManager>(10);

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
        }

        return regionToReturn;
    }

    private bool isRegionBorderSecondRegion(RegionManager regionA, RegionManager regionB)
    {
        List<RegionManager> borderingRegions = new List<RegionManager>(10);

        for (int i = 0; i < regionA.movePoints.Count; i++)
        {
            borderingRegions.Add(regionA.movePoints[i].GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>());
        }

        bool result = borderingRegions.Contains(regionB);

        return result;
    }

    private void CreateDivisionInRegion(RegionManager region, CountrySettings country)
    {
        region.CheckRegionUnits(region);

        if (region.currentCountry.country._id == country.country._id && !region.hasArmy)
        {
            UnitScriptableObject unit = ReferencesManager.Instance.gameSettings.soldierLVL1;

            if (warExpenses >= unit.moneyCost)
            {
                if (country.money >= unit.moneyCost && country.recruits >= unit.recrootsCost && country.food >= unit.foodCost)
                {
                    if (ReferencesManager.Instance.gameSettings.onlineGame)
                    {
                        Multiplayer.Instance.CreateUnit(region._id);
                    }
                    else
                    {
                        GameObject spawnedUnit = Instantiate(ReferencesManager.Instance.army.unitPrefab, region.transform);
                        spawnedUnit.transform.localScale = ReferencesManager.Instance.army.unitPrefab.transform.localScale;
                        UnitMovement division = spawnedUnit.GetComponent<UnitMovement>();

                        division.currentCountry = country;
                        division.currentProvince = region;
                        division.UpdateInfo();

                        region.hasArmy = true;
                        country.countryUnits.Add(division);

                        AddUnitToArmy(country, ReferencesManager.Instance.gameSettings.soldierLVL1, region, division);

                        CreateDivision(country, region, division);

                        region.CheckRegionUnits(region);
                        ReferencesManager.Instance.countryManager.UpdateValuesUI();
                        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
                    }
                }
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

                    foreach (UnitHealth unit in battle.defenderDivision.unitsHealth)
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
                    foreach (UnitHealth unit in fightRegion.currentDefenseUnits)
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
                catch (System.Exception) { }
            }
        }

        #endregion

        #region Attacker info

        List<float> attackerArmors = new List<float>();

        battle.attackerDivision = attackerUnit;
        battle.myUnits = battle.attackerDivision.unitsHealth;
        battle.fightRegion = fightRegion;

        foreach (UnitHealth unit in attackerUnit.unitsHealth)
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
                winChance = (new System.Random()).Next(47, 49);
            }

            else if (difference >= 15 && difference <= 20)
            {
                winChance = (new System.Random()).Next(45, 47);
            }

            else if (difference >= 20 && difference <= 25)
            {
                winChance = (new System.Random()).Next(43, 45);
            }

            else if (difference >= 25 && difference <= 30)
            {
                winChance = (new System.Random()).Next(41, 43);
            }

            else if (difference >= 35 && difference <= 40)
            {
                winChance = (new System.Random()).Next(39, 41);
            }

            else if (difference >= 45 && difference <= 50)
            {
                winChance = (new System.Random()).Next(37, 39);
            }

            else if (difference >= 55 && difference <= 60)
            {
                winChance = (new System.Random()).Next(35, 37);
            }

            else if (difference >= 65 && difference <= 70)
            {
                winChance = (new System.Random()).Next(30, 37);
            }

            else if (difference >= 75 && difference <= 80)
            {
                winChance = (new System.Random()).Next(10, 25);
            }

            else if (difference >= 85 && difference <= 100)
            {
                winChance = (new System.Random()).Next(0, 15);
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
                winChance = (new System.Random()).Next(51, 53);
            }

            else if (difference > 10 && difference <= 15)
            {
                winChance = (new System.Random()).Next(53, 56);
            }

            else if (difference > 15 && difference <= 20)
            {
                winChance = (new System.Random()).Next(56, 60);
            }

            else if (difference > 25 && difference <= 30)
            {
                winChance = (new System.Random()).Next(60, 64);
            }

            else if (difference > 30 && difference <= 40)
            {
                winChance = (new System.Random()).Next(64, 68);
            }

            else if (difference > 40 && difference <= 50)
            {
                winChance = (new System.Random()).Next(68, 74);
            }

            else if (difference > 50 && difference <= 60)
            {
                winChance = (new System.Random()).Next(74, 78);
            }

            else if (difference > 60 && difference <= 70)
            {
                winChance = (new System.Random()).Next(78, 82);
            }

            else if (difference > 70 && difference <= 80)
            {
                winChance = (new System.Random()).Next(82, 86);
            }

            else if (difference > 80 && difference <= 90)
            {
                winChance = (new System.Random()).Next(86, 90);
            }

            else if (difference > 90 && difference <= 100)
            {
                winChance = (new System.Random()).Next(90, 92);
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
        return GetBorderingCountiesWithRegion(division.currentProvince).Contains(enemy);
    }


    private bool isDivisionInRegion(UnitMovement division, RegionManager region)
    {
        return division.currentProvince == region;
    }

    public void UpgradeInfrastructure(RegionManager region)
    {
        int check = region.infrastructure_Amount + 1;

        if (buildingExpenses >= 200)
        {
            if (region.currentCountry.money >= 200 && check <= (new System.Random()).Next(5, 11))
            {
                region.infrastructure_Amount++;
                region.currentCountry.money -= 200;
                region.currentCountry.moneyNaturalIncome += 8;

                buildingExpenses -= 200;
            }

            if (ReferencesManager.Instance.gameSettings.onlineGame)
            {
                Multiplayer.Instance.SetCountryValues(
                    region.currentCountry.country._id,
                    region.currentCountry.money,
                    region.currentCountry.food,
                    region.currentCountry.recruits);
            }
        }
    }
    public void UpgradeFortification(RegionManager region)
    {
        int check = region.fortifications_Amount + 1;
        int cost = 0;
        if (region.fortifications_Amount == 0)
        {
            cost = 400;
        }
        else
        {
            cost = 400 * region.fortifications_Amount;
        }
        if (buildingExpenses >= cost)
        {
            float multiplier = 1;
            multiplier *= mult;
            foreach (CountrySettings other_country in GetBorderingCountiesWithRegion(region))
            {
                multiplier *= region.currentCountry.myRegions.Count / other_country.myRegions.Count;
            }
            if (region.currentCountry.money >= cost && check <= (new System.Random()).Next(1, 2))
            {
                region.fortifications_Amount++;
                region.currentCountry.money -= cost;

                buildingExpenses -= cost;
            }

            if (ReferencesManager.Instance.gameSettings.onlineGame)
            {
                Multiplayer.Instance.SetCountryValues(
                    region.currentCountry.country._id,
                    region.currentCountry.money,
                    region.currentCountry.food,
                    region.currentCountry.recruits
                );
            }
        }
    }

    public void UpgradeAirBase(RegionManager region)
    {
        if (new System.Random().Next(0, 3) == 0)
        {
            int check = region._airBaseLevel + 1;

            int cost = 0;

            if (region._airBaseLevel == 0)
            {
                cost = 1000;
            }
            else
            {
                cost = 1000 * region._airBaseLevel;
            }

            if (region.currentCountry.money >= cost && check <= ReferencesManager.Instance.gameSettings._airBaseMaxLevel)
            {
                region._airBaseLevel++;
                region.gameObject.AddComponent<Aviation_Storage>();
                region.currentCountry.money -= cost;
                buildingExpenses -= cost;
            }

            if (ReferencesManager.Instance.gameSettings.onlineGame)
            {
                Multiplayer.Instance.SetCountryValues(
                    region.currentCountry.country._id,
                    region.currentCountry.money,
                    region.currentCountry.food,
                    region.currentCountry.recruits);
            }
        }
    }
    private float GetDistance(RegionManager regionA, RegionManager regionB)
    {
        return Vector2.Distance(regionA.transform.position, regionB.transform.position);
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
                    random = (new System.Random()).Next(0, 25);

                    if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.artileryLVL1) ||
                        HasUnitTech(country, ReferencesManager.Instance.gameSettings.artileryLVL2))
                    {
                        random = (new System.Random()).Next(25, 100);
                    }
                }
                else if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL1) ||
                    HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL2))
                {
                    random = (new System.Random()).Next(101, 150);
                }
                else if (!HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL1) &&
                    !HasUnitTech(country, ReferencesManager.Instance.gameSettings.tankLVL2))
                {
                    if (HasUnitTech(country, ReferencesManager.Instance.gameSettings.motoLVL1) ||
                        HasUnitTech(country, ReferencesManager.Instance.gameSettings.motoLVL2))
                    {
                        random = (new System.Random()).Next(151, 200);
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
                            country.recruits >= unit.recrootsCost &&
                            country.food >= unit.foodCost)
                        {
                            country.money -= unit.moneyCost;
                            country.food -= unit.foodCost;
                            country.recruits -= unit.recrootsCost;
                            country.moneyNaturalIncome -= unit.moneyIncomeCost;
                            country.foodNaturalIncome -= unit.foodIncomeCost;

                            warExpenses -= unit.moneyCost;

                            UnitHealth newUnitHealth = new UnitHealth();
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

                            if (ReferencesManager.Instance.gameSettings.developerMode) Debug.Log($"Добавил {unit.name} для {country.country._uiName} в регионе {region}");
                        }
                    }
                }
            }
        }
    }

    private void RemoveUnitFromArmy(CountrySettings country, int index, RegionManager region, UnitMovement unitMovement)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.RemoveUnitFromArmy(unitMovement.unitsHealth[index]._id, region._id);
        }
        else
        {
            if (region.currentCountry.country._id == country.country._id)
            {
                if (unitMovement.unitsHealth.Count > 0)
                {
                    unitMovement.unitsHealth.Remove(unitMovement.unitsHealth[index]);
                }
            }
        }
    }

    private void DisbandDivision(UnitMovement division)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.DisbandDivision(
                division.currentProvince._id,
                division.currentCountry.country._id);
        }
        else
        {
            Destroy(division.gameObject);
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
                TechQueue techQueue = new()
                {
                    tech = tech,
                    moves = tech.moves
                };

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
            country.recruits);
    }

    public void SendOffer(string offer, CountrySettings sender, CountrySettings receiver)
    {
        if (offer == "Торговля")
        {
            int relationsRandom = (new System.Random()).Next(10, 15);

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
                diplomatyUI.SpawnEvent("Торговля", sender, receiver, true);
            }
        }
        else if (offer == "Пакт о ненападении")
        {
            int relationsRandom = (new System.Random()).Next(10, 15);

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
                diplomatyUI.SpawnEvent("Пакт о ненападении", sender, receiver, true);
            }
        }
        else if (offer == "Разорвать пакт о ненападении")
        {
            int relationsRandom = (new System.Random()).Next(10, 15);

            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            if (!receiver.isPlayer)
            {
                senderToReceiver.pact = false;
                receiverToSender.pact = false;

                senderToReceiver.relationship -= relationsRandom;
                receiverToSender.relationship -= relationsRandom;

            }
            else if (receiver.isPlayer)
            {
                diplomatyUI.SpawnEvent("Разорвать пакт о ненападении", sender, receiver, false);
            }
        }
        else if (offer == "Право прохода войск")
        {
            int relationsRandom = (new System.Random()).Next(10, 15);

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
                diplomatyUI.SpawnEvent("Право прохода войск", sender, receiver, true);
            }
        }
        else if (offer == "Объявить войну")
        {
            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            senderToReceiver.war = true;
            senderToReceiver.trade = false;
            senderToReceiver.right = false;
            senderToReceiver.pact = false;
            senderToReceiver.union = false;

            sender.enemies.Add(receiver);
            receiver.enemies.Add(sender);

            if (sender.myRegions.Count > 0 && receiver.myRegions.Count > 0)
            {
                sender.stability.buffs.Add(new Stability_buff("Наступательная война", (-15 * (receiver.myRegions.Count / sender.myRegions.Count)) * (1 / receiver.enemies.Count), new List<string>() { $"not;ongoing_war;{sender.country._id}" }, null, ReferencesManager.Instance.sprites.Find("offensive_war")));
                receiver.stability.buffs.Add(new Stability_buff("Оборонительная война", -5f, new List<string>() { $"not;ongoing_war;{receiver.country._id}" }, null, ReferencesManager.Instance.sprites.Find("defensive_war")));
            }

            sender.inWar = true;
            receiver.inWar = true;

            receiverToSender.war = true;
            receiverToSender.trade = false;
            receiverToSender.right = false;
            receiverToSender.pact = false;
            receiverToSender.union = false;

            senderToReceiver.relationship -= 100;
            receiverToSender.relationship -= 100;

            if (receiver.isPlayer)
            {
                diplomatyUI.SpawnEvent("Объявить войну", sender, receiver, false);
            }
        }
        else if (offer == "Союз")
        {
            int relationsRandom = (new System.Random()).Next(40, 70);

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
                diplomatyUI.SpawnEvent("Союз", sender, receiver, true);
            }
        }
    }

    private bool isCountriesAreNeibours(CountrySettings countryA, CountrySettings countryB)
    {
        bool result = false;

        foreach (RegionManager region in countryA.myRegions)
        {
            foreach (CountrySettings _country in GetBorderingCountiesWithRegion(region))
            {
                if (_country.country._id == countryB.country._id)
                {
                    result = true;
                }
            }
        }

        return result;
    }
}

[System.Serializable]
public class TechQueue
{
    public TechnologyScriptableObject tech;
    public int moves;
}