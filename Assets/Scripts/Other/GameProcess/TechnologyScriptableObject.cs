using UnityEngine;

[CreateAssetMenu(fileName = "NewTechnology", menuName = "Technologies/Technology")]
public class TechnologyScriptableObject : ScriptableObject
{
    public enum TechType
    {
        Infantry,
        Artillery,
        Heavy,
        Fleet,
        Economic,
        Aviation,
        AirPlane
    }

    public string _name;
    public string _nameEN;
    [TextArea(1, 100)]
    public string description;
    [TextArea(1, 100)]
    public string descriptionEN;

    public int moneyCost;
    public int researchPointsCost;
    public int moves;

    public TechType _type;

    public bool startReasearched;
    public bool optional;

    public TechnologyScriptableObject[] techsNeeded;

    public int _yearUnlock;

    public int oilBonus;
}
