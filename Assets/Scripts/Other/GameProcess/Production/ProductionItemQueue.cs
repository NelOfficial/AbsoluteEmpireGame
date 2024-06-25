using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProductionItemQueue : MonoBehaviour
{
    [HideInInspector] public MilitaryEquipmentScriptableObject _militaryEquipment;

    [SerializeField] private TMP_Text _eq_name;
    [SerializeField] private TMP_Text _eq_productionProgress;
    [SerializeField] private Image _eq_sliderHolder;
    [SerializeField] private TMP_Text _eq_factoriesAmount;
    [SerializeField] private Image _eq_factoriesSliderHolder;

    [HideInInspector] public int _currentProgress;
    [HideInInspector] public int _currentFactories;

    public void SetUp()
    {
        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            _eq_name.text = _militaryEquipment._nameEN;
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            _eq_name.text = _militaryEquipment._name;
        }

        UpdateProgressbar();
    }

    private void UpdateProgressbar()
    {
        _eq_sliderHolder.fillAmount = (float)_currentProgress / (float)_militaryEquipment._productionCost;
        _eq_productionProgress.text = $"{_currentProgress}/{_militaryEquipment._productionCost}";

        _eq_factoriesSliderHolder.fillAmount = (float)_currentFactories / (float)_militaryEquipment._maxFactories;
        _eq_factoriesAmount.text = $"{_currentFactories}/{_militaryEquipment._maxFactories}";
    }
}
