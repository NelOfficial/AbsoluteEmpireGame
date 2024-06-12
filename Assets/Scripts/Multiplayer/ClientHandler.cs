using Mirror;
using static Launcher;
using UnityEngine;

public class ClientHandler : MonoBehaviour
{

    void Start()
    {
        NetworkClient.RegisterHandler<RoomListMessage>(OnRoomListMessage);
        NetworkClient.RegisterHandler<RoomJoinResponse>(OnRoomJoinResponse);
    }

    void OnRoomListMessage(RoomListMessage message)
    {
        ReferencesManager.Instance.launcher.UpdateRoomList(message.rooms);
    }

    void OnRoomJoinResponse(RoomJoinResponse message)
    {
        if (message.success)
        {
            // Successfully joined the room, switch to the game scene
            // SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogError("Failed to join the room.");
        }
    }
}
