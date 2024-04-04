using UnityEngine;
using TMPro;

public class ProfileNotificationItem : MonoBehaviour
{
    public string title;
    public string description;
    public string date;

    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text descText;


    public void SetUp()
    {
        titleText.text = title;
    }

    public void ToggleDescription()
    {
        if (this.GetComponent<Animator>().GetBool("openDesc") == false)
        {
            descText.text = $"{description}\n{date}";

            this.GetComponent<Animator>().Play("expandModInfo_IN");
            this.GetComponent<Animator>().SetBool("openDesc", true);
        }
        else
        {
            descText.text = "";

            this.GetComponent<Animator>().Play("expandModInfo_OUT");
            this.GetComponent<Animator>().SetBool("openDesc", false);
        }
    }
}
