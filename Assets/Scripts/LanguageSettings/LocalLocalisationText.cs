using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class LocalLocalisationText : MonoBehaviour
{
    [Header("0 - English 1 - Russian")]
    [TextArea(1, 100)]
    public string[] localisationText;

    private void Awake()
    {
        SetUp();
    }

    public void SetUp()
    {
        int currentLanguage = PlayerPrefs.GetInt("languageId");

        if (this.GetComponent<TMP_Text>())
        {
            this.GetComponent<TMP_Text>().text = localisationText[currentLanguage];
        }
        else if (this.GetComponent<Text>())
        {
            this.GetComponent<Text>().text = localisationText[currentLanguage];
        }
    }
}
