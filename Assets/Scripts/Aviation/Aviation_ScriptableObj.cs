using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "NewAirPlane", menuName = "Airplane/default")]
public class Aviation_ScriptableObj : ScriptableObject
{
    public enum Type { fighter, stormtrooper, bomber, scout }

    [Header("�������� ����������")]
    public string name;
    public string description;
    public int price;
    public Sprite sprite;  
    public Type type;

    [Header("��������������")]
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