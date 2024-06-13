using UnityEngine;
using TMPro;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text roomPlayersCountText;

    public string _roomName;


    public void SetUp()
    {
        roomNameText.text = _roomName;
    }
}
