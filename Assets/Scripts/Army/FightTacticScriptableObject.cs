using UnityEngine;

[CreateAssetMenu(fileName = "NewTactic", menuName = "Tactics/Tactic")]
public class FightTacticScriptableObject : ScriptableObject
{
    public int _id;
    public string _name;

    public int attackBonus;
    public int defenceBonus;

    public Sprite tacticSprite;
}
