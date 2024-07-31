using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public Image unitIcon;
    public Image unitHealthBar;
    public Image fuelBar;
    public GameObject fuelSlider;
    public UnitMovement division;

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
        if (division != null)
        {
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
            fuelSlider.gameObject.SetActive(false);
            unitHealthBar.gameObject.SetActive(false);
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
            for (int i = 0; i < division.unitsHealth.Count; i++)
            {
                if (division.unitsHealth[i]._id == id && division.unitsHealth[i].unit == currentUnit)
                {
                    division.unitsHealth.Remove(division.unitsHealth[i]);

                    ReferencesManager.Instance.countryManager.currentCountry.recroots += Mathf.CeilToInt(currentUnit.recrootsCost * 0.7f);
                    ReferencesManager.Instance.countryManager.currentCountry.moneyNaturalIncome += currentUnit.moneyIncomeCost;
                    ReferencesManager.Instance.countryManager.currentCountry.foodNaturalIncome += currentUnit.foodIncomeCost;
                }
            }

            ReferencesManager.Instance.countryManager.UpdateValuesUI();
            ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();


            ReferencesManager.Instance.regionUI.UpdateDivisionUnitsIDs(division);

            ReferencesManager.Instance.regionUI.UpdateUnitsUI(true);
        }
    }

    public void SetUnit(UnitScriptableObject unit, UnitMovement movement, UnitHealth unitHealth)
    {
        currentUnit = unit;
        division = movement;
        id = unitHealth._id;
        health = unitHealth.health;
        fuel = unitHealth.fuel;

        // Update the UI elements
        unitIcon.sprite = unit.icon;
        UpdateUI();
    }
}
