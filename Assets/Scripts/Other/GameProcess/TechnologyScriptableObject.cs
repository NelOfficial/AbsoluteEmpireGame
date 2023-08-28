using UnityEngine;

[CreateAssetMenu(fileName = "NewTechnology", menuName = "Technologies/Technology")]
public class TechnologyScriptableObject : ScriptableObject
{
    public string _name;
    public string description;

    public int moneyCost;
    public int moves;

    public bool startReasearched;
    public bool optional;

    public TechnologyScriptableObject[] techsNeeded;
}
