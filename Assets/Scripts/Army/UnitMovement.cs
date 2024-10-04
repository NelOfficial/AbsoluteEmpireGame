using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using static GameSettings;
using System.Runtime.InteropServices.WindowsRuntime;

public class UnitMovement : MonoBehaviour
{
    private List<Transform> nearestPoints = new();

    [HideInInspector] public CountryManager countryManager;
    [HideInInspector] public RegionManager regionManager;
    [HideInInspector] public RegionManager currentProvince;
    [HideInInspector] public Army army;
    [HideInInspector] public RegionManager moveto;

    public CountrySettings currentCountry;
    public List<UnitHealth> unitsHealth = new List<UnitHealth>();

    private UnitMovement attackerUnit;
    private UnitMovement defenderUnit;

    [HideInInspector] public bool isSelected;

    [HideInInspector] public int fortification;
    [HideInInspector] public int _movePoints;

    [HideInInspector] public bool firstMove;
    public bool inSea = false;
    public SeaRegion _currentSeaRegion;

    private int winChance;

    [SerializeField] Image flagImage;
    [SerializeField] SpriteRenderer _renderer;
    [SerializeField] GameObject canMoveState;

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

    private bool HasEnemyInRegion_Unit(CountrySettings me, SeaRegion seaRegion)
    {
        bool result = GetEnemyInRegion_Unit(me, seaRegion);

        return result;
    }

