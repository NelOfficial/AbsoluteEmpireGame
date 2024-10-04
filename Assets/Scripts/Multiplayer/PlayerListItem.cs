using TMPro;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    private Player _player;
    private Launcher _launcher;

    [HideInInspector] public string _nickname;

    public Image selectedCountryFlag;
    public TMP_Text nicknameText;

    public int countryId;

    readonly Hashtable playerProperties = new();

    public void SelectCountry(int id)
    {
        playerProperties["playerCountryId"] = id;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public void SetUp(Player player)
    {
        _launcher = FindObjectOfType<Launcher>();

        _player = player;
        _nickname = _player.NickName;
        nicknameText.text = _nickname;

        playerProperties["playerNickname"] = _nickname;

        SelectCountry(_launcher.countriesList[0]._id);
        UpdatePlayerCountry(_player);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (_player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (_player == targetPlayer)
        {
            UpdatePlayerCountry(targetPlayer);
        }
    }

    private void UpdatePlayerCountry(Player _player)
    {
        if (_player.CustomProperties.ContainsKey("playerCountryId"))
        {
            int countryId = (int)_player.CustomProperties["playerCountryId"];
            CountryScriptableObject country = ReferencesManager.Instance.FindCountryObjectByID(countryId, _launcher.countriesList);

            selectedCountryFlag.sprite = country.countryFlag;
            playerProperties["playerCountryId"] = _player.CustomProperties["playerCountryId"];
        }
        else
        {
            int randomCountry = Random.Range(0, _launcher.countriesList.Length);

            playerProperties["playerCountryId"] = randomCountry;
        }
    }
}
