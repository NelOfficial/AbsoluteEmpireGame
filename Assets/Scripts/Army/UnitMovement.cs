using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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
            point.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "RegionInfo";
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
        UnitMovement.BattleInfo battle = new UnitMovement.BattleInfo();

        winChance = 0;

        battle.defenderForts = fightRegion.fortifications_Amount;

        #region Defender info

        if (fightRegion.hasArmy)
        {
            try
            {
                List<float> _armors = new List<float>();
                UnitMovement fightRegionUnitMovement = fightRegion.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();

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

                float _maxArmor = _armors.Max();
                float _midArmor = battle.defenderArmor / battle.defenderDivision.unitsHealth.Count;

                battle.defenderHardness = battle.defenderHardness / battle.defenderDivision.unitsHealth.Count;
                battle.defenderArmor = 0.4f * _maxArmor + 0.6f * _midArmor;
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
            battle.enemyUnits = battle.defenderDivision.unitsHealth;
            battle.fightRegion = fightRegion;

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

            float maxArmor = defenderArmors.Max();
            float midArmor = battle.defenderArmor / battle.defenderDivision.unitsHealth.Count;

            battle.defenderHardness = battle.defenderHardness / battle.defenderDivision.unitsHealth.Count;
            battle.defenderArmor = 0.4f * maxArmor + 0.6f * midArmor;
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

        float attackerMaxArmor = attackerArmors.Max();
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


        firstMove = false;

        if (battle.motoInfantry >= 6)
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

    [System.Serializable]
    public class BattleInfo
    {
        public int _id;

        public List<UnitHealth> enemyUnits = new List<UnitHealth>();
        public List<UnitHealth> myUnits = new List<UnitHealth>();

        public int myInfantry = 0;
        public int enemyInfantry = 0;

        public int motoInfantry = 0;
        public int enemyMotoInfantry = 0;

        public int myArtilery = 0;
        public int enemyArtilery = 0;

        public int myHeavy = 0;
        public int enemyHeavy = 0;

        public int defenderForts = 0;

        public float attackerSoftAttack;
        public float attackerHardAttack;
        public float attackerDefense;
        public float attackerArmor;
        public float attackerHardness;
        public float attackerArmorPiercing;

        public float defenderSoftAttack;
        public float defenderHardAttack;
        public float defenderDefense;
        public float defenderArmor;
        public float defenderHardness;
        public float defenderArmorPiercing;

        public float defender_BONUS_DEFENCE = 0;
        public float defender_BONUS_ATTACK = 0;

        public float attacker_BONUS_DEFENCE = 0;
        public float attacker_BONUS_ATTACK = 0;

        public RegionManager fightRegion;
        public RegionManager attackerRegion;

        public UnitMovement defenderDivision;
        public UnitMovement attackerDivision;

        public float attackerStrength;
        public float defenderStrength;
    }
}
