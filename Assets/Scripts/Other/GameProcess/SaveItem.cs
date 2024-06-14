using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SaveItem : MonoBehaviour
{
    public CountryScriptableObject country;

    [SerializeField] Image countryFlagImage;
    [SerializeField] TMP_Text saveNameText;
    [SerializeField] TMP_Text dateTimeText;
    [SerializeField] TMP_Text scenarioText;

    public int saveID;
    public string date;
    public string scenarioName;
    public int scenarioId;

    private SaveManager saveManager;

    private void Awake()
    {
        saveManager = FindObjectOfType<SaveManager>();
    }


    public void UpdateUI()
    {
        saveNameText.text = $"{ReferencesManager.Instance.languageManager.GetTranslation("SaveText")} {saveID}";

        dateTimeText.text = date;

        string _displayScenarioName = $"{ReferencesManager.Instance.languageManager.GetTranslation("MainMenu.SingleMode.ScenarioText")}: {scenarioName}";

        scenarioText.text = _displayScenarioName;

        countryFlagImage.sprite = country.countryFlag;
    }

    public void OnClick()
    {
        ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
        ReferencesManager.Instance.gameSettings.playMod.value = false;
        ReferencesManager.Instance.gameSettings.loadGame.value = true;

        ReferencesManager.Instance.offlineGameSettings.SelectScenario(scenarioId);

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
