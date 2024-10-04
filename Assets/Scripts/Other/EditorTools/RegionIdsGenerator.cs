//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices.WindowsRuntime;

//public class RegionIdsGenerator : MonoBehaviour
//{
//    [MenuItem("Tools/Set relations")]
//    private static void SetRelations()
//    {
//        List<CountrySettings> countries = FindObjectsOfType<CountrySettings>().ToList();

//        foreach (CountrySettings country in countries)
//        {
//            country.GetComponent<Relationships>().relationship.Clear();

//            foreach (CountrySettings _country in countries)
//            {
//                Relationships.Relation relation = new()
//                {
//                    relationship = 0,
//                    trade = false,
//                    union = false,
//                    pact = false,
//                    right = false,
//                    war = false,
//                    vassal = false,
//                    country = _country
//                };

//                country.GetComponent<Relationships>().relationship.Add(relation);
//            }
//        }
//    }

//    [MenuItem("Tools/Set regions ids")]
//    private static void SetIds()
//    {
//        List<RegionManager> regions = FindObjectsOfType<RegionManager>().ToList();

//        foreach (RegionManager region in regions)
//        {
//            if (region._id == 0 && region.name != "Region1")
//            {
//                int maxId = regions.Max(region => region._id);

//                region._id = maxId + 1;

//                Debug.Log($"Completed: {region.name}({region.currentCountry.country._nameEN}) - {region._id}");
//            }
//        }
//    }

//    [MenuItem("Tools/Set all regions points")]
//    private static void SetAllPoints()
//    {
//        List<RegionManager> regions = FindObjectsOfType<RegionManager>().ToList();

//        foreach (RegionManager region in regions)
//        {
//            if (region.transform.childCount > 0)
//            {
//                region.movePoints.Clear();

//                foreach (Transform _movePoint in region.transform)
//                {
//                    if (_movePoint.gameObject.TryGetComponent(out MovePoint _point))
//                    {
//                        if (_point.regionTo == null)
//                        {
//                            _point.SetRegionInPoint();
//                        }

//                        if (_point.regionTo != null)
//                        {
//                            region.movePoints.Add(_point.transform);

//                            Debug.Log($"Completed: {region.name}({region._id}/{region.currentCountry.country._nameEN})");
//                        }
//                    }
//                }
//            }
//        }
//    }

//    [MenuItem("Tools/Set regions points")]
//    private static void SetPoints()
//    {
//        List<RegionManager> regions = FindObjectsOfType<RegionManager>().ToList();

//        foreach (RegionManager region in regions)
//        {
//            if (region.movePoints.Count == 0 && region.transform.childCount > 0)
//            {
//                foreach (Transform _movePoint in region.transform)
//                {
//                    if (_movePoint.gameObject.TryGetComponent(out MovePoint _point))
//                    {
//                        if (_point.regionTo == null)
//                        {
//                            _point.SetRegionInPoint();
//                        }

//                        if (_point.regionTo != null)
//                        {
//                            region.movePoints.Add(_point.transform);

//                            Debug.Log($"Completed: {region.name}({region._id}/{region.currentCountry.country._nameEN})");
//                        }
//                    }
//                }
//            }
//        }
//    }

//    [MenuItem("Tools/Set All GoundPoints parent")]
//    private static void SetAllGroundPoints_parent()
//    {
//        FromSeaToGround_MovePoint[] movepoints = FindObjectsOfType<FromSeaToGround_MovePoint>();
//        SeaMovePoint[] seamovepoints = FindObjectsOfType<SeaMovePoint>();

//        for (int i = 0; i < movepoints.Length; i++)
//        {
//            movepoints[i]._destinationRegion._seaPoints.Clear();
//        }

//        for (int i = 0; i < movepoints.Length; i++)
//        {
//            SeaMovePoint point = GetPointFromSea(movepoints[i].transform.parent.GetComponent<SeaRegion>());

//            movepoints[i]._destinationRegion._seaPoints.Add(point);
//        }
//    }

//    private static SeaMovePoint GetPointFromSea(SeaRegion region)
//    {
//        SeaMovePoint point = null;

//        SeaMovePoint[] seamovepoints = FindObjectsOfType<SeaMovePoint>();

//        for (int i = 0; i < seamovepoints.Length; i++)
//        {
//            if (seamovepoints[i].regionTransit == region)
//            {
//                point = seamovepoints[i];
//            }
//        }

//        return point;
//    }

//    [MenuItem("Tools/Set All GoundPoints destionation")]
//    private static void SetAllGroundPoints_destination()
//    {
//        SeaRegion[] seas = FindObjectsOfType<SeaRegion>();

