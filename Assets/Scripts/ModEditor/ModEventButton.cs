using TMPro;
using UnityEngine;

public class ModEventButton : MonoBehaviour
{
    public int _id;
    [HideInInspector] public string _eventText;

    [SerializeField] TMP_Text _eventNameText;

    private EventCreatorManager _eventCreatorManager;

    private void Awake()
    {
        _eventCreatorManager = FindObjectOfType<EventCreatorManager>();
    }

    public void SetUp()
    {
        _eventNameText.text = _eventText;
    }

    public void LoadEvent()
    {
        _eventCreatorManager = FindObjectOfType<EventCreatorManager>();

        _eventCreatorManager.LoadEvent(_id);
    }

    public void DeleteEvent()
    {
        for (int i = 0; i < _eventCreatorManager.modEvents.Count; i++)
        {
            if (_eventCreatorManager.modEvents[i].id == _id)
            {
                EventCreatorManager.ModEvent thisModEvent = _eventCreatorManager.modEvents[i];

                _eventCreatorManager.modEvents.Remove(thisModEvent);
            }

        }

        _eventCreatorManager.UpdateEventsUI();
    }
}
