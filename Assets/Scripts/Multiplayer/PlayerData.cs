using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class PlayerData : MonoBehaviour
{
    [HideInInspector] public Player player;

    [HideInInspector] public string currentNickname;
    [HideInInspector] public int countryId;

    public bool readyToMove;
    [HideInInspector] public CountrySettings country;

    private GameSettings gameSettings;
    private CountryManager countryManager;

    private void Awake()
    {
        gameSettings = ReferencesManager.Instance.gameSettings;
        countryManager = ReferencesManager.Instance.countryManager;

        currentNickname = GetComponent<PhotonView>().Owner.NickName;

        gameSettings.multiplayer.roomPlayers.Add(this);
        player = GetComponent<PhotonView>().Owner;

        int playerCountryId = (int)this.GetComponent<PhotonView>().Owner.CustomProperties["playerCountryId"];
        countryId = playerCountryId;

        country = countryManager.FindCountryByID(countryId);

        country.isPlayer = true;
        country.onlinePlayer = true;
        country._countryPlayer = player;

        readyToMove = false;
    }
}
