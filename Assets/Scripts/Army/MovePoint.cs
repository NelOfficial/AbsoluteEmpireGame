using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

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
                this.transform.position = new Vector3(regionTo.position.x, regionTo.position.y, -1);
            }

            parentRegion.movePoints.Clear();
            foreach (Transform child in parentRegion.transform)
            {
                if (child.GetComponent<MovePoint>() != null)
                {
                    parentRegion.movePoints.Add(child);
                }
            }
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
                                if (child.GetComponent<UnitMovement>())
                                {
                                    attackerUnit = child.GetComponent<UnitMovement>();
                                }
                            }

                            if (newRegion.hasArmy)
                            {
                                foreach (Transform child in regionTo)
                                {
                                    if (child.GetComponent<UnitMovement>())
                                    {
                                        defenderUnit = child.GetComponent<UnitMovement>();
                                    }
                                }
                            }

                            Fight(newRegion);
                            attackerUnit.UpdateInfo();

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
            int myCavlry = 0;
            int myMotoInfantry = 0;
            int myArtilery = 0;
            int myHeavy = 0;

            bool hasFuel = false;

            foreach (UnitMovement.UnitHealth unit in unitMovement.unitsHealth)
            {
                if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
                {
                    myInfantry++;
                }

                if (unit.unit.type == UnitScriptableObject.Type.CAVALRY)
                {
                    myCavlry++;
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

                if (unit.fuel >= 50 || unit.unit.maxFuel <= 0)
                {
                    hasFuel = true;
                }
            }

            if (myMotoInfantry >= 6 || myHeavy >= 6 || myCavlry >= 6)
            {
                if (unitMovement.firstMove && hasFuel)
                {
                    unitMovement._movePoints++;
                }
            }
        }
    }

    private void Fight(RegionManager fightRegion)
    {
        UnitMovement.BattleInfo battle = new UnitMovement.BattleInfo();

        winChance = 0;

        battle.defenderForts = newRegion.fortifications_Amount;

        battle.defender_BONUS_ATTACK = 100;
        battle.defender_BONUS_DEFENCE = 100;

        battle.attacker_BONUS_ATTACK = 100;
        battle.attacker_BONUS_DEFENCE = 100;

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

                    float unit_defenderHardAttack = unit.unit.hardAttack;
                    float unit_defenderSoftAttack = unit.unit.softAttack;

                    if (unit.unit.type == UnitScriptableObject.Type.TANK ||
                        unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                    {
                        unit_defenderHardAttack = unit.fuel * unit.unit.hardAttack / unit.unit.maxFuel;
                        unit_defenderSoftAttack = unit.fuel * unit.unit.softAttack / unit.unit.maxFuel;
                    }

                    if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
                    {
                        unit_defenderSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.INF_defenceBonus;
                        unit_defenderHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.INF_defenceBonus;
                    }

                    if (unit.unit.type == UnitScriptableObject.Type.ARTILERY)
                    {
                        unit_defenderSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.ART_defenceBonus;
                        unit_defenderHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.ART_defenceBonus;
                    }

                    if (unit.unit.type == UnitScriptableObject.Type.CAVALRY || unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                    {
                        unit_defenderSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.MIF_defenceBonus;
                        unit_defenderHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.MIF_defenceBonus;
                    }

                    if (unit.unit.type == UnitScriptableObject.Type.TANK || unit.unit.type == UnitScriptableObject.Type.TANK)
                    {
                        unit_defenderSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.HEA_defenceBonus;
                        unit_defenderHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.HEA_defenceBonus;
                    }

                    battle.defenderHardAttack += unit_defenderHardAttack;
                    battle.defenderSoftAttack += unit_defenderSoftAttack;
                    battle.defenderDefense += unit.unit.defense;
                    battle.defenderArmor += unit.unit.armor;
                    battle.defenderArmorPiercing += unit.unit.armorPiercing;
                    battle.defenderHardness += unit.unit.hardness;
                }

                float _maxArmor = 0;
                try
                {
                    _maxArmor = _armors.Max();
                }
                catch (System.Exception) { }

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
            battle.fightRegion = fightRegion;
            battle.enemyUnits = battle.fightRegion.currentDefenseUnits;

            try
            {
                foreach (UnitMovement.UnitHealth unit in fightRegion.currentDefenseUnits)
                {
                    defenderArmors.Add(unit.unit.armor);

                    float unit_defenderHardAttack = unit.unit.hardAttack;
                    float unit_defenderSoftAttack = unit.unit.softAttack;

                    if (unit.unit.type == UnitScriptableObject.Type.TANK ||
                        unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                    {
                        unit_defenderHardAttack = unit.fuel * unit.unit.hardAttack / unit.unit.maxFuel;
                        unit_defenderSoftAttack = unit.fuel * unit.unit.softAttack / unit.unit.maxFuel;
                    }
                    if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
                    {
                        unit_defenderSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.INF_defenceBonus;
                        unit_defenderHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.INF_defenceBonus;
                    }

                    if (unit.unit.type == UnitScriptableObject.Type.ARTILERY)
                    {
                        unit_defenderSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.ART_defenceBonus;
                        unit_defenderHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.ART_defenceBonus;
                    }

                    if (unit.unit.type == UnitScriptableObject.Type.CAVALRY || unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                    {
                        unit_defenderSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.MIF_defenceBonus;
                        unit_defenderHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.MIF_defenceBonus;
                    }

                    if (unit.unit.type == UnitScriptableObject.Type.TANK)
                    {
                        unit_defenderSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.HEA_defenceBonus;
                        unit_defenderHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.HEA_defenceBonus;
                    }

                    battle.defenderSoftAttack += unit_defenderSoftAttack;
                    battle.defenderHardAttack += unit_defenderHardAttack;
                    battle.defenderDefense += unit.unit.defense;
                    battle.defenderArmor += unit.unit.armor;
                    battle.defenderArmorPiercing += unit.unit.armorPiercing;
                    battle.defenderHardness += unit.unit.hardness;
                }

                float maxArmor = defenderArmors.Max();
                float midArmor = battle.defenderArmor / battle.enemyUnits.Count;

                battle.defenderHardness = battle.defenderHardness / battle.enemyUnits.Count;
                battle.defenderArmor = 0.4f * maxArmor + 0.6f * midArmor;
            }
            catch (System.Exception) {}
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

            float unit_attackerHardAttack = unit.unit.hardAttack;
            float unit_attackerSoftAttack = unit.unit.softAttack;

            if (unit.unit.type == UnitScriptableObject.Type.TANK ||
                unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
            {
                unit_attackerHardAttack = unit.fuel * unit.unit.hardAttack / unit.unit.maxFuel;
                unit_attackerSoftAttack = unit.fuel * unit.unit.softAttack / unit.unit.maxFuel;
            }

            if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
            {
                unit_attackerSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.INF_attackBonus;
                unit_attackerHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.INF_attackBonus;
            }

            if (unit.unit.type == UnitScriptableObject.Type.ARTILERY)
            {
                unit_attackerSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.ART_attackBonus;
                unit_attackerHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.ART_attackBonus;
            }

            if (unit.unit.type == UnitScriptableObject.Type.CAVALRY || unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
            {
                unit_attackerSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.MIF_attackBonus;
                unit_attackerHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.MIF_attackBonus;
            }

            if (unit.unit.type == UnitScriptableObject.Type.TANK)
            {
                unit_attackerSoftAttack = unit.unit.softAttack * 100 / battle.fightRegion.regionTerrain.HEA_attackBonus;
                unit_attackerHardAttack = unit.unit.hardAttack * 100 / battle.fightRegion.regionTerrain.HEA_attackBonus;
            }

            battle.attackerSoftAttack += unit_attackerSoftAttack;
            battle.attackerHardAttack += unit_attackerHardAttack;
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

            else if (difference >= 30 && difference <= 40)
            {
                winChance = Random.Range(39, 41);

                offset = 45;
            }

            else if (difference >= 40 && difference <= 50)
            {
                winChance = Random.Range(37, 39);

                offset = 35;
            }

            else if (difference >= 50 && difference <= 60)
            {
                winChance = Random.Range(35, 37);

                offset = 25;
            }

            else if (difference >= 60 && difference <= 70)
            {
                winChance = Random.Range(30, 37);

                offset = 20;
            }

            else if (difference >= 70 && difference <= 80)
            {
                winChance = Random.Range(10, 25);

                offset = 10;
            }

            else if (difference >= 80 && difference <= 100)
            {
                winChance = Random.Range(0, 15);

                offset = 0;
            }
        }
        else if (battle.defenderStrength == battle.attackerStrength)
        {
            winChance = Random.Range(49, 51);
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

            else if (difference > 20 && difference <= 30)
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

        if (battle.attacker_BONUS_ATTACK >= 100) ReferencesManager.Instance.army.attackerBonus[0].text = $"+{battle.attacker_BONUS_ATTACK - 100}%";
        else ReferencesManager.Instance.army.attackerBonus[0].text = $"-{100 - battle.attacker_BONUS_ATTACK}%";

        if (battle.attacker_BONUS_DEFENCE >= 100) ReferencesManager.Instance.army.attackerBonus[1].text = $"+{battle.attacker_BONUS_DEFENCE - 100}%";
        else ReferencesManager.Instance.army.attackerBonus[1].text = $"-{100 - battle.attacker_BONUS_DEFENCE}%";

        if (battle.defender_BONUS_ATTACK >= 100) ReferencesManager.Instance.army.defenderBonus[0].text = $"+{battle.defender_BONUS_ATTACK - 100}%";
        else ReferencesManager.Instance.army.defenderBonus[0].text = $"-{100 - battle.defender_BONUS_ATTACK}%";

        if (battle.defender_BONUS_DEFENCE >= 100) ReferencesManager.Instance.army.defenderBonus[1].text = $"+{battle.defender_BONUS_DEFENCE - 100}%";
        else ReferencesManager.Instance.army.defenderBonus[1].text = $"-{100 - battle.defender_BONUS_DEFENCE}%";


        ReferencesManager.Instance.regionUI.defenderCountryFlag.sprite = newRegion.currentCountry.country.countryFlag;

        ReferencesManager.Instance.regionUI.defenderCountryName.text = ReferencesManager.Instance.languageManager.GetTranslation(newRegion.currentCountry.country._nameEN);
        
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
            try
            {
                ReferencesManager.Instance.regionUI.CreateFightUnitUI(unit.unit, ReferencesManager.Instance.regionUI.fightPanelDefenderHorizontalGroup, battle.defenderDivision);
            }
            catch (System.Exception) {}

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
            if (unit.unit.type == UnitScriptableObject.Type.CAVALRY)
            {
                battle.enemyCavlry++;
            }
        }
        ReferencesManager.Instance.regionUI.UpdateFightUnitsUI(ReferencesManager.Instance.regionUI.fightPanelAttackerHorizontalGroup.transform, battle.attackerDivision);
        ReferencesManager.Instance.regionUI.UpdateFightUnitsUI(ReferencesManager.Instance.regionUI.fightPanelDefenderHorizontalGroup.transform, battle.defenderDivision);


        if (battle.motoInfantry >= 6 || battle.myHeavy >= 6 || battle.myCavlry >= 6)
        {
            CheckMotorize(attackerUnit);
        }

        attackerUnit.firstMove = false;
        attackerUnit._movePoints--;
        attackerUnit.UpdateInfo();

        foreach (UnitMovement.UnitHealth unit in battle.attackerDivision.unitsHealth)
        {
            if (unit.unit.maxFuel > 0)
            {
                unit.fuel -= 200;
            }
        }

        try
        {
            if (battle.defenderDivision != null)
            {
                foreach (UnitMovement.UnitHealth unit in battle.defenderDivision.unitsHealth)
                {
                    if (unit.unit.maxFuel > 0)
                    {
                        unit.fuel -= 50;
                    }
                }
            }
        }
        catch (System.Exception) {}


        ReferencesManager.Instance.army.attackerArmy[0].text = (battle.myInfantry + battle.motoInfantry).ToString();
        ReferencesManager.Instance.army.attackerArmy[1].text = battle.myArtilery.ToString();
        ReferencesManager.Instance.army.attackerArmy[2].text = battle.myHeavy.ToString();
        ReferencesManager.Instance.army.attackerArmy[3].text = battle.myCavlry.ToString();

        ReferencesManager.Instance.army.defenderArmy[0].text = (battle.enemyInfantry + battle.enemyMotoInfantry).ToString();
        ReferencesManager.Instance.army.defenderArmy[1].text = battle.enemyArtilery.ToString();
        ReferencesManager.Instance.army.defenderArmy[2].text = battle.enemyHeavy.ToString();
        ReferencesManager.Instance.army.defenderArmy[3].text = battle.enemyCavlry.ToString();

        ReferencesManager.Instance.gameSettings.currentBattle = battle;
    }

    private void ApplyDamage(UnitMovement.BattleInfo battle)
    {
        int att_inf_losses = 0;
        int att_art_losses = 0;
        int att_hvy_losses = 0;
        int att_cav_losses = 0;

        int def_inf_losses = 0;
        int def_art_losses = 0;
        int def_hvy_losses = 0;
        int def_cav_losses = 0;

        float defender_losses_factor = 1;
        float attacker_losses_factor = 1;

        bool defenderWin = false;
        bool attackerWin = false;

        if (battle.winChance >= 50) attackerWin = true;
        else defenderWin = true;

        if (attackerWin)
        {
            defender_losses_factor = 1 / (winChance / (100 - winChance));
            attacker_losses_factor = ((100 - winChance) / winChance);
        }
        else if (defenderWin)
        {
            attacker_losses_factor = ((100 - winChance) / winChance);
            defender_losses_factor = 1 / (winChance / (100 - winChance));
        }

        float attackerDamage_Soft = battle.defenderSoftAttack * attacker_losses_factor;
        float defenderDamage_Soft = battle.attackerSoftAttack * defender_losses_factor;

        float attackerDamage_Hard = battle.defenderHardAttack * attacker_losses_factor;
        float defenderDamage_Hard = battle.attackerHardAttack * defender_losses_factor;

        #region Defender Losses

        if (defenderUnit != null)
        {
            for (int j = 0; j < defenderUnit.unitsHealth.Count; j++)
            {
                if (defenderUnit.unitsHealth[j].unit.hardness <= 15)
                {
                    defenderUnit.unitsHealth[j].health -= defenderDamage_Soft;
                }
                else if (defenderUnit.unitsHealth[j].unit.hardness > 15)
                {
                    defenderUnit.unitsHealth[j].health -= defenderDamage_Hard;
                }

                if (defenderUnit.unitsHealth[j].health <= 0)
                {
                    defenderUnit.currentCountry.moneyNaturalIncome += defenderUnit.unitsHealth[j].unit.moneyIncomeCost;
                    defenderUnit.currentCountry.foodNaturalIncome += defenderUnit.unitsHealth[j].unit.foodIncomeCost;

                    if (defenderUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.SOLDIER)
                    {
                        def_inf_losses++;
                    }
                    else if (defenderUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                    {
                        def_inf_losses++;
                    }
                    else if (defenderUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.ARTILERY)
                    {
                        def_art_losses++;
                    }
                    else if (defenderUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.TANK)
                    {
                        def_hvy_losses++;
                    }
                    else if (defenderUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.CAVALRY)
                    {
                        def_cav_losses++;
                    }

                    defenderUnit.currentCountry.myRegions[Random.Range(0, defenderUnit.currentCountry.myRegions.Count)].population -= defenderUnit.unitsHealth[j].unit.recrootsCost;
                    defenderUnit.unitsHealth.Remove(defenderUnit.unitsHealth[j]);
                }
            }
            if (defenderUnit.unitsHealth.Count < 1)
            {
                defenderUnit.currentProvince = transform.parent.GetComponent<RegionManager>();
                StartCoroutine(DestroyDivision_Co(defenderUnit));
            }
        }

        #endregion

        #region Attacker Losses

        for (int j = 0; j < attackerUnit.unitsHealth.Count; j++)
        {
            if (attackerUnit.unitsHealth[j].unit.hardness <= 15)
            {
                attackerUnit.unitsHealth[j].health -= attackerDamage_Soft;
            }
            else if (attackerUnit.unitsHealth[j].unit.hardness > 15)
            {
                attackerUnit.unitsHealth[j].health -= attackerDamage_Hard;
            }

            if (attackerUnit.unitsHealth[j].health <= 0)
            {
                attackerUnit.currentCountry.moneyNaturalIncome += attackerUnit.unitsHealth[j].unit.moneyIncomeCost;
                attackerUnit.currentCountry.foodNaturalIncome += attackerUnit.unitsHealth[j].unit.foodIncomeCost;

                if (attackerUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.SOLDIER)
                {
                    att_inf_losses++;
                }
                else if (attackerUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                {
                    att_inf_losses++;
                }
                else if (attackerUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.ARTILERY)
                {
                    att_art_losses++;
                }
                else if (attackerUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.TANK)
                {
                    att_hvy_losses++;
                }
                else if (attackerUnit.unitsHealth[j].unit.type == UnitScriptableObject.Type.CAVALRY)
                {
                    att_cav_losses++;
                }

                attackerUnit.currentCountry.myRegions[Random.Range(0, attackerUnit.currentCountry.myRegions.Count)].population -= attackerUnit.unitsHealth[j].unit.recrootsCost;
                attackerUnit.unitsHealth.Remove(attackerUnit.unitsHealth[j]);
            }
        }
        if (attackerUnit.unitsHealth.Count < 1)
        {
            attackerUnit.currentProvince = transform.parent.GetComponent<RegionManager>();
            StartCoroutine(DestroyDivision_Co(attackerUnit));
        }

        #endregion

        ReferencesManager.Instance.army.defenderArmyLossesValue[0] = def_inf_losses;
        ReferencesManager.Instance.army.defenderArmyLossesValue[1] = def_art_losses;
        ReferencesManager.Instance.army.defenderArmyLossesValue[2] = def_hvy_losses;
        ReferencesManager.Instance.army.defenderArmyLossesValue[3] = def_cav_losses;

        ReferencesManager.Instance.army.attackerArmyLossesValue[0] = att_inf_losses;
        ReferencesManager.Instance.army.attackerArmyLossesValue[1] = att_art_losses;
        ReferencesManager.Instance.army.attackerArmyLossesValue[2] = att_hvy_losses;
        ReferencesManager.Instance.army.attackerArmyLossesValue[3] = att_cav_losses;
    }

    private IEnumerator DestroyDivision_Co(UnitMovement division)
    {
        division.GetComponent<Animator>().Play("divisionDie");
        yield return new WaitForSecondsRealtime(0.6f);
        division.currentProvince.CheckRegionUnits(division.currentProvince);
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
        ApplyDamage(ReferencesManager.Instance.gameSettings.currentBattle);
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
            battle.attackerDivision = attackerUnit;

            foreach (UnitMovement.UnitHealth unit in battle.attackerDivision.unitsHealth)
            {
                if (unit.unit.maxFuel > 0)
                {
                    unit.fuel -= 100;
                }
            }
        }

        CountPlayerUnitData(battle);
        PlayMoveSFX(battle);

        CheckMotorize(attackerUnit);

        attackerUnit.firstMove = false;
        attackerUnit._movePoints--;

        attackerUnit.UpdateInfo();
        ReferencesManager.Instance.regionUI.UpdateUnitsUI(true);
    }


    private void CountPlayerUnitData(UnitMovement.BattleInfo battle)
    {
        foreach (UnitMovement.UnitHealth unit in battle.myUnits)
        {
            ReferencesManager.Instance.regionUI.CreateFightUnitUI(unit.unit, ReferencesManager.Instance.regionUI.fightPanelAttackerHorizontalGroup, battle.attackerDivision);

            if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
            {
                battle.myInfantry++;
            }
            if (unit.unit.type == UnitScriptableObject.Type.CAVALRY)
            {
                battle.myCavlry++;
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

        ReferencesManager.Instance.regionUI.UpdateFightUnitsUI(ReferencesManager.Instance.regionUI.fightPanelAttackerHorizontalGroup.transform, battle.attackerDivision);
        ReferencesManager.Instance.regionUI.UpdateFightUnitsUI(ReferencesManager.Instance.regionUI.fightPanelDefenderHorizontalGroup.transform, battle.defenderDivision);

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
