using UnityEngine;
using UnityEngine.UI;

public class UnitShopCheck : MonoBehaviour {

    public UnitScriptableObject currentUnit;

    [SerializeField] RegionManager regionManager;

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
