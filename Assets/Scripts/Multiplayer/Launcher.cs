using TMPro;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Launcher : MonoBehaviourPunCallbacks
{
    private static Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    public static Launcher Instance;

    [SerializeField] MainMenu mainMenu;

    [SerializeField] TMP_InputField roomNameField;
    [SerializeField] TMP_Dropdown scenariyDropdown;

    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListPrefab;
    [SerializeField] GameObject playerListPrefab;
    [SerializeField] GameObject startGameButton;

    public CountryScriptableObject[] countriesList;

    [SerializeField] BoolValue onlineGameValue;


    private void Awake()
    {
        Instance = this;
        mainMenu.loadingMenu.SetActive(false);

        if (!ReferencesManager.Instance.profileManager._LOGGED_IN && PlayerPrefs.GetString("LOGGED_IN") == "FALSE")
        {
            PhotonNetwork.NickName = "Guest player " + Random.Range(0, 9999);
            mainMenu.UpdateNickname(PhotonNetwork.NickName);
        }
    }

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        mainMenu.connectionText.color = Color.cyan;
        mainMenu.connectionText.text = "Connecting to Master...";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        mainMenu.connectionText.color = Color.yellow;
        mainMenu.connectionText.text = "Connected to Master";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        mainMenu.connectionText.color = Color.green;
        mainMenu.connectionText.text = "Joined Lobby";
    }

    public void SetOnlineValue(bool value)
    {
        onlineGameValue.value = value;
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();

        string roomName = roomNameField.text;
        if (string.IsNullOrWhiteSpace(roomName)) roomName = "Room " + Random.Range(0, 9999);

        string scenariy = scenariyDropdown.options[scenariyDropdown.value].text;

        Hashtable properties = new Hashtable();
        properties.Add("scen", scenariy);
        roomOptions.CustomRoomProperties = properties;
        roomOptions.BroadcastPropsChangeToAll = true;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        mainMenu.CloseMenus();
        mainMenu.currentRoomText.text = PhotonNetwork.CurrentRoom.Name;
        mainMenu.currentScenariyText.text = PhotonNetwork.CurrentRoom.CustomProperties["scen"].ToString();
        mainMenu.currentRoomMenu.SetActive(true);

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void StartGame()
    {
        mainMenu.loadingMenu.SetActive(true);
        PhotonNetwork.LoadLevel(2);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
    }

    public override void OnLeftRoom()
    {
        mainMenu.OpenMenu(mainMenu.menus[0]);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }

        foreach (KeyValuePair<string, RoomInfo> entry in cachedRoomList)
        {
            Instantiate(roomListPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(cachedRoomList[entry.Key]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }
}
