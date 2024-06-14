using UnityEngine;
using TMPro;


public class TechnologyButton : MonoBehaviour
{
    public TechnologyScriptableObject technology;

    [SerializeField] private TMP_Text buttonText;

    public void SetUp()
    {
        if (buttonText == null)
        {
            buttonText = transform.Find("Text (TMP)").GetComponent<TMP_Text>();
        }

        buttonText.text = ReferencesManager.Instance.languageManager.GetTranslation(technology._name);
    }
}
