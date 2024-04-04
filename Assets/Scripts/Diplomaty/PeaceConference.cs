using System.Collections.Generic;
using UnityEngine;

public class PeaceConference : MonoBehaviour
{
    public CountrySettings[] loosers;
    public WarParticipant[] winners;

    public bool conferenceOngoing;

    public void Simulate()
    {
        conferenceOngoing = true;

        for (int i = 0; i < winners.Length; i++)
        {
            for (int l = 0; l < loosers.Length; l++)
            {
                winners[i].winnerPoints = loosers[l].score / 100 * winners[i].percent;
            }
        }
    }

    private void Update()
    {
        if (conferenceOngoing)
        {
            for (int l = 0; l < loosers.Length; l++)
            {
                for (int i = 0; i < winners.Length; i++)
                {

                    Debug.Log($"{winners[i].winnerPoints} | {winners[i].country.country._name}");

                    List<RegionManager> borders = new List<RegionManager>();

                    List<RegionManager> newProvinces = new List<RegionManager>();

                    GetAllMyBorderingProvinces(winners[i].country, loosers[l], borders);

                    HighlightProvinces(borders, UnityEngine.Color.blue);

                    foreach (RegionManager region in borders)
                    {
                        RegionManager enemyProvince = GetEnemyBorderingProvince(region, loosers[l], newProvinces);

                        if (enemyProvince != null)
                        {
                            if (winners[i].winnerPoints >= enemyProvince.regionScore)
                            {
                                newProvinces.Add(enemyProvince);
                                winners[i].winnerPoints -= enemyProvince.regionScore;

                                ReferencesManager.Instance.AnnexRegion(enemyProvince, winners[i].country);

                                HighlightProvinces(newProvinces, UnityEngine.Color.green);
                            }
                        }
                    }

                    HighlightProvinces(newProvinces, UnityEngine.Color.green);

                    if (winners[i].winnerPoints <= 0 && loosers[l])
                    {
                        conferenceOngoing = false;
                    }
                }
            }
        }
    }

    private void GetAllMyBorderingProvinces(CountrySettings country, CountrySettings enemyCountry, List<RegionManager> list)
    {
        foreach (RegionManager region in country.myRegions)
        {
            foreach (Transform point in region.movePoints)
            {
                if (point.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>().currentCountry == enemyCountry)
                {
                    list.Add(region);
                }
            }
        }
    }

    private RegionManager GetEnemyBorderingProvince(RegionManager myProvince, CountrySettings enemyCountry, List<RegionManager> blackList)
    {
        RegionManager GetBorderingProvince = null;

        for (int i = 0; i < myProvince.movePoints.Count; i++)
        {
            if (myProvince.movePoints[i].GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>().currentCountry == enemyCountry)
            {
                GetBorderingProvince = myProvince.movePoints[i].GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>();
            }
        }

        return GetBorderingProvince;
    }

    public void HighlightProvinces(List<RegionManager> list, UnityEngine.Color _color)
    {
        foreach (RegionManager province in list)
        {
            province.GetComponent<SpriteRenderer>().color = _color;
        }
    }

    [System.Serializable]
    public class WarParticipant
    {
        public CountrySettings country;
        public int percent;
        public int winnerPoints;
    }
}
