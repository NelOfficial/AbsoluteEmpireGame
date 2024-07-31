using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//⠀⠀⠀⣠⠤⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣀⠀⠀
//⠀⠀⡜⠁⠀⠈⢢⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣴⠋⠷⠶⠱⡄
//⠀⢸⣸⣿⠀⠀⠀⠙⢦⡀⠀⠀⠀⠀⠀⠀⠀⢀⡴⠫⢀⣖⡃⢀⣸⢹
//⠀⡇⣿⣿⣶⣤⡀⠀⠀⠙⢆⠀⠀⠀⠀⠀⣠⡪⢀⣤⣾⣿⣿⣿⣿⣸
//⠀⡇⠛⠛⠛⢿⣿⣷⣦⣀⠀⣳⣄⠀⢠⣾⠇⣠⣾⣿⣿⣿⣿⣿⣿⣽
//⠀⠯⣠⣠⣤⣤⣤⣭⣭⡽⠿⠾⠞⠛⠷⠧⣾⣿⣿⣯⣿⡛⣽⣿⡿⡼
//⠀⡇⣿⣿⣿⣿⠟⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠻⣿⣿⣮⡛⢿⠃
//⠀⣧⣛⣭⡾⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⢿⣿⣷⣎⡇
//⠀⡸⣿⡟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⢿⣷⣟⡇
//⣜⣿⣿⡧⠀⠀⠀⠀⠀⡀⠀⠀⠀⠀⠀⠀⣄⠀⠀⠀⠀⠀⣸⣿⡜⡄
//⠉⠉⢹⡇⠀⠀⠀⢀⣞⠡⠀⠀⠀⠀⠀⠀⡝⣦⠀⠀⠀⠀⢿⣿⣿⣹
//⠀⠀⢸⠁⠀⠀⢠⣏⣨⣉⡃⠀⠀⠀⢀⣜⡉⢉⣇⠀⠀⠀⢹⡄⠀⠀
//⠀⠀⡾⠄⠀⠀⢸⣾⢏⡍⡏⠑⠆⠀⢿⣻⣿⣿⣿⠀⠀⢰⠈⡇⠀⠀
//⠀⢰⢇⢀⣆⠀⢸⠙⠾⠽⠃⠀⠀⠀⠘⠿⡿⠟⢹⠀⢀⡎⠀⡇⠀⠀
//⠀⠘⢺⣻⡺⣦⣫⡀⠀⠀⠀⣄⣀⣀⠀⠀⠀⠀⢜⣠⣾⡙⣆⡇⠀⠀
//⠀⠀⠀⠙⢿⡿⡝⠿⢧⡢⣠⣤⣍⣀⣤⡄⢀⣞⣿⡿⣻⣿⠞⠀⠀⠀
//⠀⠀⠀⢠⠏⠄⠐⠀⣼⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀⠳⢤⣉⢳⠀⠀⠀
//⢀⡠⠖⠉⠀⠀⣠⠇⣿⡿⣿⡿⢹⣿⣿⣿⣿⣧⣠⡀⠀⠈⠉⢢⡀⠀
//⢿⠀⠀⣠⠴⣋⡤⠚⠛⠛⠛⠛⠛⠛⠛⠛⠙⠛⠛⢿⣦⣄⠀⢈⡇⠀
//⠈⢓⣤⣵⣾⠁⣀⣀⠤⣤⣀⠀⠀⠀⠀⢀⡤⠶⠤⢌⡹⠿⠷⠻⢤⡀
//⢰⠋⠈⠉⠘⠋⠁⠀⠀⠈⠙⠳⢄⣀⡴⠉⠀⠀⠀⠀⠙⠂⠀⠀⢀⡇
//⢸⡠⡀⠀⠒⠂⠐⠢⠀⣀⠀⠀⠀⠀⠀⢀⠤⠚⠀⠀⢸⣔⢄⠀⢾⠀
//⠀⠑⠸⢿⠀⠀⠀⠀⢈⡗⠭⣖⡒⠒⢊⣱⠀⠀⠀⠀⢨⠟⠂⠚⠋⠀
//⠀⠀⠀⠘⠦⣄⣀⣠⠞⠀⠀⠀⠈⠉⠉⠀⠳⠤⠤⡤⠞⠀⠀⠀⠀⠀

public class ResourcesMarketManager : MonoBehaviour
{
    [Header("# UI References:")]

    [SerializeField] private GameObject _mainUI_container;

    [SerializeField] private GameObject _purchasePanel;
    [SerializeField] private TMP_InputField _input_resources;
    [SerializeField] private TMP_Text _totalPrice;
    [SerializeField] private Slider _resourcesSlider;

    [SerializeField] private GameObject _sellerItemPrefab;
    [SerializeField] private Transform _sellersContainer;

    public List<SellerData> _sellers = new();
    public List<MarketOrder> _marketOrders = new();

    [HideInInspector] public SellerData _currentSeller;

    public void ToggleUI()
    {
        if (_mainUI_container.activeSelf)
        {
            CloseUI();
        }
        else
        {
            OpenUI();
            UpdateSellersUI();
        }
    }

    public void OpenUI()
    {
        _mainUI_container.SetActive(true);
    }

    public void CloseUI()
    {
        _mainUI_container.SetActive(false);
    }

