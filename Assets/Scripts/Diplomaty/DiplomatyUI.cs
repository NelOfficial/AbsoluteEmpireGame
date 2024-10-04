using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

public class DiplomatyUI : MonoBehaviour
{
    [HideInInspector] public int senderId;
    [HideInInspector] public int receiverId;

    public GameObject diplomatyContainer;
    public GameObject messageReceiver;
    public GameObject acceptationStatePanel;
    public GameObject receiverInfo;

    //private PhotonView photonView;
    [HideInInspector] public CountryManager countryManager;
    [HideInInspector] public RegionManager regionManager;
    [HideInInspector] public RegionUI regionUI;
    [HideInInspector] public CountryInfoAdvanced countryInfoAdvanced;
    [HideInInspector] public Interpretate languageManager;

    [HideInInspector] public List<LocalOffer> localOffers = new();
    [HideInInspector] public List<LocalOffer> aiOffers = new();

    [SerializeField] Image senderCountryFlag;
    [SerializeField] Image receiverCountryFlag;

    [SerializeField] TMP_Text senderCountryNameText;
    [SerializeField] TMP_Text receiverCountryNameText;
    [SerializeField] TMP_Text acceptationStateText;
    [SerializeField] TMP_Text relationPointsText;

    [SerializeField] GameObject countryItem;

    [SerializeField] private GameObject _warCallPanel;
    [SerializeField] private GameObject _offersButtonsPanel;
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

    [HideInInspector] public List<CountrySettings> senderWars = new();
    private List<CountrySettings> receiverWars = new();

    public List<TradeBuff> globalTrades = new();

    [SerializeField] OfferButton[] offerButtons;

    public CountrySettings receiver;
    public CountrySettings sender;

    private string valueType;
    private string countriesSelectionAction;
    private int value;
    private bool accept;
    private int random;

    [SerializeField] private Transform senderWarsContainer;
    [SerializeField] private GameObject senderWarsItem;

    [HideInInspector] public string regionTransferType;

    [HideInInspector] public List<CountrySettings> _selectedCountries = new();

    private void Start()
    {
        countryManager = ReferencesManager.Instance.countryManager;
        countryInfoAdvanced = ReferencesManager.Instance.countryInfo;
        regionUI = ReferencesManager.Instance.regionUI;
        regionManager = ReferencesManager.Instance.regionManager;
        languageManager = ReferencesManager.Instance.languageManager;
    }

