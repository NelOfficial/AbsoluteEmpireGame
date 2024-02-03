using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public IntListValue localSavesIds;

    [SerializeField] Transform savesContainer;
    [SerializeField] GameObject savePrefab;

    public StringValue currentSave;


    private void Awake()
    {
        UpdateSavesList();
    }

    public void UpdateSavesList()
    {
        localSavesIds.list.Clear();

        string saves = PlayerPrefs.GetString("SAVES_IDS");
        string[] _saves = saves.Split(';');

        for (int i = 0; i < _saves.Length; i++)
        {
            int value = int.Parse(_saves[i]);

            localSavesIds.list.Add(value);
        }
    }

    public void SetSavesIDs()
    {
        string _saves = "";

        for (int i = 0; i < localSavesIds.list.Count; i++)
        {
            _saves += $"{localSavesIds.list[i]};";
        }

        PlayerPrefs.SetString("SAVES_IDS", _saves);
    }

    public void Save()
    {
        PlayerPrefs.SetString("FIRST_LOAD", "FALSE");

        int saveId = 0;

        if (localSavesIds.list.Count > 0)
        {
            saveId = localSavesIds.list.Max() + 1;
        }
        else
        {
            saveId = 1;
        }

        string date_time = $"{System.DateTime.Now}";

        PlayerPrefs.SetString($"{saveId}_DATETIME", date_time);
        PlayerPrefs.SetString($"{saveId}_DIFFICULTY", ReferencesManager.Instance.gameSettings.difficultyValue.value);

        #region CountrySave

        string _countries = "";

        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            _countries += $"{ReferencesManager.Instance.countryManager.countries[i].country._id};";
        }

        PlayerPrefs.SetInt($"{saveId}_PLAYER_COUNTRY", ReferencesManager.Instance.countryManager.currentCountry.country._id);
        PlayerPrefs.SetString($"{saveId}_COUNTRIES", _countries);

        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            CountrySettings country = ReferencesManager.Instance.countryManager.countries[i];

            PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_IDEOLOGY", country.ideology);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_MONEY", country.money);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_FOOD", country.food);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_RECROOTS", country.recroots);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_RESEARCH_POINTS", country.researchPoints);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_RECROOTS_LIMIT", country.recruitsLimit);

            #region MONEY_INCOME

            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_startMoneyIncome", country.startMoneyIncome);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_moneyNaturalIncome", country.moneyNaturalIncome);

            #endregion

            #region FOOD_INCOME

            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_startFoodIncome", country.startFoodIncome);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_foodNaturalIncome", country.foodNaturalIncome);

            #endregion

            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_recrootsIncome", country.recrootsIncome);

            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_researchPointsIncome", country.researchPointsIncome);

            if (country.mobilasing)
            {
                PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_MOBILASING", "TRUE");
            }

            if (country.deMobilasing)
            {
                PlayerPrefs.SetString($"{saveId} _COUNTRY_ {country.country._id}_DEMOBILASING", "TRUE");
            }

            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_civFactories", country.civFactories);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_farms", country.farms);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_cheFarms", country.chemicalFarms);
            PlayerPrefs.SetInt($"{saveId}_COUNTRY_{country.country._id}_resLabs", country.researchLabs);
        }

        #endregion

        #region RegionSave

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            RegionManager region = ReferencesManager.Instance.countryManager.regions[i];

            PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_CURRENTCOUNTRY_ID", region.currentCountry.country._id);

            PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_INFRASTRUCTURES", region.infrastructure_Amount);
            PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_CIV_FABRICS", region.civFactory_Amount);
            PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_FARMS", region.farms_Amount);
            PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_CHEFARMS", region.cheFarms);
            PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_RESLABS", region.researchLabs);
            PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_POPULATION", region.population);
        }

        #region Army

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            RegionManager region = ReferencesManager.Instance.countryManager.regions[i];

            region.CheckRegionUnits(region);

            if (region.hasArmy)
            {
                PlayerPrefs.SetString($"{saveId}_REGION_{region._id}_HAS_ARMY", "TRUE");

                UnitMovement unitMovement = region.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();

                int infLVL1 = 0;
                int infLVL2 = 0;
                int infLVL3 = 0;

                int artLVL1 = 0;
                int artLVL2 = 0;

                int atiLVL1 = 0;
                int atiLVL2 = 0;

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

                    else if (unit.unitName == "ATI_01")
                    {
                        artLVL1++;
                    }
                    else if (unit.unitName == "ATI_02")
                    {
                        artLVL2++;
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

                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_ATI_LVL1", atiLVL1);
                PlayerPrefs.SetInt($"{saveId}_REGION_{region._id}_ATI_LVL2", atiLVL2);
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
                        PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_TECH_{i}", "TRUE");
                    }
                    else
                    {
                        PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_TECH_{i}", "FALSE");
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

        #region Relations

        foreach (CountrySettings country in ReferencesManager.Instance.countryManager.countries)
        {
            string country_trades = "";
            string country_pacts = "";
            string country_unions = "";
            string country_rights = "";
            string country_wars = "";

            foreach (CountrySettings otherCountry in ReferencesManager.Instance.countryManager.countries)
            {
                if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(country, otherCountry).trade)
                {
                    country_trades += $"{otherCountry.country._id};";
                }
                else if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(country, otherCountry).pact)
                {
                    country_pacts += $"{otherCountry.country._id};";
                }
                else if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(country, otherCountry).union)
                {
                    country_unions += $"{otherCountry.country._id};";
                }
                else if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(country, otherCountry).right)
                {
                    country_rights += $"{otherCountry.country._id};";
                }
                else if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(country, otherCountry).war)
                {
                    country_wars += $"{otherCountry.country._id};";
                }
            }

            PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_TRADES", country_trades);
            PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_RIGHTS", country_rights);
            PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_PACTS", country_pacts);
            PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_UNIONS", country_unions);
            PlayerPrefs.SetString($"{saveId}_COUNTRY_{country.country._id}_WARS", country_wars);
        }

        #endregion

        localSavesIds.list.Add(saveId);

        string _saves = "";

        for (int i = 0; i < localSavesIds.list.Count; i++)
        {
            _saves += $"{localSavesIds.list[i]};";
        }

        PlayerPrefs.SetString("SAVES_IDS", _saves);
    }

    public void UpdateUI()
    {
        foreach (Transform child in savesContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < localSavesIds.list.Count; i++)
        {
            try
            {
                int saveID = localSavesIds.list[i];

                GameObject spawnedSavePrefab = Instantiate(savePrefab, savesContainer);

                spawnedSavePrefab.GetComponent<SaveItem>().saveID = saveID;

                CountryScriptableObject _country = null;

                int countryId = PlayerPrefs.GetInt($"{saveID}_PLAYER_COUNTRY");

                foreach (CountryScriptableObject country in ReferencesManager.Instance.globalCountries)
                {
                    if (country._id == countryId)
                    {
                        _country = country;
                    }
                }

                spawnedSavePrefab.GetComponent<SaveItem>().country = _country;
                spawnedSavePrefab.GetComponent<SaveItem>().date = PlayerPrefs.GetString($"{saveID}_DATETIME");

                spawnedSavePrefab.GetComponent<SaveItem>().UpdateUI();

            }
            catch (System.Exception) { }
        }
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
