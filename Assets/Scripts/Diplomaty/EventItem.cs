using UnityEngine;
using UnityEngine.UI;

public class EventItem : MonoBehaviour
{
    public CountrySettings sender;
    public CountrySettings receiver;
    public string offer;

    public Image senderImage;
    public Image offerImage;

    public Sprite offerSprite;

    private EventPanel eventPanel;

    public bool canDecline;

    private void Awake()
    {
        eventPanel = FindObjectOfType<EventPanel>();
    }

    public void OpenPanel()
    {
        eventPanel.currentEventItem = this;
        eventPanel.OpenUI();
    }
}
