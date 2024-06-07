using UnityEngine;

[CreateAssetMenu(fileName = "NewMilitaryEquipment", menuName = "Equipment/MilitaryEquipment")]
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
        Submarine,
        Corvette,
        Destroyer,
        LightCruiser,
        HeavyCruiser
    }

    public string _name;
    public string _nameEN;

    public Sprite _icon;

    public EquipmentType _equipmentType;

    public TechnologyScriptableObject[] _techsNeeded;

    public int _moneyCost;
    public int _productionCost;
    public int _maxFactories;

    public FleetScriptableObject _fleet_Equipment;
}
