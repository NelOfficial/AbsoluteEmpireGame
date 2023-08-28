using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] List<Transform> nearestPoints = new List<Transform>();

    [HideInInspector] public CountryManager countryManager;
    [HideInInspector] public RegionManager regionManager;
    public RegionManager currentProvince;
    [HideInInspector] public Army army;

    public CountrySettings currentCountry;
    public List<UnitHealth> unitsHealth = new List<UnitHealth>();

    private UnitMovement attackerUnit;
    private UnitMovement defenderUnit;
    private RegionManager defenderRegion;
    private bool attackerWon;

    private MovePoint[] movePoints;

    public bool isSelected;

    public int fortification;
    public int _movePoints;

    public bool firstMove;

    private int winChance;
    private int offset;
    private int motoInfantry;

    [SerializeField] Image flagImage;

    private void Awake()
    {
        army = FindObjectOfType<Army>();
        countryManager = FindObjectOfType<CountryManager>();
        regionManager = FindObjectOfType<RegionManager>();

        MovePoint[] movePoints = FindObjectsOfType<MovePoint>();

        foreach (MovePoint movePoint in movePoints)
        {
            movePoint.attackerUnit = this;
        }

        //currentProvince = this.transform.parent.GetComponent<RegionManager>();
    }

    public void ShowClosestsPoints()
    {
        nearestPoints.Clear();

        foreach (Transform point in regionManager.currentRegionManager.movePoints)
        {
            nearestPoints.Add(point);
            point.gameObject.AddComponent<SpriteRenderer>();

            if (countryManager.currentCountry == point.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>().currentCountry)
            {
                point.gameObject.GetComponent<SpriteRenderer>().sprite = army.pointSprite;
                point.GetComponent<MovePoint>().regionTo.GetComponent<SpriteRenderer>().color =
                    new Color(
                    ReferencesManager.Instance.gameSettings.greenColor.r,
                    ReferencesManager.Instance.gameSettings.greenColor.g,
                    ReferencesManager.Instance.gameSettings.greenColor.b, 0.5f);
            }
            else
            {
                foreach (Relationships.Relation realtion in countryManager.currentCountry.
                GetComponent<Relationships>().relationship)
                {
                    if (realtion.country == point.GetComponent<MovePoint>().
                        regionTo.GetComponent<RegionManager>().
                        currentCountry)
                    {
                        if (realtion.war)
                        {
                            point.gameObject.GetComponent<SpriteRenderer>().sprite = army.attackSprite;
                            point.GetComponent<MovePoint>().regionTo.GetComponent<SpriteRenderer>().color =
                                new Color(
                                ReferencesManager.Instance.gameSettings.redColor.r,
                                ReferencesManager.Instance.gameSettings.redColor.g,
                                ReferencesManager.Instance.gameSettings.redColor.b, 0.5f);
                        }
                        else if (realtion.right)
                        {
                            point.gameObject.GetComponent<SpriteRenderer>().sprite = army.pointSprite;
                            point.GetComponent<MovePoint>().regionTo.GetComponent<SpriteRenderer>().color =
                                new Color(
                                ReferencesManager.Instance.gameSettings.greenColor.r,
                                ReferencesManager.Instance.gameSettings.greenColor.g,
                                ReferencesManager.Instance.gameSettings.greenColor.b, 0.5f);
                        }
                        else if (!realtion.right)
                        {
                            point.gameObject.GetComponent<SpriteRenderer>().sprite = army.chatSprite;
                            point.GetComponent<MovePoint>().regionTo.GetComponent<SpriteRenderer>().color =
                                new Color(
                                ReferencesManager.Instance.gameSettings.blueColor.r,
                                ReferencesManager.Instance.gameSettings.blueColor.g,
                                ReferencesManager.Instance.gameSettings.blueColor.b, 0.5f);
                        }
                    }
                }
            }

            point.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 99;
        }
    }

    public void RemoveClosestsPoints()
    {
        nearestPoints.Clear();

        foreach (Transform point in regionManager.currentRegionManager.movePoints)
        {
            nearestPoints.Add(point);
            regionManager.currentRegionManager.SelectRegionNoHit(point.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>());
            Destroy(point.gameObject.GetComponent<SpriteRenderer>());
        }
    }

    public void AIMoveNoHit(RegionManager defenderRegion, RegionManager attackerRegion)
    {
        if (currentCountry.exist && this != null && attackerRegion != null)
        {
            if (defenderRegion.currentCountry == attackerRegion.currentCountry) // Just move
            { // my country
                this._movePoints--;
                this.firstMove = false;
                MoveUnit(defenderRegion, attackerRegion);

                attackerRegion.hasArmy = false;
                defenderRegion.hasArmy = true;

                this.currentProvince = defenderRegion;
            }
            else
            {
                foreach (Relationships.Relation realtion in attackerRegion.currentCountry.
                GetComponent<Relationships>().relationship)
                {
                    if (realtion.country == defenderRegion.currentCountry)
                    {
                        if (realtion.war) // War
                        {
                            this._movePoints--;
                            this.firstMove = false;

                            Fight(defenderRegion, attackerRegion);
                        }
                        else if (realtion.right)
                        {
                            this._movePoints--;
                            this.firstMove = false;
                            MoveUnit(defenderRegion, attackerRegion);
                            attackerRegion.hasArmy = false;
                            defenderRegion.hasArmy = true;

                            this.currentProvince = defenderRegion;
                        }
                        else if (!realtion.right)
                        {

                        }
                    }
                }
            }
        }
    }

    private void MoveUnit(RegionManager regionTo, RegionManager oldRegion)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.MoveUnit(oldRegion._id, regionTo._id);
        }
        else
        {
            oldRegion.hasArmy = false;

            this.transform.position = regionTo.transform.position;

            this.transform.SetParent(regionTo.transform);

            RegionManager newRegion = regionTo;

            newRegion.hasArmy = true;
        }
    }

    private void Fight(RegionManager fightRegion, RegionManager oldRegion)
    {
        List<UnitHealth> enemyUnits = new List<UnitHealth>();
        List<UnitHealth> myUnits = new List<UnitHealth>();

        int defenderForts = 0;
        float attackerFortsDebuff = 0;

        float enemyDamage = 0;
        float myDamage = 0;

        int myInfantry = 0;
        int enemyInfantry = 0;

        motoInfantry = 0;
        int enemyMotoInfantry = 0;

        int myArtilery = 0;
        int enemyArtilery = 0;

        int myHeavy = 0;
        int enemyHeavy = 0;

        winChance = 0;

        defenderForts = fightRegion.fortifications_Amount * 5 / 100;
        attackerFortsDebuff = myDamage * defenderForts;

        if (fightRegion.hasArmy)
        {
            if (fightRegion.transform.Find("Unit(Clone)"))
            {
                UnitMovement fightRegionEnemyUnitMovement = fightRegion.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();
                foreach (UnitHealth unit in fightRegionEnemyUnitMovement.unitsHealth)
                {
                    enemyUnits.Add(unit);
                    enemyDamage += unit.unit.damage;

                    //if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
                    //{
                    //    if (fightRegion.regionTerrain.INF_attackBonus < 0)
                    //    {
                    //        enemyDamage -= unit.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.INF_attackBonus / 100));
                    //    }
                    //    if (fightRegion.regionTerrain.INF_attackBonus > 0)
                    //    {
                    //        enemyDamage += unit.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.INF_attackBonus / 100));
                    //    }
                    //}
                    //else if (unit.unit.type == UnitScriptableObject.Type.ARTILERY)
                    //{
                    //    if (fightRegion.regionTerrain.ART_attackBonus < 0)
                    //    {
                    //        enemyDamage -= unit.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.ART_attackBonus / 100));
                    //    }
                    //    if (fightRegion.regionTerrain.ART_attackBonus > 0)
                    //    {
                    //        enemyDamage += unit.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.ART_attackBonus / 100));
                    //    }
                    //}
                    //else if (unit.unit.type == UnitScriptableObject.Type.TANK)
                    //{
                    //    if (fightRegion.regionTerrain.HEA_attackBonus < 0)
                    //    {
                    //        enemyDamage -= unit.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.HEA_attackBonus / 100));
                    //    }
                    //    if (fightRegion.regionTerrain.HEA_attackBonus > 0)
                    //    {
                    //        enemyDamage += unit.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.HEA_attackBonus / 100));
                    //    }
                    //}
                }
            }
        }
        else if (!fightRegion.hasArmy)
        {
            foreach (UnitScriptableObject unit in fightRegion.currentDefenseUnits)
            {
                //enemyUnits.Add(unit);
                enemyDamage += unit.damage;
            }
        }

        foreach (UnitHealth unitHealth in unitsHealth)
        {
            myUnits.Add(unitHealth);
            myDamage += unitHealth.unit.damage;

            //if (unitHealth.unit.type == UnitScriptableObject.Type.SOLDIER)
            //{
            //    if (fightRegion.regionTerrain.INF_attackBonus < 0)
            //    {
            //        myDamage -= unitHealth.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.INF_attackBonus / 100));
            //    }
            //    if (fightRegion.regionTerrain.INF_attackBonus > 0)
            //    {
            //        myDamage += unitHealth.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.INF_attackBonus / 100));
            //    }
            //}
            //else if (unitHealth.unit.type == UnitScriptableObject.Type.ARTILERY)
            //{
            //    if (fightRegion.regionTerrain.ART_attackBonus < 0)
            //    {
            //        myDamage -= unitHealth.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.ART_attackBonus / 100));
            //    }
            //    if (fightRegion.regionTerrain.ART_attackBonus > 0)
            //    {
            //        myDamage += unitHealth.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.ART_attackBonus / 100));
            //    }
            //}
            //else if (unitHealth.unit.type == UnitScriptableObject.Type.TANK)
            //{
            //    if (fightRegion.regionTerrain.HEA_attackBonus < 0)
            //    {
            //        myDamage -= unitHealth.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.HEA_attackBonus / 100));
            //    }
            //    if (fightRegion.regionTerrain.HEA_attackBonus > 0)
            //    {
            //        myDamage += unitHealth.unit.damage * (1.0f + ((float)fightRegion.regionTerrain.HEA_attackBonus / 100));
            //    }
            //}
        }

        myDamage = myDamage - attackerFortsDebuff;
        army.defenderBonus[1].text = $"+ {defenderForts}%";

        myDamage = Mathf.Abs(myDamage);

        if (myDamage < enemyDamage)
        {
            float difference = Mathf.Abs(enemyDamage) - Mathf.Abs(myDamage);
            difference = Mathf.Abs(difference * 100 / enemyDamage);

            if (difference >= 5 && difference <= 15)
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

        firstMove = false;

        if (motoInfantry >= 6)
        {
            CheckMotorize(this);
        }

        _movePoints--;


        if (winChance >= 50)
        {
            foreach (Transform child in fightRegion.transform)
            {
                if (child.GetComponent<UnitMovement>())
                {
                    UnitMovement division = child.GetComponent<UnitMovement>();
                    division.Retreat();
                }
            }

            MoveUnit(fightRegion, oldRegion);
            ReferencesManager.Instance.AnnexRegion(fightRegion, oldRegion.currentCountry);
        }

        CountLosses(winChance, offset);
    }

    private void CheckMotorize(UnitMovement unitMovement)
    {
        if (unitMovement.unitsHealth.Count > 0)
        {
            int myInfantry = 0;
            int myMotoInfantry = 0;
            int myArtilery = 0;
            int myHeavy = 0;

            foreach (UnitHealth unit in unitMovement.unitsHealth)
            {
                if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
                {
                    myInfantry++;
                }
                if (unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                {
                    myMotoInfantry++;
                }
                if (unit.unit.type == UnitScriptableObject.Type.ARTILERY)
                {
                    myArtilery++;
                }
                if (unit.unit.type == UnitScriptableObject.Type.TANK)
                {
                    myHeavy++;
                }
            }

            if (myMotoInfantry >= 6 || myHeavy >= 6)
            {
                if (unitMovement.firstMove)
                {
                    unitMovement._movePoints++;
                }
            }
        }
    }

    private void CountLosses(float winChance, int offset)
    {
        int damageToAttacker = 0;
        int damageToDefender = 0;

        int random = Random.Range(0, 100);

        if (winChance >= 50) // Атакующий победил
        {
            if (winChance >= 50 && winChance < 57)
            {
                if (random >= offset) // -1 art
                {
                    damageToDefender = 30;
                }
            }
            else if (winChance >= 50 && winChance < 57)
            {
                if (random >= offset) // -1 art; -1 infantry
                {
                    damageToAttacker = 50;
                }
            }
            else if (winChance >= 57 && winChance < 63)
            {
                if (random >= offset) // -1 art; -2 infantry
                {
                    damageToAttacker = 70;
                }
            }
            else if (winChance >= 63 && winChance < 70)
            {
                if (random >= offset) // -2 art; -3 infantry
                {
                    damageToAttacker = 90;
                }
            }
            else if (winChance >= 70 && winChance < 80)
            {
                if (random >= offset) // -2 art; -4 infantry
                {
                    damageToAttacker = 130;
                }
            }
            else if (winChance >= 80 && winChance < 90)
            {
                if (random >= offset) // -2 art; -4 infantry
                {
                    damageToAttacker = 200;
                }
            }
            else if (winChance >= 90)
            {
                if (random >= offset) // -2 art; -4 infantry
                {
                    damageToAttacker = 500;
                }
            }
        }
        else if (winChance < 50) // Защитник победил
        {
            if (winChance >= 43 && winChance < 50)
            {
                if (random >= offset) // -1 art
                {
                    damageToAttacker = 30;
                }
            }
            else if (winChance >= 39 && winChance < 43)
            {
                if (random >= offset) // -1 art; -1 infantry
                {
                    damageToAttacker = 50;
                }
            }
            else if (winChance >= 37 && winChance < 39)
            {
                if (random >= offset) // -1 art; -2 infantry
                {
                    damageToAttacker = 70;
                }
            }
            else if (winChance >= 30 && winChance < 37)
            {
                if (random >= offset) // -1 art; -2 infantry
                {
                    damageToAttacker = 90;
                }
            }
            else if (winChance >= 25f && winChance < 30f)
            {
                if (random >= offset)
                {
                    damageToAttacker = 130;
                }
            }
            else if (winChance >= 10f && winChance < 25f)
            {
                if (random >= offset)
                {
                    damageToAttacker = 200;
                }
            }
            else if (winChance >= 0f && winChance < 10f)
            {
                if (random >= offset)
                {
                    damageToAttacker = 500;
                }
            }

            ApplyDamage(damageToAttacker, damageToDefender);
        }
    }

    private void ApplyDamage(float damageToAttacker, float damageToDefender)
    {
        if (winChance >= 50)
        {
            attackerWon = true;
        }

        else if (winChance < 50)
        {
            attackerWon = false;
        }

        if (attackerWon)
        {
            if (defenderUnit != null)
            {
                for (int j = 0; j < defenderUnit.unitsHealth.Count; j++)
                {
                    defenderUnit.unitsHealth[j].health -= damageToDefender;

                    if (defenderUnit.unitsHealth[j].health <= 0)
                    {
                        currentCountry.moneyNaturalIncome += defenderUnit.unitsHealth[j].unit.moneyIncomeCost;
                        currentCountry.foodNaturalIncome += defenderUnit.unitsHealth[j].unit.foodIncomeCost;
                        defenderUnit.unitsHealth.Remove(defenderUnit.unitsHealth[j]);

                        if (defenderUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.SOLDIER || defenderUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                        {
                            army.defenderArmyLossesValue[0]++;
                        }
                        else if (defenderUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.ARTILERY)
                        {
                            army.defenderArmyLossesValue[1]++;
                        }
                        else if (defenderUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.TANK)
                        {
                            army.defenderArmyLossesValue[2]++;
                        }
                    }
                }
                if (defenderUnit.unitsHealth.Count < 1)
                {
                    currentProvince = transform.parent.GetComponent<RegionManager>();
                    currentProvince.CheckRegionUnits(currentProvince);
                }
            }
        }
        else if (!attackerWon)
        {
            for (int j = 0; j < unitsHealth.Count; j++)
            {
                unitsHealth[j].health -= damageToAttacker;

                if (unitsHealth[j].health <= 0)
                {
                    currentCountry.moneyNaturalIncome += unitsHealth[j].unit.moneyIncomeCost;
                    currentCountry.foodNaturalIncome += unitsHealth[j].unit.foodIncomeCost;

                    if (unitsHealth[j].unit.type == UnitScriptableObject.Type.SOLDIER || unitsHealth[j].unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                    {
                        army.attackerArmyLossesValue[0]++;
                    }
                    else if (unitsHealth[j].unit.type == UnitScriptableObject.Type.ARTILERY)
                    {
                        army.attackerArmyLossesValue[1]++;
                    }
                    else if (unitsHealth[j].unit.type == UnitScriptableObject.Type.TANK)
                    {
                        army.attackerArmyLossesValue[2]++;
                    }
                    unitsHealth.Remove(unitsHealth[j]);
                }
            }
            if (unitsHealth.Count < 1)
            {
                currentProvince = transform.parent.GetComponent<RegionManager>();
                currentProvince.CheckRegionUnits(currentProvince);
            }
        }

        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.SetCountryIncomeValues(
                defenderUnit.currentCountry.country._id,
                defenderUnit.currentCountry.moneyNaturalIncome,
                defenderUnit.currentCountry.foodNaturalIncome,
                defenderUnit.currentCountry.recrootsIncome);

            Multiplayer.Instance.SetCountryIncomeValues(
                currentCountry.country._id,
                currentCountry.moneyNaturalIncome, 
                currentCountry.foodNaturalIncome, 
                currentCountry.recrootsIncome);
        }
    }

    public void UpdateInfo()
    {
        flagImage.sprite = currentCountry.country.countryFlag;
    }

    public void Retreat()
    {
        RegionManager regionToRetreat;
        RegionManager myRegion = transform.parent.GetComponent<RegionManager>();
        MovePoint _point;

        if (myRegion.hasArmy)
        {
            bool reatreated = false;

            List<Transform> nearMyPoints = new List<Transform>();

            for (int i = 0; i < myRegion.movePoints.Count; i++)
            {
                _point = myRegion.movePoints[i].GetComponent<MovePoint>();

                if (_point.regionTo.GetComponent<RegionManager>().currentCountry == myRegion.currentCountry)
                {
                    nearMyPoints.Add(_point.transform);
                }
            }

            for (int i = 0; i < nearMyPoints.Count; i++) // Есть куда отступать
            {
                if (nearMyPoints.Count > 0)
                {
                    regionToRetreat = nearMyPoints[i].GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>();

                    if (!regionToRetreat.hasArmy)
                    {
                        _movePoints++;
                        AIMoveNoHit(regionToRetreat, myRegion);
                        reatreated = true;
                    }
                    else if (regionToRetreat.hasArmy) // Есть регион куда отступить, но там есть армия
                    {
                        this.Destroy();
                    }
                }
            }

            if (nearMyPoints.Count <= 0) // Нет путей
            {
                this.GetComponent<Animator>().Play("Encircled");
            }
        }
        else
        {
            myRegion.CheckRegionUnits(myRegion);
        }
    }

    public bool Encircled(RegionManager fightRegion)
    {
        bool isEncircled = false;
        MovePoint _point;

        if (fightRegion.hasArmy) // Если есть армия в боевом регионе
        {
            List<Transform> nearMyPoints = new List<Transform>();

            for (int i = 0; i < fightRegion.GetComponent<RegionManager>().movePoints.Count; i++) // Берем все точки в боевом регионе
            {
                _point = fightRegion.GetComponent<RegionManager>().movePoints[i].GetComponent<MovePoint>(); // Точка = точка в боевом регионе

                if (_point.regionTo.GetComponent<RegionManager>().currentCountry == fightRegion.currentCountry) // Если точка ведёт в нашу страну
                {
                    nearMyPoints.Add(_point.transform);
                }
            }

            for (int i = 0; i < nearMyPoints.Count; i++)
            {
                if (nearMyPoints.Count > 0) // Если такие точки есть, то дивизия не окружена
                {
                    isEncircled = false;
                }
            }

            if (nearMyPoints.Count < 1) // Если нет таких точек, то дивизия окружена
            {
                isEncircled = true;
            }
        }

        return isEncircled;
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }

    [System.Serializable]
    public class UnitHealth
    {
        public int _id;
        public float health;
        public UnitScriptableObject unit;
    }
}
