using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Interpretate : MonoBehaviour
{
    [SerializeField] private TextAsset csvFile;
    public Translate obj;
    [SerializeField] private bool DebugLog;

    private void Awake()
    {
        using (var reader = new StreamReader(new MemoryStream(csvFile.bytes)))
        {
            obj.value = new List<string[]>();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(';');

                obj.value.Add(values);
            }

            obj.value.Add(new string[] { "", "Перевод не найден :(", "No translate :(", "No translate :(", "No translate :(", "No translate :(" });

            if (DebugLog)
            {
                foreach (string[] str in obj.value)
                {
                    // ???
                    Debug.Log($"{str[0]}: (EN: {str[1]}), (RU: {str[2]}), (ES: {str[3]}), (PT: {str[4]}), (BR: {str[5]})");
                }
            }
        }
    }

    public string GetTranslation(string KEY)
    {
        int currentLanguage = PlayerPrefs.GetInt("languageId");

        List<string[]> TRANSLATION = ReferencesManager.Instance.gameSettings.translate.value;

        string[] local = new string[0];
        List<string> newstr;

        foreach (string[] str in TRANSLATION)
        {
            if (KEY == str[0])
            {
                local = str;
            }
        }

        for (int i = 0; i < local.Length; i++)
        {
            if (local[i] == "null" || local[i] == "")
            {
                local[i] = local[1];
            }
        }
        newstr = new List<string>();

        foreach (string mstr in local)
        {
            mstr.Replace("\\\\\"", "\\n\"");
            newstr.Add(mstr);
        }

        local = newstr.ToArray();
        if (local.Length == 0)
        {
            local = new string[] { "", "Перевод не найден :(", "No translate :(", "No translate :(", "No translate :(", "No translate :(" };
            Debug.Log(KEY);
        }        
        string result = "";

        if (local != null)
        {
            try
            {
                result = local[currentLanguage + 1];
            }
            catch (System.Exception) { }
        }

        return result;
    }
}