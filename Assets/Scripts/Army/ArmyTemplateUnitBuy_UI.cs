using UnityEngine;
using UnityEngine.UI;

public class ArmyTemplateUnitBuy_UI : MonoBehaviour
{
    private Button _button;

    [SerializeField] UnitScriptableObject _batalion;


    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void UpdateUI()
    {
        _button.interactable = true;

        if (_batalion != ReferencesManager.Instance.gameSettings.soldierLVL1)
        {
            if (!ReferencesManager.Instance.technologyManager.HasUnitTech(ReferencesManager.Instance.countryManager.currentCountry, _batalion))
            {
                _button.interactable = false;
            }
        }
    }
}
