using System.Collections.Generic;
using UnityEngine;

public static class PolygonSimplification
{
    public static List<Vector2> Simplify(List<Vector2> points, float tolerance)
    {
        if (points.Count < 3)
            return points;

        int startIndex = 0;
        int endIndex = points.Count - 1;
        List<int> pointIndexsToKeep = new List<int>();

        pointIndexsToKeep.Add(startIndex);
        pointIndexsToKeep.Add(endIndex);

        while (points[startIndex] == points[endIndex])
        {
            endIndex--;
        }

        SimplifySection(points, startIndex, endIndex, tolerance, ref pointIndexsToKeep);

        List<Vector2> simplifiedPoints = new List<Vector2>();
        pointIndexsToKeep.Sort();
        foreach (int index in pointIndexsToKeep)
        {
            simplifiedPoints.Add(points[index]);
        }

        return simplifiedPoints;
    }

    private static void SimplifySection(List<Vector2> points, int startIndex, int endIndex, float tolerance, ref List<int> pointIndexsToKeep)
    {
        float maxDistance = 0;
        int indexFarthest = 0;

        for (int index = startIndex + 1; index < endIndex; index++)
        {
            float distance = PerpendicularDistance(points[startIndex], points[endIndex], points[index]);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexFarthest = index;
            }
        }

        if (maxDistance > tolerance && indexFarthest != 0)
        {
            pointIndexsToKeep.Add(indexFarthest);

            SimplifySection(points, startIndex, indexFarthest, tolerance, ref pointIndexsToKeep);
            SimplifySection(points, indexFarthest, endIndex, tolerance, ref pointIndexsToKeep);
        }
    }

    private static float PerpendicularDistance(Vector2 point1, Vector2 point2, Vector2 point)
    {
        float area = Mathf.Abs(.5f * (point1.x * point2.y + point2.x * point.y + point.x * point1.y - point2.x * point1.y - point.x * point2.y - point1.x * point.y));
        float bottom = Mathf.Sqrt(Mathf.Pow(point1.x - point2.x, 2) + Mathf.Pow(point1.y - point2.y, 2));
        float height = area / bottom * 2;

        return height;
    }
}
