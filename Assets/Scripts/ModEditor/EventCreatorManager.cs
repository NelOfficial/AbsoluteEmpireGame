using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;

public class EventCreatorManager : MonoBehaviour
{
    [Header("#--UI REFS--#")]
    [SerializeField] GameObject _addButton;
    [SerializeField] Transform _eventContainerAddButton;

    [Header("#--EVENT UI--#")]
    [SerializeField] GameObject _eventPanel;
    [SerializeField] TMP_InputField _eventNameInputfield;
    [SerializeField] TMP_InputField _eventDescriptionInputfield;
    [SerializeField] TMP_InputField _eventConditionsInputfield;
    [SerializeField] TMP_InputField _eventReceiversInputfield;
    [SerializeField] TMP_InputField _eventReceiversBlacklistInputfield;
    public TMP_InputField dateInputfield;
    [SerializeField] ModEventImage _eventImage;
    [SerializeField] Toggle _eventSilentToggle;

    [Header("#--CONTAINERS--#")]
    public Transform _buttonsContainer;
    [SerializeField] Transform _eventsContainer;

    [Header("#--PREFABS--#")]
    [SerializeField] GameObject _buttonPrefab;
    [SerializeField] GameObject _eventButtonPrefab;

    [Header("#--SETTINGS--#")]
    [Range(1, 4)]
    [SerializeField] int maxButtonsInEvent;

    public List<ModEvent> modEvents = new List<ModEvent>();
    public int currentModEventIndex;

    private MapEditor _mapEditor;

    private void Start()
    {
        UpdateEventsUI();

        _mapEditor = FindObjectOfType<MapEditor>();
    }

    public void UpdateEventsUI()
    {
        foreach (Transform eventChildObject in _eventsContainer)
        {
            if (eventChildObject.GetComponent<ModEventButton>())
            {
                Destroy(eventChildObject.gameObject);
            }
        }

        for (int i = 0; i < modEvents.Count; i++)
        {
            GameObject newEventGameObject = Instantiate(_eventButtonPrefab, _eventsContainer);

            if (newEventGameObject.GetComponent<ModEventButton>())
            {
                ModEventButton newEventGameObject_wc = newEventGameObject.GetComponent<ModEventButton>();

                newEventGameObject_wc._id = modEvents[i].id;
                newEventGameObject_wc._eventText = modEvents[i]._name;

                newEventGameObject_wc.SetUp();
            }
        }

        _eventContainerAddButton.transform.SetAsLastSibling();
    }

    public void LoadEvent(int _id)
    {
        _eventPanel.SetActive(true);

        for (int i = 0; i < modEvents.Count; i++)
        {
            if (modEvents[i].id == _id)
            {
                currentModEventIndex = i;
            }
        }

        _eventNameInputfield.text = modEvents[currentModEventIndex]._name;
        _eventDescriptionInputfield.text = modEvents[currentModEventIndex].description;

        _eventConditionsInputfield.text = "";

        if (modEvents[currentModEventIndex].conditions.Count > 0)
        {
            for (int i = 0; i < modEvents[currentModEventIndex].conditions.Count; i++)
            {
                _eventConditionsInputfield.text += modEvents[currentModEventIndex].conditions[i];
            }
        }
        else
        {
            _eventConditionsInputfield.text = string.Empty;
        }

        dateInputfield.text = modEvents[currentModEventIndex].date;

        _eventReceiversInputfield.text = "";
        _eventReceiversBlacklistInputfield.text = "";

        for (int i = 0; i < modEvents[currentModEventIndex].receivers.Count; i++)
        {
            _eventReceiversInputfield.text += $"{modEvents[currentModEventIndex].receivers[i]};";
        }

        for (int i = 0; i < modEvents[currentModEventIndex].exceptionsReceivers.Count; i++)
        {
            _eventReceiversBlacklistInputfield.text += $"{modEvents[currentModEventIndex].exceptionsReceivers[i]};";
        }

        _eventImage.SetUp();
        UpdateButtonsUI();
    }

    public void CreateEvent()
    {
        int newEventID = Random.Range(1000, 999999);

        if (modEvents.Count > 0)
        {
            for (int i = 0; i < modEvents.Count; i++)
            {
                if (newEventID != modEvents[i].id)
                {
                    ModEvent newEvent = new ModEvent();
                    newEvent.id = newEventID;
                    newEvent._name = "[New event]";
                    newEvent.description = "[New event description]";
                    newEvent.conditions = new List<string>();
                    newEvent.imagePath = "";
                    newEvent.date = "1-1-1936";

                    modEvents.Add(newEvent);

                    LoadEvent(newEvent.id);

                    break;
                }
            }
        }
        else
        {
            ModEvent newEvent = new ModEvent();
            newEvent.id = newEventID;
            newEvent._name = "New event";
            newEvent.description = "New event description";
            newEvent.date = "1-1-1936";

            modEvents.Add(newEvent);

            LoadEvent(newEvent.id);
        }
    }

