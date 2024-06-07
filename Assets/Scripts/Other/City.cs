using TMPro;
using UnityEngine;

public class City : MonoBehaviour
{
    private TMP_Text m_Text;

    [Header("1910")]
    public LocalisableString _1910_name;

    [Header("1936")]
    public LocalisableString _1936_name;

    [System.Serializable]
    public class LocalisableString
    {
        public string EN;
        public string RU;
    }

    public void SetUp()
    {
        m_Text = this.transform.GetComponentInChildren<TMP_Text>();

        if (ReferencesManager.Instance.dateManager.currentDate[2] >= 1910 &&
            ReferencesManager.Instance.dateManager.currentDate[2] < 1936)
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                m_Text.text = _1910_name.EN;
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                m_Text.text = _1910_name.RU;
            }
        }
        else if (ReferencesManager.Instance.dateManager.currentDate[2] >= 1936)
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                m_Text.text = _1936_name.EN;
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                m_Text.text = _1936_name.RU;
            }
        }
    }
}
