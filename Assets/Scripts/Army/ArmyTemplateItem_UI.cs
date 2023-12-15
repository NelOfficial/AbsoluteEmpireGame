using UnityEngine;
using TMPro;

public class ArmyTemplateItem_UI : MonoBehaviour
{
    [HideInInspector] public int _index;
    [HideInInspector] public string _name;

    [SerializeField] TMP_Text _templateText;

    public void SetUp()
    {
        _templateText.name = _name;
    }

    public void RemoveTemplate()
    {
        Army.Template template = ReferencesManager.Instance.army.templates[_index];
        ReferencesManager.Instance.army.templates.Remove(template);

        ReferencesManager.Instance.countryInfo.UpdateTemplatesUI();
    }

    public void LoadTemplate()
    {
        ReferencesManager.Instance.countryInfo._currentTemplateIndex = _index;
        ReferencesManager.Instance.countryInfo.UpdateTemplateUI();
    }
}
