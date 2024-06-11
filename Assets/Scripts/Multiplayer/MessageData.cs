using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageData : MonoBehaviour
{
    public CountrySettings sender;
    public CountrySettings receiver;

    private DiplomatyUI diplomatyUI;

    public GameObject messagePanel;

    public Image messagePanelCountrySenderFlag;

    public TMP_Text messagePanelCountrySenderText;
    public TMP_Text messagePanelOfferText;

    [SerializeField] Button declineButton;

    public string d_Offer;
    public int d_OfferRelationshipPoints;
    public int d_OfferId;

    //private PhotonView photonView;

    private void Awake()
    {
        //photonView = GetComponent<PhotonView>();
        diplomatyUI = FindObjectOfType<DiplomatyUI>();
    }

    public void OpenMessage()
    {
        messagePanel.SetActive(true);
        messagePanelCountrySenderFlag.sprite = sender.country.countryFlag;
        messagePanelCountrySenderText.text = sender.country._name;
        messagePanelOfferText.text = d_Offer;

        if (d_Offer == "Объявить войну")
        {
            declineButton.interactable = false;
        }
        else
        {
            declineButton.interactable = true;
        }
    }

    public void Execute_AcceptOffer()
    {
        messagePanel.SetActive(false);
        //photonView.RPC("RPC_AcceptOnlineOffer", RpcTarget.All, d_OfferId);
    }

    public void Execute_DeclineOffer()
    {
        messagePanel.SetActive(false);
        //photonView.RPC("RPC_DeclineOnlineOffer", RpcTarget.All, d_OfferId);
    }

    //[PunRPC]
    public void RPC_AcceptOnlineOffer(int _offerId)
    {
        if (d_Offer == "Объявить войну")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

            senderToReceiver.war = true;
            senderToReceiver.trade = false;
            senderToReceiver.right = false;

            receiverToSender.war = true;
            receiverToSender.trade = false;
            receiverToSender.right = false;
        }
        else if (d_Offer == "Торговля")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);
            
            senderToReceiver.trade = true;
            receiverToSender.trade = true;
        }

        else if (d_Offer == "Пакт о ненападении")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

            senderToReceiver.pact = true;
            receiverToSender.pact = true;
        }

        else if (d_Offer == "Союз")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

            senderToReceiver.union = true;
            receiverToSender.union = true;
        }

        else if (d_Offer == "Право прохода войск")
        {
            Relationships.Relation senderToReceiver = FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = FindCountriesRelation(receiver, sender);

            senderToReceiver.right = true;
            receiverToSender.right = true;
        }

        Destroy(this.gameObject);
    }

    //[PunRPC]
    public void RPC_DeclineOnlineOffer(int _offerId)
    {
        Destroy(this.gameObject);
    }


    private Relationships.Relation FindCountriesRelation(CountrySettings who, CountrySettings toFind)
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
}