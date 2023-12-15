using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OfferButton : MonoBehaviour
{
    public string data;
    public string dataType;

    public DiplomatyUI diplomatyUI;
    public Button button;
    public TMP_Text text;

    private void Awake()
    {
        diplomatyUI = FindObjectOfType<DiplomatyUI>();
        button = this.GetComponent<Button>();
    }

    public void UpdateOfferData()
    {
        int currentLanguage = PlayerPrefs.GetInt("languageId");

        CountrySettings receiver = diplomatyUI.FindCountryById(diplomatyUI.receiverId);
        CountrySettings sender = diplomatyUI.FindCountryById(diplomatyUI.senderId);

        if (dataType == "War")
        {
            if (diplomatyUI.FindCountriesRelation(sender, receiver).war) // War
            {
                if (currentLanguage == 0)
                {
                    text.text = "Peace";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Заключить мир";
                }
                data = "Заключить мир";
            }
            else if (!diplomatyUI.FindCountriesRelation(sender, receiver).war) // Peace
            {
                if (currentLanguage == 0)
                {
                    text.text = "Declare war";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Объявить войну";
                }
                data = "Объявить войну";
            }
        }
        else if (dataType == "Trade")
        {
            if (diplomatyUI.FindCountriesRelation(sender, receiver).trade) // Distrade
            {
                if (currentLanguage == 0)
                {
                    text.text = "Terminate trading";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Прекратить торговлю";
                }
                data = "Прекратить торговлю";
            }
            else if (!diplomatyUI.FindCountriesRelation(sender, receiver).trade) // Go trade
            {
                if (currentLanguage == 0)
                {
                    text.text = "Trading";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Торговля";
                }
                data = "Торговля";
            }
            if (diplomatyUI.FindCountriesRelation(sender, receiver).war) // War
            {
                button.interactable = false;
            }
            if (!diplomatyUI.FindCountriesRelation(sender, receiver).war) // Peace
            {
                button.interactable = true;
            }
        }
        else if (dataType == "Pact")
        {
            if (diplomatyUI.FindCountriesRelation(sender, receiver).pact) // Dispact
            {
                if (currentLanguage == 0)
                {
                    text.text = "Terminate non-aggression pact";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Расторгнуть пакт о ненападении";
                }
                data = "Расторгнуть пакт о ненападении";
            }
            else if (!diplomatyUI.FindCountriesRelation(sender, receiver).pact) // Gopact
            {
                if (currentLanguage == 0)
                {
                    text.text = "Non-aggression pact";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Пакт о ненападении";
                }
                data = "Пакт о ненападении";
            }
            if (diplomatyUI.FindCountriesRelation(sender, receiver).war) // War
            {
                button.interactable = false;
            }
            if (!diplomatyUI.FindCountriesRelation(sender, receiver).war) // Peace
            {
                button.interactable = true;
            }
        }
        else if (dataType == "Union")
        {
            if (diplomatyUI.FindCountriesRelation(sender, receiver).union) // Disunion
            {
                if (currentLanguage == 0)
                {
                    text.text = "Terminate alliance";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Расторгнуть союз";
                }
                data = "Расторгнуть союз";
            }
            else if (!diplomatyUI.FindCountriesRelation(sender, receiver).union) // Union
            {
                if (currentLanguage == 0)
                {
                    text.text = "Alliance";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Союз";
                }
                data = "Союз";
            }
            if (diplomatyUI.FindCountriesRelation(sender, receiver).war) // War
            {
                button.interactable = false;
            }
            if (!diplomatyUI.FindCountriesRelation(sender, receiver).war) // Peace
            {
                button.interactable = true;
            }
        }
        else if (dataType == "Right")
        {
            if (!diplomatyUI.FindCountriesRelation(sender, receiver).right) // Disright
            {
                if (currentLanguage == 0)
                {
                    text.text = "Right of passage of troops";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Право прохода войск";
                }
                data = "Право прохода войск";
            }
            else if (diplomatyUI.FindCountriesRelation(sender, receiver).right) // Right
            {
                if (currentLanguage == 0)
                {
                    text.text = "Terminate right of passage of troops";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Расторгнуть право прохода войск";
                }
                data = "Расторгнуть право прохода войск";
            }
            if (diplomatyUI.FindCountriesRelation(sender, receiver).war) // War
            {
                button.interactable = false;
            }
            if (!diplomatyUI.FindCountriesRelation(sender, receiver).war) // Peace
            {
                button.interactable = true;
            }
        }
        else if (dataType == "Vassal")
        {
            if (diplomatyUI.FindCountriesRelation(sender, receiver).vassal) // War
            {
                if (currentLanguage == 0)
                {
                    text.text = "Release the vassal";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Освободить вассала";
                }
                data = "Освободить вассала";
            }
            else if (!diplomatyUI.FindCountriesRelation(sender, receiver).vassal) // Peace
            {
                if (currentLanguage == 0)
                {
                    text.text = "Make a vassal";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "Сделать вассалом";
                }
                data = "Сделать вассалом";
            }
            if (diplomatyUI.FindCountriesRelation(sender, receiver).war) // War
            {
                button.interactable = false;
            }
            if (!diplomatyUI.FindCountriesRelation(sender, receiver).war) // Peace
            {
                button.interactable = true;
            }
        }

        if (data == "Отправить золото")
        {
            if (currentLanguage == 0)
            {
                text.text = "Send money";
            }
            else if (currentLanguage == 1)
            {
                text.text = "Отправить золото";
            }
        }
        else if (data == "Попросить золото")
        {
            if (currentLanguage == 0)
            {
                text.text = "Ask money";
            }
            else if (currentLanguage == 1)
            {
                text.text = "Попросить золото";
            }
        }
        else if (data == "Отправить провизию")
        {
            if (currentLanguage == 0)
            {
                text.text = "Send food";
            }
            else if (currentLanguage == 1)
            {
                text.text = "Отправить провизию";
            }
        }
        else if (data == "Попросить провизию")
        {
            if (currentLanguage == 0)
            {
                text.text = "Ask food";
            }
            else if (currentLanguage == 1)
            {
                text.text = "Попросить провизию";
            }
        }
        else if (data == "Отправить рекрутов")
        {
            if (currentLanguage == 0)
            {
                text.text = "Send recruits";
            }
            else if (currentLanguage == 1)
            {
                text.text = "Отправить рекрутов";
            }
        }
        else if (data == "Попросить рекрутов")
        {
            if (currentLanguage == 0)
            {
                text.text = "Ask recruits";
            }
            else if (currentLanguage == 1)
            {
                text.text = "Попросить рекрутов";
            }
        }
    }

    public void SendData()
    {
        diplomatyUI.Execute_SendOffer(data);
    }
}
