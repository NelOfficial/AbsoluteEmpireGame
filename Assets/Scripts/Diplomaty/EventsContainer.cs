using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class EventsContainer : MonoBehaviour
{
    public EventItem[] eventItems;
    public List<EventItem> dublicateItems = new List<EventItem>();

    [SerializeField] GameObject acceptAllButton;
    [SerializeField] GameObject rejectAllButton;

    [SerializeField] GameObject container;

    public void UpdateEvents()
    {
        eventItems = FindObjectsOfType<EventItem>();

        for (int i = 0; i < eventItems.Length; i++)
        {
            for (int j = i + 1; j < eventItems.Length; j++)
            {
                if (eventItems[i].offer == eventItems[j].offer && eventItems[i].sender == eventItems[j].sender && eventItems[i].receiver == eventItems[j].receiver)
                {
                    Destroy(eventItems[j].gameObject);
                }
            }
        }

        if (container.transform.childCount <= 0)
        {
            container.SetActive(false);
        }
        else if (container.transform.childCount > 0)
        {
            container.SetActive(true);
        }

        StartCoroutine(CheckButton_Co());
    }

    private IEnumerator CheckButton_Co()
    {
        yield return new WaitForSeconds(0.01f);
        eventItems = FindObjectsOfType<EventItem>();

        if (eventItems.Length > 1)
        {
            acceptAllButton.SetActive(true);
            rejectAllButton.SetActive(true);

            container.GetComponent<Image>().enabled = true;
            container.GetComponent<Image>().raycastTarget = true;
        }
        else if (eventItems.Length <= 0)
        {
            acceptAllButton.SetActive(false);
            rejectAllButton.SetActive(false);

            container.GetComponent<Image>().enabled = false;
            container.GetComponent<Image>().raycastTarget = false;
        }
    }

    public IEnumerator CheckEventsDelay()
    {
        yield return new WaitForSeconds(0.01f);
        eventItems = FindObjectsOfType<EventItem>();

        for (int i = 0; i < eventItems.Length; i++)
        {
            for (int j = i + 1; j < eventItems.Length; j++)
            {
                if (eventItems[i].offer == eventItems[j].offer && eventItems[i].sender == eventItems[j].sender && eventItems[i].receiver == eventItems[j].receiver)
                {
                    Destroy(eventItems[j].gameObject);
                }
            }
        }
        StartCoroutine(CheckButton_Co());
        yield return new WaitForSeconds(0.01f);
    }
}
