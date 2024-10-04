using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SeaMovePoint : MonoBehaviour
{
    [HideInInspector] public Fleet _attackerFleet;
    [HideInInspector] public Fleet _defenderFleet;

    [HideInInspector] public SeaMovePoint _currentMovePoint;

    public SeaRegion regionTransit;
    [HideInInspector] public SeaRegion parentSeaRegion;
    [HideInInspector] public RegionManager parentRegion;

    [HideInInspector] public Fleet actionFleet;
    [HideInInspector] public UnitMovement actionUnit;

    [SerializeField] private bool noAutoCollider;

    private void OnMouseDown()
    {
        if (!ReferencesManager.Instance.mainCamera.IsMouseOverUI)
        {
            if (!ReferencesManager.Instance.diplomatyUI.diplomatyContainer.activeSelf &&
                !ReferencesManager.Instance.regionUI.fightPanelContainer.activeSelf)
            {
                Action();
            }
        }
    }

    private void Start()
    {
        SetRegionInPoint();
    }

    private void SetRegionInPoint()
    {
        gameObject.GetComponent<PolygonCollider2D>().enabled = false;
        gameObject.GetComponent<PolygonCollider2D>().points = regionTransit.GetComponent<PolygonCollider2D>().points;
    }

    public void SetDestination()
    {
        RaycastHit2D _hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.down);
        if (_hit.collider.gameObject.GetComponent<SeaRegion>())
        {
            regionTransit = _hit.collider.transform.GetComponent<SeaRegion>();
        }
    }

    public void Action()
    {
        if (ReferencesManager.Instance.regionManager.currentRegionManager != null)
        {
            actionUnit = ReferencesManager.Instance.regionManager.GetDivision(ReferencesManager.Instance.regionManager.currentRegionManager);
        }
        else if (ReferencesManager.Instance.seaRegionManager._currentSeaRegion != null)
        {
            actionUnit = ReferencesManager.Instance.seaRegionManager.GetDivision(ReferencesManager.Instance.seaRegionManager._currentSeaRegion);
        }

        if (actionUnit != null)
        {
            if (HasEnemyInRegion_Unit(ReferencesManager.Instance.countryManager.currentCountry, regionTransit))
            {
                Fight(regionTransit);
            }
            else
            {
                if (ReferencesManager.Instance.regionManager.currentRegionManager != null)
                {
                    MoveUnit_FromGroundRegion(regionTransit, actionUnit);
                    ReferencesManager.Instance.seaRegionManager.SelectRegion_ByObject(regionTransit);
                }
                else if(ReferencesManager.Instance.seaRegionManager._currentSeaRegion != null)
                {
                    Move(regionTransit, actionUnit);
                    ReferencesManager.Instance.seaRegionManager.SelectRegion_ByObject(regionTransit);
                }
            }
        }
        else
        {
            Debug.LogError($"ERROR: Action target is null");
        }
    }

    private void Fight(SeaRegion fightRegion)
    {
        UnitMovement.BattleInfo battle = new()
        {
            defender_BONUS_ATTACK = 100,
            defender_BONUS_DEFENCE = 100,

            attacker_BONUS_ATTACK = 100,
            attacker_BONUS_DEFENCE = 100
        };

        UnitMovement attackerUnit = actionUnit;
        UnitMovement defenderUnit = fightRegion._division;
        int winChance = 0;

        if (attackerUnit != null)
        {
            winChance = 0;

            battle.defenderForts = 0;

            //try
            //{
            //    if (defenderUnit.Encircled(defenderUnit.currentProvince))
            //    {
            //        battle.defender_BONUS_ATTACK -= 50;
            //        battle.defender_BONUS_DEFENCE -= 50;
            //    }
            //}
            //catch (System.Exception) { }

            //try
            //{
            //    if (attackerUnit.Encircled(attackerUnit.currentProvince))
            //    {
            //        battle.attacker_BONUS_ATTACK -= 50;
            //        battle.attacker_BONUS_DEFENCE -= 50;
            //    }
            //}
            //catch (System.Exception) { }


            #region Defender info

            if (fightRegion._division != null)
            {
                try
                {
                    List<float> _armors = new List<float>();
                    UnitMovement fightRegionUnitMovement = fightRegion.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();

                    if (fightRegionUnitMovement != null)
                    {
                        battle.defenderDivision = fightRegionUnitMovement;
                        battle.enemyUnits = battle.defenderDivision.unitsHealth;
                        battle.seaFightRegion = fightRegion;

                        foreach (UnitHealth unit in battle.defenderDivision.unitsHealth)
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

                            battle.defenderSoftAttack += unit_defenderSoftAttack;
                            battle.defenderHardAttack += unit_defenderHardAttack;
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
                    fightRegion._division = null;
                }
            }

            #endregion

            #region Attacker info

            List<float> attackerArmors = new List<float>();

            battle.attackerDivision = attackerUnit;
            battle.myUnits = battle.attackerDivision.unitsHealth;
            battle.seaFightRegion = fightRegion;

            foreach (UnitHealth unit in attackerUnit.unitsHealth)
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

                battle.attackerSoftAttack += unit_attackerSoftAttack;
                battle.attackerHardAttack += unit_attackerHardAttack;
                battle.attackerDefense += unit.unit.defense;
                battle.attackerArmor += unit.unit.armor;
                battle.attackerArmorPiercing += unit.unit.armorPiercing;
                battle.attackerHardness += unit.unit.hardness;
            }

            if (attackerArmors.Count > 0)
            {
                float attackerMaxArmor = attackerArmors.Max();
                float attackerMidArmor = battle.attackerArmor / battle.attackerDivision.unitsHealth.Count;

                battle.attackerHardness = battle.attackerHardness / battle.attackerDivision.unitsHealth.Count;
                battle.attackerArmor = 0.4f * attackerMaxArmor + 0.6f * attackerMidArmor;
            }

            #endregion

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

                if (difference >= 0 && difference <= 15)
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

                else if (difference >= 30 && difference <= 40)
                {
                    winChance = Random.Range(39, 41);
                }

                else if (difference >= 40 && difference <= 50)
                {
                    winChance = Random.Range(37, 39);
                }

                else if (difference >= 50 && difference <= 60)
                {
                    winChance = Random.Range(35, 37);
                }

                else if (difference >= 60 && difference <= 70)
                {
                    winChance = Random.Range(30, 37);
                }

                else if (difference >= 70 && difference <= 80)
                {
                    winChance = Random.Range(10, 25);
                }

                else if (difference >= 80 && difference <= 100)
                {
                    winChance = Random.Range(0, 15);
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

                if (difference >= 0 && difference <= 10)
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

                else if (difference > 20 && difference <= 30)
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
            battle.winChance = winChance;

            ReferencesManager.Instance.regionUI.winChance = winChance;
            ReferencesManager.Instance.regionUI.unitMovement = attackerUnit; // Атакующий юнит, с него возьмуться очки движения

            ReferencesManager.Instance.regionUI.fightPanelContainer.SetActive(true);
            ReferencesManager.Instance.regionUI.resultPanel.SetActive(false);

            ReferencesManager.Instance.regionUI.winChanceBarInner.fillAmount = (float)winChance / 100f;
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


            ReferencesManager.Instance.regionUI.defenderCountryFlag.sprite = fightRegion._division.currentCountry.country.countryFlag;
            ReferencesManager.Instance.regionUI._currentTerrainThumbnail.sprite = fightRegion._terrain.icon;

            ReferencesManager.Instance.regionUI.defenderCountryName.text = ReferencesManager.Instance.languageManager.GetTranslation(fightRegion._division.currentCountry.country._nameEN);

            foreach (Transform child in ReferencesManager.Instance.regionUI.fightPanelDefenderHorizontalGroup.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in ReferencesManager.Instance.regionUI.fightPanelAttackerHorizontalGroup.transform)
            {
                Destroy(child.gameObject);
            }

            CountPlayerUnitData(battle);

            foreach (UnitHealth unit in battle.enemyUnits)
            {
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

            ReferencesManager.Instance.regionUI.UpdateFightUnitsUI(ReferencesManager.Instance.regionUI.fightPanelAttackerHorizontalGroup.transform,
                battle.attackerDivision,
                battle.attackerRegion);

            ReferencesManager.Instance.regionUI.UpdateFightUnitsUI(ReferencesManager.Instance.regionUI.fightPanelDefenderHorizontalGroup.transform,
                battle.defenderDivision,
                battle.fightRegion);

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
    }

    private void CountPlayerUnitData(UnitMovement.BattleInfo battle)
    {
        foreach (UnitHealth unit in battle.myUnits)
        {
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
    }


    private void Move(SeaRegion destinationRegion, UnitMovement division)
    {
        //if (fleet._movePoints > 0)
        //{
        //    if (!fleet.visible)
        //    {
        //        fleet.visible = true;
        //        fleet.SetUp();
        //    }

        //    fleet._movePoints--;
        //    fleet.transform.position = fightRegion.transform.position;
        //}

        if (division._movePoints > 0 && destinationRegion._division == null)
        {
            division.inSea = true;
            division._currentSeaRegion._division = null;
            division._currentSeaRegion = destinationRegion;
            division.currentProvince = null;

            division._movePoints--;
            division.transform.position = destinationRegion.transform.position;

            destinationRegion._division = division;

            foreach (SeaMovePoint seaPoint in FindObjectsOfType(typeof(SeaMovePoint)).Cast<SeaMovePoint>())
            {
                Destroy(seaPoint.gameObject.GetComponent<SpriteRenderer>());
                seaPoint.gameObject.GetComponent<PolygonCollider2D>().enabled = false;
            }

            foreach (FromSeaToGround_MovePoint groundPoint in FindObjectsOfType(typeof(FromSeaToGround_MovePoint)).Cast<FromSeaToGround_MovePoint>())
            {
                Destroy(groundPoint.gameObject.GetComponent<SpriteRenderer>());
                groundPoint.gameObject.GetComponent<PolygonCollider2D>().enabled = false;
            }

            foreach (MovePoint movePoint in FindObjectsOfType(typeof(MovePoint)).Cast<MovePoint>())
            {
                Destroy(movePoint.gameObject.GetComponent<SpriteRenderer>());
                movePoint.gameObject.GetComponent<PolygonCollider2D>().enabled = false;
            }

            division.transform.parent = destinationRegion.transform;

            division.UpdateInfo();
        }
    }

    private void MoveUnit_FromGroundRegion(SeaRegion destinationRegion, UnitMovement division)
    {
        if (division._movePoints > 0 && destinationRegion._division == null)
        {
            division.inSea = true;
            division._currentSeaRegion = destinationRegion;

            division._movePoints--;
            division.transform.position = destinationRegion.transform.position;

            destinationRegion._division = division;
            division.RemoveClosestsPoints();

            division.transform.parent = destinationRegion.transform;

            division.currentProvince.CheckRegionUnits(division.currentProvince);
            division.currentProvince = null;
            division.UpdateInfo();
        }
    }

    private List<Fleet> GetEnemiesFleetsInRegion(CountrySettings me, SeaRegion seaRegion)
    {
        List<Fleet> enemies = new List<Fleet>();

        foreach (Fleet fleet in seaRegion._fleets)
        {
            Relationships.Relation relation = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(me, fleet._owner);

            if (relation.war)
            {
                enemies.Add(fleet);
            }
        }

        return enemies;
    }

    private List<CountrySettings> GetEnemiesInRegion(CountrySettings me, SeaRegion seaRegion)
    {
        List<CountrySettings> enemies = new List<CountrySettings>();

        foreach (Fleet fleet in seaRegion._fleets)
        {
            Relationships.Relation relation = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(me, fleet._owner);

            if (relation.war)
            {
                enemies.Add(fleet._owner);
            }
        }

        return enemies;
    }

    private CountrySettings GetEnemyInRegion_Unit(CountrySettings me, SeaRegion seaRegion)
    {
        CountrySettings country = null;

        if (seaRegion._division != null)
        {
            Relationships.Relation relation = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(me, seaRegion._division.currentCountry);

            if (relation.war)
            {
                country = relation.country;
            }
        }

        return country;
    }

    private bool HasEnemiesInRegion(CountrySettings me, SeaRegion seaRegion)
    {
        bool result = GetEnemiesFleetsInRegion(me, seaRegion).Count > 0;

        return result;
    }

    private bool HasEnemyInRegion_Unit(CountrySettings me, SeaRegion seaRegion)
    {
        bool result = GetEnemyInRegion_Unit(me, seaRegion);

        return result;
    }

}
