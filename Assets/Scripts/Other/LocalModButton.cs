using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reflection;
using System;
using System.Linq;
using System.Security.Cryptography;

public class LocalModButton : MonoBehaviour
{
    public int id;
    public int currentCountryID;
    public string description;
    public int version;

    public TMP_Text modNameText;
    [SerializeField] TMP_Text modDescText;
    private ModificationPanel modificationPanel;

    [SerializeField] Image currentCountryFlag;

    public string modName;

    private string modPath;
    private string modData;

    private string c_text;
    private byte[] c_bytes;

    private bool isBytesNotEmpty;
    private bool isTextNotEmpty;

    private void Awake()
    {
        modificationPanel = FindObjectOfType<ModificationPanel>();
    }

    public void SetUp()
    {
        modName = PlayerPrefs.GetString($"MODIFICATION_{id}");
        modPath = Path.Combine(Application.persistentDataPath, "savedMods", modName, $"{modName}.AEMod");

        StreamReader reader = new StreamReader(modPath);
        modData = reader.ReadToEnd();

        reader.Close();

        modNameText.text = modName;
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

        try
        {
            File.Delete(Path.Combine(Application.persistentDataPath, "savedMods", modName));
        }
        catch (System.Exception exception)
        {
            Debug.Log(exception);
        }

        modificationPanel.countriesListPanel.SetActive(false);

        modificationPanel.UpdateSavedIds();

        this.GetComponent<Animator>().Play("deleteModOut");
        Destroy(this.gameObject, 0.3f);
    }

    public void Play()
    {
        modificationPanel.currentLoadedModification.id = id;
        modificationPanel.currentLoadedModification.currentScenarioData = modData;
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
        //Downloading the mod

        //DownloadModWithID(id);

        StartCoroutine(EditMod_Co());
    }

    //public void DownloadModWithID(int id)
    //{
    //    bool alreadyDownloaded = modificationPanel.downloadedModsIds.list.Any(item => item.id == id);

    //    WebClient client = new WebClient();

    //    Stream data = client.OpenRead(@$"http://absolute-empire.7m.pl/media/uploads/mods/{modName}/{modName}.AEMod");
    //    StreamReader reader = new StreamReader(data);

    //    string modData = reader.ReadToEnd();

    //    string ModPath = Path.Combine($"{Application.persistentDataPath}", "savedMods", $"{modName}");
    //    string ModsPath = Path.Combine($"{Application.persistentDataPath}", "savedMods");

    //    if (!Directory.Exists(ModsPath))
    //    {
    //        modificationPanel.CreateFolder("", "savedMods");
    //    }

    //    modificationPanel.CreateFolder(ModsPath, $"{modName}");
    //    modificationPanel.CreateFolder(ModPath, $"events");
    //    modificationPanel.CreateFolder(ModPath, $"countries");

    //    string eventsInfo = modData.Split("#EVENTS#")[1];

    //    string[] eventsInfoLines = eventsInfo.Split(';');

    //    try
    //    {
    //        List<int> eventsIDS = new List<int>();

    //        foreach (string eventData in eventsInfoLines)
    //        {
    //            if (!string.IsNullOrEmpty(eventData) && !string.IsNullOrWhiteSpace(eventData))
    //            {
    //                eventsIDS.Add(int.Parse(eventData));
    //            }
    //        }

    //        for (int i = 0; i < eventsIDS.Count; i++)
    //        {
    //            string imageUrl = @$"http://absolute-empire.7m.pl/media/uploads/mods/{modName}/events/{eventsIDS[i]}/{eventsIDS[i]}.jpg";
    //            string textUrl = @$"http://absolute-empire.7m.pl/media/uploads/mods/{modName}/events/{eventsIDS[i]}/{eventsIDS[i]}.AEEvent";

    //            StartCoroutine(CreateEventData_Co(ModPath, eventsIDS[i], imageUrl, textUrl));
    //        }
    //    }
    //    catch (Exception)
    //    {
    //        Debug.Log("No events found");
    //    }

    //    modificationPanel.CreateFile(modData, Path.Combine(ModPath, $"{modName}.AEMod"));

    //    data.Close();
    //    reader.Close();

    //    if (alreadyDownloaded)
    //    {
    //        foreach (ModListValue.LocalSavedModification localMod in modificationPanel.downloadedModsIds.list)
    //        {
    //            if (localMod.id == id)
    //            {
    //                localMod.version = version;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        ModListValue.LocalSavedModification mod = new ModListValue.LocalSavedModification();
    //        mod.id = id;
    //        mod.version = version;

    //        modificationPanel.downloadedModsIds.list.Add(mod);
    //    }

    //    PlayerPrefs.SetString($"MODIFICATION_{id}", $"{modName}");

    //    modificationPanel.UpdateSavedIds();
    //}

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
                modNameText.text = "Вы удалили этот мод";
            }

            yield return new WaitForSeconds(1.5f);
            ReferencesManager.Instance.profileManager.UpdateUI();

        }
        else if (removeMod_request.isNetworkError || removeMod_request.isHttpError || removeMod_request.isNetworkError)
        {
            Debug.Log($"{removeMod_request.error}");
        }
    }

    private IEnumerator EditMod_Co()
    {
        ReferencesManager.Instance.gameSettings.editingModString.value = modName;

        yield return new WaitUntil(() => string.IsNullOrEmpty(ReferencesManager.Instance.gameSettings.editingModString.value) == false);
        
        SceneManager.LoadSceneAsync("Editor");
    }

    public bool IsNullOrEmpty(Array array)
    {
        return (array == null || array.Length == 0);
    }

    public IEnumerator GetImageByURL_Co(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Or retrieve results as binary data
            c_bytes = www.downloadHandler.data;
        }

        isBytesNotEmpty = !IsNullOrEmpty(c_bytes);
    }

    private IEnumerator GetTextByURL_Co(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Or retrieve results as binary data
            c_text = www.downloadHandler.text;
        }

        isTextNotEmpty = !string.IsNullOrEmpty(c_text);
    }

    public IEnumerator CreateEventData_Co(string ModPath, int eventID, string imageUrl, string textUrl)
    {
        StartCoroutine(GetImageByURL_Co(imageUrl));
        StartCoroutine(GetTextByURL_Co(textUrl));

        yield return new WaitUntil(() => isTextNotEmpty == true);

        CreateFolder(Path.Combine(ModPath, "events"), $"{eventID}");
        CreateFile($"{c_text}", Path.Combine(ModPath, "events", $"{eventID}", $"{eventID}.AEEvent"));

        yield return new WaitUntil(() => isBytesNotEmpty == true);

        File.WriteAllBytes(Path.Combine(ModPath, "events", $"{eventID}", $"{eventID}.jpg"), c_bytes);
    }


    public void CreateFolder(string _path, string folderName)
    {
        string path = Path.Combine(Application.persistentDataPath, $"{_path}");
        path = Path.Combine(path, $"{folderName}");

        Directory.CreateDirectory(path);
        Debug.Log($"Created folder in {path} with name: {folderName}");
    }

    public void CreateFile(string fileText, string path)
    {
        StreamWriter streamWriter;
        FileInfo file = new FileInfo(path);
        streamWriter = file.CreateText();

        streamWriter.Write(fileText);
        streamWriter.Close();

        Debug.Log($"Created file in {path} with text: {fileText}");
    }
}
