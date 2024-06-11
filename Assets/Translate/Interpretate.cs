using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Interpretate: MonoBehaviour
{
    [SerializeField] private TextAsset csvFile;
    [SerializeField] private Translate obj;
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
            obj.value.Add(new string[] { "", "Ïונוגמה םו םאיהום :(", "No translate :(", "No translate :(", "No translate :(", "No translate :(" });
            if (DebugLog)
            {
                foreach (string[] str in obj.value)
                {
                    // ûûû
                    Debug.Log($"{str[0]}: (EN: {str[1]}), (RU: {str[2]}), (ES: {str[3]}), (PT: {str[4]}), (BR: {str[5]})");
                }
            }
        }
    }
}