    public void SeaMove(UnitMovement actionUnit, SeaRegion destinationRegion)
    {
        if (actionUnit != null)
        {
            if (HasEnemyInRegion_Unit(actionUnit.currentCountry, destinationRegion))
            {
                //Fight(destinationRegion);
            }
            else
            {
                if (actionUnit.currentProvince != null)
                {
                    if (actionUnit._movePoints > 0 && destinationRegion._division == null)
                    {
                        actionUnit.inSea = true;
                        actionUnit._currentSeaRegion = destinationRegion;

                        actionUnit._movePoints--;
                        actionUnit.transform.position = destinationRegion.transform.position;

                        destinationRegion._division = actionUnit;
                        actionUnit.RemoveClosestsPoints();

                        actionUnit.transform.parent = destinationRegion.transform;

                        actionUnit.currentProvince.CheckRegionUnits(actionUnit.currentProvince);
                        actionUnit.currentProvince = null;
                        actionUnit.UpdateInfo();
                    }
                }
                else if (actionUnit._currentSeaRegion != null)
                {
                    if (actionUnit._movePoints > 0 && destinationRegion._division == null)
                    {
                        actionUnit.inSea = true;
                        actionUnit._currentSeaRegion._division = null;
                        actionUnit._currentSeaRegion = destinationRegion;
                        actionUnit.currentProvince = null;

                        actionUnit._movePoints--;
                        actionUnit.transform.position = destinationRegion.transform.position;

                        destinationRegion._division = actionUnit;
                        actionUnit.RemoveClosestsPoints();

                        actionUnit.transform.parent = destinationRegion.transform;

                        actionUnit.UpdateInfo();
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"ERROR: Action target is null");
        }
    }

    public void FromSeaToGroundMove(UnitMovement actionUnit, RegionManager _destinationRegion)
    {
        if (actionUnit._currentSeaRegion != null)
        {
            _destinationRegion.CheckRegionUnits(_destinationRegion);

            if (_destinationRegion.currentCountry == actionUnit.currentCountry)
            {
                _destinationRegion.CheckRegionUnits(_destinationRegion);

                if (actionUnit._movePoints > 0 && _destinationRegion.hasArmy == false)
                {
                    actionUnit.inSea = false;
                    actionUnit._currentSeaRegion._division = null;
                    actionUnit._currentSeaRegion = null;

                    actionUnit._movePoints--;
                    actionUnit.transform.position = _destinationRegion.transform.position;

                    _destinationRegion.hasArmy = true;
                    actionUnit.RemoveClosestsPoints();

                    actionUnit.transform.parent = _destinationRegion.transform;

                    actionUnit.currentProvince = _destinationRegion;
                    actionUnit.currentProvince.CheckRegionUnits(actionUnit.currentProvince);
                    actionUnit.UpdateInfo();
                }
            }
            else
            {
                Relationships.Relation relations = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(actionUnit.currentCountry, _destinationRegion.currentCountry);

                if (relations.war)
                {
                    //Fight(_destinationRegion, actionUnit);
                }
                else if (relations.right)
                {
                    actionUnit.inSea = false;
                    actionUnit._currentSeaRegion._division = null;
                    actionUnit._currentSeaRegion = null;

                    actionUnit._movePoints--;
                    actionUnit.transform.position = _destinationRegion.transform.position;

                    _destinationRegion.hasArmy = true;
                    actionUnit.RemoveClosestsPoints();

                    actionUnit.transform.parent = _destinationRegion.transform;

                    actionUnit.currentProvince = _destinationRegion;
                    actionUnit.currentProvince.CheckRegionUnits(actionUnit.currentProvince);
                    actionUnit.UpdateInfo();
                }
                else if (!relations.right)
                {

                }
            }
        }
    }

    public void ShowClosestsPoints()
    {
        nearestPoints.Clear();

        if (regionManager.currentRegionManager != null)
        {
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
                point.gameObject.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("RegionInfo");
            }

            foreach (SeaMovePoint seaMovePoint in regionManager.currentRegionManager._seaPoints)
            {
                seaMovePoint.gameObject.AddComponent<SpriteRenderer>();
                seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 99;
                seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "RegionInfo";
                seaMovePoint.gameObject.GetComponent<PolygonCollider2D>().enabled = true;

                if (seaMovePoint.regionTransit._division != null)
                {
                    if (seaMovePoint.regionTransit._division.currentCountry == this.currentCountry)
                    {
                        seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.pointSprite;

                        seaMovePoint.regionTransit.GetComponent<SpriteRenderer>().color =
                            new Color(
                            ReferencesManager.Instance.gameSettings.greenColor.r,
                            ReferencesManager.Instance.gameSettings.greenColor.g,
                            ReferencesManager.Instance.gameSettings.greenColor.b, 0.5f);
                    }
                    else
                    {
                        Relationships.Relation relations = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(this.currentCountry, seaMovePoint.regionTransit._division.currentCountry);

                        if (relations.war)
                        {
                            seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.attackSprite;
                            seaMovePoint.regionTransit.GetComponent<SpriteRenderer>().color =
                                new Color(
                                ReferencesManager.Instance.gameSettings.redColor.r,
                                ReferencesManager.Instance.gameSettings.redColor.g,
                                ReferencesManager.Instance.gameSettings.redColor.b, 0.5f);
                        }
                        else if (relations.right)
                        {
                            seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.pointSprite;

                            seaMovePoint.regionTransit.GetComponent<SpriteRenderer>().color =
                                new Color(
                                ReferencesManager.Instance.gameSettings.greenColor.r,
                                ReferencesManager.Instance.gameSettings.greenColor.g,
                                ReferencesManager.Instance.gameSettings.greenColor.b, 0.5f);
                        }
                        else if (!relations.right)
                        {
                            seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.chatSprite;
                            seaMovePoint.regionTransit.GetComponent<SpriteRenderer>().color =
                                new Color(
                                ReferencesManager.Instance.gameSettings.blueColor.r,
                                ReferencesManager.Instance.gameSettings.blueColor.g,
                                ReferencesManager.Instance.gameSettings.blueColor.b, 0.5f);
                        }
                    }
                }
                else
                {
                    seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.pointSprite;

                    seaMovePoint.regionTransit.GetComponent<SpriteRenderer>().color =
                        new Color(
                        ReferencesManager.Instance.gameSettings.greenColor.r,
                        ReferencesManager.Instance.gameSettings.greenColor.g,
                        ReferencesManager.Instance.gameSettings.greenColor.b, 0.5f);
                }
            }
        }

        if (ReferencesManager.Instance.seaRegionManager._currentSeaRegion != null)
        {
            foreach (SeaMovePoint seaMovePoint in ReferencesManager.Instance.seaRegionManager._currentSeaRegion._movePoints)
            {
                seaMovePoint.gameObject.AddComponent<SpriteRenderer>();
                seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 99;
                seaMovePoint.gameObject.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("RegionInfo");
                seaMovePoint.gameObject.GetComponent<PolygonCollider2D>().enabled = true;

                if (seaMovePoint.regionTransit._division != null)
                {
                    if (seaMovePoint.regionTransit._division.currentCountry == this.currentCountry)
                    {
                        seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.pointSprite;

                        seaMovePoint.regionTransit.GetComponent<SpriteRenderer>().color =
                            new Color(
                            ReferencesManager.Instance.gameSettings.greenColor.r,
                            ReferencesManager.Instance.gameSettings.greenColor.g,
                            ReferencesManager.Instance.gameSettings.greenColor.b, 0.5f);
                    }
                    else
                    {
                        Relationships.Relation relations = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(this.currentCountry, seaMovePoint.regionTransit._division.currentCountry);

                        if (relations.war)
                        {
                            seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.attackSprite;
                            seaMovePoint.regionTransit.GetComponent<SpriteRenderer>().color =
                                new Color(
                                ReferencesManager.Instance.gameSettings.redColor.r,
                                ReferencesManager.Instance.gameSettings.redColor.g,
                                ReferencesManager.Instance.gameSettings.redColor.b, 0.5f);
                        }
                        else if (relations.right)
                        {
                            seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.pointSprite;

                            seaMovePoint.regionTransit.GetComponent<SpriteRenderer>().color =
                                new Color(
                                ReferencesManager.Instance.gameSettings.greenColor.r,
                                ReferencesManager.Instance.gameSettings.greenColor.g,
                                ReferencesManager.Instance.gameSettings.greenColor.b, 0.5f);
                        }
                        else if (!relations.right)
                        {
                            seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.chatSprite;
                            seaMovePoint.regionTransit.GetComponent<SpriteRenderer>().color =
                                new Color(
                                ReferencesManager.Instance.gameSettings.blueColor.r,
                                ReferencesManager.Instance.gameSettings.blueColor.g,
                                ReferencesManager.Instance.gameSettings.blueColor.b, 0.5f);
                        }
                    }
                }
                else
                {
                    seaMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.pointSprite;

                    seaMovePoint.regionTransit.GetComponent<SpriteRenderer>().color =
                        new Color(
                        ReferencesManager.Instance.gameSettings.greenColor.r,
                        ReferencesManager.Instance.gameSettings.greenColor.g,
                        ReferencesManager.Instance.gameSettings.greenColor.b, 0.5f);
                }
            }

            foreach (FromSeaToGround_MovePoint toGroundMovePoint in ReferencesManager.Instance.seaRegionManager._currentSeaRegion._toGroundMovePoints)
            {
                toGroundMovePoint.gameObject.AddComponent<SpriteRenderer>();
                toGroundMovePoint.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 99;
                toGroundMovePoint.gameObject.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("RegionInfo");
                toGroundMovePoint.gameObject.GetComponent<PolygonCollider2D>().enabled = true;

                toGroundMovePoint._destinationRegion.CheckRegionUnits(toGroundMovePoint._destinationRegion);

                if (toGroundMovePoint._destinationRegion.currentCountry == this.currentCountry)
                {
                    if (!toGroundMovePoint._destinationRegion.GetDivision(toGroundMovePoint._destinationRegion))
                    {
                        toGroundMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.pointSprite;

                        toGroundMovePoint._destinationRegion.GetComponent<SpriteRenderer>().color =
                            new Color(
                            ReferencesManager.Instance.gameSettings.greenColor.r,
                            ReferencesManager.Instance.gameSettings.greenColor.g,
                            ReferencesManager.Instance.gameSettings.greenColor.b, 0.5f);
                    }
                    else
                    {
                        toGroundMovePoint.gameObject.GetComponent<PolygonCollider2D>().enabled = true;
                        Destroy(toGroundMovePoint.gameObject.GetComponent<SpriteRenderer>());
                    }
                }
                else
                {
                    Relationships.Relation relations = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(this.currentCountry, toGroundMovePoint._destinationRegion.currentCountry);

                    if (relations.war)
                    {
                        toGroundMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.attackSprite;
                        toGroundMovePoint._destinationRegion.GetComponent<SpriteRenderer>().color =
                            new Color(
                            ReferencesManager.Instance.gameSettings.redColor.r,
                            ReferencesManager.Instance.gameSettings.redColor.g,
                            ReferencesManager.Instance.gameSettings.redColor.b, 0.5f);
                    }
                    else if (relations.right)
                    {
                        toGroundMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.pointSprite;

                        toGroundMovePoint._destinationRegion.GetComponent<SpriteRenderer>().color =
                            new Color(
                            ReferencesManager.Instance.gameSettings.greenColor.r,
                            ReferencesManager.Instance.gameSettings.greenColor.g,
                            ReferencesManager.Instance.gameSettings.greenColor.b, 0.5f);
                    }
                    else if (!relations.right)
                    {
                        toGroundMovePoint.gameObject.GetComponent<SpriteRenderer>().sprite = army.chatSprite;
                        toGroundMovePoint._destinationRegion.GetComponent<SpriteRenderer>().color =
                            new Color(
                            ReferencesManager.Instance.gameSettings.blueColor.r,
                            ReferencesManager.Instance.gameSettings.blueColor.g,
                            ReferencesManager.Instance.gameSettings.blueColor.b, 0.5f);
                    }
                }
            }
        }
    }

    public void RemoveClosestsPoints()
    {
        nearestPoints.Clear();

        if (regionManager.currentRegionManager != null)
        {
            foreach (Transform point in regionManager.currentRegionManager.movePoints)
            {
                nearestPoints.Add(point);
                regionManager.currentRegionManager.SelectRegionNoHit(point.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>());
                Destroy(point.gameObject.GetComponent<SpriteRenderer>());
            }
        }

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
    }

    public bool AIMoveNoHit(RegionManager defenderRegion, UnitMovement division)
    {
        bool result = false;

        if (!division.inSea && defenderRegion != null && division != null)
        {
            RegionManager attackerRegion = division.currentProvince;
            defenderRegion.CheckRegionUnits(defenderRegion);

            if (division.currentCountry.exist && division != null && attackerRegion != null && division._movePoints > 0)
            {
                if (defenderRegion.currentCountry == attackerRegion.currentCountry) // Just move
                { // my country
                    if (!defenderRegion.hasArmy)
                    {
                        division._movePoints--;
                        division.firstMove = false;

                        MoveUnit(defenderRegion, attackerRegion, division);

                        attackerRegion.hasArmy = false;
                        defenderRegion.hasArmy = true;
                    }

                    result = true;
                }
                else
                {
                    foreach (Relationships.Relation realtion in
                        division.currentCountry.relations.relationship)
                    {
                        if (realtion.country == defenderRegion.currentCountry)
                        {
                            if (realtion.war) // War
                            {
                                if (division.currentProvince.currentCountry == division.currentCountry ||

                                    ReferencesManager.Instance.diplomatyUI.
                                    FindCountriesRelation(division.currentProvince.currentCountry,
                                    division.currentCountry).right)
                                {
                                    division._movePoints--;
                                    division.firstMove = false;

                                    Fight(defenderRegion, attackerRegion, division);

                                    foreach (UnitHealth unit in division.unitsHealth)
                                    {
                                        unit.fuel -= 100;
                                    }
                                }
                            }
                            else if (realtion.right || division.currentCountry == defenderRegion.currentCountry)
                            {
                                defenderRegion.CheckRegionUnits(defenderRegion);

                                if (!defenderRegion.hasArmy)
                                {
                                    division._movePoints--;
                                    division.firstMove = false;

                                    MoveUnit(defenderRegion, attackerRegion, division);

                                    attackerRegion.hasArmy = false;
                                    defenderRegion.hasArmy = true;
                                }
                            }
                            else if (!realtion.right)
                            {
                                if (division.currentCountry.isPlayer)
                                {
                                    ReferencesManager.Instance.diplomatyUI.OpenUINoClick(defenderRegion.currentCountry.country._id);
                                }
                                //division.currentCountry.GetComponent<CountryAIManager>().SendOffer("Право прохода войск",
                                //    division.currentCountry, defenderRegion.currentCountry);
                            }
                        }
                    }
                    result = true;
                }
            }
            result = false;
        }

        return result;
    }

    public void MoveUnit(RegionManager regionTo, RegionManager oldRegion, UnitMovement division)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.MoveUnit(oldRegion._id, regionTo._id);
        }
        else
        {
            oldRegion.hasArmy = false;

            division.transform.position = regionTo.transform.position;
            division.UpdateInfo();

            division.transform.SetParent(regionTo.transform);

            RegionManager newRegion = regionTo;

            newRegion.hasArmy = true;
            newRegion.CheckRegionUnits(newRegion);

            division.currentProvince = newRegion;

            foreach (UnitHealth unit in division.unitsHealth)
            {
                unit.fuel -= 50;
            }

            if (division.currentCountry.isPlayer)
            {
                PlayMoveSFX(division);
            }
        }
    }

    private void PlayMoveSFX(UnitMovement division)
    {
        int heavy = 0;
        int infantry = 0;
        int motorized = 0;

        foreach (var batalion in division.unitsHealth)
        {
            if (batalion.unit.type == UnitScriptableObject.Type.SOLDIER || 
                batalion.unit.type == UnitScriptableObject.Type.ARTILERY ||
                batalion.unit.type == UnitScriptableObject.Type.CAVALRY)
            {
                infantry++;
            }
            else if (batalion.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
            {
                motorized++;
            }
            else if (batalion.unit.type == UnitScriptableObject.Type.TANK)
            {
                heavy++;
            }
        }

        if (infantry / division.unitsHealth.Count >= 0.55f)
        {
            UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.gameSettings.m_infantry_move[Random.Range(0, 1)]);
        }
        else if (motorized / division.unitsHealth.Count >= 0.55f)
        {
            UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.gameSettings.m_motorized_infantry_move[Random.Range(0, 1)]);
        }
        else if (heavy / division.unitsHealth.Count >= 0.55f)
        {
            UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.gameSettings.m_heavy_move[Random.Range(0, 1)]);
        }
    }

    private void Fight(RegionManager fightRegion, RegionManager oldRegion, UnitMovement division)
    {
        UnitMovement.BattleInfo battle = new UnitMovement.BattleInfo();

        battle.defender_BONUS_ATTACK = 100;
        battle.defender_BONUS_DEFENCE = 100;

        battle.attacker_BONUS_ATTACK = 100;
        battle.attacker_BONUS_DEFENCE = 100;

        attackerUnit = division;

        fightRegion.CheckRegionUnits(fightRegion);
        oldRegion.CheckRegionUnits(oldRegion);

        if (attackerUnit != null)
        {
            winChance = 0;

            battle.defenderForts = fightRegion.fortifications_Amount;

            try
            {
                if (defenderUnit.Encircled(defenderUnit.currentProvince))
                {
                    battle.defender_BONUS_ATTACK -= 50;
                    battle.defender_BONUS_DEFENCE -= 50;
                }
            }
            catch (System.Exception) {}

            try
            {
                if (attackerUnit.Encircled(attackerUnit.currentProvince))
                {
                    battle.attacker_BONUS_ATTACK -= 50;
                    battle.attacker_BONUS_DEFENCE -= 50;
                }
            }
            catch (System.Exception) {}


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
                    fightRegion.hasArmy = false;
                }
            }
            else if (!fightRegion.hasArmy)
            {
                List<float> defenderArmors = new List<float>();

                battle.defenderDivision = null;
                battle.fightRegion = fightRegion;
                battle.enemyUnits = battle.fightRegion.currentDefenseUnits;

                foreach (UnitHealth unit in fightRegion.currentDefenseUnits)
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

                    battle.defenderSoftAttack += unit_defenderSoftAttack;
                    battle.defenderHardAttack += unit_defenderHardAttack;
                    battle.defenderDefense += unit.unit.defense;
                    battle.defenderArmor += unit.unit.armor;
                    battle.defenderArmorPiercing += unit.unit.armorPiercing;
                    battle.defenderHardness += unit.unit.hardness;
                }

                float maxArmor = defenderArmors.Max();
                float midArmor = battle.defenderArmor / battle.fightRegion.currentDefenseUnits.Count;

                battle.defenderHardness = battle.defenderHardness / battle.fightRegion.currentDefenseUnits.Count;
                battle.defenderArmor = 0.4f * maxArmor + 0.6f * midArmor;
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
                        UnitMovement _division = child.GetComponent<UnitMovement>();
                        _division.Retreat(_division);
                        break;
                    }
                }

                MoveUnit(fightRegion, oldRegion, attackerUnit);

                CountrySettings newOwner = new CountrySettings();

                List<CountryScriptableObject> myAllies_SCO = new List<CountryScriptableObject>();

                newOwner = currentCountry;

                foreach (CountrySettings country in countryManager.countries)
                {
                    if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(currentCountry, country).union)
                    {
                        if (fightRegion.regionClaims.Contains(country.country))
                        {
                            myAllies_SCO.Add(country.country);

                            if (ReferencesManager.Instance.isCountryHasBordersWithProvince(country, fightRegion))
                            {
                                newOwner = country;
                                break;
                            }
                        }
                    }
                }

                ReferencesManager.Instance.AnnexRegion(fightRegion, newOwner);
            }

            battle.winChance = winChance;
            ApplyDamage(battle);

            UpdateInfo();
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
            int myCavlry = 0;

            bool hasFuel = false;

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

                if (unit.fuel >= 50 || unit.unit.maxFuel <= 0)
                {
                    hasFuel = true;
                }
            }

            if (myMotoInfantry / unitMovement.unitsHealth.Count >= 0.55f || 
                myHeavy / unitMovement.unitsHealth.Count >= 0.55f || 
                myCavlry / unitMovement.unitsHealth.Count >= 0.55f)
            {
                if (unitMovement.firstMove && hasFuel)
                {
                    unitMovement._movePoints++;
                }
            }
        }
    }

    private void ApplyDamage(UnitMovement.BattleInfo battle)
    {
        int att_inf_losses = 0;
        int att_art_losses = 0;
        int att_hvy_losses = 0;

        int def_inf_losses = 0;
        int def_art_losses = 0;
        int def_hvy_losses = 0;

        float defender_losses_factor = 1;
        float attacker_losses_factor = 1;

        bool defenderWin = false;
        bool attackerWin = false;

        if (battle.winChance >= 50) attackerWin = true;
        else defenderWin = true;

        if (winChance <= 0)
        {
            winChance = Random.Range(1, 5);
        }

        if (attackerWin)
        {
            defender_losses_factor = 1 / (winChance / (101f - winChance));
            attacker_losses_factor = ((101f - winChance) / winChance);
        }
        else if (defenderWin)
        {
            attacker_losses_factor = ((101f - winChance) / winChance);
            defender_losses_factor = 1 / (winChance / (101f - winChance));
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

                    defenderUnit.currentCountry.myRegions[Random.Range(0, defenderUnit.currentCountry.myRegions.Count)].population -= defenderUnit.unitsHealth[j].unit.recrootsCost;
                    defenderUnit.unitsHealth.Remove(defenderUnit.unitsHealth[j]);
                }
            }
            if (defenderUnit.unitsHealth.Count < 1)
            {
                defenderUnit.currentProvince = transform.parent.GetComponent<RegionManager>();
                StartCoroutine(DestroyDivision_Co());
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

                attackerUnit.currentCountry.myRegions[Random.Range(0, attackerUnit.currentCountry.myRegions.Count)].population -= attackerUnit.unitsHealth[j].unit.recrootsCost;
                attackerUnit.unitsHealth.Remove(attackerUnit.unitsHealth[j]);
            }
        }
        if (attackerUnit.unitsHealth.Count < 1)
        {
            attackerUnit.currentProvince = transform.parent.GetComponent<RegionManager>();
            StartCoroutine(DestroyDivision_Co());
        }

        #endregion

        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.SetCountryIncomeValues(
                defenderUnit.currentCountry.country._id,
                defenderUnit.currentCountry.moneyNaturalIncome,
                defenderUnit.currentCountry.foodNaturalIncome,
                defenderUnit.currentCountry.recruitsIncome);

            Multiplayer.Instance.SetCountryIncomeValues(
                currentCountry.country._id,
                currentCountry.moneyNaturalIncome, 
                currentCountry.foodNaturalIncome, 
                currentCountry.recruitsIncome);
        }
    }

    public IEnumerator DestroyDivision_Co()
    {
        this.GetComponent<Animator>().Play("divisionDie");
        currentProvince.hasArmy = false;
        yield return new WaitForSecondsRealtime(0.6f);
        attackerUnit.currentProvince.CheckRegionUnits(attackerUnit.currentProvince);
    }

    public void UpdateInfo()
    {
        flagImage.sprite = currentCountry.country.countryFlag;

        if (ReferencesManager.Instance.countryManager.currentCountry == currentCountry)
        {
            if (_movePoints > 0)
            {
                canMoveState.SetActive(true);
            }
            else
            {
                canMoveState.SetActive(false);
            }
        }
        else
        {
            canMoveState.SetActive(false);
        }

        int myInfantry = 0;
        int myMotoInfantry = 0;
        int myArtilery = 0;
        int myHeavy = 0;
        int myCavlry = 0;

        foreach (UnitHealth unit in unitsHealth)
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
            if (unit.unit.type == UnitScriptableObject.Type.CAVALRY)
            {
                myCavlry++;
            }
        }

        if (!inSea)
        {
            try
            {
                if ((float)myInfantry / (float)unitsHealth.Count >= 0.55f)
                {
                    _renderer.sprite = ReferencesManager.Instance.gameSettings.soldierLVL2.icon;
                }
                else if ((float)myArtilery / (float)unitsHealth.Count >= 0.55f)
                {
                    _renderer.sprite = ReferencesManager.Instance.gameSettings.artileryLVL1.icon;
                }
                else if ((float)myHeavy / (float)unitsHealth.Count >= 0.55f)
                {
                    _renderer.sprite = ReferencesManager.Instance.gameSettings.tankLVL2.icon;
                }
                else if ((float)myMotoInfantry / (float)unitsHealth.Count >= 0.55f)
                {
                    _renderer.sprite = ReferencesManager.Instance.gameSettings.motoLVL1.icon;
                }
                else if ((float)myCavlry / (float)unitsHealth.Count >= 0.55f)
                {
                    _renderer.sprite = ReferencesManager.Instance.gameSettings.cavLVL2.icon;
                }
            }
            catch (System.Exception) { }
        }
        else
        {
            _renderer.sprite = ReferencesManager.Instance.fleetManager._destroyerSprite;
        }
    }

    public void Retreat(UnitMovement division)
    {
        //Debug.Log(division.currentCountry);
        //Instantiate(ReferencesManager.Instance.army.encircleAnimation, division.currentProvince.transform);
        //Destroy(division.gameObject);

        if (!division.inSea)
        {

            RegionManager regionToRetreat;
            RegionManager myRegion = division.transform.parent.GetComponent<RegionManager>();
            MovePoint _point;

            myRegion.CheckRegionUnits(myRegion);

            if (myRegion.hasArmy)
            {
                List<Transform> nearMyPoints = new List<Transform>();
                List<RegionManager> nearRegionsWithArmy = new List<RegionManager>();

                for (int i = 0; i < myRegion.movePoints.Count; i++)
                {
                    _point = myRegion.movePoints[i].GetComponent<MovePoint>();
                    RegionManager regionTo = _point.regionTo.GetComponent<RegionManager>();
                    regionTo.CheckRegionUnits(regionTo);

                    if (regionTo.currentCountry == division.currentCountry)
                    {
                        if (!regionTo.hasArmy)
                        {
                            nearMyPoints.Add(_point.transform);
                        }
                        else
                        {
                            nearRegionsWithArmy.Add(regionTo);
                        }
                    }
                }

                if (nearMyPoints.Count > 0)
                {
                    for (int i = 0; i < nearMyPoints.Count; i++) // Есть куда отступать
                    {
                        regionToRetreat = nearMyPoints[i].GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>();

                        regionToRetreat.CheckRegionUnits(regionToRetreat);

                        if (!regionToRetreat.hasArmy && regionToRetreat.currentCountry.country._id == division.currentCountry.country._id)
                        {
                            if (division._movePoints <= 0)
                            {
                                division._movePoints++;
                            }

                            if (AIMoveNoHit(regionToRetreat, division))
                            {
                                return;
                            }
                        }
                    }

                }
                else if (nearRegionsWithArmy.Count > 0 && nearMyPoints.Count < 0)
                {
                    foreach (var province in nearRegionsWithArmy)
                    {
                        UnitMovement divisionInOtherReg = province.GetDivision(province);
                        divisionInOtherReg.Retreat(divisionInOtherReg);
                        if (!province.hasArmy)
                        {
                            if (AIMoveNoHit(province, myRegion.GetDivision(myRegion)))
                            {
                                return;
                            }
                        }
                    }
                    Instantiate(ReferencesManager.Instance.army.encircleAnimation, myRegion.transform);
                    Destroy(myRegion.gameObject);
                }
                else /*if (nearMyPoints.Count <= 0)*/ // Нет путей вообще (Фактически котел на 1 провинцию)
                {
                    division.currentProvince.CheckRegionUnits(division.currentProvince);

                    foreach (Transform child in division.currentProvince.transform)
                    {
                        if (child.gameObject.GetComponent<UnitMovement>())
                        {
                            Instantiate(ReferencesManager.Instance.army.encircleAnimation, division.currentProvince.transform);
                            Destroy(child.gameObject);
                        }
                    }
                }
            }
            else
            {
                myRegion.CheckRegionUnits(myRegion);
            }
        }
        else
        {
            List<SeaMovePoint> pointsWithArmy = new();
            List<SeaMovePoint> pointsFree = new();

            List<FromSeaToGround_MovePoint> groundPointsWithArmy = new();
            List<FromSeaToGround_MovePoint> groundPointsFree = new();

            foreach (SeaMovePoint seaMovePoint in division._currentSeaRegion._movePoints)
            {
                if (seaMovePoint.regionTransit._division != null && seaMovePoint.regionTransit._division.currentCountry == division.currentCountry)
                {
                    pointsWithArmy.Add(seaMovePoint);
                }
                else if (seaMovePoint.regionTransit._division == null)
                {
                    pointsFree.Add(seaMovePoint);
                }
            }

            foreach (FromSeaToGround_MovePoint ground_MovePoint in division._currentSeaRegion._toGroundMovePoints)
            {
                UnitMovement groundPointArmy = ground_MovePoint._destinationRegion.GetDivision(ground_MovePoint._destinationRegion);
                if (groundPointArmy != null && groundPointArmy.currentCountry == division.currentCountry)
                {
                    groundPointsWithArmy.Add(ground_MovePoint);
                }
                else if (groundPointArmy == null)
                {
                    groundPointsFree.Add(ground_MovePoint);
                }
            }

            if (pointsFree.Count > 0)
            {
                division._movePoints = 1;

                if (division._movePoints > 0)
                {
                    SeaMove(division, pointsFree[Random.Range(0, pointsFree.Count)].regionTransit);
                }
            }
            else if (pointsFree.Count < 0 && groundPointsFree.Count > 0)
            {
                division._movePoints = 1;

                if (division._movePoints > 0)
                {
                    FromSeaToGroundMove(division, groundPointsFree[Random.Range(0, groundPointsFree.Count)]._destinationRegion);
                }
            }
        }
    }

    public List<RegionManager> GetNeiboursOfRegion(RegionManager region)
    {
        var regions = new List<RegionManager>();

        foreach (var movePoint in region.movePoints)
        {
            try
            {
                RegionManager _region = null;

                _region = movePoint.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>();

                if (_region != null)
                {
                    regions.Add(_region);
                }
            }
            catch (System.Exception)
            {
                Debug.LogError($"{movePoint.parent.name} -> {movePoint.name}");
            }
        }

        return regions;
    }

    public bool Encircled(RegionManager fightRegion)
    {
        bool isEncircled = false;
        MovePoint _point;

        fightRegion.CheckRegionUnits(fightRegion);

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

            if (nearMyPoints.Count < 1 && fightRegion.currentCountry.myRegions.Count > 1) // Если нет таких точек, то дивизия окружена
            {
                isEncircled = true;
            }
        }

        return isEncircled;
    }

    public void Destroy()
    {
        if (gameObject.activeSelf)
        {
            Destroy(this.gameObject);
        }
    }

    [System.Serializable]
    public class BattleInfo
    {
        public int _id;

        public List<UnitHealth> enemyUnits = new List<UnitHealth>();
        public List<UnitHealth> myUnits = new List<UnitHealth>();

        public int myInfantry = 0;
        public int enemyInfantry = 0;

        public int myCavlry = 0;
        public int enemyCavlry = 0;

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

        public SeaRegion seaFightRegion;
        public SeaRegion seaAttackerRegion;

        public UnitMovement defenderDivision;
        public UnitMovement attackerDivision;

        public float attackerStrength;
        public float defenderStrength;

        public int winChance;
    }
}

[System.Serializable]
public class UnitHealth
{
    public int _id;
    public float health;
    public UnitScriptableObject unit;
    public float fuel;
}
