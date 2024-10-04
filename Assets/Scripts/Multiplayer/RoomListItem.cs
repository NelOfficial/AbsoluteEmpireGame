using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text roomPlayersCountText;

    public RoomInfo info;

    public void SetUp(RoomInfo _info)
    {
        info = _info;
        roomNameText.text = _info.Name;
        roomPlayersCountText.text = $"{_info.PlayerCount} / {_info.MaxPlayers}";
    }

    public void OnClick()
    {
        Launcher.Instance.JoinRoom(info);
    }
}
