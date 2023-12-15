using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEvent", menuName = "Events/Event")]
public class EventScriptableObject : ScriptableObject
{
    public int id;
    [TextArea(1, 100)]
    public string _name;

    [TextArea(1, 100)]
    public string description;
    public string date;

    public Sprite image;

    public bool _checked;
    public bool silentEvent;

    [TextArea(1, 100)]
    public List<string> conditions = new List<string>();

    public List<int> receivers = new List<int>();
    public List<int> exceptionsReceivers = new List<int>();
    public int aiWillDo_Index = 0;

    public List<EventButton> buttons = new List<EventButton>();

    public bool IS_GAME_MAIN_EVENT;

    [System.Serializable]
    public class EventButton
    {
        public string name;
        [TextArea(1,100)]
        public List<string> actions = new List<string>();
        public bool rejectUltimatum;
    }
}