    public void OpenUI()
    {
        if (countryManager == null)
        {
            countryManager = ReferencesManager.Instance.countryManager;
        }

        DestroyChildrens();

        _warCallPanel.SetActive(false);
        _offersButtonsPanel.SetActive(true);
        offersContent.gameObject.SetActive(true);
        valuePanel.SetActive(false);

        UISoundEffect.Instance.PlayAudio(regionUI.paper_01);

        BackgroundUI_Overlay.Instance.OpenOverlay(diplomatyContainer);

        acceptationStatePanel.SetActive(false);
        receiverInfo.SetActive(false);

        senderId = countryManager.currentCountry.country._id;
        receiverId = regionManager.currentRegionManager.currentCountry.country._id;

        regionUI.barContent.SetActive(false);

        if (senderId != receiverId)
        {
            sender = FindCountryById(senderId);
            receiver = FindCountryById(receiverId);

            diplomatyContainer.SetActive(true);

            receiverCountryFlag.sprite = receiver.country.countryFlag;
            receiverCountryNameText.text = languageManager.GetTranslation(receiver.country._nameEN);
            senderCountryNameText.text = languageManager.GetTranslation(sender.country._nameEN);

            senderCountryFlag.sprite = sender.country.countryFlag;

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
        DestroyChildrens();

        _warCallPanel.SetActive(false);
        _offersButtonsPanel.SetActive(true);
        offersContent.gameObject.SetActive(true);
        valuePanel.SetActive(false);

        UISoundEffect.Instance.PlayAudio(regionUI.paper_01);

        diplomatyContainer.SetActive(true);

        acceptationStatePanel.SetActive(false);
        receiverInfo.SetActive(false);

        senderId = countryManager.currentCountry.country._id;
        receiverId = _receiverId;

        regionUI.barContent.SetActive(false);

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

            receiverCountryFlag.sprite = receiver.country.countryFlag;
            senderCountryFlag.sprite = sender.country.countryFlag;
            receiverCountryNameText.text = languageManager.GetTranslation(receiver.country._nameEN);
            senderCountryNameText.text = languageManager.GetTranslation(sender.country._nameEN);

            UpdateDiplomatyUI(sender, receiver);

            BackgroundUI_Overlay.Instance.InteractableFix();
            BackgroundUI_Overlay.Instance.OpenOverlay(diplomatyContainer);
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
        regionUI.barContent.SetActive(true);

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

    public void Execute_SendOffer(string offer, ReferencesManager referencesManager, bool countRandom = true)
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
                    acceptationStateText.text = $"Ваше предложение <b><color=\"blue\">было отправлено</color></b> игроку <b><color=\"yellow\">{receiver}</b></color>";
                }
                else if (PlayerPrefs.GetInt("languageId") == 1)
                {
                    acceptationStateText.text = $"Your offer <b><color=\"blue\">was sent</color></b>to the player<b><color=\"yellow\">{receiver}</b></color>";
                }
            }
        }
        else if(!receiver.isPlayer)
        {
            if (offer == "Объявить войну")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                accept = true;

                senderToReceiver.war = true;
                senderToReceiver.trade = false;
                senderToReceiver.right = false;
                senderToReceiver.pact = false;
                senderToReceiver.union = false;

                sender.enemies.Add(receiver);
                receiver.enemies.Add(sender);

                if (sender.myRegions.Count > 0 && receiver.myRegions.Count > 0)
                {
                    sender.stability.buffs.Add(new Stability_buff("Наступательная война", (-15 * (receiver.myRegions.Count / sender.myRegions.Count)) * (1 / receiver.enemies.Count), new List<string>() { $"not;ongoing_war;{sender.country._id}" }, null, ReferencesManager.Instance.sprites.Find("offensive_war")));
                    receiver.stability.buffs.Add(new Stability_buff("Оборонительная война", -5f, new List<string>() { $"not;ongoing_war;{receiver.country._id}" }, null, ReferencesManager.Instance.sprites.Find("defensive_war")));
                }

                sender.inWar = true;
                receiver.inWar = true;

                sender.inWar = sender.enemies.Count > 0;
                receiver.inWar = receiver.enemies.Count > 0;

                receiverToSender.war = true;
                receiverToSender.trade = false;
                receiverToSender.right = false;
                receiverToSender.pact = false;
                receiverToSender.union = false;

                ResourcesMarketManager market = ReferencesManager.Instance.resourcesMarketManager;

                var order = market.GetOrder(
                    sender.country, receiver.country, GameSettings.Resource.Oil);

                var orderSecond = market.GetOrder(
                    receiver.country, sender.country, GameSettings.Resource.Oil);

                market._marketOrders.Remove(order);
                market._marketOrders.Remove(orderSecond);

                senderToReceiver.relationship -= 100;
                receiverToSender.relationship -= 100;

                acceptationStatePanel.SetActive(true);
            }

            else if (offer == "Заключить мир")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                if (countRandom) random = UnityEngine.Random.Range(0, 25);
                else random = 100;

                if (sender.score >= receiver.score)
                {
                    random += UnityEngine.Random.Range(10, 30);
                }
                else if (sender.score < receiver.score)
                {
                    random -= UnityEngine.Random.Range(10, 30);
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

                    sender.enemies.Remove(receiver);
                    receiver.enemies.Remove(sender);

                    sender.inWar = sender.enemies.Count > 0;
                    receiver.inWar = receiver.enemies.Count > 0;

                    senderToReceiver.relationship += 25;
                    receiverToSender.relationship += 25;

                    acceptationStatePanel.SetActive(true);
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

                if (countRandom) random = UnityEngine.Random.Range(0, 100);
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
                }
                else if (random < 50)
                {
                    accept = false;

                    acceptationStatePanel.SetActive(true);
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
            }

            else if (offer == "Пакт о ненападении")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                if (countRandom) random = UnityEngine.Random.Range(0, 100);
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
                }
                else if (random < 50)
                {
                    accept = false;

                    acceptationStatePanel.SetActive(true);
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
            }

            else if (offer == "Союз")
            {
                senderWars.Clear();
                receiverWars.Clear();
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                if (countRandom) random = UnityEngine.Random.Range(10, 100);
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

                if (random >= 70)
                {
                    accept = true;

                    senderToReceiver.union = true;
                    receiverToSender.union = true;

                    senderToReceiver.relationship += 60;
                    receiverToSender.relationship += 60;

                    acceptationStatePanel.SetActive(true);
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
            }

            else if (offer == "Право прохода войск")
            {
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                if (countRandom) random = UnityEngine.Random.Range(0, 100);
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

                senderToReceiver.relationship -= 18;
                receiverToSender.relationship -= 18;

                acceptationStatePanel.SetActive(true);
            }

            else if (offer == "Сделать вассалом")
            {
                senderWars.Clear();
                Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
                Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

                if (countRandom) random = UnityEngine.Random.Range(0, 20);
                else random = 100;

                if (sender.myRegions.Count / receiver.myRegions.Count >= UnityEngine.Random.Range(2f, 4f))
                {
                    random += UnityEngine.Random.Range(5, 20);
                }

                if (receiver.civFactories > 0)
                {
                    if (sender.civFactories / receiver.civFactories >= UnityEngine.Random.Range(2f, 3f))
                    {
                        random += UnityEngine.Random.Range(5, 30);
                    }
                }

                if (receiver.farms > 0)
                {
                    if (sender.farms / receiver.farms >= UnityEngine.Random.Range(2f, 3f))
                    {
                        random += UnityEngine.Random.Range(0, 30);
                    }
                }

                if (receiver.chemicalFarms > 0)
                {
                    if (sender.chemicalFarms / receiver.chemicalFarms >= UnityEngine.Random.Range(2f, 3f))
                    {
                        random += UnityEngine.Random.Range(0, 30);
                    }
                }

                if (receiver.vassalOf != null)
                {
                    random -= UnityEngine.Random.Range(0, 15);
                }


                int relationsBonus = receiverToSender.relationship / 15;

                random += relationsBonus;

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

                    receiver.vassalOf = sender;

                    senderToReceiver.relationship += 60;
                    receiverToSender.relationship += 60;

                    acceptationStatePanel.SetActive(true);
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
                }
                else if (random < 100)
                {
                    accept = false;

                    acceptationStatePanel.SetActive(true);
                }
            }

            UpdateDiplomatyUI(sender, receiver);

            if (accept)
            {
                acceptationStateText.text = languageManager.GetTranslation("Diplomaty.Accepted");
            }
            else
            {
                acceptationStateText.text = languageManager.GetTranslation("Diplomaty.Declined");
            }
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
                textValueField.text = languageManager.GetTranslation("Diplomaty.SendMoney");

                valueSlider.maxValue = sender.money;
            }
            else if (data[1] == "ask")
            {
                textValueField.text = languageManager.GetTranslation("Diplomaty.AskMoney");

                valueSlider.maxValue = receiver.money;
            }
        }
        else if (data[0] == "food")
        {
            if (data[1] == "send")
            {
                textValueField.text = languageManager.GetTranslation("Diplomaty.SendFood");

                valueSlider.maxValue = sender.food;
            }
            else if (data[1] == "ask")
            {
                textValueField.text = languageManager.GetTranslation("Diplomaty.AskFood");

                valueSlider.maxValue = receiver.food;
            }
        }
        else if (data[0] == "recroots")
        {
            if (data[1] == "send")
            {
                textValueField.text = languageManager.GetTranslation("Diplomaty.SendRecruits");

                valueSlider.maxValue = sender.recruits;
            }
            else if (data[1] == "ask")
            {
                textValueField.text = languageManager.GetTranslation("Diplomaty.AskRecruits");

                valueSlider.maxValue = receiver.recruits;
            }
        }
        else if (data[0] == "fuel")
        {
            if (data[1] == "send")
            {
                textValueField.text = languageManager.GetTranslation("Diplomaty.SendFuel");

                valueSlider.maxValue = sender.fuel;
            }
            else if (data[1] == "ask")
            {
                textValueField.text = languageManager.GetTranslation("Diplomaty.AskFuel");

                valueSlider.maxValue = receiver.fuel;
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
                    valueField.text = sender.food.ToString();
                }
            }
        }
        else if (data[0] == "recroots")
        {
            if (!string.IsNullOrEmpty(valueField.text))
            {
                if (int.Parse(valueField.text) > sender.recruits)
                {
                    valueField.text = sender.recruits.ToString();
                }
            }
        }
        else if (data[0] == "fuel")
        {
            if (!string.IsNullOrEmpty(valueField.text))
            {
                if (int.Parse(valueField.text) > sender.fuel)
                {
                    valueField.text = sender.fuel.ToString();
                }
            }
        }
    }

    public void LinkInputFieldToSlider()
    {
        try
        {
            valueField.text = valueSlider.value.ToString();
            value = int.Parse(valueField.text);
        }
        catch (Exception) { }
    }

    public void LinkSliderToInputField()
    {
        try
        {
            valueSlider.value = int.Parse(valueField.text);
            value = (int)valueSlider.value;
        }
        catch (Exception) { }
    }

    public void ApplyValue()
    {
        Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
        Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

        int random = UnityEngine.Random.Range(1, 10);
        int randomDiplomaty = UnityEngine.Random.Range(0, 100);

        string[] data = valueType.Split(' ');

        if (data[0] == "gold")
        {
            if (data[1] == "send")
            {
                accept = true;
                sender.money -= value;
                receiver.money += value;
                receiverToSender.relationship += value / 500;
                senderToReceiver.relationship += value / 500;
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
                        receiverToSender.relationship -= value / 500;
                        senderToReceiver.relationship -= value / 500;
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
                receiverToSender.relationship += value / 1000;
                senderToReceiver.relationship += value / 1000;
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
                        receiverToSender.relationship -= value / 1000;
                        senderToReceiver.relationship -= value / 1000;
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
                sender.recruits -= value;
                receiver.recruits += value;
                receiverToSender.relationship += value / 1200;
                senderToReceiver.relationship += value / 1200;
            }
            else if (data[1] == "ask")
            {
                if (receiver.recruits >= value)
                {
                    if (receiverToSender.relationship >= 60)
                    {
                        accept = true;
                        sender.recruits += value;
                        receiver.recruits -= value;
                        receiverToSender.relationship -= value / 1200;
                        senderToReceiver.relationship -= value / 1200;
                    }
                    else accept = false;
                }
            }
        }
        else if (data[0] == "fuel")
        {
            if (data[1] == "send")
            {
                accept = true;
                sender.fuel -= value;
                receiver.fuel += value;
                receiverToSender.relationship += value / 750;
                senderToReceiver.relationship += value / 750;
            }
            else if (data[1] == "ask")
            {
                if (receiver.recruits >= value)
                {
                    if (receiverToSender.relationship >= 60)
                    {
                        accept = true;
                        sender.fuel += value;
                        receiver.fuel -= value;
                        receiverToSender.relationship -= value / 750;
                        senderToReceiver.relationship -= value / 750;
                    }
                    else accept = false;
                }
            }
        }

        UpdateDiplomatyUI(sender, receiver);

        Multiplayer.Instance.SetCountryValues(
            sender.country._id,
            sender.money,
            sender.food,
            sender.recruits);

        Multiplayer.Instance.SetCountryValues(
            receiver.country._id,
            receiver.money,
            receiver.food,
            receiver.recruits);

        ShowResultPanel();

        countryManager.UpdateValuesUI();
        offersContent.gameObject.SetActive(true);
        valuePanel.SetActive(false);
    }

    private void ShowResultPanel()
    {
        if (accept)
        {
            acceptationStatePanel.SetActive(true);

            string text = languageManager.GetTranslation("Diplomaty.Accepted");
            acceptationStateText.text = text;
        }
        else
        {
            acceptationStatePanel.SetActive(true);

            string _text = languageManager.GetTranslation("Diplomaty.Declined");
            acceptationStateText.text = _text;
        }
    }

    public void UpdateAskOfWarsContainer()
    {
        foreach (Transform child in senderWarsContainer)
        {
            Destroy(child.gameObject);
        }

        senderWars.Clear();

        foreach (CountrySettings country in countryManager.countries)
        {
            Relationships.Relation relation = FindCountriesRelation(sender, country);

            if (relation.war && country != receiver)
            {
                senderWars.Add(country);
            }
        }

        foreach (CountrySettings country in senderWars)
        {
            SelectCountryButton _senderWarCountryButton = Instantiate(senderWarsItem, senderWarsContainer).GetComponent<SelectCountryButton>();
            Relationships.Relation receiverRelations = FindCountriesRelation(receiver, country);

            if (receiverRelations.war)
            {
                _senderWarCountryButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                _senderWarCountryButton.GetComponent<Button>().interactable = true;
            }

            _senderWarCountryButton.askOfWar = true;
            _senderWarCountryButton.country_ScriptableObject = country.country;
            _senderWarCountryButton.country = country;
            _senderWarCountryButton.UpdateUI();
        }
    }

    public void SelectCountry(int countryID)
    {
        _selectedCountries.Add(countryManager.FindCountryByID(countryID));
    }

    public void DeSelectCountry(int countryID)
    {
        _selectedCountries.Remove(countryManager.FindCountryByID(countryID));
    }

    public void ApplySelectionOfCountries(string a)
    {
        countriesSelectionAction = a;

        string[] data = countriesSelectionAction.Split(';');

        if (data[0] == "ask_for_war")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

            int _random = UnityEngine.Random.Range(0, 30);

            if (ReferencesManager.Instance.gameSettings.diplomatyCheats)
            {
                _random = 99999;
            }

            if (senderToReceiver.union)
            {
                _random += UnityEngine.Random.Range(20, 50);
            }

            if (senderToReceiver.trade)
            {
                _random += UnityEngine.Random.Range(1, 5);
            }

            if (senderToReceiver.pact)
            {
                _random += UnityEngine.Random.Range(1, 10);
            }

            if (senderToReceiver.right)
            {
                _random += UnityEngine.Random.Range(1, 10);
            }

            if (_random >= 50)
            {
                accept = true;
            }
            else
            {
                accept = false;
            }

            if (accept)
            {
                for (int i = 0; i < _selectedCountries.Count; i++)
                {
                    AISendOffer("Объявить войну", receiver, _selectedCountries[i], false);
                }
            }

            ShowResultPanel();

            UpdateDiplomatyUI(sender, receiver);
        }
    }

    public Relationships.Relation FindCountriesRelation(CountrySettings who, CountrySettings toFind)
    {
        Relationships.Relation result = null;
        try
        {
            foreach (Relationships.Relation relation in who.GetComponent<Relationships>().relationship)
            {
                if (relation.country == toFind)
                {
                    result = relation;
                }
            }
        }
        catch (Exception)
        {
            Debug.LogError($"Who: {who.country._name} toFind {toFind.country._name}");
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
        Dictionary<string, Func<Relationships.Relation, bool>> relationCheckers = new()
        {
            { "vassal", relation => relation.vassal },
            { "union", relation => relation.union },
            { "trade", relation => relation.trade },
            { "wars", relation => relation.war }
        };

        if (!relationCheckers.ContainsKey(data))
            return;

        foreach (Relationships.Relation relation in country.GetComponent<Relationships>().relationship)
        {
            if (relationCheckers[data](relation))
            {
                FillCountryFlag spawnedItem = Instantiate(countryItem, horizontalScroll.transform).GetComponent<FillCountryFlag>();
                spawnedItem.country = relation.country.country;
                spawnedItem.InDiplomatyUI = true;
                spawnedItem.FillInfo();
            }
        }

        horizontalScroll.GetComponent<RectTransform>().sizeDelta = new Vector2(80 * horizontalScroll.transform.childCount, 75);
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

    public void AISendOffer(string offer, CountrySettings sender, CountrySettings receiver, bool countRandom)
    {
        Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
        Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);
        ResourcesMarketManager market = ReferencesManager.Instance.resourcesMarketManager;
        int random = countRandom ? UnityEngine.Random.Range(0, 100) : 9999999;
        bool accept = random >= 50;

        switch (offer)
        {
            case "Объявить войну":
                if (receiver.myRegions.Count > 0 && sender.myRegions.Count > 0)
                {
                    senderToReceiver.war = receiverToSender.war = true;
                    senderToReceiver.trade = receiverToSender.trade = false;
                    senderToReceiver.right = receiverToSender.right = false;
                    senderToReceiver.pact = receiverToSender.pact = false;
                    senderToReceiver.union = receiverToSender.union = false;
                    senderToReceiver.vassal = receiverToSender.vassal = false;

                    sender.enemies.Add(receiver);
                    receiver.enemies.Add(sender);
                    sender.inWar = receiver.inWar = true;

                    sender.stability.buffs.Add(new Stability_buff("Наступательная война", (-15 * (receiver.myRegions.Count / sender.myRegions.Count)) * (1 / receiver.enemies.Count), new List<string>() { $"not;ongoing_war;{sender.country._id}" }, null, ReferencesManager.Instance.sprites.Find("offensive_war")));
                    receiver.stability.buffs.Add(new Stability_buff("Оборонительная война", -5f, new List<string>() { $"not;ongoing_war;{receiver.country._id}" }, null, ReferencesManager.Instance.sprites.Find("defensive_war")));

                    // Удаление всех связанных с обоими странами заказов
                    market._marketOrders.RemoveAll(order =>
                        (order._seller == sender.country && order._customer == receiver.country) ||
                        (order._seller == receiver.country && order._customer == sender.country));

                    senderToReceiver.relationship -= 100;
                    receiverToSender.relationship -= 100;
                }
                break;

            case "Заключить мир":
                senderToReceiver.war = receiverToSender.war = false;
                senderToReceiver.trade = receiverToSender.trade = false;
                senderToReceiver.right = receiverToSender.right = false;
                senderToReceiver.pact = receiverToSender.pact = false;
                senderToReceiver.union = receiverToSender.union = false;
                senderToReceiver.vassal = receiverToSender.vassal = false;
                senderToReceiver.relationship = receiverToSender.relationship = 0;
                break;

            case "Торговля":
                if (accept)
                {
                    senderToReceiver.trade = receiverToSender.trade = true;
                    senderToReceiver.relationship += 12;
                    receiverToSender.relationship += 12;
                }
                break;

            case "Пакт о ненападении":
                if (accept)
                {
                    senderToReceiver.pact = receiverToSender.pact = true;
                    senderToReceiver.relationship += 18;
                    receiverToSender.relationship += 18;
                }
                break;

            case "Союз":
                if (accept)
                {
                    senderToReceiver.union = receiverToSender.union = true;
                    senderToReceiver.relationship += 60;
                    receiverToSender.relationship += 60;
                }
                break;

            case "Право прохода войск":
                if (accept)
                {
                    senderToReceiver.right = receiverToSender.right = true;
                    senderToReceiver.relationship += 18;
                    receiverToSender.relationship += 18;
                }
                break;

            case "Сделать вассалом":
                if (accept)
                {
                    senderToReceiver.vassal = receiverToSender.vassal = true;
                    senderToReceiver.relationship += 60;
                    receiverToSender.relationship += 60;
                }
                break;
        }

        //if (offer != "Заключить мир")
        //{
        //    acceptationStatePanel.SetActive(true);
        //    string message = accept ? "приняли" : "отклонили";
        //    string color = accept ? "green" : "red";
        //    string languageText = PlayerPrefs.GetInt("languageId") == 0 ? (accept ? "accepted" : "rejected") : message;

        //    acceptationStateText.text = $"They <b><color=\"{color}\">{languageText}</color></b> your offer";
        //}

        //UpdateDiplomatyUI(sender, receiver);
    }

    public void SpawnEvent(string offer, CountrySettings sender, CountrySettings receiver, bool canDecline)
    {
        GameObject spawned = Instantiate(ReferencesManager.Instance.regionUI.messageEvent);
        spawned.transform.SetParent(ReferencesManager.Instance.regionUI.messageReceiver);
        spawned.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        spawned.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);

        spawned.GetComponent<EventItem>().sender = sender;
        spawned.GetComponent<EventItem>().receiver = receiver;
        spawned.GetComponent<EventItem>().offer = offer;
        spawned.GetComponent<EventItem>().canDecline = canDecline;
        spawned.GetComponent<EventItem>().guildId = -1;

        spawned.GetComponent<EventItem>().senderImage.sprite = sender.country.countryFlag;


        if (offer == "Торговля")
        {
            spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.tradeSprite;
        }
        else if (offer == "Объявить войну")
        {
            spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.warSprite;
        }
        else if (offer == "Пакт о ненападении")
        {
            spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.pactSprite;
        }
        else if (offer == "Право прохода войск")
        {
            spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.moveSprite;
        }
        else if (offer == "Разорвать пакт о ненападении")
        {
            spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.AntipactSprite;
        }
        else if (offer == "GuildInvite")
        {
            spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.unionSprite;
        }
    }

    public void SpawnGuildMessage(MessageSettings settings)
    {
        if (ReferencesManager.Instance.countryManager.currentCountry == settings.receiver)
        {
            GameObject spawned = Instantiate(ReferencesManager.Instance.regionUI.messageEvent);
            spawned.transform.SetParent(ReferencesManager.Instance.regionUI.messageReceiver);
            spawned.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            spawned.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);

            EventItem messageItem = spawned.GetComponent<EventItem>();

            messageItem.sender = settings.sender;
            messageItem.receiver = settings.receiver;
            messageItem.offer = settings.offer;
            messageItem.canDecline = settings.isCanDecline;
            messageItem.guildId = settings.guildId;

            messageItem.senderImage.sprite = settings.sender.country.countryFlag;
        }
    }

    private IEnumerator UpdateUI_Co(CountrySettings sender, CountrySettings receiver)
    {
        yield return new WaitForSeconds(0.1f);

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
        yield break;
    }

    public void SetDiploRegionSelectionMode(string type)
    {
        ReferencesManager.Instance.gameSettings.provincesList.Clear();

        ReferencesManager.Instance.regionUI.cancelRegionSelectionModeButton.SetActive(true);
        ReferencesManager.Instance.regionUI.CloseAllUI();
        ReferencesManager.Instance.regionUI.barContent.SetActive(false);
        ReferencesManager.Instance.regionManager.DeselectRegions();

        ReferencesManager.Instance.gameSettings.regionSelectionMode = true;

        if (type == "receiver_country")
        {
            ReferencesManager.Instance.gameSettings.regionSelectionModeType = $"other_country;{receiverId}";
            ReferencesManager.Instance.gameSettings.provincesListMax = receiver.myRegions.Count;
            regionTransferType = "ask";
        }
        else
        {
            ReferencesManager.Instance.gameSettings.regionSelectionModeType = type;
            ReferencesManager.Instance.gameSettings.provincesListMax = sender.myRegions.Count;
            regionTransferType = "give";
        }
    }

    public void AcceptRegionsAndSendOffer()
    {
        regionManager.DeselectRegions();

        bool countRandom = !ReferencesManager.Instance.gameSettings.diplomatyCheats;

        Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
        Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

        if (regionTransferType == "give")
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

            if (countRandom) random = UnityEngine.Random.Range(0, 100);
            else random = 9999;

            random = 50;

            if (senderToReceiver.relationship > 50)
            {
                random += UnityEngine.Random.Range(10, 20);
            }

            if (sender.ideology == receiver.ideology)
            {
                random += UnityEngine.Random.Range(10, 15);
            }

            foreach (RegionManager province in ReferencesManager.Instance.gameSettings.provincesList)
            {
                foreach (CountryScriptableObject country in province.regionClaims)
                {
                    if (country._id == receiverId)
                    {
                        random += UnityEngine.Random.Range(20, 40);
                    }
                    //else if (country._id != receiverId)
                    //{
                    //    random -= UnityEngine.Random.Range(20, 40);
                    //}
                }
            }

            if (random >= 50)
            {
                accept = true;

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }

                int randomRelations = UnityEngine.Random.Range(5, 15);

                senderToReceiver.relationship += ReferencesManager.Instance.gameSettings.provincesList.Count * randomRelations;
                receiverToSender.relationship += ReferencesManager.Instance.gameSettings.provincesList.Count * randomRelations;


                foreach (RegionManager province in ReferencesManager.Instance.gameSettings.provincesList)
                {
                    ReferencesManager.Instance.AnnexRegion(province, receiver);
                }
            }
            else
            {
                accept = false;

                acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                }
            }
        }
        else if (regionTransferType == "ask")
        {
            accept = false;
            random = UnityEngine.Random.Range(0, 10);

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

            if (senderToReceiver.relationship > 50)
            {
                random += UnityEngine.Random.Range(5, 15);
            }

            if (sender.ideology == receiver.ideology)
            {
                random += UnityEngine.Random.Range(5, 10);
            }

            if (sender.ideology != receiver.ideology)
            {
                random -= UnityEngine.Random.Range(20, 25);
            }

            if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver).vassal)
            {
                random += 100;
            }

            foreach (RegionManager province in ReferencesManager.Instance.gameSettings.provincesList)
            {
                foreach (CountryScriptableObject country in province.regionClaims)
                {
                    if (country._id == senderId)
                    {
                        random += UnityEngine.Random.Range(20, 40);
                    }
                    else if (country._id != senderId)
                    {
                        random -= UnityEngine.Random.Range(20, 40);
                    }
                }
            }

            if (random >= 50)
            {
                accept = true;

                acceptationStateText.text = "Они <b><color=\"green\">приняли</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"green\">accepted</color></b> your offer";
                }

                foreach (RegionManager province in ReferencesManager.Instance.gameSettings.provincesList)
                {
                    ReferencesManager.Instance.AnnexRegion(province, sender);
                }
            }
            else
            {
                accept = false;

                acceptationStateText.text = "Они <b><color=\"red\">отклонили</color></b> ваше предложение";

                if (currentLanguage == "EN")
                {
                    acceptationStateText.text = "They are <b><color=\"red\">rejected</color></b> your offer";
                }
            }
        }

        ReferencesManager.Instance.gameSettings.regionSelectionMode = false;
        ReferencesManager.Instance.regionUI.cancelRegionSelectionModeButton.SetActive(false);
        UpdateDiplomatyUI(sender, receiver);
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

    public class MessageSettings
    {
        public CountrySettings sender;
        public CountrySettings receiver;

        public int guildId;
        public string offer;
        public bool isCanDecline;

        public MessageSettings(CountrySettings _sender, CountrySettings _receiver, int _guildId, string _offer, bool _isCanDecline)
        {
            this.sender = _sender;
            this.receiver = _receiver;
            this.guildId = _guildId;
            this.offer = _offer;
            this.isCanDecline = _isCanDecline;
        }
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