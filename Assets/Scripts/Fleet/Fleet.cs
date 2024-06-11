using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fleet : MonoBehaviour
{
    [HideInInspector] public CountrySettings _owner;

    [HideInInspector] public List<FleetUnitData> _fleetUnits = new List<FleetUnitData>();

    [HideInInspector] public SeaRegion _currentSeaRegion;
    [HideInInspector] public RegionManager _currentProvince;

    [HideInInspector] public bool _onBase;

    [SerializeField] private Image _owner_flag;
    [SerializeField] private SpriteRenderer _icon;


    public void SetUp()
    {
        _owner_flag.sprite = _owner.country.countryFlag;
    }


    [System.Serializable]
    public class FleetUnitData
    {
        public int _id;

        public FleetScriptableObject _unit;
        public float _health;
        public float _fuel;
    }
}
