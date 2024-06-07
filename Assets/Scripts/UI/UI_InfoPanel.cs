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
        infoPanelText.text = "No info.";
        infoPanel.SetActive(true);
            
        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            string localisation_data = ReferencesManager.Instance.localisation_en.value;
            string[] localisation_parts = localisation_data.Split(';');

            for (int i = 0; i < localisation_parts.Length; i++)
            {
                string current_localisation_data = localisation_parts[i].Split(":")[0];
                current_localisation_data = current_localisation_data.Trim('\n');
                current_localisation_data = current_localisation_data.Trim(' ');

                text = text.Trim(' ');

                if (current_localisation_data == text)
                {
                    string newText = localisation_parts[i].Split(":")[1];
                    infoPanelText.text = newText;
                }
            }
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            string localisation_data = ReferencesManager.Instance.localisation_ru.value;
            string[] localisation_parts = localisation_data.Split(';');

            for (int i = 0; i < localisation_parts.Length; i++)
            {
                string current_localisation_data = localisation_parts[i].Split(":")[0];

                if (current_localisation_data == text)
                {
                    infoPanelText.text = localisation_parts[i].Split(":")[1];
                }
            }
        }

        if (mainMenu != null)
        {
            mainMenu.ScrollEffect(infoPanelContent);
        }
        else
        {
            regionUI.ScrollEffect(infoPanelContent);
        }
    }
}
