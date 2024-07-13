using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TechnologyButton : MonoBehaviour
{
    public TechnologyScriptableObject technology;

    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private GameObject _lockedUntilHolder;
    [SerializeField] private TMP_Text _lockedUntil_text;

    public void SetUp()
    {
        if (buttonText == null)
        {
            buttonText = transform.Find("Text (TMP)").GetComponent<TMP_Text>();
        }

        buttonText.text = ReferencesManager.Instance.languageManager.GetTranslation(technology._name);

        if (ReferencesManager.Instance.dateManager.currentDate[2] >= technology._yearUnlock)
        {
            _lockedUntilHolder.SetActive(false);
            GetComponent<Button>().interactable = true;
        }
        else
        {
            GetComponent<Button>().interactable = false;
            _lockedUntilHolder.SetActive(true);
            _lockedUntil_text.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("TechnologyPanel.LockedUntil")} {technology._yearUnlock}";
        }
    }
}
