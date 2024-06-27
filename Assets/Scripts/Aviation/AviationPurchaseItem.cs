using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AviationPurchaseItem : MonoBehaviour
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _itemName;
    [SerializeField] private TMP_Text _itemDesc;
    [SerializeField] private TMP_Text _moneyCostText;
    [SerializeField] private TMP_Text _recruitsCostText;
    [SerializeField] private Button _buyButton;

    public Aviation_ScriptableObj _plane;

    public void SetUp()
    {
        if (_plane != null)
        {
            _itemIcon.sprite = _plane.sprite;

            var refMan = ReferencesManager.Instance;
            var langManager = refMan.languageManager;

            _itemName.text = _plane.name;
            _itemDesc.text = _plane.description;

            _moneyCostText.text = refMan.GoodNumberString(_plane.price);
            _recruitsCostText.text = _plane.recruitsCost.ToString();

            _buyButton.interactable = false;

            var country = refMan.countryManager.currentCountry;

            if (country.money >= _plane.price && country.recroots >= _plane.recruitsCost)
            {
                bool hasTech = ReferencesManager.Instance.aviationUI.HasPlaneTech(_plane, country);

                _buyButton.interactable = hasTech;
            }
        }
        else
        {
            Debug.LogError("ERROR: _plane object is null!");
        }
    }

    public void Buy()
    {
        var airport = ReferencesManager.Instance.regionManager.currentRegionManager.GetComponent<Aviation_Storage>();

        if (airport.planes.Count < 4)
        {
            var countryManager = ReferencesManager.Instance.countryManager;
            var country = countryManager.currentCountry;

            country.money -= _plane.price;
            country.recroots -= _plane.recruitsCost;

            countryManager.UpdateIncomeValuesUI();
            countryManager.UpdateValuesUI();

            Aviation_Cell cell = new Aviation_Cell(_plane, country);

            airport.planes.Add(cell);

            if (airport.planes.Count >= 4)
            {
                _buyButton.interactable = false;
            }
        }
    }
}
