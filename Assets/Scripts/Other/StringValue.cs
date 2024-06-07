using UnityEngine;


[CreateAssetMenu(fileName = "NewStringValue", menuName = "Values/StringValue")]
public class StringValue : ScriptableObject
{
    [TextArea(1, 100)]
    public string value;
}
