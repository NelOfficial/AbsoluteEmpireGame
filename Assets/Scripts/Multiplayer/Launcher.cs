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

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        UpdatePlayerListUI();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        PlayerInfo player = players.Find(p => p.connectionId == conn.connectionId);
        if (player != null)
        {
            players.Remove(player);
        }
        UpdatePlayerListUI();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // ѕолучение ника из сохраненных данных или по умолчанию
        string playerName = PlayerPrefs.GetString("nickname", $"Guest{1, 9999}");

        // —оздание и добавление нового игрока
        PlayerInfo newPlayer = new PlayerInfo
        {
            connectionId = conn.connectionId,
            _playerNickname = playerName
        };

        players.Add(newPlayer);
        Debug.Log("Player added: " + playerName);
        UpdatePlayerListUI();
    }

    private void UpdatePlayerListUI()
    {
        if (_roomInfoUI != null)
        {
            _roomInfoUI.UpdatePlayerList(players);
        }
    }

    public void CreateRoom()
    {
        string roomName = _roomNameInput.text;
        if (!string.IsNullOrEmpty(roomName))
        {
            StartHost();

            _currentRoomInfo_Panel.SetActive(true);
        }
    }

    public class PlayerInfo
    {
        public int connectionId;
        public string _playerNickname;
        public int _countryID;
    }

    public struct RoomInfo
    {
        public string roomName;
        public int hostConnectionId;
        public List<PlayerInfo> players;
    }
}
