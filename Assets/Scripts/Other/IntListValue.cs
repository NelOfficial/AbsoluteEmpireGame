using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIntListValue", menuName = "Values/IntListValue")]
public class IntListValue : ScriptableObject
{
    public List<int> list = new List<int>();
}
