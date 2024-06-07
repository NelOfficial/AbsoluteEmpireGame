using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class PlayerData : MonoBehaviour
{
    public Player player;

    public string currentNickname;
    public int countryIndex;
    public bool readyToMove;
    public CountrySettings country;

    private GameSettings gameSettings;
    private CountryManager countryManager;

    private void Awake()
    {
        gameSettings = FindObjectOfType<GameSettings>();
        countryManager = FindObjectOfType<CountryManager>();
        currentNickname = GetComponent<PhotonView>().Owner.NickName;

        gameSettings.multiplayer.roomPlayers.Add(this);
        player = GetComponent<PhotonView>().Owner;

        int playerCountryIndex = (int)this.GetComponent<PhotonView>().Owner.CustomProperties["playerCountryIndex"];
        countryIndex = playerCountryIndex;
        country = countryManager.countries[countryIndex];

        country.isPlayer = true;
        country.onlinePlayer = true;
        country._countryPlayer = player;

        Debug.Log($"{currentNickname} {playerCountryIndex}");
    }
}
