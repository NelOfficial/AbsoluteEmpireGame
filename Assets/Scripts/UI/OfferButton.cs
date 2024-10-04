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
        try
        {
            int currentLanguage = PlayerPrefs.GetInt("languageId");

            CountrySettings receiver = diplomatyUI.FindCountryById(diplomatyUI.receiverId);
            CountrySettings sender = diplomatyUI.FindCountryById(diplomatyUI.senderId);

            if (dataType == "War")
            {
                if (diplomatyUI.FindCountriesRelation(sender, receiver).war) // War
                {
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.Peace");

                    data = "Заключить мир";
                }
                else if (!diplomatyUI.FindCountriesRelation(sender, receiver).war) // Peace
                {
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.SendWar");

                    data = "Объявить войну";
                }
            }
            else if (dataType == "Trade")
            {
                if (diplomatyUI.FindCountriesRelation(sender, receiver).trade) // Distrade
                {
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.UnTrade");

                    data = "Прекратить торговлю";
                }
                else if (!diplomatyUI.FindCountriesRelation(sender, receiver).trade) // Go trade
                {
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.Trade");

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
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.AntiPact");

                    data = "Расторгнуть пакт о ненападении";
                }
                else if (!diplomatyUI.FindCountriesRelation(sender, receiver).pact) // Gopact
                {
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.Pact");

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
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.DeUnion");

                    data = "Расторгнуть союз";
                }
                else if (!diplomatyUI.FindCountriesRelation(sender, receiver).union) // Union
                {
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.Union");

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
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.Right");

                    data = "Право прохода войск";
                }
                else if (diplomatyUI.FindCountriesRelation(sender, receiver).right) // Right
                {
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.DeRight");

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
                if (diplomatyUI.FindCountriesRelation(sender, receiver).vassal) // Vassal
                {
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.FreeVassal");

                    data = "Освободить вассала";
                }
                else if (!diplomatyUI.FindCountriesRelation(sender, receiver).vassal) // FreeVassal
                {
                    text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.Vassal");

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
            else if (dataType == "resource_transfer")
            {
                if (diplomatyUI.FindCountriesRelation(sender, receiver).war) // War
                {
                    button.interactable = false;
                }
                if (!diplomatyUI.FindCountriesRelation(sender, receiver).war) // Peace
                {
                    button.interactable = true;
                }
            }
            else if (dataType == "ask_war")
            {
                if (diplomatyUI.FindCountriesRelation(sender, receiver).war) // War
                {
                    button.interactable = false;
                }
                if (!diplomatyUI.FindCountriesRelation(sender, receiver).war) // Peace
                {
                    button.interactable = true;
                }
                text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.AskForWar");
            }
            else if (dataType == "region_transfer")
            {
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
                text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.SendMoney");
            }
            else if (data == "Попросить золото")
            {
                text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.AskMoney");
            }
            else if (data == "Отправить провизию")
            {
                text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.SendFood");
            }
            else if (data == "Попросить провизию")
            {
                text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.AskFood");
            }
            else if (data == "Отправить рекрутов")
            {
                text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.SendRecruits");
            }
            else if (data == "Попросить рекрутов")
            {
                text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.AskRecruits");
            }
            else if (data == "Отправить топливо")
            {
                text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.SendFuel");
            }
            else if (data == "Попросить топливо")
            {
                text.text = ReferencesManager.Instance.languageManager.GetTranslation("Diplomaty.AskFuel");
            }
        }
        catch (System.Exception) { }
    }

    public void SendData()
    {
        diplomatyUI.Execute_SendOffer(data, ReferencesManager.Instance);
    }
}
