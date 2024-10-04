using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StabilityUI_Item : MonoBehaviour
{
    [HideInInspector] public Stability_buff _bonus;

    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bonusText;
    [SerializeField] private Image _icon;

    public void SetUp()
    {
        if (_bonus != null)
        {
            //_bonusText.text = ReferencesManager.Instance.languageManager.GetTranslation(_bonus._name);
            _titleText.text = _bonus._name;

            if (_bonus.value < 0)
            {
                _bonusText.color = Color.red;
            }
            else if (_bonus.value > 0)
            {
                _bonusText.color = Color.green;
            }
            else if (_bonus.value == 0)
            {
                _bonusText.color = Color.yellow;
            }

            _bonusText.text = $"{Mathf.RoundToInt(_bonus.value)}%";
            _icon.sprite = _bonus.icon;
        }
    }
}
