using UnityEngine;
using TMPro;


public class TechnologyButton : MonoBehaviour
{
    public TechnologyScriptableObject technology;

    [SerializeField] TMP_Text buttonText;

    public void SetUp()
    {
        if (buttonText == null)
        {
            buttonText = transform.Find("Text (TMP)").GetComponent<TMP_Text>();
        }

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            buttonText.text = technology._nameEN;
        }
        if (PlayerPrefs.GetInt("languageId") == 1)
        {
            buttonText.text = technology._name;
        }
    }
}
