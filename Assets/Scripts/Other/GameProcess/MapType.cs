using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapType : MonoBehaviour
{
    private CountryManager countryManager;

    private float minPopulation = 0;
    private float maxPopulation = 0;
    private List<float> populationList = new List<float>();
    private float currentAlpha = 0;

    public bool viewMap;

    [SerializeField] Color grayColor;


    private void Awake()
    {
        countryManager = FindObjectOfType<CountryManager>(); 
    }

    public void DefaultMap()
    {
        for (int i = 0; i < countryManager.regions.Count; i++)
        {
            RegionManager region = countryManager.regions[i];

            Color provinceColor = new Color(
                region.currentCountry.country.countryColor.r,
                region.currentCountry.country.countryColor.g,
                region.currentCountry.country.countryColor.b,
                ReferencesManager.Instance.gameSettings._regionOpacity);

            region.GetComponent<SpriteRenderer>().color = provinceColor;
        }

        viewMap = false;
    }

    public void PopulationMap()
    {

        viewMap = true;

        ///max - 255
        ///curr - x

        populationList.Clear();

        for (int i = 0; i < countryManager.regions.Count; i++)
        {
            RegionManager region = countryManager.regions[i];
            float _population = float.Parse(region.population.ToString());

            populationList.Add(_population);

            minPopulation = populationList.Min();
            maxPopulation = populationList.Max();
        }

        for (int i = 0;i < countryManager.regions.Count; i++)
        {
            RegionManager region = countryManager.regions[i];
            float _population = float.Parse(region.population.ToString());

            currentAlpha = _population / maxPopulation;

            if (currentAlpha < 0.1f)
            {
                currentAlpha = 0.1f;
            }

            SetAlphaColor(region, currentAlpha);
        }
    }

    public void TerrainMap()
    {
        viewMap = true;

        for (int i = 0; i < countryManager.regions.Count; i++)
        {
            RegionManager region = countryManager.regions[i];

            if (region.regionTerrain != null)
            {
                region.GetComponent<SpriteRenderer>().color = region.regionTerrain._color;
            }
            else
            {
                Debug.Log($"{region.name} {region.currentCountry.country._name}");
            }
        }
    }

    private void SetAlphaColor(RegionManager region, float alpha)
    {
        region.GetComponent<SpriteRenderer>().color = new Color(
            region.GetComponent<SpriteRenderer>().color.r,
            region.GetComponent<SpriteRenderer>().color.g,
            region.GetComponent<SpriteRenderer>().color.b,
            alpha);

    }

    public void MyCountryMap()
    {
        viewMap = true;

        for (int i = 0; i < countryManager.regions.Count; i++)
        {
            RegionManager region = countryManager.regions[i];

            if (region.currentCountry.country._id != countryManager.currentCountry.country._id)
            {
                region.GetComponent<SpriteRenderer>().color = grayColor;
            }
        }
    }
}
