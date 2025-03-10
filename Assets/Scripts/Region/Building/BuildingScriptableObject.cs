using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Buildings/Building")]
public class BuildingScriptableObject : ScriptableObject
{
    public string uiTitle;
    public string uiTitleEN;
    public string _name;

    public enum BuildType{ GoldProducer, FoodProducer, RecrootsProducer, Other, Dockyard}

    public BuildType buildType;

    public Sprite icon;

    public int goldCost;

    public int goldIncome;
    public int foodIncome;
    public int recrootsIncome;

    public int researchPointsIncome;

    public float moves;
}
