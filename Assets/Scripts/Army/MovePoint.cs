using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using static UnityEngine.UI.CanvasScaler;

public class MovePoint : MonoBehaviour
{
    [HideInInspector] public UnitMovement attackerUnit;
    [HideInInspector] public UnitMovement defenderUnit;

    [HideInInspector] public MovePoint currentMovePoint;

    private RegionManager newRegion;
    private RegionManager attackerRegion;
    private Transform _annexedRegion;

    [HideInInspector] public Transform regionTo;
    [HideInInspector] public RegionManager parentRegion;

    private float winChance;
    private MovePoint point;
    private RaycastHit2D hit;

    private bool attackerWon;
    private int offset;

    [SerializeField] bool noAutoCollider;


    private void OnMouseDown()
    {
        UnitAction();
    }

    private void Start()
    {
        SetRegionInPoint();
    }

    private void SetRegionInPoint()
    {
        RaycastHit2D _hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.down);
        if (_hit.collider.gameObject.GetComponent<RegionManager>())
        {
            regionTo = _hit.collider.transform;
            parentRegion = transform.parent.GetComponent<RegionManager>();
            if (!noAutoCollider)
            {
                this.GetComponent<PolygonCollider2D>().points = regionTo.GetComponent<PolygonCollider2D>().points;
            }

            this.transform.position = regionTo.position;

            //parentRegion.movePoints.Clear();
            //foreach (Transform child in parentRegion.transform)
            //{
            //    parentRegion.movePoints.Add(child);
            //}
        }
    }

    private void UnitAction()
    {
        Vector2 mainCamera = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        hit = Physics2D.Raycast(mainCamera, Input.mousePosition);

        ReferencesManager.Instance.regionUI.currentMovePoint = this;

        if (hit.collider)
        {
            currentMovePoint = this;
            if (regionTo.GetComponent<RegionManager>().currentCountry == ReferencesManager.Instance.countryManager.currentCountry) // Just move
            { // my country

                MoveUnit(hit, false, null, true);

                ReferencesManager.Instance.regionManager.moveMode = false;
                ReferencesManager.Instance.regionUI.barContent.SetActive(true);

                ReferencesManager.Instance.regionUI.ToggleColliders(true);
                ReferencesManager.Instance.regionUI.DeMoveUnitMode(false);
            }
            else
            {
                foreach (Relationships.Relation realtion in ReferencesManager.Instance.countryManager.currentCountry.
                GetComponent<Relationships>().relationship)
                {
                    if (realtion.country == regionTo.GetComponent<RegionManager>().
                        currentCountry)
                    {
                        if (realtion.war) // war
                        {
                            newRegion = regionTo.GetComponent<RegionManager>();
                            attackerRegion = this.transform.parent.GetComponent<RegionManager>();

                            foreach (Transform child in attackerRegion.transform)
                            {
                                if (child.name == "Unit(Clone)")
                                {
                                    attackerUnit = child.GetComponent<UnitMovement>();
                                }
                            }

                            if (newRegion.hasArmy)
                            {
                                foreach (Transform child in regionTo)
                                {
                                    if (child.name == "Unit(Clone)")
                                    {
                                        defenderUnit = child.GetComponent<UnitMovement>();
                                    }
                                }
                            }

                            Fight(newRegion, hit);

                            ReferencesManager.Instance.regionManager.moveMode = false;
                            ReferencesManager.Instance.regionUI.barContent.SetActive(true);
                            ReferencesManager.Instance.regionUI.DeMoveUnitMode(false);
                        }
                        else if (realtion.right)
                        {
                            MoveUnit(hit, false, null, true);

                            ReferencesManager.Instance.regionManager.moveMode = false;
                            ReferencesManager.Instance.regionUI.barContent.SetActive(true);

                            ReferencesManager.Instance.regionUI.ToggleColliders(true);
                            ReferencesManager.Instance.regionUI.DeMoveUnitMode(false);
                        }
                        else if (!realtion.right)
                        {
                            ReferencesManager.Instance.diplomatyUI.OpenUINoClick(realtion.country.country._id);

                            ReferencesManager.Instance.regionManager.moveMode = false;
                            ReferencesManager.Instance.regionUI.barContent.SetActive(true);

                            ReferencesManager.Instance.regionUI.ToggleColliders(true);
                            ReferencesManager.Instance.regionUI.DeMoveUnitMode(false);

                            ReferencesManager.Instance.regionManager.SelectRegionNoHit(attackerUnit.currentProvince);
                        }
                    }
                }
            }
        }
    }

    private void CheckMotorize(UnitMovement unitMovement)
    {
        if (unitMovement.unitsHealth.Count > 0)
        {
            int myInfantry = 0;
            int myMotoInfantry = 0;
            int myArtilery = 0;
            int myHeavy = 0;

            foreach (UnitMovement.UnitHealth unit in unitMovement.unitsHealth)
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

    private void Fight(RegionManager fightRegion, RaycastHit2D hit)
    {
        UnitMovement.BattleInfo battle = new UnitMovement.BattleInfo();

        winChance = 0;

        battle.defenderForts = newRegion.fortifications_Amount;

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

        ReferencesManager.Instance.regionUI.winChance = winChance;
        ReferencesManager.Instance.regionUI.hit = hit;
        ReferencesManager.Instance.regionUI.unitMovement = attackerUnit; // Атакующий юнит, с него возьмуться очки движения
        ReferencesManager.Instance.regionUI.actionRegion = newRegion;

        ReferencesManager.Instance.regionUI.fightPanelContainer.SetActive(true);
        ReferencesManager.Instance.regionUI.resultPanel.SetActive(false);

        ReferencesManager.Instance.regionUI.winChanceBarInner.fillAmount = winChance / 100;
        ReferencesManager.Instance.regionUI.winChanceText.text = Mathf.CeilToInt(winChance).ToString() + "%";

        ReferencesManager.Instance.regionUI.confirmFightButton.SetActive(true);
        ReferencesManager.Instance.regionUI.annexButton.SetActive(false);
        ReferencesManager.Instance.regionUI.confirmDefeatButton.SetActive(false);
        ReferencesManager.Instance.regionUI.cancelFightButton.SetActive(true);

        ReferencesManager.Instance.regionUI.defenderCountryFlag.sprite = newRegion.currentCountry.country.countryFlag;
        ReferencesManager.Instance.regionUI.defenderCountryName.text = newRegion.currentCountry.country._name;

        foreach (Transform child in ReferencesManager.Instance.regionUI.fightPanelDefenderHorizontalGroup.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in ReferencesManager.Instance.regionUI.fightPanelAttackerHorizontalGroup.transform)
        {
            Destroy(child.gameObject);
        }

        CountPlayerUnitData(battle);

        foreach (UnitMovement.UnitHealth unit in battle.enemyUnits)
        {
            ReferencesManager.Instance.regionUI.CreateFightUnitUI(unit.unit, ReferencesManager.Instance.regionUI.fightPanelDefenderHorizontalGroup);

            if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
            {
                battle.enemyInfantry++;
            }
            if (unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
            {
                battle.enemyMotoInfantry++;
            }
            if (unit.unit.type == UnitScriptableObject.Type.ARTILERY)
            {
                battle.enemyArtilery++;
            }
            if (unit.unit.type == UnitScriptableObject.Type.TANK)
            {
                battle.enemyHeavy++;
            }
        }


        if (battle.motoInfantry >= 6 || battle.myHeavy >= 6)
        {
            CheckMotorize(attackerUnit);
        }

        attackerUnit.firstMove = false;
        attackerUnit._movePoints--;

        ReferencesManager.Instance.army.attackerArmy[0].text = battle.myInfantry.ToString();
        ReferencesManager.Instance.army.attackerArmy[1].text = battle.myArtilery.ToString();
        ReferencesManager.Instance.army.attackerArmy[2].text = battle.myHeavy.ToString();

        ReferencesManager.Instance.army.defenderArmy[0].text = battle.enemyInfantry.ToString();
        ReferencesManager.Instance.army.defenderArmy[1].text = battle.enemyArtilery.ToString();
        ReferencesManager.Instance.army.defenderArmy[2].text = battle.enemyHeavy.ToString();
    }


    private void CountLosses(float winChance, int offset)
    {
        for (int i = 0; i < ReferencesManager.Instance.army.defenderArmyLossesValue.Length; i++)
        {
            ReferencesManager.Instance.army.defenderArmyLossesValue[i] = 0;
            ReferencesManager.Instance.army.attackerArmyLossesValue[i] = 0;
        }

        for (int i = 0; i < ReferencesManager.Instance.army.defenderEconomyValue.Length; i++)
        {
            ReferencesManager.Instance.army.defenderEconomyValue[i] = 0;
            ReferencesManager.Instance.army.attackerEconomyValue[i] = 0;
        }

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
                if (random >= offset) // -2 art; -3 infantry
                {
                    damageToAttacker = 90;
                }
            }
            else if (winChance >= 30 && winChance < 37)
            {
                if (random >= offset) // -2 art; -4 infantry
                {
                    damageToAttacker = 130;
                }
            }
            else if (winChance >= 10 && winChance < 25)
            {
                if (random >= offset) // -2 art; -4 infantry
                {
                    damageToAttacker = 200;
                }
            }
            else if (winChance >= 0 && winChance < 10)
            {
                if (random >= offset) // -2 art; -4 infantry
                {
                    damageToAttacker = 500;
                }
            }
        }

        ApplyDamage(damageToAttacker, damageToDefender);
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
                        defenderUnit.currentCountry.moneyNaturalIncome += defenderUnit.unitsHealth[j].unit.moneyIncomeCost;
                        defenderUnit.currentCountry.foodNaturalIncome += defenderUnit.unitsHealth[j].unit.foodIncomeCost;
                        defenderUnit.unitsHealth.Remove(defenderUnit.unitsHealth[j]);
                    }
                }
                if (defenderUnit.unitsHealth.Count < 1)
                {
                    defenderUnit.currentProvince = transform.parent.GetComponent<RegionManager>();
                    defenderUnit.currentProvince.CheckRegionUnits(defenderUnit.currentProvince);
                }
            }
        }
        else if (!attackerWon)
        {
            for (int j = 0; j < attackerUnit.unitsHealth.Count; j++)
            {
                attackerUnit.unitsHealth[j].health -= damageToAttacker;

                if (attackerUnit.unitsHealth[j].health <= 0)
                {
                    attackerUnit.currentCountry.moneyNaturalIncome += attackerUnit.unitsHealth[j].unit.moneyIncomeCost;
                    attackerUnit.currentCountry.foodNaturalIncome += attackerUnit.unitsHealth[j].unit.foodIncomeCost;
                    attackerUnit.unitsHealth.Remove(attackerUnit.unitsHealth[j]);
                }
            }
            if (attackerUnit.unitsHealth.Count < 1)
            {
                attackerUnit.currentProvince = transform.parent.GetComponent<RegionManager>();
                attackerUnit.currentProvince.CheckRegionUnits(attackerUnit.currentProvince);
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
                attackerUnit.currentCountry.country._id,
                attackerUnit.currentCountry.moneyNaturalIncome,
                attackerUnit.currentCountry.foodNaturalIncome,
                attackerUnit.currentCountry.recrootsIncome);
        }
    }


    public void ConfirmFight()
    {
        for (int i = 0; i < ReferencesManager.Instance.army.defenderArmyLossesValue.Length; i++)
        {
            ReferencesManager.Instance.army.defenderArmyLossesValue[i] = 0;
        }
        for (int i = 0; i < ReferencesManager.Instance.army.attackerArmyLossesValue.Length; i++)
        {
            ReferencesManager.Instance.army.defenderArmyLossesValue[i] = 0;
        }

        if (winChance >= 50)
        {
            _annexedRegion = regionTo;
            RegionManager annexedRegion = _annexedRegion.GetComponent<RegionManager>();

            int annexedRegionGoldIncome = (ReferencesManager.Instance.gameSettings.fabric.goldIncome * annexedRegion.civFactory_Amount) + (ReferencesManager.Instance.gameSettings.farm.goldIncome * annexedRegion.farms_Amount) + (8 * annexedRegion.infrastructure_Amount);
            int annexedRegionFoodIncome = ReferencesManager.Instance.gameSettings.farm.foodIncome* annexedRegion.farms_Amount;

            ReferencesManager.Instance.army.attackerEconomyValue[0] = annexedRegionGoldIncome;
            ReferencesManager.Instance.army.attackerEconomyValue[1] = annexedRegionFoodIncome;
            ReferencesManager.Instance.army.attackerEconomyValue[2] = annexedRegion.population;
            ReferencesManager.Instance.army.attackerEconomyValue[3] = annexedRegion.civFactory_Amount;
            ReferencesManager.Instance.army.attackerEconomyValue[4] = annexedRegion.farms_Amount;

            ReferencesManager.Instance.regionUI._annexedRegion = _annexedRegion;

            foreach (Transform child in annexedRegion.transform)
            {
                if (child.GetComponent<UnitMovement>())
                {
                    UnitMovement division = child.GetComponent<UnitMovement>();
                    division.Retreat();
                }
            }
        }
        CountLosses(winChance, offset);
    }

    public void CancelFight()
    {
        ReferencesManager.Instance.regionUI.fightPanelContainer.SetActive(false);
    }

    public void ConfirmAnnexation() 
    {
        ReferencesManager.Instance.AnnexRegion(ReferencesManager.Instance.regionUI.actionRegion, ReferencesManager.Instance.countryManager.currentCountry);
        ReferencesManager.Instance.regionUI.fightPanelContainer.SetActive(false);

        foreach (Transform child in ReferencesManager.Instance.regionUI.fightPanelAttackerHorizontalGroup.transform) Destroy(child.gameObject);
        foreach (Transform child in ReferencesManager.Instance.regionUI.fightPanelDefenderHorizontalGroup.transform) Destroy(child.gameObject);
    }

    public void ConfirmDefeat()
    {
        ReferencesManager.Instance.regionUI.fightPanelContainer.SetActive(false);

        foreach (Transform child in ReferencesManager.Instance.regionUI.fightPanelAttackerHorizontalGroup.transform) Destroy(child.gameObject);
        foreach (Transform child in ReferencesManager.Instance.regionUI.fightPanelDefenderHorizontalGroup.transform) Destroy(child.gameObject);
    }

    public void MoveUnit(RaycastHit2D hit, bool forceParent, Transform parent, bool selectRegion)
    {
        UnitMovement.BattleInfo battle = new UnitMovement.BattleInfo();

        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.MoveUnit(parent.GetComponent<RegionManager>()._id, regionTo.GetComponent<RegionManager>()._id);
        }
        else
        {
            attackerUnit.transform.position = hit.collider.transform.position;

            //attackerUnit.transform.position = Vector3.Lerp(attackerUnit.transform.position, hit.collider.transform.position, Time.deltaTime * 5f);

            attackerUnit.currentProvince.hasArmy = false; // old region are not having army now
            if (!forceParent) attackerUnit.transform.SetParent(regionTo);
            else attackerUnit.transform.SetParent(parent);

            RegionManager newRegion = regionTo.GetComponent<RegionManager>();

            newRegion.hasArmy = true;
            if (selectRegion) newRegion.SelectRegionNoHit(newRegion);
            attackerUnit.currentProvince = regionTo.GetComponent<RegionManager>();


            battle.myUnits = attackerUnit.unitsHealth;
        }

        CountPlayerUnitData(battle);
        PlayMoveSFX(battle);

        CheckMotorize(attackerUnit);

        attackerUnit.firstMove = false;
        attackerUnit._movePoints--;

        ReferencesManager.Instance.regionUI.UpdateUnitsUI();
    }


    private void CountPlayerUnitData(UnitMovement.BattleInfo battle)
    {
        foreach (UnitMovement.UnitHealth unit in battle.myUnits)
        {
            ReferencesManager.Instance.regionUI.CreateFightUnitUI(unit.unit, ReferencesManager.Instance.regionUI.fightPanelAttackerHorizontalGroup);

            if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
            {
                battle.myInfantry++;
            }
            if (unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
            {
                battle.motoInfantry++;
            }
            if (unit.unit.type == UnitScriptableObject.Type.ARTILERY)
            {
                battle.myArtilery++;
            }
            if (unit.unit.type == UnitScriptableObject.Type.TANK)
            {
                battle.myHeavy++;
            }
        }
    }

    private void PlayMoveSFX(UnitMovement.BattleInfo battle)
    {
        if (battle.myInfantry >= 5)
        {
            UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.gameSettings.m_infantry_move[Random.Range(0, 1)]);
        }
        if (battle.motoInfantry >= 6)
        {
            UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.gameSettings.m_motorized_infantry_move[Random.Range(0, 1)]);
        }
        if (battle.myHeavy >= 5)
        {
            UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.gameSettings.m_heavy_move[Random.Range(0, 1)]);
        }
    }
}
