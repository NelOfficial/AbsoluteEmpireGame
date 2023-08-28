using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public Image unitIcon;
    public GameObject unitWorldObject;

    public int id;

    public UnitScriptableObject currentUnit;

    private void Awake()
    {
        ReferencesManager.Instance.army.CheckUnitTech();
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

            UnitMovement unitMovement = unitTransform.GetComponent<UnitMovement>();

            if (unitMovement.unitsHealth.Count > 0)
            {
                UnitScriptableObject unit = currentUnit;

                ReferencesManager.Instance.countryManager.currentCountry.recroots += unit.recrootsCost;
                ReferencesManager.Instance.countryManager.currentCountry.moneyNaturalIncome += unit.moneyIncomeCost;
                ReferencesManager.Instance.countryManager.currentCountry.foodNaturalIncome += unit.foodIncomeCost;

                for (int i = 0; i < unitMovement.unitsHealth.Count; i++)
                {
                    if (unitMovement.unitsHealth[i]._id == id)
                    {
                        unitMovement.unitsHealth.Remove(unitMovement.unitsHealth[i]);
                    }
                }

                ReferencesManager.Instance.countryManager.UpdateValuesUI();
                ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();

                if (unitMovement.unitsHealth.Count <= 0)
                {
                    ReferencesManager.Instance.regionManager.currentRegionManager.DeselectRegions();
                    unitTransform.GetComponent<UnitMovement>().currentCountry.countryUnits.Remove(unitTransform.GetComponent<UnitMovement>());
                    Destroy(unitTransform.gameObject);
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }

            ReferencesManager.Instance.regionUI.UpdateUnitsUI();
        }
    }
}
