using UnityEngine;

public class FleetManager : MonoBehaviour
{
    [SerializeField] private GameObject fleetPrefab;

    [SerializeField] private LayerMask layerMask;

    [SerializeField] private float checkDistance = 0.6f;
    [SerializeField] private float shoreOffset = 0.5f;  // Смещение корабля к берегу

    [SerializeField] private Transform _fleetHorizontalContainer;

    [SerializeField] private GameObject _shipUIPrefab;
    [SerializeField] private GameObject _addShipButton;


    public void CreateFleet()
    {
        RegionManager province = ReferencesManager.Instance.regionManager.currentRegionManager;

        province._hasFleet = true;

        Vector2 moveDirection = DetermineMoveDirection(province);

        if (moveDirection != Vector2.zero)
        {
            SpawnShipAtEdge(moveDirection, province);
        }
        else
        {
            Debug.Log("Все стороны заняты");
        }
    }

    public void AddUnitToFleet(Fleet fleet, FleetScriptableObject unit)
    {
        if (fleet._fleetUnits.Count + 1 <= 10)
        {
            Fleet.FleetUnitData fleetUnitData = new Fleet.FleetUnitData();

            fleetUnitData._id = fleet._fleetUnits.Count;
            fleetUnitData._unit = unit;
            fleetUnitData._health = unit.health;
            fleetUnitData._fuel = unit.maxFuel;

            for (int i = 0; i < fleet._owner._fleet.Count; i++)
            {
                if (fleet._owner._fleet[i]._fleet_Equipment == unit)
                {
                    fleet._owner._fleet.Remove(fleet._owner._fleet[i]);
                    break;
                }
            }

            fleet._fleetUnits.Add(fleetUnitData);
        }
        else
        {
            WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.FleetLimit"));
        }
    }

    public void RemoveUnitFromFleet(Fleet fleet, int id)
    {
        for (int i = 0; i < fleet._fleetUnits.Count; i++)
        {
            if (fleet._fleetUnits[i]._id == id)
            {
                fleet._owner._fleet.Add(fleet._fleetUnits[i]._unit._equipment);

                fleet._fleetUnits.Remove(fleet._fleetUnits[i]);
                break;
            }
        }

        UpdateFleetUnitsIDs(fleet);

        UpdateFleetUI();
    }

    private void UpdateFleetUnitsIDs(Fleet fleet)
    {
        for (int i = 0; i < fleet._fleetUnits.Count; i++)
        {
            fleet._fleetUnits[i]._id = Random.Range(1, 9999);
        }
    }

    private Vector2 DetermineMoveDirection(RegionManager province)
    {
        Vector2 position = province.transform.position;

        bool hasNeighborLeft = CheckForRegionManager(position + Vector2.left * checkDistance);
        bool hasNeighborRight = CheckForRegionManager(position + Vector2.right * checkDistance);
        bool hasNeighborUp = CheckForRegionManager(position + Vector2.up * checkDistance);
        bool hasNeighborDown = CheckForRegionManager(position + Vector2.down * checkDistance);

        if (!hasNeighborLeft) return Vector2.left;
        if (!hasNeighborRight) return Vector2.right;
        if (!hasNeighborUp) return Vector2.up;
        if (!hasNeighborDown) return Vector2.down;

        return Vector2.zero; // Если все стороны заняты
    }

    private bool CheckForRegionManager(Vector2 checkPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPosition, 0.1f, layerMask);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.GetComponent<RegionManager>() != null)
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnShipAtEdge(Vector2 direction, RegionManager province)
    {
        Vector2 newPosition = (Vector2)transform.position + direction * (checkDistance - shoreOffset);

        GameObject spawnedFleet = Instantiate(fleetPrefab, newPosition, Quaternion.identity, province.transform);

        spawnedFleet.GetComponent<Fleet>()._onBase = true;
        spawnedFleet.GetComponent<Fleet>()._currentProvince = province;
        spawnedFleet.GetComponent<Fleet>()._owner = province.currentCountry;

        spawnedFleet.GetComponent<Fleet>().SetUp();

        province._currentFleet = spawnedFleet.GetComponent<Fleet>();

        AddUnitToFleet(spawnedFleet.GetComponent<Fleet>(), province.currentCountry._fleet[Random.Range(0, province.currentCountry._fleet.Count)]._fleet_Equipment);
    }



    public void UpdateFleetUI()
    {
        foreach (Transform child in _fleetHorizontalContainer)
        {
            if (child.gameObject.GetComponent<FleetHorizontalUI>())
            {
                Destroy(child.gameObject);
            }
        }

        if (ReferencesManager.Instance.regionManager.currentRegionManager._hasFleet)
        {
            if (ReferencesManager.Instance.regionManager.currentRegionManager._currentFleet._fleetUnits.Count >= 10)
            {
                _addShipButton.SetActive(false);
            }
            else
            {
                _addShipButton.SetActive(true);
            }

            foreach (Fleet.FleetUnitData ship in ReferencesManager.Instance.regionManager.currentRegionManager._currentFleet._fleetUnits)
            {
                GameObject spawnedShipUI = Instantiate(_shipUIPrefab, _fleetHorizontalContainer);
                spawnedShipUI.transform.SetAsLastSibling();
                _addShipButton.transform.SetAsLastSibling();

                FleetHorizontalUI shipUI = spawnedShipUI.GetComponent<FleetHorizontalUI>();

                shipUI._id = ship._id;
                shipUI._fleetUnitData = ship;
                shipUI._fleet = ReferencesManager.Instance.regionManager.currentRegionManager._currentFleet;

                shipUI.SetUp();
            }
        }
    }

    public void DestroyFleet()
    {
        RegionManager province = ReferencesManager.Instance.regionManager.currentRegionManager;

        foreach (Transform child in province.transform)
        {
            province._hasFleet = false;
            province._currentFleet = null;

            if (child.GetComponent<Fleet>())
            {
                Destroy(child.gameObject);
            }
        }
    }
}
