using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Realtime;

public class CountrySettings : MonoBehaviour
{
    public CountryScriptableObject country;

    public List<RegionManager> myRegions = new List<RegionManager>();

    public RegionManager capitalRegion;

    public bool capitulated = false;
    public bool exist = true;
    public float inflation = 0;
    public float inflationDebuff = 0;
    public float[] popularities;

    public bool isPlayer;
    public bool onlinePlayer;
    public Player _countryPlayer;
    public string ideology;

    public CountryInfoAdvanced.Nation[] formableNations;

    [Header("Values")]
    public int money;
    public int food;
    public int recruits;
    public int researchPoints;
    public int recruitsLimit;
    public int population;

    public int startMoneyIncome;
    public int moneyTradeIncome;
    public int moneyIncomeUI;
    public int moneyNaturalIncome;
    public int foodIncomeUI;
    public int foodTradeIncome;
    public int foodNaturalIncome;
    public int startFoodIncome;
    public int recruitsIncome;

    public float fuelIncome;

    public int researchPointsIncome;

    public int mobilizationLaw;

    public Stability stability = null;

    [Header("Resources")]
    public int oil;
    public float fuel;

    public int score = 0;
    public int maxScore = 0;

    public int regionCosts;

    public int civFactories;
    public int farms;
    public int chemicalFarms;
    public int milFactories;
    public int researchLabs;
    public int dockyards;

    [Header("Army/Military")]
    public bool inWar;

    public List<CountrySettings> enemies;
    public List<TechnologyScriptableObject> countryTechnologies = new List<TechnologyScriptableObject>();
    public List<UnitMovement> countryUnits = new List<UnitMovement>();

    public int armyPersonnel;
    public int manpower;

    public bool mobilasing;
    public bool deMobilasing;

    [Header("Buffs")]
    public int BONUS_FACTORIES_INCOME_MONEY;
    public int BONUS_FARM_INCOME_MONEY;
    public int BONUS_FARM_INCOME_FOOD;
    public int BONUS_CHEFARM_INCOME_MONEY;
    public int BONUS_CHEFARM_INCOME_FOOD;

    public float BONUS_INCOME_FUEL;

    [Header("AI Settings")]
    public float aiAccuracy = 1;

    public CountrySettings vassalOf;

    [Header("Production Settings")]
    public List<ProductionManager.ProductionQueue> _prodQueue = new List<ProductionManager.ProductionQueue>();

    public List<MilitaryEquipmentScriptableObject> _fleet = new List<MilitaryEquipmentScriptableObject>();

    public List<Guild> guilds = new(10);

    public Relationships relations;


    private void Awake()
    {
        relations = GetComponent<Relationships>();
    }

    public void UpdateCapitulation()
    {
        int totalScore = 0;
        int maxPopulation = 0;
        int totalCiv_factories = 0;
        int totalFarms = 0;
        int totalCheFarms = 0;
        int totalMil_factories = 0;
        int totalLabs = 0;
        int totalDockyards = 0;

        int populationCost = 0;

        if (score > maxScore)
        {
            maxScore = score;
        }

        if (moneyNaturalIncome + moneyTradeIncome > 0)
        {
            inflation = (money / (moneyNaturalIncome + moneyTradeIncome)) / ((stability.value + 25f) / 100);
            inflation = Mathf.Abs(inflation);
        }

        if (exist)
        {
            foreach (RegionManager region in myRegions)
            {
                maxPopulation += region.population;
                totalScore += region.regionScore;
                totalCiv_factories += region.civFactory_Amount;
                totalFarms += region.farms_Amount;
                totalCheFarms += region.cheFarms;
                totalMil_factories += region.milFactory_Amount;
                totalDockyards += region.dockyards;
                totalLabs += region.researchLabs;
                
            }
            populationCost = population / 20000;
            regionCosts = totalScore + populationCost;

            population = maxPopulation;
            score = totalScore;
            civFactories = totalCiv_factories;
            researchLabs = totalLabs;
            farms = totalFarms;
            chemicalFarms = totalCheFarms;
            milFactories = totalMil_factories;
            dockyards = totalDockyards;

            if (maxScore <= 0)
            {
                maxScore = totalScore;
            }
            if (maxScore > 0)
            {
                float capLimit = totalScore * 100 / maxScore;

                if (capLimit < country.capitulateLimit)
                {
                    capitulated = true;
                }
            }

            if (capitulated == true && exist && enemies.Count > 0)
            {
                StartCoroutine(Capitulation_Co());
            }
        }

        if (myRegions.Count == 0)
        {
            exist = false;
            inWar = false;
            foreach (CountrySettings country in enemies)
            {
                country.enemies.Remove(this);

                country.inWar = country.enemies.Count > 0;
            }
        }
        else if (myRegions.Count > 0)
        {
            exist = true;
        }

        if (!exist)
        {
            DiplomatyUI diplomatyUI = ReferencesManager.Instance.gameSettings.diplomatyUI;

            if (diplomatyUI != null)
            {
                for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
                {
                    CountrySettings country = ReferencesManager.Instance.countryManager.countries[i];

                    Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(this, country);
                    Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(country, this);

                    senderToReceiver.war = false;
                    receiverToSender.war = false;

                    senderToReceiver.trade = false;
                    receiverToSender.trade = false;

                    senderToReceiver.pact = false;
                    receiverToSender.pact = false;

                    senderToReceiver.right = false;
                    receiverToSender.right = false;

                    senderToReceiver.union = false;
                    receiverToSender.union = false;

                    senderToReceiver.vassal = false;
                    receiverToSender.vassal = false;

                    senderToReceiver.relationship = 0;
                    receiverToSender.relationship = 0;
                }

            }

            inWar = false;
            enemies.Clear();
        }

        moneyIncomeUI = moneyTradeIncome + moneyNaturalIncome - Mathf.FloorToInt(inflationDebuff) - regionCosts;
    }

