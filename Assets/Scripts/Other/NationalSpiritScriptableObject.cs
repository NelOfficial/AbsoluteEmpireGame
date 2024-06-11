using UnityEngine;

[CreateAssetMenu(fileName = "NewNationalSpirit", menuName = "Spirits/NationalSpirit")]
public class NationalSpiritScriptableObject : ScriptableObject
{
    [TextArea(1, 100)]
    public string title;
    [TextArea(1, 100)]
    public string titleEN;

    [TextArea(1, 100)]
    public string description;
    [TextArea(1, 100)]
    public string descriptionEN;

    [TextArea(1, 100)]
    public string custonTooltip;

    [TextArea(1, 100)]
    public string custonTooltipEN;

    public int target;
    public Sprite icon;

    public int moves;

    [Header("National spirit bonuses")]
    public string[] bonuses;
}