    public void UpdateButtonsUI()
    {
        foreach (Transform button in _buttonsContainer)
        {
            if (button.GetComponent<EventButtonUI>())
            {
                Destroy(button.gameObject);
            }
        }

        if (modEvents[currentModEventIndex].buttons != null)
        {
            for (int i = 0; i < modEvents[currentModEventIndex].buttons.Count; i++)
            {
                GameObject newButton = Instantiate(_buttonPrefab, _buttonsContainer);
                newButton.transform.localScale = Vector3.one;

                if (newButton.GetComponent<EventButtonUI>())
                {
                    EventButtonUI newButton_wc = newButton.GetComponent<EventButtonUI>();

                    if (!string.IsNullOrEmpty(modEvents[currentModEventIndex].buttons[i].name))
                    {
                        newButton_wc._buttonName = modEvents[currentModEventIndex].buttons[i].name;
                        newButton_wc._buttonActions = modEvents[currentModEventIndex].buttons[i].actions.ToArrayPooled();
                        newButton_wc._buttonRejectUltimatum = modEvents[currentModEventIndex].buttons[i].rejectUltimatum;

                        newButton_wc.SetUp();
                    }
                }
            }
        }

        if (modEvents[currentModEventIndex].buttons.Count < maxButtonsInEvent)
        {
            _addButton.SetActive(true);
        }
        else if (modEvents[currentModEventIndex].buttons.Count >= maxButtonsInEvent)
        {
            _addButton.SetActive(false);
        }

        _addButton.transform.SetAsLastSibling();
    }

    public void AddButton()
    {
        EventScriptableObject.EventButton newEventButton = new EventScriptableObject.EventButton();
        modEvents[currentModEventIndex].buttons.Add(newEventButton);

        UpdateButtonsUI();
    }

    public void SaveEventChanges()
    {
        modEvents[currentModEventIndex]._name = _eventNameInputfield.text;
        modEvents[currentModEventIndex].description = _eventDescriptionInputfield.text;
        modEvents[currentModEventIndex].date = dateInputfield.text;
        
        modEvents[currentModEventIndex].conditions = _eventConditionsInputfield.text.Split('\n').ToListPooled();

        modEvents[currentModEventIndex].silentEvent = _eventSilentToggle.isOn;

        List<int> receiversList = new List<int>();
        List<int> receiversBlackList = new List<int>();

        string[] receiversString = _eventReceiversInputfield.text.Split(';');
        string[] receiversBlacklistString = _eventReceiversBlacklistInputfield.text.Split(';');

        for (int i = 0; i < receiversString.Length; i++)
        {
            if (!string.IsNullOrEmpty(receiversString[i]))
            {
                receiversList.Add(int.Parse(receiversString[i]));
            }
        }
        
        for (int i = 0; i < receiversBlacklistString.Length; i++)
        {
            if (!string.IsNullOrEmpty(receiversBlacklistString[i]))
            {
                receiversBlackList.Add(int.Parse(receiversBlacklistString[i]));
            }
        }

        modEvents[currentModEventIndex].receivers = receiversList;
        modEvents[currentModEventIndex].exceptionsReceivers = receiversBlackList;

        for (int i = 0; i < modEvents[currentModEventIndex].buttons.Count; i++)
        {
            EventScriptableObject.EventButton eventButton = modEvents[currentModEventIndex].buttons[i];

            eventButton.actions = _buttonsContainer.GetChild(i).GetComponent<EventButtonUI>().buttonActions.text.Split('\n').ToListPooled();
            eventButton.name = _buttonsContainer.GetChild(i).GetComponent<EventButtonUI>().buttonName.text;
        }

        UpdateEventsUI();
    }

    public void CreateEventButton()
    {
        CreateEvent();

        UpdateEventsUI();
    }

    [System.Serializable]
    public class ModEvent
    {
        public int id;
        [TextArea(1, 100)]
        public string _name;

        [TextArea(1, 100)]
        public string description;
        public string date;

        public string imagePath;
        public Texture2D texture;

        public bool _checked;
        public bool silentEvent;

        [TextArea(1, 100)]
        public List<string> conditions = new List<string>();

        public List<int> receivers = new List<int>();
        public List<int> exceptionsReceivers = new List<int>();
        public int aiWillDo_Index = 0;

        public List<EventScriptableObject.EventButton> buttons = new List<EventScriptableObject.EventButton>();
    }
}