    private void Start()
    {
        if (ReferencesManager.Instance.gameSettings.loadGame.value == false && ReferencesManager.Instance.gameSettings.playMod.value == false && ReferencesManager.Instance.gameSettings.playTestingMod.value == false)
        {
            if (money <= 0 || food <= 0 || recruits <= 0)
            {

                if (country.countryType == CountryScriptableObject.CountryType.Poor)
                {
                    money = 3750;
                    food = 500;
                    recruits = 12000;
                }
                else if (country.countryType == CountryScriptableObject.CountryType.SemiPoor)
                {
                    money = 5500;
                    food = 1000;
                    recruits = 25000;
                }
                else if (country.countryType == CountryScriptableObject.CountryType.Middle)
                {
                    money = 10750;
                    food = 2050;
                    recruits = 34000;
                }
                else if (country.countryType == CountryScriptableObject.CountryType.SemiRich)
                {
                    money = 28000;
                    food = 6000;
                    recruits = 50040;
                }
                else if (country.countryType == CountryScriptableObject.CountryType.Rich)
                {
                    money = 34500;
                    food = 9000;
                    recruits = 78080;
                }
                else if (country.countryType == CountryScriptableObject.CountryType.Ussr)
                {
                    money = 50000;
                    food = 17000;
                    recruits = 156000;
                }
            }
        }

        startFoodIncome = foodNaturalIncome;
        startMoneyIncome = moneyIncomeUI;

        int difficulty_AI_BUFF = 0;
        int difficulty_PLAYER_BUFF = 0;
        float _aiAccuracy_increment = 1;

        if (ReferencesManager.Instance.gameSettings.difficultyValue.value == "EASY")
        {
            difficulty_AI_BUFF = -15;
            difficulty_PLAYER_BUFF = 15;
            _aiAccuracy_increment = 0.5f;
            if (country.countryType == CountryScriptableObject.CountryType.Rich ||
                country.countryType == CountryScriptableObject.CountryType.Ussr)
            {
                _aiAccuracy_increment *= 2f;
            }
        }
        else if (ReferencesManager.Instance.gameSettings.difficultyValue.value == "HARD")
        {
            difficulty_AI_BUFF = 20;
            difficulty_PLAYER_BUFF = -20;

            _aiAccuracy_increment = 1.5f;
        }
        else if (ReferencesManager.Instance.gameSettings.difficultyValue.value == "INSANE")
        {
            difficulty_AI_BUFF = 40;
            difficulty_PLAYER_BUFF = -40;

            _aiAccuracy_increment = 2f;
        }
        else if (ReferencesManager.Instance.gameSettings.difficultyValue.value == "HARDCORE")
        {
            difficulty_AI_BUFF = 75;
            difficulty_PLAYER_BUFF = -75;

            _aiAccuracy_increment = 3f;
        }

        aiAccuracy *= _aiAccuracy_increment;

        stability = new Stability(this);

        if (aiAccuracy > 1) aiAccuracy = 1;

        if (isPlayer)
        {
            money += money / 100 * difficulty_PLAYER_BUFF;
            food += money / 100 * difficulty_PLAYER_BUFF;
            recruits += money / 100 * difficulty_PLAYER_BUFF;
        }
        else
        {
            money += money / 100 * difficulty_AI_BUFF;
            food += money / 100 * difficulty_AI_BUFF;
            recruits += money / 100 * difficulty_AI_BUFF;
        }

        UpdateCountryGraphics(this.ideology);

        if (isPlayer)
        {
            ReferencesManager.Instance.countryManager.UpdateCountryInfo();
            ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
            ReferencesManager.Instance.countryManager.UpdateValuesUI();
        }
    }

