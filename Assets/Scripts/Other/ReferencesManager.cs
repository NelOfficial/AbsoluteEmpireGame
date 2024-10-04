using System.Linq;
using System.Collections.Generic;
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
    public GameEventUI eventUI;
    public MapType mapType;
    public MainMenu mainMenu;
    public ProfileManager profileManager;
    public RegionLoader regionLoader;
    public OfflineGameSettings offlineGameSettings;

    public ResourcesMarketManager resourcesMarketManager;

    public Dict<Sprite> sprites;

    public FleetManager fleetManager;
    public Launcher launcher;

    public Transform countriesParent;
    public CountrySettings countrySettingsPrefab;
    public Settings settings;

    public StringValue localisation_en;
    public StringValue localisation_ru;

    public CountryScriptableObject[] globalCountries;

    public ChatManager chatManager;

    public SaveManager saveManager;

    public Interpretate languageManager;

    public Aviation_UI aviationUI;
    public AviationManager aviationManager;

    public string debugString;

    public Guild.FlagSprite[] guildImages;

    private void Awake()
    {
        Instance = this;

        foreach (CountryScriptableObject country in globalCountries)
        {
            debugString += $"{country._nameEN}\n";
        }
    }

    public CountryScriptableObject FindCountryObjectByID(int id, CountryScriptableObject[] _countries)
    {
        CountryScriptableObject _foundedCountry = null;

        foreach (CountryScriptableObject country in _countries)
        {
            if (country._id == id)
            {
                _foundedCountry = country;
                break;
            }
        }

        return _foundedCountry;
    }

    public CountrySettings CreateCountry(CountryScriptableObject countryScriptableObject, string ideology)
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

        if (createdCountry.GetComponent<CountrySettings>().country._id != countryManager.currentCountry.country._id)
        {
            createdCountry.AddComponent<CountryAIManager>();
            aiManager.AICountries.Add(createdCountry.GetComponent<CountrySettings>());
        }

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

    public CountrySettings CreateCountry_Component(CountryScriptableObject countryScriptableObject, string ideology)
    {
        GameObject createdCountry = Instantiate(countrySettingsPrefab.gameObject, countriesParent);

        createdCountry.GetComponent<CountrySettings>().country = countryScriptableObject;

        createdCountry.name = countryScriptableObject._nameEN;
        createdCountry.GetComponent<CountrySettings>().enemies = new List<CountrySettings>();
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

        if (createdCountry.GetComponent<CountrySettings>().country._id != countryManager.currentCountry.country._id)
        {
            createdCountry.AddComponent<CountryAIManager>();
            aiManager.AICountries.Add(createdCountry.GetComponent<CountrySettings>());
        }

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

            annexedRegion.gameObject.name = $"AnxReg_{UnityEngine.Random.Range(0, 9999)}";
            annexedRegion.transform.SetParent(newCountry.transform);

            annexedRegion.currentCountry = newCountry;
            annexedRegion.transform.SetAsLastSibling();

            annexedRegion.GetComponent<SpriteRenderer>().color = newCountry.country.countryColor;

            annexedRegion.selectedColor.r = newCountry.country.countryColor.r + 0.1f;
            annexedRegion.selectedColor.g = newCountry.country.countryColor.g + 0.1f;
            annexedRegion.selectedColor.b = newCountry.country.countryColor.b + 0.1f;

            newCountry.population += annexedRegion.population;
        }

        ChangeMobilizationLaw(newCountry.mobilizationLaw, newCountry);
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

            if (country.recruits < 0)
            {
                country.recruits = 0;
            }

            country.manpower = country.recruits + country.armyPersonnel;

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

            country.recruitsIncome = recruitsNeeded / 25;

            country.mobilizationLaw = id;

        }
    }

    public string GoodNumberString(int integer)
    {
        string result = "";

        if (integer < 1000000)
        {
            float resultInteger = (float)integer / 1000f;

            result = $"{resultInteger.ToString("0.0")} {languageManager.GetTranslation("NumberGoodiser.thounsand")}";
        }

        //Millions

        if (integer > 1000000)
        {
            float resultInteger = integer / 1000000f;

            result = $"{resultInteger.ToString("0.0")} {languageManager.GetTranslation("NumberGoodiser.million")}";
        }

        return result;
    }

    public void CalculateTradeBuff(CountrySettings sender, CountrySettings receiver)
    {
        TradeBuff tradeBuff = new TradeBuff();
        tradeBuff.id = UnityEngine.Random.Range(0, 99999);
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

    public void AnnexCountry()
    {
        CountrySettings country = regionManager.currentRegionManager.currentCountry;

        if (country != countryManager.currentCountry)
        {
            while (country.myRegions.Count > 0)
            {
                for (int i = 0; i < country.myRegions.Count; i++)
                {
                    AnnexRegion(country.myRegions[i], countryManager.currentCountry);
                }
            }

            if (!country.isPlayer)
            {
                aiManager.AICountries.Remove(country);
            }
        }
    }
}
