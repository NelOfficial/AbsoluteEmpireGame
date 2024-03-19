using UnityEngine;

public class SeaMovePoint : MonoBehaviour
{
    [HideInInspector] public Fleet _attackerFleet;
    [HideInInspector] public Fleet _defenderFleet;

    [HideInInspector] public SeaMovePoint _currentMovePoint;

    private RegionManager newRegion;
    private RegionManager attackerRegion;

    [HideInInspector] public SeaRegion regionTransit;
    [HideInInspector] public SeaRegion parentRegion;

    [SerializeField] private bool noAutoCollider;


    private void Start()
    {
        SetRegionInPoint();
    }

    private void SetRegionInPoint()
    {
        RaycastHit2D _hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.down);
        if (_hit.collider.gameObject.GetComponent<RegionManager>())
        {
            regionTransit = _hit.collider.transform.GetComponent<SeaRegion>();
            parentRegion = transform.parent.GetComponent<SeaRegion>();

            if (!noAutoCollider)
            {
                this.GetComponent<PolygonCollider2D>().points = regionTransit.GetComponent<PolygonCollider2D>().points;
                this.transform.position = new Vector3(regionTransit.transform.position.x, regionTransit.transform.position.y, -1);
            }

            parentRegion._movePoints.Clear();
            foreach (Transform child in parentRegion.transform)
            {
                if (child.GetComponent<SeaMovePoint>() != null)
                {
                    parentRegion._movePoints.Add(child.GetComponent<SeaMovePoint>());
                }
            }
        }
    }
}
