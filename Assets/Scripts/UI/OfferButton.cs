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
                    text.text = "��������� ���";
                }
                data = "��������� ���";
            }
            else if (!diplomatyUI.FindCountriesRelation(sender, receiver).war) // Peace
            {
                if (currentLanguage == 0)
                {
                    text.text = "Declare war";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "�������� �����";
                }
                data = "�������� �����";
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
                    text.text = "���������� ��������";
                }
                data = "���������� ��������";
            }
            else if (!diplomatyUI.FindCountriesRelation(sender, receiver).trade) // Go trade
            {
                if (currentLanguage == 0)
                {
                    text.text = "Trading";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "��������";
                }
                data = "��������";
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
                    text.text = "����������� ���� � �����������";
                }
                data = "����������� ���� � �����������";
            }
            else if (!diplomatyUI.FindCountriesRelation(sender, receiver).pact) // Gopact
            {
                if (currentLanguage == 0)
                {
                    text.text = "Non-aggression pact";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "���� � �����������";
                }
                data = "���� � �����������";
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
                    text.text = "����������� ����";
                }
                data = "����������� ����";
            }
            else if (!diplomatyUI.FindCountriesRelation(sender, receiver).union) // Union
            {
                if (currentLanguage == 0)
                {
                    text.text = "Alliance";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "����";
                }
                data = "����";
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
                    text.text = "����� ������� �����";
                }
                data = "����� ������� �����";
            }
            else if (diplomatyUI.FindCountriesRelation(sender, receiver).right) // Right
            {
                if (currentLanguage == 0)
                {
                    text.text = "Terminate right of passage of troops";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "����������� ����� ������� �����";
                }
                data = "����������� ����� ������� �����";
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
                    text.text = "���������� �������";
                }
                data = "���������� �������";
            }
            else if (!diplomatyUI.FindCountriesRelation(sender, receiver).vassal) // Peace
            {
                if (currentLanguage == 0)
                {
                    text.text = "Make a vassal";
                }
                else if (currentLanguage == 1)
                {
                    text.text = "������� ��������";
                }
                data = "������� ��������";
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

        if (data == "��������� ������")
        {
            if (currentLanguage == 0)
            {
                text.text = "Send money";
            }
            else if (currentLanguage == 1)
            {
                text.text = "��������� ������";
            }
        }
        else if (data == "��������� ������")
        {
            if (currentLanguage == 0)
            {
                text.text = "Ask money";
            }
            else if (currentLanguage == 1)
            {
                text.text = "��������� ������";
            }
        }
        else if (data == "��������� ��������")
        {
            if (currentLanguage == 0)
            {
                text.text = "Send food";
            }
            else if (currentLanguage == 1)
            {
                text.text = "��������� ��������";
            }
        }
        else if (data == "��������� ��������")
        {
            if (currentLanguage == 0)
            {
                text.text = "Ask food";
            }
            else if (currentLanguage == 1)
            {
                text.text = "��������� ��������";
            }
        }
        else if (data == "��������� ��������")
        {
            if (currentLanguage == 0)
            {
                text.text = "Send recruits";
            }
            else if (currentLanguage == 1)
            {
                text.text = "��������� ��������";
            }
        }
        else if (data == "��������� ��������")
        {
            if (currentLanguage == 0)
            {
                text.text = "Ask recruits";
            }
            else if (currentLanguage == 1)
            {
                text.text = "��������� ��������";
            }
        }
    }

    public void SendData()
    {
        diplomatyUI.Execute_SendOffer(data);
    }
}
