using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectCountryButton : MonoBehaviour
{
    public CountrySettings country;
    public CountryScriptableObject country_ScriptableObject;

    [SerializeField] TMP_Text countryName;
    [SerializeField] Image countryFlag;

    [SerializeField] GameObject _checkmark;

    public bool map_editor;
    public bool askOfWar;

    private MapEditor m_Editor;

    public void UpdateUI()
    {
        if (country != null)
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                countryName.text = country.country._nameEN;
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                countryName.text = country.country._name;
            }
            countryFlag.sprite = country.country.countryFlag;
        }
        else if (country_ScriptableObject != null)
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                countryName.text = country_ScriptableObject._nameEN;
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                countryName.text = country_ScriptableObject._name;
            }
            countryFlag.sprite = country_ScriptableObject.countryFlag;
        }
    }

    public void EditCountry()
    {
        if (m_Editor == null)
        {
            m_Editor = FindObjectOfType<MapEditor>();
        }

        m_Editor.editingCountry = country;

        m_Editor.OpenCountryEditPanel();
    }

    public void CreateVassal()
    {
        ReferencesManager.Instance.countryInfo.countryInfoPanelAdvanced.SetActive(false);
        ReferencesManager.Instance.gameSettings.regionSelectionMode = true;
        ReferencesManager.Instance.gameSettings.regionSelectionModeType = "my_country";
        ReferencesManager.Instance.gameSettings.provincesList.Clear();

        ReferencesManager.Instance.regionUI.createVassalRegionSelectionModeButton.SetActive(true);
        BackgroundUI_Overlay.Instance.CloseOverlay();

        ReferencesManager.Instance.countryInfo.newVassal = ReferencesManager.Instance.CreateCountry_Component(country_ScriptableObject, ReferencesManager.Instance.countryManager.currentCountry.ideology);

        ReferencesManager.Instance.diplomatyUI.senderWars.Clear();
        Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(ReferencesManager.Instance.countryManager.currentCountry, ReferencesManager.Instance.countryInfo.newVassal);
        Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(ReferencesManager.Instance.countryInfo.newVassal, ReferencesManager.Instance.countryManager.currentCountry);

        foreach (CountrySettings country in ReferencesManager.Instance.countryManager.countries)
        {
            Relationships.Relation relation = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(ReferencesManager.Instance.countryManager.currentCountry, country);

            if (relation.war)
            {
                ReferencesManager.Instance.diplomatyUI.senderWars.Add(country);
            }
        }

        foreach (CountrySettings _cn in ReferencesManager.Instance.diplomatyUI.senderWars)
        {
            ReferencesManager.Instance.countryInfo.newVassal.GetComponent<CountryAIManager>().SendOffer("Объявить войну", ReferencesManager.Instance.countryInfo.newVassal, _cn);
        }

        senderToReceiver.vassal = true;
        receiverToSender.vassal = false;

        ReferencesManager.Instance.diplomatyUI.AISendOffer("Торговля", ReferencesManager.Instance.countryManager.currentCountry, ReferencesManager.Instance.countryInfo.newVassal, false);
        ReferencesManager.Instance.diplomatyUI.AISendOffer("Право прохода войск", ReferencesManager.Instance.countryManager.currentCountry, ReferencesManager.Instance.countryInfo.newVassal, false);
        ReferencesManager.Instance.diplomatyUI.AISendOffer("Пакт о ненападении", ReferencesManager.Instance.countryManager.currentCountry, ReferencesManager.Instance.countryInfo.newVassal, false);
        ReferencesManager.Instance.diplomatyUI.AISendOffer("Союз", ReferencesManager.Instance.countryManager.currentCountry, ReferencesManager.Instance.countryInfo.newVassal, false);

        senderToReceiver.relationship += 60;
        receiverToSender.relationship += 60;
    }

    public void OnClick()
    {
        if (!map_editor)
        {
            if (askOfWar)
            {
                ToggleSelectionOfCountry();
            }
            else
            {
                PlayerPrefs.SetInt("currentCountryIndex", country_ScriptableObject._id);
            }
        }
        else
        {
            if (m_Editor == null)
            {
                m_Editor = FindObjectOfType<MapEditor>();
            }

            m_Editor.selectedCountry = country;
            m_Editor.paintMapMode = true;
        }
    }


    public void ToggleSelectionOfCountry()
    {
        bool result = ReferencesManager.Instance.diplomatyUI._selectedCountries.Any(item => item.country._id == country_ScriptableObject._id);

        if (result)
        {
            DeSelectCountry();
        }
        else
        {
            SelectCountry();
        }
    }

    public void SelectCountry()
    {
        ReferencesManager.Instance.diplomatyUI.SelectCountry(country_ScriptableObject._id);
        _checkmark.SetActive(true);
    }

    public void DeSelectCountry()
    {
        ReferencesManager.Instance.diplomatyUI.DeSelectCountry(country_ScriptableObject._id);
        _checkmark.SetActive(false);
    }
}
