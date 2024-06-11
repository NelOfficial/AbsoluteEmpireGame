using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EventButtonUI : MonoBehaviour
{
    public int buttonIndex;
    public string _buttonName;
    public string[] _buttonActions;
    public bool _buttonRejectUltimatum;

    public TMP_InputField buttonName;
    public TMP_InputField buttonActions;
    public Toggle buttonRejectUltimatumToggle;

    public TMP_Text _buttonNameText;

    private GameEventUI gameEventUI;

    public void SetUp()
    {
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            buttonName.text = _buttonName;
            buttonActions.text = "";

            for (int i = 0; i < _buttonActions.Length; i++)
            {
                buttonActions.text += _buttonActions[i];
            }

            buttonRejectUltimatumToggle.isOn = _buttonRejectUltimatum;
        }
        else
        {
            _buttonNameText.text = _buttonName;
        }

        gameEventUI = FindObjectOfType<GameEventUI>();
    }

    public void OnClick()
    {
        gameEventUI = FindObjectOfType<GameEventUI>();

        gameEventUI.ProceedEvent(buttonIndex);
        gameEventUI.gameObject.SetActive(false);
        UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.regionUI.click_01);
    }

    public void SetRejectUltimatumValue(bool value)
    {
        _buttonRejectUltimatum = value;
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
