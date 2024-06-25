using UnityEngine;

[CreateAssetMenu(fileName = "NewShip", menuName = "Fleet/Ship")]
public class FleetScriptableObject : ScriptableObject
{
    public enum Type
    {
        SUBMARINE,
        CORVETTE,
        LIGHT_CRUISER,
        HEAVY_CRUISER,
        DESTROYER
    }

    public Type type;

    public Sprite icon;

    [Header("Cost")]
    public int recruitsCost;

    [Header("Info")]
    public int level;
    public int unlockLevel;
    public int rank;
    public float maxFuel;

    public MilitaryEquipmentScriptableObject _equipment;

    [Header("Battle parameters")]
    public float health;

    public float _speed;
    public float _weight;

    public float _artAttack;
    public float _airAttack;

    public float _armor;
    public float _armorPiercing;

    public float _hardness;

    public string unitName;
}
