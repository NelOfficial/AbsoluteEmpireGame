using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

public class DiplomatyUI : MonoBehaviour
{
    public int senderId;
    public int receiverId;

    public GameObject diplomatyContainer;
    public GameObject messageReceiver;
    public GameObject acceptationStatePanel;
    public GameObject receiverInfo;

    //private PhotonView photonView;
    [HideInInspector] public CountryManager countryManager;
    [HideInInspector] public RegionManager regionManager;
    [HideInInspector] public RegionUI regionUI;
    [HideInInspector] public CountryInfoAdvanced countryInfoAdvanced;

    public List<LocalOffer> localOffers = new List<LocalOffer>();
    public List<LocalOffer> aiOffers = new List<LocalOffer>();

    [SerializeField] Image senderCountryFlag;
    [SerializeField] Image receiverCountryFlag;

    [SerializeField] TMP_Text senderCountryNameText;
    [SerializeField] TMP_Text receiverCountryNameText;
    [SerializeField] TMP_Text acceptationStateText;
    [SerializeField] TMP_Text relationPointsText;

    [SerializeField] GameObject countryItem;

    [SerializeField] GameObject valuePanel;
    [SerializeField] TMP_InputField valueField;
    [SerializeField] TMP_Text textValueField;
    [SerializeField] Slider valueSlider;

    [SerializeField] GameObject[] horizontalSenderScrolls;
    [SerializeField] GameObject[] horizontalReceiverScrolls;

    [SerializeField] RectTransform offersContent;

    [SerializeField] TMP_Text[] horizontalSenderCount;
    [SerializeField] TMP_Text[] horizontalReceiverCount;

    [HideInInspector] public int _messageOfferId;

    private List<CountrySettings> senderWars = new List<CountrySettings>();
    private List<CountrySettings> receiverWars = new List<CountrySettings>();

    public List<TradeBuff> globalTrades = new List<TradeBuff>();

    [SerializeField] OfferButton[] offerButtons;

    public CountrySettings receiver;
    public CountrySettings sender;

    private string valueType;
    private int value;
    private bool accept;
    private int random;

    private void Start()
    {
        countryManager = FindObjectOfType<CountryManager>();
        countryInfoAdvanced = FindObjectOfType<CountryInfoAdvanced>();
        regionUI = FindObjectOfType<RegionUI>();
        regionManager = FindObjectOfType<RegionManager>();
    }

    public void OpenUI()
    {
        countryManager = FindObjectOfType<CountryManager>();
        DestroyChildrens();

        UISoundEffect.Instance.PlayAudio(regionUI.paper_01);

        BackgroundUI_Overlay.Instance.OpenOverlay(diplomatyContainer);

        acceptationStatePanel.SetActive(false);
        receiverInfo.SetActive(false);

        senderId = countryManager.currentCountry.country._id;
        receiverId = regionManager.currentRegionManager.currentCountry.country._id;
        regionManager.DeselectRegions();

        if (senderId != receiverId)
        {
            for (int i = 0; i < countryManager.countries.Count; i++)
            {
                if (countryManager.countries[i].country._id == senderId)
                {
                    sender = countryManager.countries[i];
                }

                else if (countryManager.countries[i].country._id == receiverId)
                {
                    receiver = countryManager.countries[i];
                }
            }


            diplomatyContainer.SetActive(true);

            receiverCountryFlag.sprite = receiver.country.countryFlag;
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                receiverCountryNameText.text = receiver.country._nameEN;
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                receiverCountryNameText.text = receiver.country._name;
            }


            senderCountryFlag.sprite = sender.country.countryFlag;
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                senderCountryNameText.text = sender.country._nameEN;
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                senderCountryNameText.text = sender.country._name;
            }


