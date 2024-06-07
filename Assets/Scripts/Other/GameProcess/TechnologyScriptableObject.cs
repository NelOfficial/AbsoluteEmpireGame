using UnityEngine;

[CreateAssetMenu(fileName = "NewTechnology", menuName = "Technologies/Technology")]
public class TechnologyScriptableObject : ScriptableObject
{
    public string _name;
    public string _nameEN;
    [TextArea(1, 100)]
    public string description;
    [TextArea(1, 100)]
    public string descriptionEN;

    public int moneyCost;
    public int researchPointsCost;
    public int moves;

    public bool startReasearched;
    public bool optional;

    public TechnologyScriptableObject[] techsNeeded;

    public int oilBonus;
}
