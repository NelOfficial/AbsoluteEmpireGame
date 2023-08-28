using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "Units/Unit")]
public class UnitScriptableObject : ScriptableObject
{
    public enum Type
    {
        SOLDIER,
        SOLDIER_MOTORIZED,
        ARTILERY,
        TANK
    }

    public Type type;

    public Sprite icon;

    public int moneyCost;
    public int moneyIncomeCost;
    public int foodIncomeCost;
    public int recrootsCost;

    public int level;
    public int unlockLevel;
    public int rank;

    public float damage;
    public float defense;
    public float health;

    public string unitName;
}
