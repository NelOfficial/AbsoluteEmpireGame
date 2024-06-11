using UnityEngine;
using TMPro;

public class ChatListItem : MonoBehaviour
{
    [SerializeField] TMP_Text nicknameText;
    [SerializeField] TMP_Text messageText;

    public string _author;
    public string _text;

    public void SetUp()
    {
        nicknameText.text = _author;
        messageText.text = _text;
    }
}
