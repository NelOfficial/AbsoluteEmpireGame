using UnityEngine;
using UnityEngine.UI;

public class UnitPrefab : MonoBehaviour
{
    [SerializeField] Image icon;

    public Image _healthBar;

    public void SetUpUnit(UnitMovement.UnitHealth unit)
    {
        icon.sprite = unit.unit.icon;
        _healthBar.fillAmount = unit.health / unit.unit.health;
    }

    public void SetUpBuilding(BuildingScriptableObject building)
    {
        icon.sprite = building.icon;
        _healthBar.fillAmount = 1;
    }
}
