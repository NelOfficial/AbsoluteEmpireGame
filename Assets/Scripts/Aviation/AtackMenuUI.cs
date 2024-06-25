using UnityEngine;
using UnityEngine.UI;

public class AttackMenuUI : MonoBehaviour
{
	[SerializeField] private GameObject Cell;
	[SerializeField] private GameObject Cell2;
	[SerializeField] private Transform View;
	[SerializeField] private Transform View2;
	[SerializeField] private Button button;

	[SerializeField] private Aviation_UI aviation_UI;

    public void ToggleUI()
	{
		if (ReferencesManager.Instance.gameSettings.provincesList.Count > 0)
		{
            for (int i = View.childCount - 1; i >= 0; i--)
            {
                Destroy(View.GetChild(i).gameObject);
            }

            for (int i = View2.childCount - 1; i >= 0; i--)
            {
                Destroy(View2.GetChild(i).gameObject);
            }

            if (Aviation_UI.attackMode == 0)
            {
                foreach (RegionManager reg in ReferencesManager.Instance.gameSettings.provincesList)
                {
                    if (reg.hasArmy)
                    {
                        foreach (UnitMovement.UnitHealth unit in reg.transform.Find("Unit(Clone)").GetComponent<UnitMovement>().unitsHealth)
                        {
                            GameObject obj = Instantiate(Cell, View);
                            obj.GetComponent<UnitPrefab>().SetUpUnit(unit);
                        }
                    }
                }
            }
            else if (Aviation_UI.attackMode == 1)
            {
                foreach (RegionManager reg in ReferencesManager.Instance.gameSettings.provincesList)
                {
                    foreach (BuildingScriptableObject build in reg.buildings)
                    {
                        GameObject obj = Instantiate(Cell, View);
                        obj.GetComponent<UnitPrefab>().SetUpBuilding(build);
                    }
                }
            }

            GameObject obj2 = Instantiate(Cell2, View2);
			obj2.GetComponent<Aviation_Cell_obj>()._interactable = false;
            obj2.GetComponent<Aviation_Cell_obj>().SetUp(Aviation_UI.SelectedPlane.GetComponent<Aviation_Cell_obj>().MyObject);
            obj2.transform.position = Vector3.zero;

            gameObject.SetActive(true);

            if (Aviation_UI.SelectedPlane.GetComponent<Aviation_Cell_obj>().MyObject.fuel > Aviation_UI.SelectedPlane.GetComponent<Aviation_Cell_obj>().MyObject.AirPlane.fuelperattack)
            {
                button.interactable = true;
            }
		}
		else
		{
			WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Aviation.NoProvinces"));

			aviation_UI.cancelattack();
        }
    }

	public void attack()
	{
		if (Aviation_UI.attackMode == 0)
		{
			Aviation_Cell plane = Aviation_UI.SelectedPlane.GetComponent<Aviation_Cell_obj>().MyObject;
			float damage = (float)plane.AirPlane.army_damage;

			foreach (RegionManager reg in ReferencesManager.Instance.gameSettings.provincesList)
			{
				if (reg.hasArmy)
				{
					while (damage != 0f)
					{
						UnitMovement.UnitHealth unit = reg.GetDivision(reg).unitsHealth[0];

						float unithp = unit.health * (1 + (ArmorBreak(unit.unit.armor, plane.AirPlane.armorBreak) / 100)) / (plane.hp / plane.AirPlane.maxhp);

						if (damage >= unithp)
						{
							damage -= unithp;

							reg.GetDivision(reg).unitsHealth.Remove(unit);

							plane.hp -= unit.unit.aviationAttack / (1 + (plane.AirPlane.armor / 100));
						}
						else
						{
							unit.health -= damage * (1 + (ArmorBreak(unit.unit.armor, plane.AirPlane.armorBreak) / 100)) / (plane.hp / plane.AirPlane.maxhp);

							damage = 0;

							plane.hp -= unit.unit.aviationAttack / (1 + (plane.AirPlane.armor / 100));
						}
						if (reg.GetDivision(reg).unitsHealth.Count <= 0)
						{
							reg.hasArmy = false;
							Destroy(reg.GetDivision(reg).gameObject);
							break;
						}
					}
				}
			}

			plane.fuel -= plane.AirPlane.fuelperattack;

			ToggleUI();

			if (plane.fuel < plane.AirPlane.fuelperattack)
			{
				button.interactable = false;
			}

            if (plane.hp <= 0)
            {
				Aviation_Storage airport = ReferencesManager.Instance.regionManager.currentRegionManager.GetComponent<Aviation_Storage>();

				for (int i = 0; i < airport.planes.Count; i++)
				{
                    if (airport.planes[i].AirPlane == plane.AirPlane && airport.planes[i].hp <= 0)
                    {
						airport.planes.Remove(airport.planes[i]);
                    }
                }
            }

            float ArmorBreak(float armor, float armorbreak)
			{
				if (armorbreak >= armor)
				{
					return 0f;
				}
				return armor - armorbreak;
			}
		}

		else if (Aviation_UI.attackMode == 1)
		{
			Aviation_Cell plane = Aviation_UI.SelectedPlane.GetComponent<Aviation_Cell_obj>().MyObject;
			float damage = (float)plane.AirPlane.builds_damage;

			foreach (RegionManager reg in ReferencesManager.Instance.gameSettings.provincesList)
			{	
				while (reg.buildings.Count != 0)
				{
					RegionManager.BuildingQueueItem item = new RegionManager.BuildingQueueItem();

					item.building = reg.buildings[0];
					item.region = reg;
					item.movesLasts = reg.buildings[0].moves * (damage / 100);
					reg.buildingsQueue.Add(item);
					reg.buildings.Remove(reg.buildings[0]);
				}				
			}

			plane.fuel -= plane.AirPlane.fuelperattack;

			for (int i = 0; i < View.childCount; i++)
			{
				View.GetChild(i).GetComponent<UnitPrefab>()._healthBar.fillAmount = 1 - (damage / 100);
            }

			if (plane.fuel < plane.AirPlane.fuelperattack)
			{
				button.interactable = false;
			}
		}
	}
}
