using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapType : MonoBehaviour
{
    private CountryManager countryManager;

    private int minPopulation = 0;
    private int maxPopulation = 0;
    private List<int> populationList = new List<int>();
    private float currentAlpha = 0;

    public bool viewMap;


    private void Awake()
    {
        countryManager = FindObjectOfType<CountryManager>(); 
    }

    public void DefaultMap()
    {
        for (int i = 0; i < countryManager.regions.Length; i++)
        {
            RegionManager region = countryManager.regions[i];

            region.GetComponent<SpriteRenderer>().color = region.currentCountry.country.countryColor;
        }

        viewMap = false;
    }

    public void PopulationMap()
    {

        viewMap = true;

        ///max - 255
        ///curr - x

        populationList.Clear();
        for (int i = 0; i < countryManager.regions.Length; i++)
        {
            RegionManager region = countryManager.regions[i];

            populationList.Add(region.population);
            minPopulation = populationList.Min();
            maxPopulation = populationList.Max();

            if (region.population == minPopulation)
            {
                SetAlphaColor(region, 0.15f);
            }
            else if (region.population == maxPopulation)
            {
                SetAlphaColor(region, 1f);
            }
            else
            {
                currentAlpha = (float)region.population / (float)maxPopulation;
                SetAlphaColor(region, currentAlpha);
            }
        }
    }

    public void TerrainMap()
    {
        viewMap = true;

        for (int i = 0; i < countryManager.regions.Length; i++)
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
}
