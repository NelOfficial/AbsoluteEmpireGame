using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectCountryButton : MonoBehaviour
{
    public CountrySettings country;
    public CountryScriptableObject country_ScriptableObject;

    [SerializeField] TMP_Text countryName;
    [SerializeField] Image countryFlag;

    public bool map_editor;

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

    public void OnClick()
    {
        if (!map_editor)
        {
            PlayerPrefs.SetInt("currentCountryIndex", country_ScriptableObject._id);
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
}
