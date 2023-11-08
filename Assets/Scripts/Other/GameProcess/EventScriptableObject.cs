using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEvent", menuName = "Events/Event")]
public class EventScriptableObject : ScriptableObject
{
    public int id;
    public string _name;

    [TextArea(0, 50)]
    public string description;
    public string date;

    public Sprite image;

    public bool _checked;
    public bool silentEvent;

    public List<string> conditions = new List<string>();

    public List<EventButton> buttons = new List<EventButton>();

    public bool IS_GAME_MAIN_EVENT;

    [System.Serializable]
    public class EventButton
    {
        public string name;
        public List<string> actions = new List<string>();
        public bool rejectUltimatum;
    }
}
