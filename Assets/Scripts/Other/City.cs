using TMPro;
using UnityEngine;

public class City : MonoBehaviour
{
    public CenturyLocalise[] centuryLocalises;

    public string GetKey()
    {
        string key = "";

        for (int i = 0; i < centuryLocalises.Length; i++)
        {
            if (ReferencesManager.Instance.dateManager.currentDate[2] >= centuryLocalises[i].yearMin &&
                ReferencesManager.Instance.dateManager.currentDate[2] < centuryLocalises[i].yearMax)
            {
                key = centuryLocalises[i]._key;
            }
        }

        return key;
    }

    [System.Serializable]
    public class CenturyLocalise
    {
        public int yearMin;
        public int yearMax;
        public string _key;
    }
}
