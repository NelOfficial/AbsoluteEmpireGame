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

        _countryName.text = $"{ReferencesManager.Instance.languageManager.GetTranslation(_sellerData._seller._nameEN)}";

        ResourcesMarketManager.MarketOrder order = ReferencesManager.Instance.resourcesMarketManager.GetPlayerOrder(_sellerData._seller, _sellerData._resource);

        Button _button = GetComponent<Button>();

        if (order != null)
        {
            if (order._amountOfRes > 0)
            {
                _currentOrderAmount.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("Market.AlreadyPurchasing")} ({order._amountOfRes})";

                _sellerData._currentResAmount = _sellerData._maxResAmount - order._amountOfRes;
                _button.targetGraphic.color = _alreadyPurchased;
            }
            else
            {
                _currentOrderAmount.text = "";
                _sellerData._currentResAmount = _sellerData._maxResAmount;
                _button.targetGraphic.color = _defaultColor;
            }
        }
        else
        {
            _currentOrderAmount.text = "";
            _sellerData._currentResAmount = _sellerData._maxResAmount;
            _button.targetGraphic.color = _defaultColor;
        }

        _resourcesMax.text = $"{_sellerData._currentResAmount}/{_sellerData._maxResAmount}";
        _price.text = $"{_sellerData._cost}";

        CountryManager countryManager = ReferencesManager.Instance.countryManager;

        CountrySettings seller = countryManager.FindCountryByID(_sellerData._seller._id);

        Relationships.Relation senderToReceiver = 
            ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(
                seller, countryManager.currentCountry);

        _button.interactable = !senderToReceiver.war;
    }
}
