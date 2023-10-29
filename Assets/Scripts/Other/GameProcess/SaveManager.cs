using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public IntListValue localSavesIds;

    public GameObject savesContainer;
    public GameObject savePrefab;

    public void Save()
    {
        PlayerPrefs.SetString("FIRST_LOAD", "FALSE");

        //int saveId = localSavesIds.list[localSavesIds.list.Count] + 1;
        int saveId = 1;

        #region CountrySave

        PlayerPrefs.SetInt($"{saveId}_PLAYER_COUNTRY", ReferencesManager.Instance.countryManager.currentCountry.country._id);

        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            CountrySettings country = ReferencesManager.Instance.countryManager.countries[i];

            PlayerPrefs.SetString($"{saveId}_COUNTRY_{i}_IDEOLOGY", country.ideology);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{i}_MONEY", country.money);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{i}_FOOD", country.food);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{i}_RECROOTS", country.recroots);

            #region MONEY_INCOME

            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{i}_startMoneyIncome", country.startMoneyIncome);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{i}_moneyNaturalIncome", country.moneyNaturalIncome);

            #endregion

            #region FOOD_INCOME

            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{i}_startFoodIncome", country.startFoodIncome);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{i}_foodNaturalIncome", country.foodNaturalIncome);
            #endregion

            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{i}_civFactories", country.civFactories);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{i}_milFactories", country.milFactories);
        }

        #endregion

        #region RegionSave

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            RegionManager region = ReferencesManager.Instance.countryManager.regions[i];

            PlayerPrefs.SetInt($"{saveId}_REGION_{i}_CURRENTCOUNTRY_ID", region.currentCountry.country._id);

            PlayerPrefs.SetInt($"{saveId}_REGION_{i}_INFRASTRUCTURES", region.infrastructure_Amount);
            PlayerPrefs.SetInt($"{saveId}_REGION_{i}_CIV_FABRICS", region.civFactory_Amount);
            PlayerPrefs.SetInt($"{saveId}_REGION_{i}_FARMS", region.farms_Amount);
            PlayerPrefs.SetInt($"{saveId}_REGION_{i}_CHEFARMS", region.cheFarms);
        }

        #region Army

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            RegionManager region = ReferencesManager.Instance.countryManager.regions[i];

            if (region.hasArmy)
            {
                PlayerPrefs.SetString($"{saveId}_REGION_{i}_HAS_ARMY", "TRUE");

                UnitMovement unitMovement = region.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();

                int infLVL1 = 0;
                int infLVL2 = 0;
                int infLVL3 = 0;

                int artLVL1 = 0;
                int artLVL2 = 0;

                int tankLVL1 = 0;
                int tankLVL2 = 0;

                int motoLVL1 = 0;
                int motoLVL2 = 0;

                for (int unitIndex = 0; unitIndex < unitMovement.unitsHealth.Count; unitIndex++)
                {
                    UnitScriptableObject unit = unitMovement.unitsHealth[unitIndex].unit;

                    if (unit.type == UnitScriptableObject.Type.SOLDIER)
                    {
                        if (unit.level == 1)
                        {
                            infLVL1++;
                        }
                        else if (unit.level == 2)
                        {
                            infLVL2++;
                        }
                        else if (unit.level == 3)
                        {
                            infLVL3++;
                        }
                    }
                    else if (unit.type == UnitScriptableObject.Type.ARTILERY)
                    {
                        if (unit.level == 1)
                        {
                            artLVL1++;
                        }
                        else if (unit.level == 2)
                        {
                            artLVL2++;
                        }
                    }
                    else if (unit.type == UnitScriptableObject.Type.TANK)
                    {
                        if (unit.level == 1)
                        {
                            tankLVL1++;
                        }
                        else if (unit.level == 2)
                        {
                            tankLVL2++;
                        }
                    }
                    else if (unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                    {
                        if (unit.level == 1)
                        {
                            motoLVL1++;
                        }
                        else if (unit.level == 2)
                        {
                            motoLVL2++;
                        }
                    }
                }

                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_INF_LVL1", infLVL1);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_INF_LVL2", infLVL2);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_INF_LVL3", infLVL3);

                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_ART_LVL1", artLVL1);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_ART_LVL2", artLVL2);

                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_TANK_LVL1", tankLVL1);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_TANK_LVL2", tankLVL2);

                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_MOTO_LVL1", motoLVL1);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_MOTO_LVL2", motoLVL2);
            }
            else if (!region.hasArmy)
            {
                PlayerPrefs.SetString($"{saveId}_REGION_{region._id}_HAS_ARMY", "FALSE");
            }
        }

        #endregion

        #endregion

        #region TechnologySave

        for (int c = 0; c < ReferencesManager.Instance.countryManager.countries.Count; c++)
        {
            CountrySettings country = ReferencesManager.Instance.countryManager.countries[c];

            for (int i = 0; i < ReferencesManager.Instance.gameSettings.technologies.Length; i++)
            {
                TechnologyScriptableObject tech = ReferencesManager.Instance.gameSettings.technologies[i];

                if (!tech.startReasearched)
                {
                    if (HasTech(country, tech))
                    {
                        PlayerPrefs.SetString($"{saveId}_COUNTRY_{c}_TECH_{i}", "TRUE");
                    }
                    else
                    {
                        PlayerPrefs.SetString($"{saveId}_COUNTRY_{c}_TECH_{i}", "FALSE");
                    }
                }
            }
        }

        #endregion

        #region Events and Date

        PlayerPrefs.SetInt($"{saveId}_DATE_0", ReferencesManager.Instance.dateManager.currentDate[0]);
        PlayerPrefs.SetInt($"{saveId}_DATE_1", ReferencesManager.Instance.dateManager.currentDate[1]);
        PlayerPrefs.SetInt($"{saveId}_DATE_2", ReferencesManager.Instance.dateManager.currentDate[2]);

        for (int i = 0; i < ReferencesManager.Instance.gameSettings.gameEvents.Count; i++)
        {
            if (ReferencesManager.Instance.gameSettings.gameEvents[i]._checked)
            {
                PlayerPrefs.SetString($"{saveId}_EVENT_{i}", "TRUE");
            }
            else
            {
                PlayerPrefs.SetString($"{saveId}_EVENT_{i}", "FALSE");
            }
        }

        #endregion

        localSavesIds.list.Add(saveId);
    }

    private bool HasTech(CountrySettings country, TechnologyScriptableObject tech)
    {
        if (country.countryTechnologies.Contains(tech))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UpdateLocalSavesUI()
    {

    }

    public void Load()
    {
        PlayerPrefs.SetString("FIRST_LOAD", "FALSE");

        SceneManager.LoadScene("EuropeSceneOffline");
    }
}
