using UnityEngine;
using UnityEngine.UI;

public class UnitPrefab : MonoBehaviour
{
    [SerializeField] Image icon;

    public Image _healthBar;

    public void SetUpUnit(UnitMovement.UnitHealth unit)
    {
        _healthBar.fillAmount = unit.health / unit.unit.health;
        icon.sprite = unit.unit.icon;
    }

    public void SetUpBuilding(BuildingScriptableObject building)
    {
        icon.sprite = building.icon;
        _healthBar.fillAmount = 1;
    }

    public void SetUpBuildingQueue(RegionManager.BuildingQueueItem building)
    {
        icon.sprite = building.building.icon;
        _healthBar.fillAmount = building.movesLasts / building.building.moves;
    }
}
