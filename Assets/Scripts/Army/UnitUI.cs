using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public Image unitIcon;
    public Image unitHealthBar;
    public Image fuelBar;
    public GameObject fuelSlider;
    public GameObject unitWorldObject;

    public int id;
    public float health;
    public float fuel;

    public UnitScriptableObject currentUnit;

    private void Awake()
    {
        ReferencesManager.Instance.army.CheckUnitTech();
    }

    public void UpdateUI()
    {
        if (unitWorldObject != null)
        {
            UnitMovement division = unitWorldObject.GetComponent<UnitMovement>();

            for (int i = 0; i < division.unitsHealth.Count; i++)
            {
                if (division.unitsHealth[i]._id == id)
                {
                    health = division.unitsHealth[i].health;
                    fuel = division.unitsHealth[i].fuel;

                    unitHealthBar.fillAmount = health / currentUnit.health;

                    if (currentUnit.maxFuel > 0)
                    {
                        fuelBar.fillAmount = fuel / currentUnit.maxFuel;
                    }
                    else
                    {
                        fuelSlider.gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            health = 0;
        }
    }

    public void SelfDestroy()
    {
        Destroy(gameObject);
    }

    public void RemoveUnitFromArmy()
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.RemoveUnitFromArmy(currentUnit.unitName, ReferencesManager.Instance.regionManager.currentRegionManager._id);
        }
        else
        {
            Transform unitTransform = ReferencesManager.Instance.regionManager.currentRegionManager.transform.Find("Unit(Clone)");
            UnitMovement division = unitTransform.GetComponent<UnitMovement>();

            UnitMovement.UnitHealth batalion = new UnitMovement.UnitHealth();

            for (int i = 0; i < division.unitsHealth.Count; i++)
            {
                if (division.unitsHealth[i]._id == id)
                {
                    batalion = division.unitsHealth[i];
                }
            }

            if (division.unitsHealth.Count - 1 > 0)
            {
                for (int i = 0; i < division.unitsHealth.Count; i++)
                {
                    if (division.unitsHealth[i]._id == batalion._id)
                    {
                        division.unitsHealth.Remove(batalion);
                    }
                }

                ReferencesManager.Instance.regionUI.UpdateUnitsUI(true);
            }

            if (division.unitsHealth.Count - 1 <= 0)
            {
                ReferencesManager.Instance.regionManager.currentRegionManager.DeselectRegions();
                ReferencesManager.Instance.regionUI.CloseAllUI();
                division.currentCountry.countryUnits.Remove(division);
                Destroy(division.gameObject);
            }

            ReferencesManager.Instance.countryManager.currentCountry.recroots += Mathf.CeilToInt(batalion.unit.recrootsCost * 0.7f);
            ReferencesManager.Instance.countryManager.currentCountry.moneyNaturalIncome += batalion.unit.moneyIncomeCost;
            ReferencesManager.Instance.countryManager.currentCountry.foodNaturalIncome += batalion.unit.foodIncomeCost;

            ReferencesManager.Instance.countryManager.UpdateValuesUI();
            ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
        }
    }

    public void SetUnit(UnitScriptableObject unit, UnitMovement movement, UnitMovement.UnitHealth unitHealth)
    {
        currentUnit = unit;
        unitWorldObject = movement.gameObject;
        id = unitHealth._id;
        health = unitHealth.health;
        fuel = unitHealth.fuel;

        // Update the UI elements
        unitIcon.sprite = unit.icon;
        UpdateUI();
    }
}
