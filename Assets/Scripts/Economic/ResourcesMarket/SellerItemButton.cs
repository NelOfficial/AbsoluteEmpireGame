using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellerItemButton : MonoBehaviour
{
    [SerializeField] private FillCountryFlag _countryFlag;
    [SerializeField] private TMP_Text _countryName;
    [SerializeField] private TMP_Text _resourcesMax;
    [SerializeField] private TMP_Text _price;
    [SerializeField] private TMP_Text _currentOrderAmount;

    [HideInInspector] public ResourcesMarketManager.SellerData _sellerData;

    [SerializeField] private Color _alreadyPurchased;
    [SerializeField] private Color _defaultColor;

    public void SetUp()
    {
        _countryFlag.country = _sellerData._seller;
        _countryFlag.FillInfo();

        if (ReferencesManager.Instance.gameSettings._language == GameSettings.Language.EN)
        {
            _countryName.text = $"{_sellerData._seller._nameEN}";
        }
        else
        {
            _countryName.text = $"{_sellerData._seller._name}";
        }

        ResourcesMarketManager.MarketOrder order = ReferencesManager.Instance.resourcesMarketManager.GetPlayerOrder(_sellerData._seller, _sellerData._resource);

        if (order != null)
        {
            if (order._amountOfRes > 0)
            {
                if (ReferencesManager.Instance.gameSettings._language == GameSettings.Language.RU)
                {
                    _currentOrderAmount.text = $"Вы уже покупаете это ({order._amountOfRes})";
                }
                else
                {
                    _currentOrderAmount.text = $"You already buying this ({order._amountOfRes})";
                }

                GetComponent<Button>().targetGraphic.color = _alreadyPurchased;
            }
            else
            {
                _currentOrderAmount.text = "";
                GetComponent<Button>().targetGraphic.color = _defaultColor;
            }
        }
        else
        {
            _currentOrderAmount.text = "";
            GetComponent<Button>().targetGraphic.color = _defaultColor;
        }

        _resourcesMax.text = $"{_sellerData._currentResAmount}/{_sellerData._maxResAmount}";
        _price.text = $"{_sellerData._cost}";
    }
}
