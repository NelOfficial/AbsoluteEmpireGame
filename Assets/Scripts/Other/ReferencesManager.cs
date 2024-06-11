using System.Linq;
using UnityEngine;

public class ReferencesManager : MonoBehaviour
{
    public static ReferencesManager Instance;

    public Army army;
    public RegionUI regionUI;
    public CountryManager countryManager;
    public RegionManager regionManager;
    public SeaRegion seaRegionManager;
    public GameSettings gameSettings;
    public DateManager dateManager;
    public AIManager aiManager;
    public TechnologyManager technologyManager;
    public ProductionManager productionManager;
    public CountryInfoAdvanced countryInfo;
    public CameraMovement mainCamera;
    public MapEditor mEditor;
    public DiplomatyUI diplomatyUI;
    public EventsContainer eventsContainer;
    public MapType mapType;
    public MainMenu mainMenu;
    public ProfileManager profileManager;
    public RegionLoader regionLoader;
    public OfflineGameSettings offlineGameSettings;

    public ResourcesMarketManager resourcesMarketManager;

    public FleetManager fleetManager;

    public Transform countriesParent;
    public CountrySettings countrySettingsPrefab;
    public Settings settings;

    public StringValue localisation_en;
    public StringValue localisation_ru;

    public CountryScriptableObject[] globalCountries;

    public ChatManager chatManager;

    public SaveManager saveManager;

    private void Awake()
    {
        Instance = this;
    }

    public void CreateCountry(CountryScriptableObject countryScriptableObject, string ideology)
    {
        GameObject createdCountry = Instantiate(countrySettingsPrefab.gameObject, countriesParent);

        createdCountry.GetComponent<CountrySettings>().country = countryScriptableObject;

        createdCountry.name = countryScriptableObject._nameEN;

        foreach (CountrySettings country in countryManager.countries)
        {
            Relationships.Relation newRelation = new Relationships.Relation();
            newRelation.country = country;
            newRelation.war = false;
            newRelation.trade = false;
            newRelation.right = false;
            newRelation.pact = false;
            newRelation.union = false;
            newRelation.vassal = false;
            newRelation.relationship = 0;
            newRelation.relationshipID = 0;

            createdCountry.GetComponent<Relationships>().relationship.Add(newRelation);
        }

        foreach (CountrySettings country in countryManager.countries)
        {
            Relationships.Relation newRelation = new Relationships.Relation();
            newRelation.country = createdCountry.GetComponent<CountrySettings>();
            country.GetComponent<Relationships>().relationship.Add(newRelation);
        }

        Relationships.Relation myselfRelation = new Relationships.Relation();
        myselfRelation.country = createdCountry.GetComponent<CountrySettings>();
        createdCountry.GetComponent<Relationships>().relationship.Add(myselfRelation);

        countryManager.countries.Add(createdCountry.GetComponent<CountrySettings>());
        createdCountry.AddComponent<CountryAIManager>();
        aiManager.AICountries.Add(createdCountry.GetComponent<CountrySettings>());

        createdCountry.GetComponent<CountrySettings>().ideology = ideology;
        createdCountry.GetComponent<CountrySettings>().UpdateCountryGraphics(ideology);

        for (int i = 0; i < gameSettings.technologies.Length; i++)
        {
            if (gameSettings.technologies[i].moves == 0)
            {
                createdCountry.GetComponent<CountrySettings>().countryTechnologies.Add(gameSettings.technologies[i]);
            }
        }

        countryManager.UpdateRegionsColor();
    }

