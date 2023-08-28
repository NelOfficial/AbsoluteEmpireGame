using TMPro;
using UnityEngine;

public class UI_InfoPanel : MonoBehaviour
{
    [SerializeField] TMP_Text infoPanelText;
    [SerializeField] GameObject infoPanel;
    [SerializeField] RectTransform infoPanelContent;

    private MainMenu mainMenu;
    private RegionUI regionUI;
    private static UI_InfoPanel Instance;

    private void Awake()
    {
        Instance = this;
        mainMenu = FindObjectOfType<MainMenu>();
        regionUI = FindObjectOfType<RegionUI>();
    }

    public void ShowPanel(string text)
    {
        infoPanel.SetActive(true);
        if (mainMenu != null)
        {
            mainMenu.ScrollEffect(infoPanelContent);
            if (text == "DIPLOMATY_UI_INFO_TEXT")
            {
                infoPanelText.text = ReferencesManager.Instance.gameSettings.DIPLOMATY_UI_INFO_TEXT;
            }
            else if (text != "DIPLOMATY_UI_INFO_TEXT")
            {
                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    infoPanelText.text = text.Split('\t')[0];
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    infoPanelText.text = text.Split('\t')[1];
                }
            }
        }
        else
        {
            if (text == "DIPLOMATY_UI_INFO_TEXT")
            {
                infoPanelText.text = ReferencesManager.Instance.gameSettings.DIPLOMATY_UI_INFO_TEXT;
            }
            else if (text == "BUILDING_INFO_TEXT")
            {
                infoPanelText.text = ReferencesManager.Instance.gameSettings.BUILDING_INFO_TEXT;
            }
            else if (text != "DIPLOMATY_UI_INFO_TEXT")
            {
                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    infoPanelText.text = text.Split('\t')[0];
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    infoPanelText.text = text.Split('\t')[1];
                }
            }
            regionUI.ScrollEffect(infoPanelContent);
        }
    }
}
