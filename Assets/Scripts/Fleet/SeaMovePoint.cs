using System.Collections.Generic;
using UnityEngine;

public class SeaMovePoint : MonoBehaviour
{
    [HideInInspector] public Fleet _attackerFleet;
    [HideInInspector] public Fleet _defenderFleet;

    [HideInInspector] public SeaMovePoint _currentMovePoint;

    [HideInInspector] public SeaRegion regionTransit;
    [HideInInspector] public SeaRegion parentSeaRegion;
    [HideInInspector] public RegionManager parentRegion;

    [HideInInspector] public Fleet actionFleet;

    [SerializeField] private bool noAutoCollider;

    //private void OnMouseDown()
    //{
    //    Action();
    //}

    //private void Start()
    //{
    //    SetRegionInPoint();
    //}

    private void SetRegionInPoint()
    {
        gameObject.AddComponent<PolygonCollider2D>();

        if (transform.parent.GetComponent<SeaRegion>())
        {
            parentSeaRegion = transform.parent.GetComponent<SeaRegion>();

            RaycastHit2D _hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.down);

            if (_hit.collider.gameObject.GetComponent<SeaRegion>())
            {
                regionTransit = _hit.collider.transform.GetComponent<SeaRegion>();

                if (!noAutoCollider)
                {
                    GetComponent<PolygonCollider2D>().points = regionTransit.GetComponent<PolygonCollider2D>().points;
                    transform.position = new Vector3(regionTransit.transform.position.x, regionTransit.transform.position.y, -1);
                }

                parentSeaRegion._movePoints.Clear();
                foreach (Transform child in parentSeaRegion.transform)
                {
                    if (child.GetComponent<SeaMovePoint>() != null)
                    {
                        parentSeaRegion._movePoints.Add(child.GetComponent<SeaMovePoint>());
                    }
                }
            }
        }
        else if (transform.parent.GetComponent<RegionManager>())
        {
            parentRegion = transform.parent.GetComponent<RegionManager>();

            RaycastHit2D _hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.down);

            if (_hit.collider.gameObject.GetComponent<SeaRegion>())
            {
                regionTransit = _hit.collider.transform.GetComponent<SeaRegion>();

                if (!noAutoCollider)
                {
                    GetComponent<PolygonCollider2D>().points = regionTransit.GetComponent<PolygonCollider2D>().points;
                    transform.position = new Vector3(regionTransit.transform.position.x, regionTransit.transform.position.y, -1);
                }

                parentRegion._seaPoints.Clear();
                foreach (Transform child in parentRegion.transform)
                {
                    if (child.GetComponent<SeaMovePoint>() != null)
                    {
                        parentRegion._seaPoints.Add(child.GetComponent<SeaMovePoint>());
                    }
                }
            }
        }
    }

    public void Action()
    {
        if (actionFleet != null)
        {
            if (HasEnemiesInRegion(ReferencesManager.Instance.countryManager.currentCountry, regionTransit))
            {
                Fight(regionTransit);
            }
            else
            {
                Move(regionTransit, actionFleet);
            }
        }
        else
        {
            Debug.LogError($"ERROR: ActionFleet is null");
        }

    }

    private void Fight(SeaRegion fightRegion)
    {

    }

    private void Move(SeaRegion fightRegion, Fleet fleet)
    {
        if (fleet._movePoints > 0)
        {
            if (!fleet.visible)
            {
                fleet.visible = true;
                fleet.SetUp();
            }

            fleet._movePoints--;
            fleet.transform.position = fightRegion.transform.position;
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

    private bool HasEnemiesInRegion(CountrySettings me, SeaRegion seaRegion)
    {
        bool result = GetEnemiesFleetsInRegion(me, seaRegion).Count > 0;

        return result;
    }
}
