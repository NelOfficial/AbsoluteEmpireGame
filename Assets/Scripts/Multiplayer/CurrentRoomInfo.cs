//using UnityEngine;
//using Mirror;
//using System.Collections.Generic;
//using TMPro;

//public class CurrentRoomInfo : MonoBehaviour
//{
//    [SerializeField] private Transform _playerList_Transform;
//    [SerializeField] private GameObject _playerListItem;

//    public void UpdatePlayerList(List<Launcher.PlayerInfo> players)
//    {
//        foreach (Transform child in _playerList_Transform)
//        {
//            if (child.GetComponent<PlayerListItem>())
//            {
//                Destroy(child.gameObject);
//            }
//        }

//        foreach (var player in players)
//        {
//            GameObject spawnedPlayerListItem = Instantiate(_playerListItem, _playerList_Transform);

//            spawnedPlayerListItem.GetComponent<PlayerListItem>()._nickname = player._playerNickname;
//            spawnedPlayerListItem.GetComponent<PlayerListItem>().SetUp();
//        }
//    }
//}
