using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class LocalModButton : MonoBehaviour
{
    public int id;
    public int currentCountryID;
    public string description;

    public TMP_Text modNameText;
    [SerializeField] TMP_Text modDescText;
    private ModificationPanel modificationPanel;

    [SerializeField] Image currentCountryFlag;

    public string modString;

    private void Awake()
    {
        modificationPanel = FindObjectOfType<ModificationPanel>();
    }

    public void SetUp()
    {
        modString = PlayerPrefs.GetString($"MODIFICATION_{id}");

        string[] lines = modString.Split(';');

        try
        {
            string _name = GetValue(lines, 0);
            modNameText.text = _name;
        }
        catch (System.Exception)
        {
        }
    }

    private string GetValue(string[] _lines, int line)
    {
        string _line = _lines[line];
        string part = _line.Split('[')[1];
        string value = part.Remove(part.Length - 1);

        return value;
    }

    public void DeleteMod()
    {
        PlayerPrefs.DeleteKey($"MODIFICATION_{id}");

        for (int i = 0; i < modificationPanel.downloadedModsIds.list.Count; i++)
        {
            if (modificationPanel.downloadedModsIds.list[i].id == id)
            {
                modificationPanel.downloadedModsIds.list.Remove(modificationPanel.downloadedModsIds.list[i]);
            }
        }
        modificationPanel.countriesListPanel.SetActive(false);

        modificationPanel.UpdateSavedIds();

        this.GetComponent<Animator>().Play("deleteModOut");
        Destroy(this.gameObject, 0.3f);
    }

    public void Play()
    {
        modificationPanel.currentLoadedModification.id = id;
        modificationPanel.currentLoadedModification.currentScenarioData = PlayerPrefs.GetString($"MODIFICATION_{id}");
        modificationPanel.countriesListPanel.SetActive(true);

        modificationPanel.UpdateCountriesList();
        modificationPanel.UpdateCountriesList();
    }

    public void ToggleDescription()
    {
        if (this.GetComponent<Animator>().GetBool("openDesc") == false)
        {
            modDescText.text = description;

            this.GetComponent<Animator>().Play("expandModInfo_IN");
            this.GetComponent<Animator>().SetBool("openDesc", true);
        }
        else
        {
            modDescText.text = "";

            this.GetComponent<Animator>().Play("expandModInfo_OUT");
            this.GetComponent<Animator>().SetBool("openDesc", false);
        }
    }

    public void Edit()
    {
        PlayerPrefs.SetInt($"CURRENT_EDITING_MODIFICATION", id);

        //Downloading the mod

        DownloadModWithID(id);

        SceneManager.LoadScene("Editor");
    }

    public void DownloadModWithID(int id)
    {
        for (int i = 0; i < ReferencesManager.Instance.profileManager.loadedModifications.Count; i++)
        {
            ModificationPanel.Modification mod = ReferencesManager.Instance.profileManager.loadedModifications[i];

            if (mod.id == id)
            {
                PlayerPrefs.SetString($"MODIFICATION_{id}", $"{mod.currentScenarioData}");

                ModListValue.LocalSavedModification localMod = new ModListValue.LocalSavedModification();
                localMod.id = id;
                localMod.version = mod.version;

                modificationPanel.downloadedModsIds.list.Add(localMod);
            }
        }

        modificationPanel.UpdateSavedIds();
    }

    public void RemoveModFromServer()
    {
        StartCoroutine(RemoveModFromServer_Co());
    }

    private IEnumerator RemoveModFromServer_Co()
    {
        WWWForm form = new WWWForm();
        form.AddField("id", id);

        UnityWebRequest removeMod_request = UnityWebRequest.Post("http://our-empire.7m.pl/core/remove_mod.php", form);
        yield return removeMod_request.SendWebRequest();

        if (!removeMod_request.isHttpError || !removeMod_request.isNetworkError || !removeMod_request.isNetworkError)
        {
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                modNameText.text = "You've successfully removed this modification";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                modNameText.text = "�� ������� ������� ��� �����������";
            }

            yield return new WaitForSeconds(1.5f);
            ReferencesManager.Instance.profileManager.UpdateUI();

        }
        else if (removeMod_request.isNetworkError || removeMod_request.isHttpError || removeMod_request.isNetworkError)
        {
            Debug.Log($"{removeMod_request.error}");
        }
    }
}