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
        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            countryNameText.text = country.country._nameEN;
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            countryNameText.text = country.country._name;
        }

        if (country.ideology != "Неопределено" &&
            country.ideology != "Неопределённый" &&
            country.ideology != "Коммунизм" &&
            country.ideology != "Демократия" &&
            country.ideology != "Монархия" &&
            country.ideology != "Фашизм")
        {
            countryIdeologyText.text = country.ideology;
        }

        if (country.ideology == "Неопределено" || country.ideology == "Неопределённый")
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                countryIdeologyText.text = "Neutral";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                countryIdeologyText.text = country.ideology;
            }
        }

        if (country.ideology == "Коммунизм")
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                countryIdeologyText.text = "Communism";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                countryIdeologyText.text = country.ideology;
            }
        }

        if (country.ideology == "Демократия")
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                countryIdeologyText.text = "Democracy";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                countryIdeologyText.text = country.ideology;
            }
        }

        if (country.ideology == "Фашизм")
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                countryIdeologyText.text = "Fascism";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                countryIdeologyText.text = country.ideology;
            }
        }

        if (country.ideology == "Монархия")
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                countryIdeologyText.text = "Monarchy";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                countryIdeologyText.text = country.ideology;
            }
        }

        countryFlagImage.sprite = country.country.countryFlag;

        regionsCountText.text = country.myRegions.Count.ToString();

        populationCountText.text = ReferencesManager.Instance.GoodNumberString(country.population);
    }
}
