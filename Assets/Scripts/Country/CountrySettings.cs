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

    public int mobilizationLaw;

    public int score = 0;
    public int maxScore = 0;

    public int regionCosts;

    public int civFactories;
    public int farms;
    public int chemicalFarms;
    public int milFactories;

    public bool inWar;

    public CountrySettings enemy;
    public List<TechnologyScriptableObject> countryTechnologies = new List<TechnologyScriptableObject>();
    public List<UnitMovement> countryUnits = new List<UnitMovement>();

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

        moneyIncomeUI = moneyTradeIncome + moneyNaturalIncome - Mathf.FloorToInt(inflationDebuff) - regionCosts;
    }

    private void Start()
    {
        if (PlayerPrefs.GetString("LOAD_GAME_THROUGH_MENU") == "FALSE" || !PlayerPrefs.HasKey("LOAD_GAME_THROUGH_MENU"))
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

        startFoodIncome = foodNaturalIncome;
        startMoneyIncome = moneyIncomeUI;
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
}