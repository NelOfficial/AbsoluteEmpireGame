using UnityEngine;
using UnityEngine.UI;

public class UI_CloseButton : MonoBehaviour
{
    private UI_Panel _panel;
    private Button _closeButton;

    private void Awake()
    {
        if (this.transform.parent.GetComponent<UI_Panel>())
        {
            _panel = this.transform.parent.GetComponent<UI_Panel>();
        }
        else
        {
            _panel = this.transform.parent.parent.GetComponent<UI_Panel>();
        }

        _closeButton = this.GetComponent<Button>();

        _closeButton.onClick.AddListener(ClosePanel);
    }

    public void ClosePanel()
    {
        _panel.ClosePanel();
    }
}
