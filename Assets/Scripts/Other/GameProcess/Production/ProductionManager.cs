using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionManager : MonoBehaviour
{
    [Header("# UI:")]
    [SerializeField] private GameObject _productionManager_Panel;
    [SerializeField] private GameObject[] _tabs;
    [SerializeField] private Image[] _tabButtons;
    [SerializeField] private GameObject _selectionPanel;

    [SerializeField] private GameObject _productionQueueItemPrefab;
    [SerializeField] private Transform _productionQueueContainer;
    [SerializeField] private TMP_Text _selectedEquipmentTitle;

    [SerializeField] private TMP_Text _selectedEquipment_moneyCost;
    [SerializeField] private TMP_Text _selectedEquipment_productionCost;

    [SerializeField] Transform neededTechsGrid;
    [SerializeField] GameObject neededTechsPanel;

    [SerializeField] TMP_Text neededTechPrefab;

    [SerializeField] Button _productionButton;

    private MilitaryEquipmentScriptableObject _selectedEquipment;

    [SerializeField] Color defaultColor;
    [SerializeField] Color defaultButtonColor;
    [SerializeField] Color selectedColor;

    [SerializeField] Color greenColor;
    [SerializeField] Color yellowColor;
    [SerializeField] Color redColor;

    private ProductionButton[] productionButtons;
    private int lastTab;


    public void ChangeTab(int index)
    {
        for (int i = 0; i < _tabs.Length; i++)
        {
            _tabs[i].SetActive(false);

            _tabButtons[i].color = defaultColor;
        }

        _tabs[index].SetActive(true);
        _tabButtons[index].color = selectedColor;
        productionButtons = FindObjectsOfType<ProductionButton>();

        for (int i = 0; i < productionButtons.Length; i++)
        {
            productionButtons[i].SetUp();
        }

        _selectionPanel.SetActive(false);
        lastTab = index;
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
            ReferencesManager.Instance.regionUI.CloseAllUI();
            BackgroundUI_Overlay.Instance.CloseOverlay();

            UpdateQueueUI();

            ChangeTab(lastTab);
        }
    }

    public void SelectEquipment(MilitaryEquipmentScriptableObject militaryEquipment)
    {
        _selectedEquipment = militaryEquipment;

        UpdateSelectionPanel();
    }

    private void UpdateSelectionPanel()
    {
        _selectionPanel.SetActive(true);

        foreach (Transform child in neededTechsGrid)
        {
            Destroy(child.gameObject);
        }

        _selectedEquipment_moneyCost.text = _selectedEquipment._moneyCost.ToString();
        _selectedEquipment_productionCost.text = _selectedEquipment._productionCost.ToString();

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            _selectedEquipmentTitle.text = _selectedEquipment._nameEN;
        }
        if (PlayerPrefs.GetInt("languageId") == 1)
        {
            _selectedEquipmentTitle.text = _selectedEquipment._name;
        }

        if (_selectedEquipment._techsNeeded.Length > 0)
        {
            neededTechsPanel.SetActive(true);

            for (int i = 0; i < _selectedEquipment._techsNeeded.Length; i++)
            {
                TMP_Text spawnedNeededTech = Instantiate(neededTechPrefab);
                spawnedNeededTech.transform.SetParent(neededTechsGrid);
                spawnedNeededTech.transform.localScale = new Vector3(1, 1, 1);

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    spawnedNeededTech.text = _selectedEquipment._techsNeeded[i]._nameEN;
                }
                if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    spawnedNeededTech.text = _selectedEquipment._techsNeeded[i]._name;
                }

                if (ReferencesManager.Instance.technologyManager.Researched(_selectedEquipment._techsNeeded[i]))
                {
                    spawnedNeededTech.color = ReferencesManager.Instance.gameSettings.greenColor;
                    spawnedNeededTech.fontStyle = FontStyles.Strikethrough;
                }
                else
                {
                    spawnedNeededTech.color = ReferencesManager.Instance.gameSettings.redColor;
                }
            }
        }

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            _selectedEquipmentTitle.text = $"{_selectedEquipment._nameEN}";
        }
        if (PlayerPrefs.GetInt("languageId") == 1)
        {
            _selectedEquipmentTitle.text = $"{_selectedEquipment._name}";
        }
    }

    public void AddEquipmentToQueue_M()
    {
        AddEquipmentToQueue(ReferencesManager.Instance.countryManager.currentCountry, _selectedEquipment);
    }

    public int GetFreeDockyards(CountrySettings country)
    {
        int busyDockyards = 0;

        for (int i = 0; i < country._prodQueue.Count; i++)
        {
            ProductionQueue productionQueue = country._prodQueue[i];
            busyDockyards += productionQueue._currentFactories;
        }

        return country.dockyards - busyDockyards;
    }

    public void AddEquipmentToQueue(CountrySettings country, MilitaryEquipmentScriptableObject equipment)
    {
        //if (country.dockyards > 0 && GetFreeDockyards(country) > 0)
        //{
        if (country.money >= equipment._moneyCost)
        {
            country.money -= equipment._moneyCost;

            int dockyardsToQueue = GetFreeDockyards(country);

            if (dockyardsToQueue >= equipment._maxFactories)
            {
                dockyardsToQueue = equipment._maxFactories;
            }

            ProductionQueue _prodQueueItem = new ProductionQueue();
            _prodQueueItem._id = Random.Range(0, 99999);
            _prodQueueItem._currentProgress = 0;
            _prodQueueItem._owner = country;
            _prodQueueItem._equipment = equipment;
            _prodQueueItem._currentFactories = dockyardsToQueue;

            country._prodQueue.Add(_prodQueueItem);
        }
        else
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                WarningManager.Instance.Warn("Not enough gold");
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                WarningManager.Instance.Warn("Недостаточно золота");
            }
        }

        //}
        //else
        //{
        //    if (PlayerPrefs.GetInt("languageId") == 0)
        //    {
        //        WarningManager.Instance.Warn("You don't have any manufacturing factories");
        //    }
        //    else if (PlayerPrefs.GetInt("languageId") == 1)
        //    {
        //        WarningManager.Instance.Warn("У вас нет производственных предприятий");
        //    }
        //}

        ReferencesManager.Instance.countryManager.UpdateValuesUI();
    }

    public void UpdateQueueUI()
    {
        foreach (Transform child in _productionQueueContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (ProductionQueue item in ReferencesManager.Instance.countryManager.currentCountry._prodQueue)
        {
            GameObject spawnedItem = Instantiate(_productionQueueItemPrefab, _productionQueueContainer);

            spawnedItem.GetComponent<ProductionItemQueue>()._militaryEquipment = item._equipment;
            spawnedItem.GetComponent<ProductionItemQueue>()._currentProgress = item._currentProgress;
            spawnedItem.GetComponent<ProductionItemQueue>()._currentFactories = item._currentFactories;
            spawnedItem.GetComponent<ProductionItemQueue>().SetUp();
        }
    }

    public void CheckProductionButton()
    {
        int neededTechResearched = 0;

        if (_selectedEquipment._techsNeeded.Length > 0)
        {
            for (int i = 0; i < _selectedEquipment._techsNeeded.Length; i++)
            {
                if (ReferencesManager.Instance.technologyManager.Researched(_selectedEquipment._techsNeeded[i]))
                {
                    neededTechResearched++;
                }
            }
        }
        else
        {
            _productionButton.interactable = true;
        }

        if (ReferencesManager.Instance.countryManager.currentCountry.money >= _selectedEquipment._moneyCost)
        {
            if (_selectedEquipment._techsNeeded.Length > 0)
            {
                if (neededTechResearched == _selectedEquipment._techsNeeded.Length)
                {
                    _productionButton.interactable = true;
                }
                else
                {
                    _productionButton.interactable = false;
                }
            }
            else
            {
                _productionButton.interactable = true;
            }
        }
        else
        {
            _productionButton.interactable = false;
        }
    }


    [System.Serializable]
    public class ProductionQueue
    {
        public int _id;

        public MilitaryEquipmentScriptableObject _equipment;
        public int _currentProgress;
        public int _currentFactories;
        public CountrySettings _owner;
    }
}
