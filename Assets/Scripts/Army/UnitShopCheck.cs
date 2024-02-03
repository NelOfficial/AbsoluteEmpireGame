using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitShopCheck : MonoBehaviour {

    public UnitScriptableObject currentUnit;

    [SerializeField] RegionManager regionManager;

    [SerializeField] TMP_Text _moneyCostText;
    [SerializeField] TMP_Text _recruitsCostText;

    private void Awake()
    {
        _moneyCostText.text = currentUnit.moneyCost.ToString();
        _recruitsCostText.text = currentUnit.recrootsCost.ToString();
    }

    public void CheckLockState()
    {
        if (regionManager.currentRegionManager.armyLevel >= currentUnit.unlockLevel)
        {
            GetComponent<Button>().interactable = true;
        }
        else
        {
            GetComponent<Button>().interactable = false;
        }
    }
}
