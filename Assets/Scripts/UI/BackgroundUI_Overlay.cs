using UnityEngine;

public class BackgroundUI_Overlay : MonoBehaviour
{
    public GameObject overlay;
    public GameObject currentPanel;

    public static BackgroundUI_Overlay Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CloseOverlay()
    {
        overlay.SetActive(false);
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }
    }

    public void OpenOverlay(GameObject _currentPanel)
    {
        overlay.SetActive(true);

        currentPanel = _currentPanel;
        currentPanel.SetActive(true);
    }
}
