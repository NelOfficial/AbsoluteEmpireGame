using UnityEngine;
using UnityEngine.UI;

public class ArmyTemplateUnit_UI : MonoBehaviour
{
    public int index;

    public UnitScriptableObject batalion;

    public Image batalionIcon;

    public void SetUp()
    {
        batalionIcon.sprite = batalion.icon;
    }

    public void RemoveBatalionFromTemplate()
    {
        Army.Template template = ReferencesManager.Instance.army.templates[ReferencesManager.Instance.countryInfo._currentTemplateIndex];
        template._batalions.Remove(template._batalions[index]);

        ReferencesManager.Instance.countryInfo.UpdateTemplateUI();
    }
}
