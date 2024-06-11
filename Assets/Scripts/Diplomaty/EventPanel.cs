using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class EventPanel : MonoBehaviour
{
    public EventItem currentEventItem;

    [SerializeField] GameObject panel;
    [SerializeField] Image countryFlagImage;
    [SerializeField] TMP_Text countryNameText;
    [SerializeField] TMP_Text offerText;

    private DiplomatyUI diplomatyUI;
    private EventsContainer eventContainer;

    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _declineButton;

    private void Awake()
    {
        diplomatyUI = FindObjectOfType<DiplomatyUI>();
        eventContainer = FindObjectOfType<EventsContainer>();
    }

    public void OpenUI()
    {
        panel.SetActive(true);

        _declineButton.interactable = currentEventItem.canDecline;

        countryFlagImage.sprite = currentEventItem.sender.country.countryFlag;

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            countryNameText.text = currentEventItem.sender.country._nameEN;
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            countryNameText.text = currentEventItem.sender.country._name;
        }

        string offer_text = "";

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            if (currentEventItem.offer == "Торговля")
            {
                offer_text = "Trading";
            }
            else if (currentEventItem.offer == "Пакт о ненападении")
            {
                offer_text = "Non-agression pact";
            }
            else if (currentEventItem.offer == "Право прохода войск")
            {
                offer_text = "Right of troops passage";
            }
            else if (currentEventItem.offer == "Союз")
            {
                offer_text = "Union";
            }

            offerText.text = $"They offer you {offer_text}";
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            offerText.text = currentEventItem.offer;
        }
    }

    public void Accept()
    {
        AcceptForce(currentEventItem);
    }

    public void AcceptForce(EventItem eventItem)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            Multiplayer.Instance.AcceptOffer(eventItem.sender.country._id, eventItem.receiver.country._id, eventItem.offer);
        }
        else
        {
            if (eventItem.offer == "Торговля")
            {
                int relationsRandom = Random.Range(10, 15);

                Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(eventItem.sender, eventItem.receiver);
                Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(eventItem.receiver, eventItem.sender);

                senderToReceiver.trade = true;
                receiverToSender.trade = true;

                senderToReceiver.relationship += relationsRandom;
                receiverToSender.relationship += relationsRandom;

                ReferencesManager.Instance.CalculateTradeBuff(eventItem.sender, eventItem.receiver);

                Destroy(eventItem.gameObject);
            }
            else if (eventItem.offer == "Пакт о ненападении")
            {
                int relationsRandom = Random.Range(10, 15);

                Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(eventItem.sender, eventItem.receiver);
                Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(eventItem.receiver, eventItem.sender);

                senderToReceiver.pact = true;
                receiverToSender.pact = true;

                senderToReceiver.relationship += relationsRandom;
                receiverToSender.relationship += relationsRandom;

                Destroy(eventItem.gameObject);
            }
            else if (eventItem.offer == "Право прохода войск")
            {
                int relationsRandom = Random.Range(10, 15);

                Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(eventItem.sender, eventItem.receiver);
                Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(eventItem.receiver, eventItem.sender);

                senderToReceiver.right = true;
                receiverToSender.right = true;

                senderToReceiver.relationship += relationsRandom;
                receiverToSender.relationship += relationsRandom;

                Destroy(eventItem.gameObject);
            }
        }

        Destroy(eventItem.gameObject);
        panel.SetActive(false);

        diplomatyUI.countryManager.UpdateIncomeValuesUI();
        eventContainer.UpdateEvents();
    }

    public void RejectForce(EventItem eventItem)
    {
        Destroy(eventItem.gameObject);

        panel.SetActive(false);

        diplomatyUI.countryManager.UpdateIncomeValuesUI();
        eventContainer.UpdateEvents();
    }

    public void AcceptAll()
    {
        StartCoroutine(eventContainer.CheckEventsDelay());
        for (int i = 0; i < eventContainer.eventItems.Length; i++)
        {
            AcceptForce(eventContainer.eventItems[i]);
        }
        eventContainer.UpdateEvents();
    }

    public void RejectAll()
    {
        StartCoroutine(eventContainer.CheckEventsDelay());
        for (int i = 0; i < eventContainer.eventItems.Length; i++)
        {
            RejectForce(eventContainer.eventItems[i]);
        }
        eventContainer.UpdateEvents();
    }

    public void Reject()
    {
        RejectForce(currentEventItem);
    }
}
