using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveItem : MonoBehaviour
{
    public CountryScriptableObject country;

    [SerializeField] Image countryFlagImage;
    [SerializeField] TMP_Text saveNameText;
    [SerializeField] TMP_Text dateTimeText;

    public int saveID;
    public string date;

    private SaveManager saveManager;

    private void Awake()
    {
        saveManager = FindObjectOfType<SaveManager>();
    }


    public void UpdateUI()
    {
        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            saveNameText.text = $"Save {saveID}";
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            saveNameText.text = $"Сохранение {saveID}";
        }

        dateTimeText.text = date;

        countryFlagImage.sprite = country.countryFlag;
    }

    public void OnClick()
    {
        ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
        ReferencesManager.Instance.gameSettings.playMod.value = false;
        ReferencesManager.Instance.gameSettings.loadGame.value = true;

        PlayerPrefs.DeleteKey("CURRENT_EDITING_MODIFICATION");

        saveManager.currentSave.value = saveID.ToString();

        ReferencesManager.Instance.mainMenu.LoadScene("EuropeSceneOffline");
    }

    public void DeleteSave()
    {
        saveManager.localSavesIds.list.Remove(saveID);

        saveManager.SetSavesIDs();

        saveManager.UpdateUI();
    }
}