    public void UpdateSellersUI()
    {
        foreach (Transform seller in _sellersContainer)
        {
            Destroy(seller.gameObject);
        }

        foreach (SellerData seller in _sellers)
        {
            GameObject sellerItem = Instantiate(_sellerItemPrefab, _sellersContainer);

            SellerItemButton sellerItemButton = sellerItem.GetComponent<SellerItemButton>();

            sellerItemButton._sellerData = seller;
            sellerItemButton.SetUp();

            sellerItem.GetComponent<Button>().onClick.AddListener(delegate
            {
                OpenPurchasePanel(seller);
            });
        }
    }

    public void OpenPurchasePanel(SellerData sellerData)
    {
        _purchasePanel.SetActive(true);

        _currentSeller = sellerData;

        _resourcesSlider.maxValue = _currentSeller._maxResAmount;

        MarketOrder order = GetPlayerOrder(_currentSeller._seller, _currentSeller._resource);

        _input_resources.text = $"{order._amountOfRes}";
        _resourcesSlider.value = int.Parse(_input_resources.text);
    }

    public void UpdateValue(string _value)
    {
        int value = int.Parse(_value);

        if (value >= _currentSeller._maxResAmount)
        {
            _input_resources.text = $"{_currentSeller._maxResAmount}";
        }
        else if (value < 0)
        {
            _input_resources.text = "0";
        }

        UpdatePriceText(value);
    }

    private void UpdatePriceText(int amount)
    {
        _totalPrice.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("Market.YouWillPay")}: {CountPrice(amount, _currentSeller._cost)}";

        MarketOrder playerOrder = GetPlayerOrder(_currentSeller._seller, _currentSeller._resource);

        if (playerOrder != null)
        {
            if (playerOrder._amountOfRes > 0)
            {
                if (CountPrice(amount, _currentSeller._cost) <= playerOrder._cost)
                {
                    _totalPrice.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("Market.Refund")}: {playerOrder._cost - CountPrice(amount, _currentSeller._cost)}";
                }
            }
        }
    }

    private int CountPrice(int resAmount, int costPerRes)
    {
        int price = resAmount * costPerRes;

        return price;
    }

    public void LinkInputToSlider()
    {
        _input_resources.text = $"{_resourcesSlider.value}";
        UpdateValue(_input_resources.text);
    }

    public void LinkSliderToInput()
    {
        _resourcesSlider.value = int.Parse(_input_resources.text);
    }

    public void Purchase()
    {
        _purchasePanel.SetActive(false);

        int amount = int.Parse(_input_resources.text);
        int totalPrice = CountPrice(amount, _currentSeller._cost);

        FormOrder(_currentSeller, ReferencesManager.Instance.countryManager.currentCountry.country, totalPrice, amount);

        UpdateSellersUI();
    }

    public void FormOrder(SellerData sellerData, CountryScriptableObject customer, int price, int amount)
    {
        MarketOrder order = GetOrder(sellerData._seller, customer, _currentSeller._resource);

        if (order != null)
        {
            _marketOrders.Remove(order);
        }

        if (amount > 0)
        {
            MarketOrder newOrder = new MarketOrder();

            newOrder._seller = sellerData._seller;
            newOrder._customer = customer;
            newOrder._resource = _currentSeller._resource;
            newOrder._cost = price;
            newOrder._amountOfRes = amount;

            _marketOrders.Add(newOrder);
        }
    }

    public MarketOrder GetOrder(CountryScriptableObject _seller, CountryScriptableObject _customer, GameSettings.Resource _resource)
    {
        MarketOrder marketOrder = new MarketOrder();

        foreach (MarketOrder order in _marketOrders)
        {
            if (order._seller == _seller && order._customer == _customer &&
                order._resource == _resource)
            {
                marketOrder = order;
            }
        }

        return marketOrder;
    }

    public MarketOrder GetPlayerOrder(CountryScriptableObject _seller, GameSettings.Resource _resource)
    {
        MarketOrder marketOrder = GetOrder(_seller, ReferencesManager.Instance.countryManager.currentCountry.country, _resource);

        return marketOrder;
    }


    public int CountAllSellResources(CountryScriptableObject _seller, GameSettings.Resource _resource)
    {
        int amount = 0;

        foreach (MarketOrder order in _marketOrders)
        {
            if (order._seller == _seller &&
                order._resource == _resource)
            {
                amount += order._amountOfRes;
            }
        }

        return amount;
    }

    public int CountAllSellerIncome(CountryScriptableObject _seller)
    {
        int income = 0;

        foreach (MarketOrder order in _marketOrders)
        {
            if (order._seller == _seller)
            {
                income += order._cost;
            }
        }

        return income;
    }

    public int CountAllCustomerExpenses(CountryScriptableObject _customer)
    {
        int expenses = 0;

        foreach (MarketOrder order in _marketOrders)
        {
            if (order._customer == _customer)
            {
                expenses += order._cost;
            }
        }

        return expenses;
    }

    public int CountAllCustomerResourceGains(CountryScriptableObject _customer, GameSettings.Resource _resource)
    {
        int resourceAmount = 0;

        foreach (MarketOrder order in _marketOrders)
        {
            if (order._customer == _customer &&
                order._resource == _resource)
            {
                resourceAmount += order._amountOfRes;
            }
        }

        return resourceAmount;
    }



    [System.Serializable]
    public class SellerData
    {
        public CountryScriptableObject _seller;

        public GameSettings.Resource _resource;

        public int _cost;

        public int _maxResAmount;
        public int _currentResAmount;
    }

    [System.Serializable]
    public class MarketOrder
    {
        public CountryScriptableObject _seller;
        public CountryScriptableObject _customer;

        public GameSettings.Resource _resource;

        public int _cost;
        public int _amountOfRes;
    }
}
