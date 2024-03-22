using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProductionManager : MonoBehaviour
{
    public List<ProductionQueue> _productionQueue = new List<ProductionQueue>();


    [Header("# UI:")]
    [SerializeField] private GameObject _productionManager_Panel;
    [SerializeField] private GameObject[] _tabs;
    [SerializeField] private GameObject _selectionPanel;

    [SerializeField] private GameObject _productionQueueItemPrefab;
    [SerializeField] private TMP_Text _selectedEquipmentTitle;
    [SerializeField] private TMP_Text _selectedEquipmentDescription;

    [SerializeField] private TMP_Text _selectedEquipment_moneyCost;
    [SerializeField] private TMP_Text _selectedEquipment_productionCost;


    public void ChangeTab(GameObject _tab)
    {
        for (int i = 0; i < _tabs.Length; i++)
        {
            _tabs[i].SetActive(false);
        }

        _tab.SetActive(true);
    }


    public void ToggleUI()
    {
        if (_productionManager_Panel.activeSelf)
        {
            _productionManager_Panel.SetActive(false);
        }
        else
        {
            _productionManager_Panel.SetActive(true);
        }
    }

    public void SelectEquipment()
    {

    }

    private void UpdateSelectionPanel()
    {

    }

    public void AddEquipmentToQueue()
    {

    }

    public void UpdateQueueUI()
    {

    }


    [System.Serializable]
    public class ProductionQueue
    {
        public int _id;

        public MilitaryEquipmentScriptableObject _equipment;
        public int _currentProgress;
        public CountrySettings _owner;
    }
}
