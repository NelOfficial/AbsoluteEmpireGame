using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountryInfo : MonoBehaviour
{
    [SerializeField] GameObject countryInfoPanel;
    [SerializeField] Image countryFlagImage;
    [SerializeField] TMP_Text countryNameText;
    [SerializeField] TMP_Text countryIdeologyText;
    [SerializeField] TMP_Text regionsCountText;
    [SerializeField] TMP_Text populationCountText;

    [HideInInspector] public CountrySettings country;

    [SerializeField] DiplomatyUI diplomatyUI;

    public void ToggleUI()
    {
        for (int i = 0; i < diplomatyUI.countryManager.countries.Count; i++)
        {
            if (diplomatyUI.countryManager.countries[i].country._id == diplomatyUI.receiverId)
            {
                country = diplomatyUI.countryManager.countries[i];
            }
        }

        if (!countryInfoPanel.activeSelf) // Open
        {
            countryInfoPanel.SetActive(true);
        }
        else if (countryInfoPanel.activeSelf) // Close
        {
            countryInfoPanel.SetActive(false);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        countryFlagImage.sprite = country.country.countryFlag;
        countryNameText.text = country.country._name;
        countryIdeologyText.text = country.ideology;
        regionsCountText.text = country.myRegions.Count.ToString();
        populationCountText.text = ReferencesManager.Instance.GoodNumberString(country.population);
    }
}
