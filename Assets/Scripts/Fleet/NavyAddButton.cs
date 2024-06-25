using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NavyAddButton : MonoBehaviour
{
    [SerializeField] private FleetScriptableObject _fleetObject;

    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _amountText;

    private void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        CheckAmount();
    }

    private void CheckAmount()
    {
        _button.interactable = ReferencesManager.Instance.countryManager.currentCountry._fleet.Contains(_fleetObject._equipment);

        string displayText = ReferencesManager.Instance.languageManager.GetTranslation("RegionUI.Fleet.InStock");

        _amountText.text = $"{displayText} {GetAmount(_fleetObject._equipment)}";
    }

    private int GetAmount(MilitaryEquipmentScriptableObject equip)
    {
        int amount = ReferencesManager.Instance.countryManager.currentCountry._fleet.Where(item => item == equip).Count();

        return amount;
    }

    public void AddShipToFleet()
    {
        foreach (Transform child in ReferencesManager.Instance.regionManager.currentRegionManager.transform)
        {
            if (child.GetComponent<Fleet>())
            {
                Fleet fleet = child.GetComponent<Fleet>();

                ReferencesManager.Instance.fleetManager.AddUnitToFleet(fleet, _fleetObject);
                ReferencesManager.Instance.fleetManager.UpdateFleetUI();
            }
        }

        SetUp();
    }
}
