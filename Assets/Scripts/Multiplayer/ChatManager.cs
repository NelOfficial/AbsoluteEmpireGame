using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Chat;
using ExitGames.Client.Photon;
using TMPro;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    [SerializeField] string userID;
    public ChatClient chatClient;
    public string roomName;

    [SerializeField] TMP_InputField inputField;

    [SerializeField] Transform chatContent;
    [SerializeField] GameObject chatListPrefab;

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log($"{level} {message}");
    }

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log(state);
    }

    public void OnConnected()
    {
        chatClient.Subscribe($"roomChat_{roomName}");
    }

    public void OnDisconnected()
    {
        chatClient.Unsubscribe(new string[] { $"roomChat_{roomName}" });
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            SendMessage(channelName, senders, messages, i);
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new System.NotImplementedException();
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            GameObject spawnedChatItem = Instantiate(chatListPrefab, chatContent);
            spawnedChatItem.transform.localScale = new Vector3(1, 1, 1);

            spawnedChatItem.GetComponent<ChatListItem>()._text = $"�� ���������� � {channels[i]}";
            spawnedChatItem.GetComponent<ChatListItem>()._author = "[�������]";
            spawnedChatItem.GetComponent<ChatListItem>().SetUp();

            chatContent.GetComponent<VerticalLayoutGroup>().spacing = 10;
            chatContent.GetComponent<VerticalLayoutGroup>().spacing = 0;
        }
    }

    public void OnUnsubscribed(string[] channels)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            GameObject spawnedChatItem = Instantiate(chatListPrefab, chatContent);
            spawnedChatItem.transform.localScale = new Vector3(1, 1, 1);

            spawnedChatItem.GetComponent<ChatListItem>()._text = $"�� ��������� �� {channels[i]}";
            spawnedChatItem.GetComponent<ChatListItem>()._author = "[�������]";
            spawnedChatItem.GetComponent<ChatListItem>().SetUp();

            chatContent.GetComponent<VerticalLayoutGroup>().spacing = 10;
            chatContent.GetComponent<VerticalLayoutGroup>().spacing = 0;
        }
    }

    public void OnUserSubscribed(string channel, string user)
    {
        GameObject spawnedChatItem = Instantiate(chatListPrefab, chatContent);
        spawnedChatItem.transform.localScale = new Vector3(1, 1, 1);

        spawnedChatItem.GetComponent<ChatListItem>()._text = $"����� {user} ����������� � {channel}";
        spawnedChatItem.GetComponent<ChatListItem>()._author = "[�������]";
        spawnedChatItem.GetComponent<ChatListItem>().SetUp();

        chatContent.GetComponent<VerticalLayoutGroup>().spacing = 10;
        chatContent.GetComponent<VerticalLayoutGroup>().spacing = 0;
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        GameObject spawnedChatItem = Instantiate(chatListPrefab, chatContent);

        spawnedChatItem.transform.localScale = new Vector3(1, 1, 1);
        spawnedChatItem.GetComponent<ChatListItem>()._text = $"����� {user} ��������� �� {channel}";
        spawnedChatItem.GetComponent<ChatListItem>()._author = "[�������]";
        spawnedChatItem.GetComponent<ChatListItem>().SetUp();

        chatContent.GetComponent<VerticalLayoutGroup>().spacing = 10;
        chatContent.GetComponent<VerticalLayoutGroup>().spacing = 0;
    }

    public void DissconectUser()
    {
        chatClient.Unsubscribe(new string[] { $"roomChat_{roomName}" });
        chatClient.Disconnect();

        foreach (Transform child in chatContent)
        {
            Destroy(child.gameObject);
        }
    }

    void Start()
    {
        ConnectUser();
    }

    public void ConnectUser()
    {
        userID = PlayerPrefs.GetString("nickname");

        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion, new AuthenticationValues(userID));
    }

    private void Update()
    {
        chatClient.Service();
    }

    public void SendMessageButton()
    {
        if (!string.IsNullOrEmpty(inputField.text) && !string.IsNullOrWhiteSpace(inputField.text))
        {
            chatClient.PublishMessage($"roomChat_{roomName}", inputField.text);
            inputField.text = "";
        }
    }

    private void SendMessage(string channelName, string[] senders, object[] messages, int i)
    {
        GameObject spawnedChatItem = Instantiate(chatListPrefab, chatContent);

        spawnedChatItem.transform.localScale = new Vector3(1, 1, 1);
        spawnedChatItem.GetComponent<ChatListItem>()._text = messages[i].ToString();
        spawnedChatItem.GetComponent<ChatListItem>()._author = senders[i].ToString();
        spawnedChatItem.GetComponent<ChatListItem>().SetUp();

        chatContent.GetComponent<VerticalLayoutGroup>().spacing = 10;
        chatContent.GetComponent<VerticalLayoutGroup>().spacing = 0;
    }
}