//        for (int i = 0; i < seas.Length; i++)
//        {
//            seas[i]._toGroundMovePoints.Clear();
//        }

//        FromSeaToGround_MovePoint[] movepoints = FindObjectsOfType<FromSeaToGround_MovePoint>();

//        for (int i = 0; i < movepoints.Length; i++)
//        {
//            movepoints[i].SetRegionInPoint();
//        }
//    }

//    [MenuItem("Tools/SetAllSeasMovepoints")]
//    private static void SetAllSeasMovepoints()
//    {
//        SeaMovePoint[] points = FindObjectsOfType<SeaMovePoint>();

//        for (int i = 0; i < points.Length; i++)
//        {
//            points[i].GetComponent<PolygonCollider2D>().enabled = true;
//        }

//        SeaRegion[] seas = FindObjectsOfType<SeaRegion>();
//        float expandAmount = 0.1f;

//        for (int i = 0; i < seas.Length; i++)
//        {
//            seas[i]._movePoints.Clear();
//        }

//        for (int i = 0; i < seas.Length; i++)
//        {
//            PolygonCollider2D polygonCollider = seas[i].GetComponent<PolygonCollider2D>();

//            if (polygonCollider == null)
//            {
//                Debug.LogError("Коллайдер не найден на объекте!");
//            }

//            Vector2[] originalPoints = polygonCollider.points;
//            Vector2[] expandedPoints = new Vector2[originalPoints.Length];

//            // Увеличиваем каждый пункт полигона по направлению от центра
//            Vector2 center = polygonCollider.bounds.center;
//            for (int a = 0; a < originalPoints.Length; a++)
//            {
//                Vector2 worldPoint = seas[i].transform.TransformPoint(originalPoints[a]); // Преобразование в мировые координаты
//                Vector2 direction = (worldPoint - center).normalized;
//                expandedPoints[a] = worldPoint + direction * expandAmount; // Увеличение на заданное расстояние
//            }

//            // Проверяем пересечение с другими коллайдерами
//            foreach (var point in expandedPoints)
//            {
//                Collider2D[] hitColliders = Physics2D.OverlapPointAll(point);

//                foreach (var hitCollider in hitColliders)
//                {
//                    if (hitCollider.gameObject != seas[i].gameObject)
//                    {
//                        SeaMovePoint seaMovePoint = hitCollider.GetComponent<SeaMovePoint>();
//                        if (seaMovePoint != null && seaMovePoint.regionTransit != seas[i])
//                        {
//                            seas[i]._movePoints.Add(hitCollider.gameObject.GetComponent<SeaMovePoint>());
//                        }
//                    }
//                }
//            }
//        }

//        for (int i = 0; i < seas.Length; i++)
//        {
//            List<SeaMovePoint> list = seas[i]._movePoints.Distinct().ToList();
//            seas[i]._movePoints = list;
//        }
//    }


//    [MenuItem("Tools/Set All MovePoints Destinations")]
//    private static void SetAllMovePoints_Destination()
//    {
//        try
//        {
//            MovePoint[] movepoints = FindObjectsOfType<MovePoint>();

//            for (int i = 0; i < movepoints.Length; i++)
//            {
//                movepoints[i].SetRegionInPoint();
//                Debug.Log($"{movepoints[i].name} is completed");
//            }
//        }
//        catch (System.Exception ex)
//        {
//            Debug.LogError(ex);
//        }
//    }

//    [MenuItem("Tools/Set All Sea MovePoints Destinations")]
//    private static void SetAllSeaMovePoints_Destination()
//    {
//        SeaMovePoint[] movepoints = FindObjectsOfType<SeaMovePoint>();

//        for (int i = 0; i < movepoints.Length; i++)
//        {
//            movepoints[i].SetDestination();
//            Debug.Log($"{movepoints[i].name} is completed");
//        }
//    }

//    [MenuItem("Tools/Set all regions list")]
//    private static void SetAllRegionsList()
//    {
//        try
//        {
//            CountryManager countryManager = FindObjectOfType<CountryManager>();

//            countryManager.regions.Clear();

//            RegionManager[] regions = FindObjectsOfType<RegionManager>();

//            countryManager.regions = regions.ToList();

//            Debug.Log($"Completed");
//        }
//        catch (System.Exception ex)
//        {
//            Debug.LogError($"Error: {ex.Message}");
//        }
//    }

//    [MenuItem("Tools/Get Max Country Id")]
//    private static void GetMaxCountryId()
//    {
//        List<CountryScriptableObject> countries = Resources.LoadAll<CountryScriptableObject>("ScriptableObjects/").ToList();

//        Debug.Log($"{countries.Max(item => item._id)}");
//    }
//}