    private IEnumerator Capitulation_Co()
    {
        if (enemies.Count > 0)
        {
            CountrySettings playerEnemy = null;

            foreach (CountrySettings enemy in enemies)
            {
                if (enemy.isPlayer)
                {
                    playerEnemy = enemy;
                }
            }

            foreach (UnitMovement unit in countryUnits)
            {
                if (unit.currentCountry == this && unit != null)
                {
                    Destroy(unit.gameObject);
                }
            }

            countryUnits.Clear();

            if (!isPlayer)
            {
                ReferencesManager.Instance.aiManager.AICountries.Remove(this);
            }

            for (int v = 0; v < myRegions.Count; v++)
            {
                if (playerEnemy != null)
                {
                    AnnexCountry(myRegions.ToArray(), playerEnemy);
                }
                else
                {
                    CountrySettings winnerCountry = enemies[Random.Range(0, enemies.Count)];

                    while (!winnerCountry.exist && winnerCountry.myRegions.Count <= 0)
                    {
                        winnerCountry = enemies[Random.Range(0, enemies.Count)];
                    }

                    AnnexCountry(myRegions.ToArray(), winnerCountry);
                }
            }

            if (isPlayer)
            {
                yield return new WaitForSeconds(1.5f);
                ReferencesManager.Instance.aiManager.losePanel.SetActive(true);
            }
        }

        exist = false;

        yield break;
    }

    private void AnnexCountry(RegionManager[] provinces, CountrySettings winner)
    {
        for (int i = 0; i < provinces.Length; i++)
        {
            AnnexRegion(provinces[i], winner);
        }
    }

    private void AnnexRegion(RegionManager annexedRegion, CountrySettings newCountry)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
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

            Color provinceColor = new Color(
                newCountry.country.countryColor.r,
                newCountry.country.countryColor.g,
                newCountry.country.countryColor.b,
                ReferencesManager.Instance.gameSettings._regionOpacity);

            annexedRegion.GetComponent<SpriteRenderer>().color = provinceColor;

            annexedRegion.selectedColor.r = newCountry.country.countryColor.r + 0.1f;
            annexedRegion.selectedColor.g = newCountry.country.countryColor.g + 0.1f;
            annexedRegion.selectedColor.b = newCountry.country.countryColor.b + 0.1f;

            annexedRegion.SelectRegionNoHit(annexedRegion);
        }
        ReferencesManager.Instance.ChangeMobilizationLaw(newCountry.mobilizationLaw, newCountry);
    }

    public void UpdateCountryGraphics(string ideology)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            if (country != null)
            {
                Multiplayer.Instance.M_UpdateCountryGraphics(country._id, ideology);
            }
        }
        else
        {
            if (ideology == "Неопределённый")
            {
                country.countryColor = country.countryIdeologyColors[1];
                country.countryFlag = country.countryIdeologyFlags[1];
            }
            else if (ideology == "Демократия")
            {
                country.countryColor = country.countryIdeologyColors[1];
                country.countryFlag = country.countryIdeologyFlags[1];
            }
            else if (ideology == "Монархия")
            {
                country.countryColor = country.countryIdeologyColors[2];
                country.countryFlag = country.countryIdeologyFlags[2];
            }
            else if (ideology == "Фашизм")
            {
                country.countryColor = country.countryIdeologyColors[4];
                country.countryFlag = country.countryIdeologyFlags[4];
            }
            else if (ideology == "Коммунизм")
            {
                country.countryColor = country.countryIdeologyColors[5];
                country.countryFlag = country.countryIdeologyFlags[5];
            }

            // Новый цвет
            foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
            {
                region.selectedColor.r = region.currentCountry.country.countryColor.r + 0.2f;
                region.selectedColor.g = region.currentCountry.country.countryColor.g + 0.2f;
                region.selectedColor.b = region.currentCountry.country.countryColor.b + 0.2f;
            }

            ReferencesManager.Instance.countryManager.UpdateRegionsColor();
        }
    }

    public void UpdateCountryFlagOnIdeology(string ideology)
    {
        if (ideology == "Неопределённый")
        {
            country.countryColor = country.countryIdeologyColors[1];
            country.countryFlag = country.countryIdeologyFlags[1];
        }
        else if (ideology == "Демократия")
        {
            country.countryColor = country.countryIdeologyColors[1];
            country.countryFlag = country.countryIdeologyFlags[1];
        }
        else if (ideology == "Монархия")
        {
            country.countryColor = country.countryIdeologyColors[2];
            country.countryFlag = country.countryIdeologyFlags[2];
        }
        else if (ideology == "Фашизм")
        {
            country.countryColor = country.countryIdeologyColors[4];
            country.countryFlag = country.countryIdeologyFlags[4];
        }
        else if (ideology == "Коммунизм")
        {
            country.countryColor = country.countryIdeologyColors[5];
            country.countryFlag = country.countryIdeologyFlags[5];
        }
    }
}
