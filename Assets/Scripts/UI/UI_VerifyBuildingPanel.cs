using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class UI_VerifyBuildingPanel : MonoBehaviour
{
    public RegionManager regionManager;
    public BuildingScriptableObject currentBuilding;

    [SerializeField] GameObject container;
    [SerializeField] TMP_Text descText;
    [SerializeField] Image buildingImage;

    public void Show()
    {
        container.SetActive(true);

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            descText.text = $"¬ы действительно хотите разрушить здание: {currentBuilding.uiTitleEN}";
        }
        if (PlayerPrefs.GetInt("languageId") == 1)
        {
            descText.text = $"¬ы действительно хотите разрушить здание: {currentBuilding.uiTitle}";
        }
        buildingImage.sprite = currentBuilding.icon;

        UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.gameSettings.m_paper_01);
        BackgroundUI_Overlay.Instance.OpenOverlay(container);
    }

    public void ConfirmBreakBuilding()
    {
        regionManager.currentRegionManager.buildings.Remove(currentBuilding);
        regionManager.currentRegionManager.currentCountry.moneyNaturalIncome -= currentBuilding.goldIncome;
        regionManager.currentRegionManager.currentCountry.foodNaturalIncome -= currentBuilding.foodIncome;
        regionManager.currentRegionManager.currentCountry.recrootsIncome -= currentBuilding.recrootsIncome;
        ReferencesManager.Instance.regionUI.UpdateBuildingUI();

        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
        ReferencesManager.Instance.countryManager.UpdateValuesUI();

        BackgroundUI_Overlay.Instance.CloseOverlay();

        Multiplayer.Instance.SetCountryValues(
            regionManager.currentRegionManager.currentCountry.country._id,
            regionManager.currentRegionManager.currentCountry.money,
            regionManager.currentRegionManager.currentCountry.food,
            regionManager.currentRegionManager.currentCountry.recroots);
    }
}
