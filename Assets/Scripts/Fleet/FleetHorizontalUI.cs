using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FleetHorizontalUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Image _healthSliderInner;
    [SerializeField] private Image _fuelSliderInner;

    [HideInInspector] public Fleet.FleetUnitData _fleetUnitData;
    [HideInInspector] public Fleet _fleet;

    public int _id;

    public void SetUp()
    {
        _icon.sprite = _fleetUnitData._unit.icon;

        _healthSliderInner.fillAmount = _fleetUnitData._health / _fleetUnitData._unit.health;
        _fuelSliderInner.fillAmount = _fleetUnitData._fuel / _fleetUnitData._unit.maxFuel;
    }

    public void RemoveShipFromFleet()
    {
        ReferencesManager.Instance.fleetManager.RemoveUnitFromFleet(_fleet, _id);
    }
}
