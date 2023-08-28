using UnityEngine;

[CreateAssetMenu(fileName = "NewTerrain", menuName = "Region/Terrain")]
public class TerrainScriptableObject : ScriptableObject
{
    public string _name;
    [TextArea(1, 20)]
    public string _description;
    public Sprite icon;

    public Color _color;

    [Header("Влияние на боевую единиицу")]
    public int INF_attackBonus;
    public int INF_defenceBonus;

    public int MIF_attackBonus;
    public int MIF_defenceBonus;
    public int MAR_attackBonus;
    public int MAR_defenceBonus;

    public int ART_attackBonus;
    public int ART_defenceBonus;

    public int HEA_attackBonus;
    public int HEA_defenceBonus;
}
