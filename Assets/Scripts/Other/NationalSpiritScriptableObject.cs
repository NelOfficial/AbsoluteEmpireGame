using UnityEngine;

[CreateAssetMenu(fileName = "NewNationalSpirit", menuName = "Spirits/NationalSpirit")]
public class NationalSpiritScriptableObject : ScriptableObject
{
    public string title;
    public string description;

    [Header("National spirit bonuses")]
    public string[] bonuses;
}
