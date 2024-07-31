using UnityEngine;

[CreateAssetMenu(fileName = "NewRegionUpgrade", menuName = "Region/Upgrade")]
public class RegionUpgrade_ScriptableObject : ScriptableObject
{
    public string tag;
    public TechnologyScriptableObject tech;
}
