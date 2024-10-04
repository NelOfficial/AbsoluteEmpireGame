using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class ArmyTemplateItem_UI : MonoBehaviour
{
    [HideInInspector] public int _index;
    [HideInInspector] public string _name;
    [HideInInspector] public Sprite _icon;

    [SerializeField] private Image _iconImage;

    [SerializeField] TMP_Text _templateText;

    public void SetUp()
    {
        _templateText.text = _name;
        _iconImage.sprite = _icon;
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
        ReferencesManager.Instance.countryInfo.UpdateTemplateUnits();
    }

    public void BuyTemplate()
    {
        Army.Template template = ReferencesManager.Instance.army.templates[_index];

        UnitMovement division = new UnitMovement();

        foreach (Transform child in ReferencesManager.Instance.regionManager.currentRegionManager.transform)
        {
            if (child.gameObject.GetComponent<UnitMovement>())
            {
                division = child.gameObject.GetComponent<UnitMovement>();
            }
        }

        if (division.unitsHealth.Count == 1)
        {
            ReferencesManager.Instance.army.RemoveUnitFromDivision(division.unitsHealth[0], division, false);
        }

        foreach (UnitScriptableObject unit in template._batalions)
        {
            if (division.unitsHealth.Count + 1 < 11)
            {
                ReferencesManager.Instance.army.AddUnitToArmyNoUI(unit);
            }
            else
            {
                WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.ArmyLimit"));
            }
        }

        StartCoroutine(UpdateUI());
    }

    private IEnumerator UpdateUI()
    {
        yield return new WaitForSeconds(0.01f);

        ReferencesManager.Instance.regionUI.UpdateUnitsUI(true);

        yield break;
    }
}
