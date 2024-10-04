using UnityEngine;

public class GuildUI : MonoBehaviour
{
    public static GuildUI Instance;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void EnableUI()
    {
        GuildManageMenu.Instance.Enable();

        gameObject.SetActive(true);
    } 
}