            UpdateDiplomatyUI(sender, receiver);
        }
        else
        {
            regionUI.CloseAllUI();
            regionManager.DeselectRegions();

            countryInfoAdvanced.ToggleUI();
        }
    }

    public void OpenUINoClick(int _receiverId)
    {
        countryManager = FindObjectOfType<CountryManager>();
        DestroyChildrens();

        UISoundEffect.Instance.PlayAudio(regionUI.paper_01);

        BackgroundUI_Overlay.Instance.OpenOverlay(diplomatyContainer);

        acceptationStatePanel.SetActive(false);
        receiverInfo.SetActive(false);

        senderId = countryManager.currentCountry.country._id;
        receiverId = _receiverId;

        regionManager.DeselectRegions();

        if (senderId != receiverId)
        {
            for (int i = 0; i < countryManager.countries.Count; i++)
            {
                if (countryManager.countries[i].country._id == senderId)
                {
                    sender = countryManager.countries[i];
                }

                else if (countryManager.countries[i].country._id == receiverId)
                {
                    receiver = countryManager.countries[i];
                }
            }


            diplomatyContainer.SetActive(true);

            receiverCountryFlag.sprite = receiver.country.countryFlag;
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                receiverCountryNameText.text = receiver.country._nameEN;
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                receiverCountryNameText.text = receiver.country._name;
            }

            senderCountryFlag.sprite = sender.country.countryFlag;
            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                senderCountryNameText.text = sender.country._nameEN;
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                senderCountryNameText.text = sender.country._name;
            }

            UpdateDiplomatyUI(sender, receiver);
        }
        else
        {
            regionUI.CloseAllUI();
            regionManager.DeselectRegions();

            countryInfoAdvanced.ToggleUI();
        }
    }

    public void ClsoeUI()
    {
        if (diplomatyContainer.activeSelf)
        {
            diplomatyContainer.GetComponent<UI_Panel>().ClosePanel();
        }
        BackgroundUI_Overlay.Instance.CloseOverlay();
    }

    public void DestroyChildrens()
    {
        for (int i = 0; i < horizontalSenderScrolls.Length; i++)
        {
            DestroyChildrens(horizontalSenderScrolls[i].transform);
        }

        for (int i = 0; i < horizontalReceiverScrolls.Length; i++)
        {
            DestroyChildrens(horizontalReceiverScrolls[i].transform);
        }
    }

    public void Execute_SendOffer(string offer, bool countRandom = true)
    {
        receiver = FindCountryById(receiverId);
        sender = FindCountryById(senderId);

        UISoundEffect.Instance.PlayAudio(regionUI.paper_01);

        if (receiver.isPlayer)
        {
            if (ReferencesManager.Instance.gameSettings.onlineGame)
            {
                receiverId = regionManager.currentRegionManager.currentCountry.country._id;
                Debug.Log(receiverId);
                Multiplayer.Instance.SendOffer(senderId, receiverId, offer);

                acceptationStatePanel.SetActive(true);
                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    acceptationStateText.text = $"Ваше предложение <b><color=\"blue\">было отправлено</color></b> игроку <b><color=\"yellow\">{receiver._countryPlayer.NickName}</b></color>";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    acceptationStateText.text = $"Your offer <b><color=\"blue\">was sent</color></b>to the player<b><color=\"yellow\">{receiver._countryPlayer.NickName}</b></color>";
                }
            }
        }
        else if(!receiver.isPlayer)
        {
            if (offer == "Объявить войну")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                senderToReceiver.war = true;
                senderToReceiver.trade = false;
                senderToReceiver.right = false;
                senderToReceiver.pact = false;
                senderToReceiver.union = false;

                sender.enemy = receiver;
                receiver.enemy = sender;

                sender.inWar = true;
                receiver.inWar = true;

                receiverToSender.war = true;
                receiverToSender.trade = false;
                receiverToSender.right = false;
                receiverToSender.pact = false;
                receiverToSender.union = false;

                senderToReceiver.relationship -= 100;
                receiverToSender.relationship -= 100;

                acceptationStatePanel.SetActive(true);

                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }
            }

            else if (offer == "Заключить мир")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                if (countRandom) random = Random.Range(0, 100);
                else random = 100;

                if (sender.score >= receiver.score)
                {
                    random += 40;
                }
                else if (sender.score < receiver.score)
                {
                    random -= 45;
                }

                if (ReferencesManager.Instance.gameSettings.diplomatyCheats)
                {
                    random = 9999;
                }

                if (random >= 50)
                {
                    accept = true;

                    senderToReceiver.war = false;
                    receiverToSender.war = false;

                    senderToReceiver.relationship += 25;
                    receiverToSender.relationship += 25;

                    acceptationStatePanel.SetActive(true);

                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                    }
                }
                else if (random < 50)
                {
                    accept = false;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                    }
                }
            }

            else if (offer == "Торговля")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                if (countRandom) random = Random.Range(0, 100);
                else random = 100;

                if (sender.ideology != receiver.ideology)
                {
                    random -= 10;
                }

                if (sender.myRegions.Count / receiver.myRegions.Count >= 1.4f)
                {
                    random += 40;
                }

                if (sender.ideology == "Фашизм" && receiver.ideology != "Фашизм")
                {
                    random -= 100;
                }

                if (receiver.ideology == "Фашизм" && sender.ideology != "Фашизм")
                {
                    random -= 100;
                }

                if (receiver.ideology == "Фашизм" && sender.ideology == "Фашизм")
                {
                    random += 100;
                }

                if (ReferencesManager.Instance.gameSettings.diplomatyCheats)
                {
                    random = 9999;
                }

                if (random >= 50)
                {
                    accept = true;

                    senderToReceiver.trade = true;
                    receiverToSender.trade = true;

                    senderToReceiver.relationship += 12;
                    receiverToSender.relationship += 12;

                    ReferencesManager.Instance.CalculateTradeBuff(sender, receiver);

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                    }
                }
                else if (random < 50)
                {
                    accept = false;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                    }
                }
            }

            else if (offer == "Прекратить торговлю")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                accept = true;

                senderToReceiver.trade = false;
                receiverToSender.trade = false;

                senderToReceiver.relationship -= 12;
                receiverToSender.relationship -= 12;

                for (int i = 0; i < globalTrades.Count; i++)
                {
                    if (globalTrades[i].sender == receiver && globalTrades[i].receiver == sender)
                    {
                        TradeBuff trade = globalTrades[i];

                        sender.moneyTradeIncome -= trade.senderMoneyTrade;
                        sender.foodTradeIncome -= trade.senderFoodTrade;

                        receiver.moneyTradeIncome -= trade.receiverMoneyTrade;
                        receiver.foodTradeIncome -= trade.receiverFoodTradee;

                        globalTrades.Remove(trade);
                    }
                }

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }
            }

            else if (offer == "Пакт о ненападении")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                if (countRandom) random = Random.Range(0, 100);
                else random = 100;

                if (senderToReceiver.relationship <= 12)
                {
                    random -= 25;
                }
                if (sender.ideology != receiver.ideology)
                {
                    random = -10;
                }

                if (ReferencesManager.Instance.gameSettings.diplomatyCheats)
                {
                    random = 9999;
                }

                if (random >= 50)
                {
                    accept = true;

                    senderToReceiver.pact = true;
                    receiverToSender.pact = true;

                    senderToReceiver.relationship += 18;
                    receiverToSender.relationship += 18;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                    }
                }
                else if (random < 50)
                {
                    accept = false;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                    }
                }
            }

            else if (offer == "Расторгнуть пакт о ненападении")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                accept = true;

                senderToReceiver.pact = false;
                receiverToSender.pact = false;

                senderToReceiver.relationship -= 18;
                receiverToSender.relationship -= 18;

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }
            }

            else if (offer == "Союз")
            {
                senderWars.Clear();
                receiverWars.Clear();
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                if (countRandom) random = Random.Range(0, 100);
                else random = 100;

                if (sender.ideology != receiver.ideology)
                {
                    random = -20;
                }

                foreach (CountrySettings country in countryManager.countries)
                {
                    Relationships.Relation relation = FindCountriesRelation(sender, country);

                    if (relation.war)
                    {
                        senderWars.Add(country);
                    }
                }

                foreach (CountrySettings country in countryManager.countries)
                {
                    Relationships.Relation relation = FindCountriesRelation(receiver, country);

                    if (relation.war)
                    {
                        receiverWars.Add(country);
                    }
                }

                foreach (CountrySettings country in senderWars)
                {
                    foreach (CountrySettings countryReceiverList in receiverWars)
                    {
                        if (country == countryReceiverList)
                        {
                            random += 100;
                        }
                    }
                }

                if (ReferencesManager.Instance.gameSettings.diplomatyCheats)
                {
                    random = 9999;
                }

                if (random >= 100)
                {
                    accept = true;

                    senderToReceiver.union = true;
                    receiverToSender.union = true;

                    senderToReceiver.relationship += 60;
                    receiverToSender.relationship += 60;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                    }
                }
                else if (random < 100)
                {
                    accept = false;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                    }
                }
            }

            else if (offer == "Расторгнуть союз")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                accept = true;

                senderToReceiver.union = false;
                receiverToSender.union = false;

                senderToReceiver.relationship -= 60;
                receiverToSender.relationship -= 60;

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }
            }

            else if (offer == "Право прохода войск")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                if (countRandom) random = Random.Range(0, 100);
                else random = 100;

                if (sender.ideology != receiver.ideology)
                {
                    random = -20;
                }

                if (senderToReceiver.relationship <= 12)
                {
                    random -= 25;
                }

                if (ReferencesManager.Instance.gameSettings.diplomatyCheats)
                {
                    random = 9999;
                }

                if (random >= 50)
                {
                    accept = true;

                    senderToReceiver.right = true;
                    receiverToSender.right = true;

                    senderToReceiver.relationship += 18;
                    receiverToSender.relationship += 18;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                    }
                }
                else if (random < 50)
                {
                    accept = false;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                    }
                }
            }

            else if (offer == "Расторгнуть право прохода войск")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                accept = true;

                senderToReceiver.right = false;
                receiverToSender.right = false;

                senderToReceiver.relationship -= 6;
                receiverToSender.relationship -= 6;

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }
            }

            else if (offer == "Сделать вассалом")
            {
                senderWars.Clear();
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                if (countRandom) random = Random.Range(0, 100);
                else random = 100;

                if (sender.myRegions.Count / receiver.myRegions.Count >= 3f)
                {
                    random += 150;
                }

                foreach (CountrySettings country in countryManager.countries)
                {
                    Relationships.Relation relation = FindCountriesRelation(sender, country);

                    if (relation.war)
                    {
                        senderWars.Add(country);
                    }
                }

                if (ReferencesManager.Instance.gameSettings.diplomatyCheats)
                {
                    random = 9999;
                }

                if (random >= 100)
                {
                    accept = true;

                    foreach (CountrySettings _cn in senderWars)
                    {
                        receiver.GetComponent<CountryAIManager>().SendOffer("Объявить войну", receiver, _cn);
                    }

                    senderToReceiver.vassal = true;
                    receiverToSender.vassal = false;

                    senderToReceiver.relationship += 60;
                    receiverToSender.relationship += 60;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                    }
                }
                else if (random < 100)
                {
                    accept = false;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                    }
                }
            }

            else if (offer == "Освободить вассала")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                random = 100;

                if (ReferencesManager.Instance.gameSettings.diplomatyCheats)
                {
                    random = 9999;
                }

                if (random >= 100)
                {
                    accept = true;

                    senderToReceiver.vassal = false;
                    receiverToSender.vassal = false;

                    senderToReceiver.relationship += 60;
                    receiverToSender.relationship += 60;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                    }
                }
                else if (random < 100)
                {
                    accept = false;

                    acceptationStatePanel.SetActive(true);
                    string currentLanguage = "";

                    if (PlayerPrefs.GetInt("languageId") == 0)
                    {
                        currentLanguage = "EN";
                    }
                    else if (PlayerPrefs.GetInt("languageId") == 1)
                    {
                        currentLanguage = "RU";
                    }

                    acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                    if (currentLanguage == "EN")
                    {
                        acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                    }
                }
            }

            UpdateDiplomatyUI(sender, receiver);
        }

        countryManager.UpdateIncomeValuesUI();
    }

    public void ShowValuePanel(string type)
    {
        offersContent.gameObject.SetActive(false);
        valuePanel.SetActive(true);

        valueType = type;

        string[] data = type.Split(' ');

        if (data[0] == "gold")
        {
            if (data[1] == "send")
            {
                textValueField.text = "Отправить золото";
                valueSlider.maxValue = sender.money;
            }
            else if (data[1] == "ask")
            {
                textValueField.text = "Попросить золото";
                valueSlider.maxValue = receiver.money;
            }
        }
        else if (data[0] == "food")
        {
            if (data[1] == "send")
            {
                textValueField.text = "Отправить провизию";
                valueSlider.maxValue = sender.food;
            }
            else if (data[1] == "ask")
            {
                textValueField.text = "Попросить провизию";
                valueSlider.maxValue = receiver.food;
            }
        }
        else if (data[0] == "recroots")
        {
            if (data[1] == "send")
            {
                textValueField.text = "Отправить рекрутов";
                valueSlider.maxValue = sender.recroots;
            }
            else if (data[1] == "ask")
            {
                textValueField.text = "Попросить рекрутов";
                valueSlider.maxValue = receiver.recroots;
            }
        }
    }

    public void CheckInputField()
    {
        string[] data = valueType.Split('\t');

        if (data[0] == "gold")
        {
            if (!string.IsNullOrEmpty(valueField.text))
            {
                if (int.Parse(valueField.text) > sender.money)
                {
                    valueField.text = sender.money.ToString();
                }
            }

        }
        else if (data[0] == "food")
        {
            if (!string.IsNullOrEmpty(valueField.text))
            {
                if (int.Parse(valueField.text) > sender.food)
                {
                    valueField.text = sender.money.ToString();
                }
            }
        }
        else if (data[0] == "recroots")
        {
            if (!string.IsNullOrEmpty(valueField.text))
            {
                if (int.Parse(valueField.text) > sender.recroots)
                {
                    valueField.text = sender.money.ToString();
                }
            }
        }
    }

    public void LinkInputFieldToSlider()
    {
        valueField.text = valueSlider.value.ToString();
        value = int.Parse(valueField.text);
    }

    public void LinkSliderToInputField()
    {
        valueSlider.value = int.Parse(valueField.text);
        value = (int)valueSlider.value;
    }

    public void ApplyValue()
    {
        Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
        Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

        int random = Random.Range(1, 10);
        int randomDiplomaty = Random.Range(0, 100);

        string[] data = valueType.Split(' ');

        if (data[0] == "gold")
        {
            if (data[1] == "send")
            {
                accept = true;
                sender.money -= value;
                receiver.money += value;
            }
            else if (data[1] == "ask")
            {
                if (receiver.money >= value)
                {
                    if (receiverToSender.relationship >= 60)
                    {
                        accept = true;
                        sender.money += value;
                        receiver.money -= value;
                    }
                    else accept = false;
                }
            }
        }
        else if (data[0] == "food")
        {
            if (data[1] == "send")
            {
                accept = true;
                sender.food -= value;
                receiver.food += value;
            }
            else if (data[1] == "ask")
            {
                if (receiver.food >= value)
                {
                    if (receiverToSender.relationship >= 60)
                    {
                        accept = true;
                        sender.food += value;
                        receiver.food -= value;
                    }
                    else accept = false;
                }
            }
        }
        else if (data[0] == "recroots")
        {
            if (data[1] == "send")
            {
                accept = true;
                sender.recroots -= value;
                receiver.recroots += value;
            }
            else if (data[1] == "ask")
            {
                if (receiver.recroots >= value)
                {
                    if (receiverToSender.relationship >= 60)
                    {
                        accept = true;
                        sender.recroots += value;
                        receiver.recroots -= value;
                    }
                    else accept = false;
                }
            }
        }

        Multiplayer.Instance.SetCountryValues(
            sender.country._id,
            sender.money,
            sender.food,
            sender.recroots);

        Multiplayer.Instance.SetCountryValues(
            receiver.country._id,
            receiver.money,
            receiver.food,
            receiver.recroots);

        if (accept)
        {
            acceptationStatePanel.SetActive(true);
            string currentLanguage = "";

            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                currentLanguage = "EN";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                currentLanguage = "RU";
            }

            acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

            if (currentLanguage == "EN")
            {
                acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
            }
        }
        else
        {
            acceptationStatePanel.SetActive(true);
            string currentLanguage = "";

            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                currentLanguage = "EN";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                currentLanguage = "RU";
            }

            acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

            if (currentLanguage == "EN")
            {
                acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
            }
        }

        countryManager.UpdateValuesUI();
        offersContent.gameObject.SetActive(true);
        valuePanel.SetActive(false);
    }

    public Relationships.Relation FindCountriesRelation(CountrySettings who, CountrySettings toFind)
    {
        Relationships.Relation result = null;

        foreach (Relationships.Relation relation in who.GetComponent<Relationships>().relationship)
        {
            if (relation.country == toFind)
            {
                result = relation;
            }
        }

        return result;
    }

    public CountrySettings FindCountryById(int id)
    {
        CountrySettings country = null;

        foreach (CountrySettings countryItem in countryManager.countries)
        {
            if (id == countryItem.country._id)
            {
                country = countryItem;
            }
        }

        return country;
    }

    private void UpdateCountryRelationsUI(GameObject horizontalScroll, CountrySettings country, string data)
    {
        if (data == "vassal")
        {
            foreach (Relationships.Relation relation in country.GetComponent<Relationships>().relationship)
            {
                if (relation.vassal)
                {
                    GameObject spawnedItem = Instantiate(countryItem, horizontalScroll.transform);
                    spawnedItem.GetComponent<FillCountryFlag>().country = relation.country.country;
                    spawnedItem.GetComponent<FillCountryFlag>().InDiplomatyUI = true;
                    spawnedItem.GetComponent<FillCountryFlag>().FillInfo();
                }
            }

            horizontalScroll.GetComponent<RectTransform>().sizeDelta = new Vector2(80 * horizontalScroll.transform.childCount, 75);
        }
        else if (data == "union")
        {
            foreach (Relationships.Relation relation in country.GetComponent<Relationships>().relationship)
            {
                if (relation.union)
                {
                    GameObject spawnedItem = Instantiate(countryItem, horizontalScroll.transform);
                    spawnedItem.GetComponent<FillCountryFlag>().country = relation.country.country;
                    spawnedItem.GetComponent<FillCountryFlag>().InDiplomatyUI = true;
                    spawnedItem.GetComponent<FillCountryFlag>().FillInfo();
                }
            }

            horizontalScroll.GetComponent<RectTransform>().sizeDelta = new Vector2(80 * horizontalScroll.transform.childCount, 75);
        }
        else if (data == "trade")
        {
            foreach (Relationships.Relation relation in country.GetComponent<Relationships>().relationship)
            {
                if (relation.trade)
                {
                    GameObject spawnedItem = Instantiate(countryItem, horizontalScroll.transform);
                    spawnedItem.GetComponent<FillCountryFlag>().country = relation.country.country;
                    spawnedItem.GetComponent<FillCountryFlag>().InDiplomatyUI = true;
                    spawnedItem.GetComponent<FillCountryFlag>().FillInfo();
                }
            }
            horizontalScroll.GetComponent<RectTransform>().sizeDelta = new Vector2(80 * horizontalScroll.transform.childCount, 75);
        }
        else if (data == "wars")
        {
            foreach (Relationships.Relation relation in country.GetComponent<Relationships>().relationship)
            {
                if (relation.war)
                {
                    GameObject spawnedItem = Instantiate(countryItem, horizontalScroll.transform);
                    spawnedItem.GetComponent<FillCountryFlag>().country = relation.country.country;
                    spawnedItem.GetComponent<FillCountryFlag>().InDiplomatyUI = true;
                    spawnedItem.GetComponent<FillCountryFlag>().FillInfo();
                }
            }
            horizontalScroll.GetComponent<RectTransform>().sizeDelta = new Vector2(80 * horizontalScroll.transform.childCount, 75);
        }
    }

    private void DestroyChildrens(Transform parent)
    {
        List<GameObject> childrens = new List<GameObject>();

        foreach (Transform child in parent.Cast<Transform>().ToArray())
        {
            childrens.Add(child.gameObject);
        }

        foreach (GameObject child in childrens)
        {
            child.transform.SetParent(null);
            DestroyImmediate(child);
        }
    }

    private void UpdateCountryCountTextUI(TMP_Text text, Transform parent)
    {
        int childcount = parent.childCount;
        text.text = childcount.ToString();
        parent.GetComponent<RectTransform>().sizeDelta = new Vector2(80 * parent.transform.childCount, 75);
    }

    public void UpdateDiplomatyUI(CountrySettings sender, CountrySettings receiver)
    {
        StartCoroutine(UpdateUI_Co(sender, receiver));

        for (int i = 0; i < offerButtons.Length; i++)
        {
            offerButtons[i].UpdateOfferData();
        }
    }

    public void AISendOffer(string offer, CountrySettings sender, CountrySettings receiver)
    {
        if (offer == "Объявить войну")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

            senderToReceiver.war = true;
            senderToReceiver.trade = false;
            senderToReceiver.right = false;

            sender.enemy = receiver;
            receiver.enemy = sender;

            sender.inWar = true;
            receiver.inWar = true;

            receiverToSender.war = true;
            receiverToSender.trade = false;
            receiverToSender.right = false;

            senderToReceiver.relationship -= 100;
            receiverToSender.relationship -= 100;

            acceptationStatePanel.SetActive(true);
            string currentLanguage = "";

            if (PlayerPrefs.GetInt("languageId") == 0)
            {
                currentLanguage = "EN";
            }
            else if (PlayerPrefs.GetInt("languageId") == 1)
            {
                currentLanguage = "RU";
            }

            acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

            if (currentLanguage == "EN")
            {
                acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
            }
        }

        else if (offer == "Заключить мир")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

            senderToReceiver.war = false;
            senderToReceiver.trade = false;
            senderToReceiver.right = false;


            receiverToSender.war = false;
            receiverToSender.trade = false;
            receiverToSender.right = false;

            senderToReceiver.relationship = 0;
            receiverToSender.relationship = 0;
        }

        else if (offer == "Торговля")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

            int random = Random.Range(0, 100);

            if (random >= 50)
            {
                accept = true;

                senderToReceiver.trade = true;
                receiverToSender.trade = true;

                senderToReceiver.relationship += 12;
                receiverToSender.relationship += 12;

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }
            }
            else if (random < 50)
            {
                accept = false;

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                }
            }
        }

        else if (offer == "Пакт о ненападении")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);


            int random = Random.Range(0, 100);

            if (random >= 50)
            {
                accept = true;

                senderToReceiver.pact = true;
                receiverToSender.pact = true;

                senderToReceiver.relationship += 18;
                receiverToSender.relationship += 18;

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }
            }
            else if (random < 50)
            {
                accept = false;

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                }
            }
        }

        else if (offer == "Союз")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);


            int random = Random.Range(0, 100);

            if (random >= 50)
            {
                accept = true;

                senderToReceiver.union = true;
                receiverToSender.union = true;

                senderToReceiver.relationship += 60;
                receiverToSender.relationship += 60;

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }
            }
            else if (random < 50)
            {
                accept = false;

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                }
            }
        }

        else if (offer == "Право прохода войск")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

            int random = Random.Range(0, 100);

            if (random >= 50)
            {
                accept = true;

                senderToReceiver.right = true;
                receiverToSender.right = true;

                senderToReceiver.relationship += 18;
                receiverToSender.relationship += 18;

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }
            }
            else if (random < 50)
            {
                accept = false;

                acceptationStatePanel.SetActive(true);
                string currentLanguage = "";

                if (PlayerPrefs.GetInt("languageId") == 0)
                {
                    currentLanguage = "EN";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    currentLanguage = "RU";
                }

                if (string.IsNullOrEmpty(currentLanguage))
                {
                    currentLanguage = "EN";
                }

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }
                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                }
            }
        }

        UpdateDiplomatyUI(sender, receiver);
    }

    private IEnumerator UpdateUI_Co(CountrySettings sender, CountrySettings receiver)
    {
        for (int i = 0; i < horizontalSenderScrolls.Length; i++)
        {
            DestroyChildrens(horizontalSenderScrolls[i].transform);
        }

        for (int i = 0; i < horizontalReceiverScrolls.Length; i++)
        {
            DestroyChildrens(horizontalReceiverScrolls[i].transform);
        }

        UpdateCountryRelationsUI(horizontalSenderScrolls[0], sender, "vassal");
        UpdateCountryRelationsUI(horizontalSenderScrolls[1], sender, "union");
        UpdateCountryRelationsUI(horizontalSenderScrolls[2], sender, "trade");
        UpdateCountryRelationsUI(horizontalSenderScrolls[3], sender, "wars");
        UpdateCountryRelationsUI(horizontalReceiverScrolls[0], receiver, "vassal");
        UpdateCountryRelationsUI(horizontalReceiverScrolls[1], receiver, "union");
        UpdateCountryRelationsUI(horizontalReceiverScrolls[2], receiver, "trade");
        UpdateCountryRelationsUI(horizontalReceiverScrolls[3], receiver, "wars");


        for (int i = 0; i < horizontalSenderCount.Length; i++)
        {
            UpdateCountryCountTextUI(horizontalSenderCount[i], horizontalSenderScrolls[i].transform);
        }


        for (int i = 0; i < horizontalReceiverCount.Length; i++)
        {
            UpdateCountryCountTextUI(horizontalReceiverCount[i], horizontalReceiverScrolls[i].transform);
        }

        relationPointsText.text = FindCountriesRelation(receiver, sender).relationship.ToString();

        yield return new WaitForSeconds(0.1f);
    }

    [System.Serializable]
    public class LocalOffer
    {
        public int id;
        public int relationshipPoints;
        public string offer;

        public CountrySettings sender;
        public CountrySettings receiver;
    }
}

[System.Serializable]
public class TradeBuff
{
    public int id;

    public CountrySettings sender;
    public CountrySettings receiver;

    public int senderMoneyTrade;
    public int receiverMoneyTrade;
    public int senderFoodTrade;
    public int receiverFoodTradee;
}