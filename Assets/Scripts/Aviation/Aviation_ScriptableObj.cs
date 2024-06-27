using UnityEngine;

[CreateAssetMenu(fileName = "NewAirPlane", menuName = "Airplane/default")]
public class Aviation_ScriptableObj : ScriptableObject
{
    public enum Type { fighter, stormtrooper, bomber, scout }

    [Header("Основная информация")]
    public string name;
    public string description;
    public int price;
    public Sprite sprite;  
    public Type type;
    public int recruitsCost;
    public TechnologyScriptableObject _tech;

    [Header("Характеристики")]
    public int maxhp;
    public int airplane_damage;
    public int army_damage;
    public int builds_damage;
    public int armorBreak;
    public int armor;
    public int distance;
    public int fuelMax;
    public int fuelperattack;
    public int hitprice;
}
