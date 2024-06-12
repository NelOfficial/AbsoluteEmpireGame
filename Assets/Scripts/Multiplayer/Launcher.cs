using UnityEngine;
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class Launcher : NetworkManager
{
    public List<PlayerInfo> players = new List<PlayerInfo>();
    [SerializeField] private CurrentRoomInfo _roomInfoUI;

    [SerializeField] private TMP_InputField _roomNameInput;
    [SerializeField] private GameObject _currentRoomInfo_Panel;
    [SerializeField] private Transform _roomListPanel;
    [SerializeField] private GameObject _roomPrefab;

    public List<PlayerData> playersData = new List<PlayerData>();

    public List<RoomInfo> rooms = new List<RoomInfo>();

    [HideInInspector] public RoomInfo currentRoom;

    void RegisterClientHandlers()
    {
        NetworkClient.RegisterHandler<RoomListMessage>(OnRoomListMessage);
        NetworkClient.RegisterHandler<RoomJoinResponse>(OnRoomJoinResponse);
    }

    void OnRoomListMessage(RoomListMessage message)
    {
        UpdateRoomList(message.rooms);

        foreach (var room in message.rooms)
        {
            if (room.roomName == _roomNameInput.text) // Assuming this is the room the player joined
            {
                foreach (PlayerInfo player in room.players)
                {
                    Debug.Log(player);
                }

                Debug.Log(room.players.Count);

                UpdatePlayerListUI(room.players);
                break;
            }
        }
    }

    void OnRoomJoinResponse(RoomJoinResponse message)
    {
        if (message.success)
        {

        }
        else
        {
            Debug.LogError($"Failed to join the room. {message}");
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        RegisterClientHandlers();
        NetworkServer.RegisterHandler<CreateRoomMessage>(OnCreateRoom);
        NetworkServer.RegisterHandler<JoinRoomMessage>(OnJoinRoom);
        NetworkServer.RegisterHandler<PlayerNameMessage>(OnPlayerName);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        // Send the list of available rooms to the client
        if (rooms.Count > 0)
        {
            conn.Send(new RoomListMessage { rooms = rooms });
        }
    }

    public void OnCreateRoom(NetworkConnectionToClient conn, CreateRoomMessage message)
    {
        Debug.Log("OnCreateRoom called");
        Debug.Log($"Creating room: {message.roomName} with connectionId: {conn.connectionId}");

        RoomInfo newRoom = new RoomInfo { roomName = message.roomName, hostConnectionId = conn.connectionId, players = new List<PlayerInfo>() };
        rooms.Add(newRoom);
        NotifyAllClients();

        JoinRoom(_roomNameInput.text);
    }

    public void OnJoinRoom(NetworkConnectionToClient conn, JoinRoomMessage message)
    {
        Debug.Log("OnJoinRoom called");
        Debug.Log($"Joining room: {message.roomName} with connectionId: {conn.connectionId}");

        RoomInfo room = rooms.Find(r => r.roomName == message.roomName);
        if (room.hostConnectionId != 0)
        {
            Debug.Log($"Joining room: {message.roomName} with connectionId: {conn.connectionId}");

            PlayerInfo newPlayer = new PlayerInfo
            {
                connectionId = conn.connectionId,
                _playerNickname = "Player " + conn.connectionId.ToString()
            };

            room.players.Add(newPlayer);
            conn.Send(new RoomJoinResponse { success = true });

            currentRoom = room;

            NotifyAllClients();
        }
        else
        {
            Debug.LogError($"Failed to join the room. {room.hostConnectionId}");
            conn.Send(new RoomJoinResponse { success = false });
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        foreach (var room in rooms)
        {
            PlayerInfo player = room.players.Find(p => p.connectionId == conn.connectionId);
            if (player.connectionId != 0)
            {
                room.players.Remove(player);
                NotifyAllClients();
                break;
            }
        }
    }

    private void NotifyAllClients()
    {
        NetworkServer.SendToAll(new RoomListMessage { rooms = rooms });
    }

    private void OnPlayerName(NetworkConnectionToClient conn, PlayerNameMessage message)
    {
        foreach (var room in rooms)
        {
            PlayerInfo? player = room.players.Find(p => p.connectionId == conn.connectionId);
            if (player != null)
            {
                player._playerNickname = message.playerName;
                NotifyAllClients();
                break;
            }
            else
            {
                Debug.LogWarning($"Player with connectionId {conn.connectionId} not found in room {room.roomName}");
            }
        }
    }


    private void UpdatePlayerListUI(List<PlayerInfo> _players)
    {
        if (_roomInfoUI != null)
        {
            _roomInfoUI.UpdatePlayerList(_players);
        }
    }

    public void CreateRoom()
    {
        string roomName = _roomNameInput.text;
        if (!string.IsNullOrEmpty(roomName))
        {
            networkAddress = "localhost";
            StartHost();

            // Отправка сообщения о создании комнаты на сервер
            NetworkClient.Send(new CreateRoomMessage { roomName = roomName });
        }
    }

    public void UpdateRoomList(List<RoomInfo> rooms)
    {
        foreach (Transform child in _roomListPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (RoomInfo room in rooms)
        {
            GameObject roomListItem = Instantiate(_roomPrefab, _roomListPanel);
            roomListItem.GetComponent<RoomListItem>()._roomName = room.roomName;
            roomListItem.GetComponent<RoomListItem>().SetUp();

            roomListItem.GetComponent<Button>().onClick.AddListener(() => JoinRoom(room.roomName));
        }
    }

    void JoinRoom(string roomName)
    {
        networkAddress = "localhost";
        StartClient();

        // Отправка сообщения о присоединении к комнате на сервер
        NetworkClient.Send(new JoinRoomMessage { roomName = roomName });

        // Get nickname
        string playerName = PlayerPrefs.GetString("nickname", $"Player{Random.Range(0, 9999)}");

        NetworkClient.Send(new PlayerNameMessage { playerName = playerName });

        _currentRoomInfo_Panel.SetActive(true);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("Player added: " + conn.connectionId);
    }

    public class PlayerInfo
    {
        public int connectionId;
        public string _playerNickname;
        public int _countryIndex;
    }

    public struct RoomInfo
    {
        public string roomName;
        public int hostConnectionId;
        public List<PlayerInfo> players;
    }

    public struct CreateRoomMessage : NetworkMessage
    {
        public string roomName;
    }

    public struct JoinRoomMessage : NetworkMessage
    {
        public string roomName;
    }

    public struct RoomListMessage : NetworkMessage
    {
        public List<RoomInfo> rooms;
    }

    public struct RoomJoinResponse : NetworkMessage
    {
        public bool success;
    }

    public struct PlayerNameMessage : NetworkMessage
    {
        public string playerName;
    }
}
