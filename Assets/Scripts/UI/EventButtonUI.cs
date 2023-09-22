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

    public void RemoveButton()
    {
        Destroy(this.gameObject);

        int currentButtonIndex = 0;

        EventCreatorManager eventCreatorManager = FindObjectOfType<EventCreatorManager>();

        for (int i = 0; i < eventCreatorManager._buttonsContainer.childCount; i++)
        {
            if (this.gameObject == eventCreatorManager._buttonsContainer.GetChild(i))
            {
                currentButtonIndex = i;
            }
        }

        EventScriptableObject.EventButton button = eventCreatorManager.modEvents[eventCreatorManager.currentModEventIndex].buttons[currentButtonIndex];
        eventCreatorManager.modEvents[eventCreatorManager.currentModEventIndex].buttons.Remove(button);

        eventCreatorManager.UpdateButtonsUI();
    }
}
