using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewModListValue", menuName = "Values/ModListValue")]
public class ModListValue : ScriptableObject
{
    public List<LocalSavedModification> list = new List<LocalSavedModification>();

    [System.Serializable]
    public class LocalSavedModification
    {
        public int id;
        public int version;
    }
}
