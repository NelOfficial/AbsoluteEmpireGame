using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "Units/Unit")]
public class UnitScriptableObject : ScriptableObject
{
    public enum Type
    {
        SOLDIER,
        SOLDIER_MOTORIZED,
        CAVALRY,
        ARTILERY,
        TANK
    }

    public Type type;

    public Sprite icon;

    [Header("Cost")]
    public short moneyCost;
    public short foodCost;
    public short moneyIncomeCost;
    public short foodIncomeCost;
    public short recrootsCost;

    [Header("Info")]
    public short level;
    public short unlockLevel;
    public float maxFuel;

    [Header("Battle parameters")]
    public float health;

    public float softAttack;
    public float hardAttack;

    public float defense;
    public float maxEntrenchment;

    public float armor;
    public float armorPiercing;

    public float hardness;

    public string unitName;
}
