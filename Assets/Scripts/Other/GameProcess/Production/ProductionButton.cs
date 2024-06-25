using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionButton : MonoBehaviour
{
    public MilitaryEquipmentScriptableObject _militaryEquipment;

    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private Image _icon;

    public void SetUp()
    {
        if (_buttonText == null)
        {
            _buttonText = transform.Find("Text (TMP)").GetComponent<TMP_Text>();
        }

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            _buttonText.text = _militaryEquipment._nameEN;
        }
        if (PlayerPrefs.GetInt("languageId") == 1)
        {
            _buttonText.text = _militaryEquipment._name;
        }

        _icon.sprite = _militaryEquipment._icon;
    }

    public void SelectEquipment()
    {
        ReferencesManager.Instance.productionManager.SelectEquipment(_militaryEquipment);
    }
}
