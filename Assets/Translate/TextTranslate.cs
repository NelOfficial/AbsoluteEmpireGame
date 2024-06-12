using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Runtime.CompilerServices;

public class TextTranslate : MonoBehaviour
{
    [Header("����")]
    [TextArea(1, 100)]
    public string KEY;
    private Translate TRANSLATION;
    private string[] local;
    private List<string> newstr;
    private void Start()
    {
        TRANSLATION = ReferencesManager.Instance.gameSettings.translate;
        //local = TRANSLATION.value[TRANSLATION.value.Count];
        foreach (string[] str in TRANSLATION.value)
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
        SetUp();
    }

    public void SetUp()
    {
        int currentLanguage = PlayerPrefs.GetInt("languageId");
        if (local != null)
        {
            if (this.GetComponent<TMP_Text>())
            {
                this.GetComponent<TMP_Text>().text = local[currentLanguage + 1];
            }
            else if (this.GetComponent<Text>())
            {
                this.GetComponent<Text>().text = local[currentLanguage + 1];
            }
        }
    }
}
