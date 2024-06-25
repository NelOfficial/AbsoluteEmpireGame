using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewsItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _contentText;
    [SerializeField] private Button _button;

    [HideInInspector] public string _text;
    [HideInInspector] public string _url;

    public void SetUp()
    {
        string[] content = _text.Split("\t");

        _contentText.text = content[PlayerPrefs.GetInt("languageId", 1)];

        _button.onClick.AddListener(delegate
        {
            ReferencesManager.Instance.mainMenu.OpenUrl(_url);
        });
    }
}
