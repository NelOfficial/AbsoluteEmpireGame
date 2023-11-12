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

    [Header("Cost")]
    public int moneyCost;
    public int moneyIncomeCost;
    public int foodIncomeCost;
    public int recrootsCost;

    [Header("Info")]
    public int level;
    public int unlockLevel;
    public int rank;

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
