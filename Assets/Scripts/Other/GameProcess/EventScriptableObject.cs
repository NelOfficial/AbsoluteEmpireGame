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

    public string[] conditions;

    public EventButton[] buttons;

    [System.Serializable]
    public class EventButton
    {
        public string name;
        public string[] actions;
        public bool rejectUltimatum;
    }
}
