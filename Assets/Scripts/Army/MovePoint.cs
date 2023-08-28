using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    [HideInInspector] public int motoInfantry;
    [HideInInspector] public int myInfantry;
    [HideInInspector] public int myArtilery;
    [HideInInspector] public int myHeavy;

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
                else
                {
                    Debug.Log($"{unitMovement.currentProvince.name} ({unitMovement.currentCountry.country._nameEN}) {unitMovement.firstMove} {unitMovement._movePoints}");
                }
            }
        }
    }

    private void Fight(RegionManager fightRegion, RaycastHit2D hit)
    {
        List<UnitScriptableObject> enemyUnits = new List<UnitScriptableObject>();
        List<UnitMovement.UnitHealth> myUnits = new List<UnitMovement.UnitHealth>();

        float defenderForts = 0;
        float attackerFortsDebuff = 0;

        float defenderGlobalBonus_DEFENCE = 0;
        float defenderGlobalBonus_ATTACK = 0;
        float attackerGlobalBonus_DEFENCE = 0;
        float attackerGlobalBonus_ATTACK = 0;

        float enemyDamage = 0;
        float myDamage = 0;

        myInfantry = 0;
        int enemyInfantry = 0;

        motoInfantry = 0;
        int enemyMotoInfantry = 0;

        myArtilery = 0;
        int enemyArtilery = 0;

        myHeavy = 0;
        int enemyHeavy = 0;

        winChance = 0;

        defenderForts = newRegion.fortifications_Amount * 5 / 100;

        if (fightRegion.hasArmy)
        {
            try
            {
                UnitMovement fightRegionUnitMovement = fightRegion.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();

                foreach (UnitMovement.UnitHealth unit in fightRegionUnitMovement.unitsHealth)
                {
                    enemyUnits.Add(unit.unit);
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
            catch (System.NullReferenceException)
            {
                fightRegion.hasArmy = false;
            }
        }
        else if (!fightRegion.hasArmy)
        {
            foreach (UnitScriptableObject unit in fightRegion.currentDefenseUnits)
            {
                enemyUnits.Add(unit);
                enemyDamage += unit.damage;
                //if (unit.type == UnitScriptableObject.Type.SOLDIER)
                //{
                //    if (fightRegion.regionTerrain.INF_attackBonus < 0)
                //    {
                //        enemyDamage -= unit.damage * (1.0f + ((float)fightRegion.regionTerrain.INF_attackBonus / 100));
                //    }
                //    if (fightRegion.regionTerrain.INF_attackBonus > 0)
                //    {
                //        enemyDamage += unit.damage * (1.0f + ((float)fightRegion.regionTerrain.INF_attackBonus / 100));
                //    }
                //}
                //else if (unit.type == UnitScriptableObject.Type.ARTILERY)
                //{
                //    if (fightRegion.regionTerrain.ART_attackBonus < 0)
                //    {
                //        enemyDamage -= unit.damage * (1.0f + ((float)fightRegion.regionTerrain.ART_attackBonus / 100));
                //    }
                //    if (fightRegion.regionTerrain.ART_attackBonus > 0)
                //    {
                //        enemyDamage += unit.damage * (1.0f + ((float)fightRegion.regionTerrain.ART_attackBonus / 100));
                //    }
                //}
                //else if (unit.type == UnitScriptableObject.Type.TANK)
                //{
                //    if (fightRegion.regionTerrain.HEA_attackBonus < 0)
                //    {
                //        enemyDamage -= unit.damage * (1.0f + ((float)fightRegion.regionTerrain.HEA_attackBonus / 100));
                //    }
                //    if (fightRegion.regionTerrain.HEA_attackBonus > 0)
                //    {
                //        enemyDamage += unit.damage * (1.0f + ((float)fightRegion.regionTerrain.HEA_attackBonus / 100));
                //    }
                //}
            }
        }

        foreach (UnitMovement.UnitHealth unit in attackerUnit.unitsHealth)
        {
            myUnits.Add(unit);
            myDamage += unit.unit.damage;

            //if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
            //{
            //    if (fightRegion.regionTerrain.INF_attackBonus < 0)
            //    {
            //        myDamage -= unit.unit.damage * (1 - (fightRegion.regionTerrain.INF_attackBonus / 100));
            //    }
            //    if (fightRegion.regionTerrain.INF_attackBonus > 0)
            //    {
            //        myDamage += unit.unit.damage * (1 + (fightRegion.regionTerrain.INF_attackBonus / 100));
            //    }
            //}
            //else if (unit.unit.type == UnitScriptableObject.Type.ARTILERY)
            //{
            //    if (fightRegion.regionTerrain.ART_attackBonus < 0)
            //    {
            //        myDamage -= unit.unit.damage * (1 - (fightRegion.regionTerrain.ART_attackBonus / 100));
            //    }
            //    if (fightRegion.regionTerrain.ART_attackBonus > 0)
            //    {
            //        myDamage += unit.unit.damage * (1 + (fightRegion.regionTerrain.ART_attackBonus / 100));
            //    }
            //}
            //else if (unit.unit.type == UnitScriptableObject.Type.TANK)
            //{
            //    if (fightRegion.regionTerrain.HEA_attackBonus < 0)
            //    {
            //        myDamage -= unit.unit.damage * (1 - (fightRegion.regionTerrain.HEA_attackBonus / 100));
            //    }
            //    if (fightRegion.regionTerrain.HEA_attackBonus > 0)
            //    {
            //        myDamage += unit.unit.damage * (1 + (fightRegion.regionTerrain.HEA_attackBonus / 100));
            //    }
            //}
        }

        myDamage = Mathf.Abs(myDamage);

        if (defenderForts > 0)
        {
            attackerFortsDebuff = defenderForts * 7.5f;
            defenderGlobalBonus_DEFENCE += myDamage * (attackerFortsDebuff / 100);
            myDamage = myDamage - (myDamage * (attackerFortsDebuff / 100));
        }

        ReferencesManager.Instance.army.defenderBonus[1].text = $"+{attackerFortsDebuff}%";

        if (myDamage < enemyDamage)
        {
            float difference = Mathf.Abs(enemyDamage) - Mathf.Abs(myDamage);
            difference = Mathf.Abs(difference * 100 / enemyDamage);

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
        else if (myDamage == enemyDamage)
        {
            winChance = 50;
        }
        else if (myDamage > enemyDamage)
        {
            float difference = Mathf.Abs(myDamage) - Mathf.Abs(enemyDamage);
            difference = Mathf.Abs(difference * 100 / myDamage);

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

        /*
        myDMG = 100%
        enemyDMG = chance
        chance = enemyDamage * 100 / myDamage
        */

        ReferencesManager.Instance.regionUI.winChance = winChance;
        ReferencesManager.Instance.regionUI.hit = hit;
        ReferencesManager.Instance.regionUI.unitMovement = attackerUnit; // ��������� ����, � ���� ���������� ���� ��������
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

        CountPlayerUnitData(myUnits);

        foreach (UnitScriptableObject unit in enemyUnits)
        {
            ReferencesManager.Instance.regionUI.CreateFightUnitUI(unit, ReferencesManager.Instance.regionUI.fightPanelDefenderHorizontalGroup);

            if (unit.type == UnitScriptableObject.Type.SOLDIER)
            {
                enemyInfantry++;
            }
            if (unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
            {
                enemyMotoInfantry++;
            }
            if (unit.type == UnitScriptableObject.Type.ARTILERY)
            {
                enemyArtilery++;
            }
            if (unit.type == UnitScriptableObject.Type.TANK)
            {
                enemyHeavy++;
            }
        }


        if (motoInfantry >= 6 || myHeavy >= 6)
        {
            CheckMotorize(attackerUnit);
        }

        attackerUnit.firstMove = false;
        attackerUnit._movePoints--;

        ReferencesManager.Instance.army.attackerArmy[0].text = myInfantry.ToString();
        ReferencesManager.Instance.army.attackerArmy[1].text = myArtilery.ToString();
        ReferencesManager.Instance.army.attackerArmy[2].text = myHeavy.ToString();

        ReferencesManager.Instance.army.defenderArmy[0].text = enemyInfantry.ToString();
        ReferencesManager.Instance.army.defenderArmy[1].text = enemyArtilery.ToString();
        ReferencesManager.Instance.army.defenderArmy[2].text = enemyHeavy.ToString();
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

        if (winChance >= 50) // ��������� �������
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
        else if (winChance < 50) // �������� �������
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
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.MoveUnit(parent.GetComponent<RegionManager>()._id, regionTo.GetComponent<RegionManager>()._id);
        }
        else
        {
            attackerUnit.transform.position = hit.collider.transform.position;
            attackerUnit.currentProvince.hasArmy = false; // old region are not having army now
            if (!forceParent) attackerUnit.transform.SetParent(regionTo);
            else attackerUnit.transform.SetParent(parent);

            RegionManager newRegion = regionTo.GetComponent<RegionManager>();

            newRegion.hasArmy = true;
            if (selectRegion) newRegion.SelectRegionNoHit(newRegion);
            attackerUnit.currentProvince = regionTo.GetComponent<RegionManager>();
        }

        CountPlayerUnitData(attackerUnit.unitsHealth);
        PlayMoveSFX();

        if (motoInfantry >= 6)
        {
            CheckMotorize(attackerUnit);
        }
        attackerUnit.firstMove = false;
        attackerUnit._movePoints--;

        ReferencesManager.Instance.regionUI.UpdateUnitsUI();
    }


    private void CountPlayerUnitData(List<UnitMovement.UnitHealth> myUnits)
    {
        foreach (UnitMovement.UnitHealth unit in myUnits)
        {
            ReferencesManager.Instance.regionUI.CreateFightUnitUI(unit.unit, ReferencesManager.Instance.regionUI.fightPanelAttackerHorizontalGroup);

            if (unit.unit.type == UnitScriptableObject.Type.SOLDIER)
            {
                myInfantry++;
            }
            if (unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
            {
                motoInfantry++;
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
    }

    private void PlayMoveSFX()
    {
        if (myInfantry >= 5)
        {
            UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.gameSettings.m_infantry_move[Random.Range(0, 1)]);
        }
        if (motoInfantry >= 6)
        {
            UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.gameSettings.m_motorized_infantry_move[Random.Range(0, 1)]);
        }
        if (myHeavy >= 5)
        {
            UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.gameSettings.m_heavy_move[Random.Range(0, 1)]);
        }
    }
}