    public CountrySettings CreateCountry_Component(CountryScriptableObject countryScriptableObject, string ideology)
    {
        GameObject createdCountry = Instantiate(countrySettingsPrefab.gameObject, countriesParent);

        createdCountry.GetComponent<CountrySettings>().country = countryScriptableObject;

        createdCountry.name = countryScriptableObject._nameEN;
        createdCountry.GetComponent<CountrySettings>().enemy = null;
        createdCountry.GetComponent<CountrySettings>().inWar = false;

        foreach (CountrySettings country in countryManager.countries)
        {
            Relationships.Relation newRelation = new Relationships.Relation();
            newRelation.country = country;
            newRelation.war = false;
            newRelation.trade = false;
            newRelation.right = false;
            newRelation.pact = false;
            newRelation.union = false;
            newRelation.vassal = false;
            newRelation.relationship = 0;
            newRelation.relationshipID = 0;

            createdCountry.GetComponent<Relationships>().relationship.Add(newRelation);
        }

        foreach (CountrySettings country in countryManager.countries)
        {
            Relationships.Relation newRelation = new Relationships.Relation();
            newRelation.country = createdCountry.GetComponent<CountrySettings>();
            country.GetComponent<Relationships>().relationship.Add(newRelation);
        }

        Relationships.Relation myselfRelation = new Relationships.Relation();
        myselfRelation.country = createdCountry.GetComponent<CountrySettings>();
        createdCountry.GetComponent<Relationships>().relationship.Add(myselfRelation);

        countryManager.countries.Add(createdCountry.GetComponent<CountrySettings>());
        createdCountry.AddComponent<CountryAIManager>();
        aiManager.AICountries.Add(createdCountry.GetComponent<CountrySettings>());

        createdCountry.GetComponent<CountrySettings>().ideology = ideology;
        createdCountry.GetComponent<CountrySettings>().UpdateCountryGraphics(ideology);

        for (int i = 0; i < gameSettings.technologies.Length; i++)
        {
            if (gameSettings.technologies[i].moves == 0)
            {
                createdCountry.GetComponent<CountrySettings>().countryTechnologies.Add(gameSettings.technologies[i]);
            }
        }

        countryManager.UpdateRegionsColor();

        return createdCountry.GetComponent<CountrySettings>();
    }

    public void AnnexRegion(RegionManager annexedRegion, CountrySettings newCountry)
    {
        if (Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.AnnexRegion(annexedRegion._id, newCountry.country._id);
        }
        else
        {
            newCountry.myRegions.Add(annexedRegion);
            annexedRegion.currentCountry.myRegions.Remove(annexedRegion);

            annexedRegion.currentCountry = newCountry;
            annexedRegion.defaultColor = newCountry.country.countryColor;

            annexedRegion.gameObject.name = $"AnxReg_{Random.Range(0, 9999)}";
            annexedRegion.transform.SetParent(newCountry.transform);

            annexedRegion.currentCountry = newCountry;
            annexedRegion.transform.SetAsLastSibling();

            annexedRegion.GetComponent<SpriteRenderer>().color = newCountry.country.countryColor;

            annexedRegion.selectedColor.r = newCountry.country.countryColor.r + 0.1f;
            annexedRegion.selectedColor.g = newCountry.country.countryColor.g + 0.1f;
            annexedRegion.selectedColor.b = newCountry.country.countryColor.b + 0.1f;

            annexedRegion.SelectRegionNoHit(annexedRegion);

            newCountry.population += annexedRegion.population;
        }

        ReferencesManager.Instance.ChangeMobilizationLaw(newCountry.mobilizationLaw, newCountry);
    }

    public void ChangeMobilizationLaw(int id, CountrySettings country)
    {
        SetRecroots(id, country);

        countryManager.UpdateValuesUI();
    }

    public void SetRecroots(int id, CountrySettings country)
    {
        if (country.population > 0)
        {
            int newManpower = country.population / 100 * Instance.gameSettings.mobilizationPercent[id];

            if (country.recroots < 0)
            {
                country.recroots = 0;
            }

            country.manpower = country.recroots + country.armyPersonnel;

            if (newManpower > country.manpower)
            {
                country.recruitsLimit = newManpower - country.armyPersonnel;
                country.mobilasing = true;
                country.deMobilasing = false;
            }
            else
            {
                country.recruitsLimit = newManpower - country.armyPersonnel;
                country.mobilasing = false;
                country.deMobilasing = true;
            }

            int recruitsNeeded = country.recruitsLimit - country.manpower;

            country.recrootsIncome = recruitsNeeded / 25;

            country.mobilizationLaw = id;

        }
    }

