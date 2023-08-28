using TMPro;
using UnityEngine;

public class EventButtonUI : MonoBehaviour
{
    public int buttonIndex;
    public string _text;

    [SerializeField] TMP_Text buttonText;

    private GameEventUI gameEventUI;

    public void SetUp()
    {
        buttonText.text = _text;
        gameEventUI = FindObjectOfType<GameEventUI>();
    }

    public void OnClick()
    {
        gameEventUI.ProceedEvent(buttonIndex);
        gameEventUI.gameObject.SetActive(false);
        UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.regionUI.click_01);
    }
}
