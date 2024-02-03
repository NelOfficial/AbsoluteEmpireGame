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
    public int recroots;
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
    public int recrootsIncome;

    public int researchPointsIncome;

    public int mobilizationLaw;

    public int score = 0;
    public int maxScore = 0;

    public int regionCosts;

    public int civFactories;
    public int farms;
    public int chemicalFarms;
    public int milFactories;
    public int researchLabs;

    public bool inWar;

    public CountrySettings enemy;
    public List<TechnologyScriptableObject> countryTechnologies = new List<TechnologyScriptableObject>();
    public List<UnitMovement> countryUnits = new List<UnitMovement>();

    public int armyPersonnel;
    public int manpower;

    public bool mobilasing;
    public bool deMobilasing;

    public int BONUS_FACTORIES_INCOME_MONEY;
    public int BONUS_FARM_INCOME_MONEY;
    public int BONUS_FARM_INCOME_FOOD;
    public int BONUS_CHEFARM_INCOME_MONEY;
    public int BONUS_CHEFARM_INCOME_FOOD;

    [Header("AI Settings")]
    public float aiAccuracy = 1;

    public void UpdateCapitulation()
    {
        int totalScore = 0;
        int maxPopulation = 0;
        int totalCiv_factories = 0;
        int totalFarms = 0;
        int totalCheFarms = 0;
        int totalMil_factories = 0;

        int buildingIncrement = 0;
        int populationCost = 0;

        if (score > maxScore)
        {
            maxScore = score;
        }

        if (money > 0 && moneyIncomeUI > 0)
        {
            inflation = money / moneyIncomeUI;
            inflation = Mathf.Abs(inflation);
        }
        else
        {
            inflation = 0;
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

                if (region.civFactory_Amount > 0)
                {
                    buildingIncrement += region.civFactory_Amount * 7;
                }
                if (region.farms_Amount > 0)
                {
                    buildingIncrement += region.farms_Amount * 3;
                }
                if (region.cheFarms > 0)
                {
                    buildingIncrement += region.cheFarms * 4;
                }
                if (region.infrastructure_Amount > 0)
                {
                    buildingIncrement += region.infrastructure_Amount * 2;
                }
                
            }
            populationCost = population / 20000;
            regionCosts = totalScore + buildingIncrement + populationCost;

            population = maxPopulation;
            score = totalScore;
            civFactories = totalCiv_factories;
            farms = totalFarms;
            chemicalFarms = totalCheFarms;
            milFactories = totalMil_factories;

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

            if (capitulated == true && exist)
            {
                StartCoroutine(Capitulation_Co());
            }
        }

        if (myRegions.Count == 0)
        {
            exist = false;
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
        }

        moneyIncomeUI = moneyTradeIncome + moneyNaturalIncome - Mathf.FloorToInt(inflationDebuff) - regionCosts;
    }

    private void Start()
    {
        if (ReferencesManager.Instance.gameSettings.loadGame.value == false && ReferencesManager.Instance.gameSettings.playMod.value == false && ReferencesManager.Instance.gameSettings.playTestingMod.value == false)
        {
            if (country.countryType == CountryScriptableObject.CountryType.Poor)
            {
                money = 1750;
                food = 50;
                recroots = 120;
            }
            else if (country.countryType == CountryScriptableObject.CountryType.SemiPoor)
            {
                money = 3500;
                food = 100;
                recroots = 240;
            }
            else if (country.countryType == CountryScriptableObject.CountryType.Middle)
            {
                money = 8750;
                food = 250;
                recroots = 600;
            }
            else if (country.countryType == CountryScriptableObject.CountryType.SemiRich)
            {
                money = 21000;
                food = 600;
                recroots = 3940;
            }
            else if (country.countryType == CountryScriptableObject.CountryType.Rich)
            {
                money = 24500;
                food = 700;
                recroots = 3680;
            }
            else if (country.countryType == CountryScriptableObject.CountryType.Ussr)
            {
                money = 50000;
                food = 1700;
                recroots = 5400;
            }
        }

        if (isPlayer)
        {
            ReferencesManager.Instance.countryManager.UpdateCountryInfo();
            ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
            ReferencesManager.Instance.countryManager.UpdateValuesUI();
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

        if (aiAccuracy > 1) aiAccuracy = 1;

        if (isPlayer)
        {
            money += money / 100 * difficulty_PLAYER_BUFF;
            food += money / 100 * difficulty_PLAYER_BUFF;
            recroots += money / 100 * difficulty_PLAYER_BUFF;
        }
        else
        {
            money += money / 100 * difficulty_AI_BUFF;
            food += money / 100 * difficulty_AI_BUFF;
            recroots += money / 100 * difficulty_AI_BUFF;
        }
    }

    private IEnumerator Capitulation_Co()
    {
        if (enemy != null)
        {
            foreach (UnitMovement unit in countryUnits)
            {
                if (unit.currentCountry == this)
                {
                    Destroy(unit);
                }
            }
            countryUnits.Clear();

            for (int v = 0; v < myRegions.Count; v++)
            {
                myRegions[v].CheckRegionUnits(myRegions[v]);
                AnnexRegion(myRegions[v], enemy);
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

            annexedRegion.GetComponent<SpriteRenderer>().color = newCountry.country.countryColor;

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
            Multiplayer.Instance.M_UpdateCountryGraphics(country._id, ideology);
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
                region.selectedColor.r = region.currentCountry.country.countryColor.r + 0.1f;
                region.selectedColor.g = region.currentCountry.country.countryColor.g + 0.1f;
                region.selectedColor.b = region.currentCountry.country.countryColor.b + 0.1f;
            }

            ReferencesManager.Instance.countryManager.UpdateRegionsColor();
        }
    }
}