    public string GoodNumberString(int integer)
    {
        string result = "";

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            if (integer < 1000000)
            {
                float resultInteger = integer / 1000;
                result = $"{resultInteger}K";
            }
        }

        if (PlayerPrefs.GetInt("languageId") == 1)
        {
            if (integer < 1000000)
            {
                float resultInteger = integer / 1000;
                result = $"{resultInteger} тыс.";
            }
        }

        //Millions

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            if (integer > 1000000)
            {
                float resultInteger = integer / 1000000;
                result = $"{resultInteger}M";
            }
        }

        if (PlayerPrefs.GetInt("languageId") == 1)
        {
            if (integer > 1000000)
            {
                float resultInteger = integer / 1000000;
                result = $"{resultInteger} млн.";
            }
        }

        return result;
    }

    public void CalculateTradeBuff(CountrySettings sender, CountrySettings receiver)
    {
        TradeBuff tradeBuff = new TradeBuff();
        tradeBuff.id = Random.Range(0, 99999);
        tradeBuff.sender = sender;
        tradeBuff.receiver = receiver;

        if (sender.moneyNaturalIncome > 0 && sender.foodNaturalIncome > 0)
        {
            tradeBuff.senderMoneyTrade = Mathf.Abs(sender.moneyNaturalIncome / 100 * 2);
            tradeBuff.senderFoodTrade = Mathf.Abs(sender.foodNaturalIncome / 100 * 2);
        }

        if (receiver.moneyNaturalIncome > 0 && receiver.foodNaturalIncome > 0)
        {
            tradeBuff.receiverMoneyTrade = Mathf.Abs(receiver.moneyNaturalIncome / 100 * 2);
            tradeBuff.receiverFoodTradee = Mathf.Abs(receiver.foodNaturalIncome / 100 * 2);
        }

        sender.moneyTradeIncome += tradeBuff.receiverMoneyTrade;
        sender.foodTradeIncome += tradeBuff.receiverFoodTradee;

        receiver.moneyTradeIncome += tradeBuff.senderMoneyTrade;
        receiver.foodTradeIncome += tradeBuff.senderFoodTrade;

        diplomatyUI.globalTrades.Add(tradeBuff);
    }

    public void PaintRegion(RegionManager region, CountrySettings newCountry)
    {

        //currentRegionManager = hit.collider.gameObject.GetComponent<RegionManager>();
        region.currentCountry.myRegions.Remove(region);

        region.currentCountry = newCountry;
        newCountry.myRegions.Add(region);
        region.transform.SetParent(newCountry.transform);

        region.UpdateRegions();
    }

    public bool isCountryHasClaims(CountrySettings a, CountrySettings b)
    {
        bool result = b.myRegions.Any(item => item.regionClaims.Contains(a.country));

        return result;
    }

    public bool isCountryHasBordersWith(CountrySettings a, CountrySettings b)
    {
        bool result = false;

        int provincesBorderingWith = 0;

        foreach (RegionManager province in a.myRegions)
        {
            foreach (Transform movePoint in province.movePoints)
            {
                if (movePoint.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>().currentCountry == b)
                {
                    provincesBorderingWith++;
                }
            }
        }

        if (provincesBorderingWith > 0)
        {
            result = true;
        }
        else
        {
            result = false;
        }

        return result;
    }

    public bool isCountryHasBordersWithProvince(CountrySettings a, RegionManager b)
    {
        bool result = false;

        int provincesBorderingWith = 0;

        foreach (RegionManager province in a.myRegions)
        {
            foreach (Transform movePoint in province.movePoints)
            {
                if (movePoint.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>()._id == b._id)
                {
                    provincesBorderingWith++;
                }
            }
        }

        if (provincesBorderingWith > 0)
        {
            result = true;
        }
        else
        {
            result = false;
        }

        return result;
    }

    public CountrySettings GetCountry(CountryScriptableObject countryScriptableObject)
    {
        CountrySettings _country = new CountrySettings();

        foreach (CountrySettings country in countryManager.countries)
        {
            if (country.country == countryScriptableObject)
            {
                _country = country;
            }
        }

        return _country;
    }
}
