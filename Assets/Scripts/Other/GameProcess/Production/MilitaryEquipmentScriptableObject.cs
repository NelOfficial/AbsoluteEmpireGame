using UnityEngine;

public class MilitaryEquipmentScriptableObject : ScriptableObject
{
    public enum EquipmentType
    {
        Weapon,
        Artillery,
        AntiTank,
        LightTank,
        MiddleTank,
        HeavyTank,
        Submarine
    }

    public string _name;
    public string _nameEN;

    public Sprite _icon;

    public EquipmentType _equipmentType;

    public int _moneyCost;
    public int _productionCost;
}
