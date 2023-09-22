using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class EventCreatorManager : MonoBehaviour
{
    [Header("#--UI REFS--#")]
    [SerializeField] GameObject _addButton;
    [SerializeField] Transform _eventContainerAddButton;

    [Header("#--EVENT UI--#")]
    [SerializeField] GameObject _eventPanel;
    [SerializeField] TMP_InputField _eventNameInputfield;
    [SerializeField] TMP_InputField _eventDescriptionInputfield;
    [SerializeField] ModEventImage _eventImage;

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
            newEvent._name = "[New event]";
            newEvent.description = "[New event description]";

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

                    if (!string.IsNullOrEmpty(newButton_wc._text))
                    {
                        newButton_wc._text = modEvents[currentModEventIndex].buttons[i].name;
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
        public string _name;

        [TextArea(0, 50)]
        public string description;
        public string date;

        public string imagePath;
        public Texture2D texture;

        public bool _checked;
        public bool silentEvent;

        public List<string> conditions = new List<string>();

        public List<EventScriptableObject.EventButton> buttons = new List<EventScriptableObject.EventButton>();
    }
}
